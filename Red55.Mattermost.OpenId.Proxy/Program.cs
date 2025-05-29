using System.Reflection;

using Destructurama;

using Microsoft.Extensions.Options;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;
using Red55.Mattermost.OpenId.Proxy.Models;
using Red55.Mattermost.OpenId.Proxy.Transforms;

using Refit;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;

var builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
    .Destructure.UsingAttributes ()
    .Enrich.FromLogContext ()
    .Enrich.WithCorrelationId ()
    .Enrich.WithExceptionDetails (new DestructuringOptionsBuilder ()
        .WithDefaultDestructurers ()
        .WithDestructurers ([new ApiExceptionDestructurer (destructureCommonExceptionProperties: false)]))
    .MinimumLevel.Debug ()
    .WriteTo.Console (outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger ();

try
{
    var environment = builder.Environment.EnvironmentName;
    Log.Logger.Debug ("Starting application in environment '{Environment}'", environment);
    Log.Logger.Debug ("Command line arguments: {Args}", string.Join (" ", args));

    builder.Configuration.Sources.Clear ();
    _ = builder.Configuration
        .AddEnvironmentVariables ("ASPNETCORE_")
        .AddCommandLine (args)
        .AddEnvironmentVariables ("DOTNET_")
        .AddYamlFile ("appsettings.yml", optional: false)
        .AddYamlFile ($"appsettings.{environment}.yml", optional: true)
        .AddUserSecrets (Assembly.GetExecutingAssembly ())
        .AddEnvironmentVariables ();

    var appConfigSection = builder.Configuration.GetRequiredSection (AppConfig.SectionName);
    var appConfig = EnsureArg.IsNotNull (appConfigSection.Get<AppConfig> ());

    _ = builder.Services
        .AddOptions<AppConfig> ()
        .Bind (appConfigSection)
        .ValidateDataAnnotations ()
        .ValidateOnStart ();

    _ = builder.Services
        .AddReverseProxy ()
        .AddTransformFactory<TransformFactory> ()
        .LoadFromConfig (builder.Configuration.GetRequiredSection ("ReverseProxy"));


    _ = builder.Services
        .AddRefitClient<IUser> (services =>
        {
            return new ()
            {
                AuthorizationHeaderValueGetter = (req, cancel) =>
                {
                    return Task.FromResult (appConfig.GitLab.PAT);

                },
                ContentSerializer = new SystemTextJsonContentSerializer (
                    new ()
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
                    }
                    ),
            };
        })
        .ConfigureHttpClient (c => c.BaseAddress = appConfig.GitLab.Url);


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

    _ = builder.Host.UseSerilog ((context, services, configuration) => configuration
                         .ReadFrom.Configuration (context.Configuration)
                         .ReadFrom.Services (services)
                         .Enrich.FromLogContext ()
                         .Enrich.WithCorrelationId ()
                         .Enrich.WithSpan ()
                         .Destructure.UsingAttributes ());

    var app = builder.Build ();


    _ = app.MapControllers ();

    _ = app.MapReverseProxy ();

    if (Log.IsEnabled (Serilog.Events.LogEventLevel.Information))
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