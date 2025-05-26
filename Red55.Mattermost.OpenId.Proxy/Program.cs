using Microsoft.EntityFrameworkCore;

using OpenIddict.Client;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;

using Refit;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;

using static OpenIddict.Abstractions.OpenIddictConstants;

string[] scopes = ["openid", "profile", "email"];

var builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
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

    builder.Configuration.Sources.Clear ();
    builder.Configuration
        .AddEnvironmentVariables ("ASPNETCORE_")
        .AddCommandLine (args)
        .AddEnvironmentVariables ("DOTNET_")
        .AddYamlFile ("appsettings.yml", optional: false)
        .AddYamlFile ($"appsettings.{environment}.yml", optional: true);

    var appConfigSection = builder.Configuration.GetRequiredSection (AppConfig.SectionName);
    var appConfig = EnsureArg.IsNotNull (appConfigSection.Get<AppConfig> ());

    builder.Services
        .AddOptions<AppConfig> ()
        .Bind (appConfigSection)
        .ValidateDataAnnotations ()
        .ValidateOnStart ();

    builder.Services
        .AddTransient<Red55.Mattermost.OpenId.Proxy.Transforms.Request> ()
        .AddTransient<Red55.Mattermost.OpenId.Proxy.Transforms.Response> ()
        .AddReverseProxy ()
        .LoadFromConfig (builder.Configuration.GetRequiredSection ("ReverseProxy"))
        .AddTransforms (builder =>
        {
            builder.RequestTransforms.Add (builder.Services.GetRequiredService<Red55.Mattermost.OpenId.Proxy.Transforms.Request> ());
            builder.ResponseTransforms.Add (builder.Services.GetRequiredService<Red55.Mattermost.OpenId.Proxy.Transforms.Response> ());
        });

    builder.Services
        .AddRefitClient<IUser> (services =>
        {
            return new ()
            {
                AuthorizationHeaderValueGetter = async (req, cancel) =>
                {

                    var c = services.GetRequiredService<OpenIddictClientService> ();

                    var challenge = await c.ChallengeInteractivelyAsync (new ()
                    {
                        CancellationToken = cancel,
                        GrantType = GrantTypes.AuthorizationCode,
                        CodeChallengeMethod = CodeChallengeMethods.Sha256,
                        ResponseType = ResponseTypes.Code
                    });

                    var authentication = await c.AuthenticateInteractivelyAsync (new ()
                    {
                        CancellationToken = cancel,
                        Nonce = challenge.Nonce,
                    });
                    /*
                    var t = await c.AuthenticateWithPasswordAsync (new ()
                    {
                        CancellationToken = cancel,
                        Username = appConfig.Gitlab.AppId,
                        Password = appConfig.Gitlab.AppSecret,

                    });*/

                    return "";
                    //authentication.BackchannelAccessToken ?? authentication.FrontchannelAccessToken;
                },
            };
        })
        .ConfigureHttpClient (c => c.BaseAddress = appConfig.Gitlab.Url);


    builder.Services
        .AddControllers ()
            .AddJsonOptions (o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
            });
    builder.Services
        .ConfigureHttpJsonOptions (o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
            o.SerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        });
    builder.Services
        .AddDbContext<DbContext> (options =>
        {
            options.UseInMemoryDatabase ("openiddict");
            options.UseOpenIddict ();
        });
    builder.Services
        .AddOpenIddict ()
        .AddCore (o =>
        {
            o.UseEntityFrameworkCore ()
                .UseDbContext<DbContext> ();
        })
        .AddClient (o =>
        {
            o//.AllowClientCredentialsFlow ()
            .AllowAuthorizationCodeFlow ()
            .AllowPasswordFlow ();
            //.DisableTokenStorage ();

            o.UseSystemIntegration ();
            o.UseSystemNetHttp ()
                .ConfigureHttpClientHandler (ch =>
                {
                    if (environment == "Development")
                    {
                        // In development, we allow self-signed certificates
                        // This is not recommended for production use
#pragma warning disable S4830
                        ch.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#pragma warning restore S4830
                    }
                });

            var issuer = appConfig.Gitlab.Url;

            o.AddDevelopmentEncryptionCertificate ()
                .AddDevelopmentSigningCertificate ();

            o.UseAspNetCore ()
                .EnableRedirectionEndpointPassthrough ();
            OpenIddictClientRegistration reg = new ()
            {
                Issuer = issuer,
                ClientId = appConfig.Gitlab.AppId,
                ClientSecret = appConfig.Gitlab.AppSecret,
                Configuration = new ()
                {
                    Issuer = issuer,
                    AuthorizationEndpoint = new (issuer, "/oauth/authorize"),
                    TokenEndpoint = new (issuer, "/oauth/token"),
                    RevocationEndpoint = new (issuer, "/oauth/revoke"),
                    IntrospectionEndpoint = new (issuer, "/oauth/introspect"),
                    UserInfoEndpoint = new (issuer, "/oauth/userinfo"),
                    JsonWebKeySetUri = new (issuer, "/oauth/discovery/keys"),
                },
                RedirectUri = new ("http://gl-proxy.localhost/oauth/callback/gitlab"),
                ConfigurationEndpoint = new (issuer, "/.well-known/openid-configuration"),
            };
            reg.Configuration.GrantTypesSupported.UnionWith ([GrantTypes.Password, GrantTypes.AuthorizationCode,
                 GrantTypes.ClientCredentials, GrantTypes.DeviceCode, GrantTypes.RefreshToken,
                GrantTypes.Implicit, GrantTypes.DeviceCode]);
            reg.Scopes.UnionWith (scopes);


            o.AddRegistration (reg);

        });

    builder.Host.UseSerilog ((context, services, configuration) => configuration
                         .ReadFrom.Configuration (context.Configuration)
                         .ReadFrom.Services (services)
                         .Enrich.FromLogContext ()
                         .Enrich.WithCorrelationId ()
                         .Enrich.WithSpan ());

    var app = builder.Build ();

    // Configure the HTTP request pipeline.

    //app.UseHttpsRedirection();

    //app.UseAuthorization();

    app.MapControllers ();

    app.MapReverseProxy ();

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