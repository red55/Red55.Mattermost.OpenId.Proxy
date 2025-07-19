using System.Reflection;

using Destructurama;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;
using Red55.Mattermost.OpenId.Proxy.Extensions;
using Red55.Mattermost.OpenId.Proxy.Middlewares.HttpClient;
using Red55.Mattermost.OpenId.Proxy.Middlewares.Kestrel;
using Red55.Mattermost.OpenId.Proxy.Models;
using Red55.Mattermost.OpenId.Proxy.Services;
using Red55.Mattermost.OpenId.Proxy.Services.HealthChecks;
using Red55.Mattermost.OpenId.Proxy.Storage;
using Red55.Mattermost.OpenId.Proxy.Transforms;

using Refit;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Templates;

var builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
    .Destructure.UsingAttributes ()
    .Enrich.FromLogContext ()
    .Enrich.WithCorrelationId ()
    .Enrich.WithExceptionDetails (new DestructuringOptionsBuilder ()
        .WithDefaultDestructurers ()
        .WithDestructurers ([new ApiExceptionDestructurer (destructureCommonExceptionProperties: false)]))
    .MinimumLevel.Debug ()
    .WriteTo.Console (new ExpressionTemplate ("[{@t:yyyy-MM-ddTHH:mm:ss} {Coalesce(CorrelationId, '0000000000000:00000000')} {@l:u3}] {@m}\n{@x}"))
    .CreateBootstrapLogger ();
var version = Assembly.GetExecutingAssembly ().GetCustomAttribute<AssemblyFileVersionAttribute> ();
if (version is not null)
{
    Log.Logger.Information ("Starting Red55.Mattermost.OpenId.Proxy v{Version}", version.Version);
}
else
{
    Log.Logger.Information ("Starting Red55.Mattermost.OpenId.Proxy (version unknown)");
}

try
{
    var environment = builder.Environment.EnvironmentName;

    builder.Configuration.Sources.Clear ();
    _ = builder.Configuration
        .AddEnvironmentVariables ("ASPNETCORE_")
        .AddCommandLine (args)
        .AddEnvironmentVariables ("DOTNET_")
        .AddYamlFile ("appsettings.yml", optional: false)
        .AddYamlFile ($"appsettings.{environment}.yml", optional: true)
        .AddUserSecrets (Assembly.GetExecutingAssembly ())
        .AddEnvironmentVariables ("");

    var appConfigSection = builder.Configuration.GetRequiredSection (AppConfig.SectionName);
    var appConfig = EnsureArg.IsNotNull (appConfigSection.Get<AppConfig> ());

    _ = builder.Services
        .AddOptions<AppConfig> ()
        .Bind (appConfigSection)
        .ValidateDataAnnotations ()
        .ValidateOnStart ();

    _ = builder.Services
        .AddSingleton<IPatStore, PatStore> ()
        .AddTransient<HttpRequestOptionsHandler> ()
        .AddHostedService<PatRotateService> ()
        ;

    _ = builder.Services
        .AddHttpContextAccessor ()
        .AddReverseProxy ()
        .AddTransformFactory<DisableSecureCookiesTransformFactory> ()
        .AddTransformFactory<ReplaceInResponseTransformFactory> ()
        .AddTransformFactory<ReplaceInRequestTransformFactory> ()
        .LoadFromConfig (builder.Configuration.GetRequiredSection ("ReverseProxy"));
    // Set DangerousAcceptAnyServerCertificate in appsettings.yml/appsettings.Development.yml under each cluster's HttpClient section.
    var refitSettings = new RefitSettings ()
    {
        AuthorizationHeaderValueGetter = async (req, cancel) =>
        {
            cancel.ThrowIfCancellationRequested ();
            if (req.Options.TryGetValue (new HttpRequestOptionsKey<IPatStore> (nameof (IPatStore)), out var patStore))
            {
                return await patStore.GetTokenAsync (cancel);
            }

            return appConfig.GitLab.PAT.BootstrapToken;

        },
        ContentSerializer = new SystemTextJsonContentSerializer (new ()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
        }
        ),
    };

    _ = builder.Services
        .AddRefitClient<IUser> (services => refitSettings)
        .ConfigureHttpClient (c => c.BaseAddress = appConfig.GitLab.Url)
        .ConfigureAdditionalHttpMessageHandlers ((handlers, services) => handlers.Add (services.GetRequiredService<HttpRequestOptionsHandler> ()));

    _ = builder.Services
        .AddRefitClient<IPersonalAccessTokens> (services => refitSettings)
        .ConfigureHttpClient (c => c.BaseAddress = appConfig.GitLab.Url)
        .ConfigureAdditionalHttpMessageHandlers ((handlers, services) => handlers.Add (services.GetRequiredService<HttpRequestOptionsHandler> ()));
    _ = builder.Services
          .AddRefitClient<IGitlabHealthChecks> (services => refitSettings)
          .ConfigureHttpClient (c => c.BaseAddress = appConfig.GitLab.Url)
          .ConfigureAdditionalHttpMessageHandlers ((handlers, services) => handlers.Add (services.GetRequiredService<HttpRequestOptionsHandler> ()));


    // Register the named HttpClient for GitLab API
    _ = builder.Services.AddHttpClient ("GitLabApi", c =>
    {
        c.BaseAddress = appConfig.GitLab.Url;
    });

    _ = builder.Services
        .AddControllers ()
            .AddJsonOptions (o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
            });
    _ = builder.Services
        .ConfigureHttpJsonOptions (o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
            o.SerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        });

    _ = builder.Services
        .AddHttpClient ("OpenIdApiChecks", c =>
        {
        }).ConfigurePrimaryHttpMessageHandler (() =>
        {
            var r = new HttpClientHandler ();

            if (appConfig.OpenId.HealthChecks != OpenIdHealthChecks.Empty &&
                appConfig.OpenId.HealthChecks.DangerousAcceptAnyServerCertificate)
            {
                r.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return r;
        });
    // Register the custom health check
    var checks = builder.Services
        .AddHealthChecks ();
    _ = checks
        .AddCheck<GitLabApiReadinessCheck> ("gitlab_readiness", tags: ["ready"])
        .AddCheck<GitLabApiLivenessCheck> ("gitlab_liveness", tags: ["alive"]);

    if (appConfig.OpenId.HealthChecks != OpenIdHealthChecks.Empty)
    {
        if (!appConfig.OpenId.HealthChecks.LivenessUrl.IsNullOrEmpty ())
        {
            _ = checks.AddCheck<OpenIdApiLivenessCheck> ("open_id_liveness", tags: ["alive"]);
        }
        if (!appConfig.OpenId.HealthChecks.ReadinessUrl.IsNullOrEmpty ())
        {
            _ = checks.AddCheck<OpenIdApiReadinessCheck> ("open_id_readiness", tags: ["ready"]);
        }
    }
    else
    {
        Log.Logger.Warning ("OpenID Health Checks are not configured. Skipping OpenID health checks registration.");
    }

    _ = builder.Host.UseSerilog ((context, services, configuration) => configuration
        .ReadFrom.Configuration (context.Configuration)
        .ReadFrom.Services (services)
        .Enrich.FromLogContext ()
        .Enrich.WithCorrelationId ()
        .Enrich.WithSpan ()
        .Destructure.UsingAttributes ());

    var app = builder.Build ();

    _ = app.UseMiddleware<CorrelationIdMiddleware> ();

    _ = app.MapHealthChecks ("/healthz/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains ("ready")
    });
    _ = app.MapHealthChecks ("/healthz/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains ("alive")
    });

    _ = app.MapControllers ();

    _ = app.MapReverseProxy ();

    if (Log.Logger.IsEnabled (Serilog.Events.LogEventLevel.Information))
    {
        var cfg = app.Services.GetRequiredService<IOptions<AppConfig>> ();
        Log.Logger.Information ("Application configuration: {@AppConfig}", cfg.Value);
    }


    await app.RunAsync ();

}
catch (Exception e)
{
    Log.Logger.Error (e, "Unhandled exception in Main");
    return -1;
}
finally
{
    await Log.CloseAndFlushAsync ();
}

return 0;