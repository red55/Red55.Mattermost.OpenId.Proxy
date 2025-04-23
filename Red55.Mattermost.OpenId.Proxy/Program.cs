using Serilog;
using Serilog.Enrichers.Span;

var builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
    .Enrich.FromLogContext ()
    .Enrich.WithCorrelationId ()
    .WriteTo.Console (outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger ();

builder.Services
    .AddTransient<Red55.Mattermost.OpenId.Proxy.Transforms.Request> ()
    .AddReverseProxy ()
    .LoadFromConfig (builder.Configuration.GetRequiredSection ("ReverseProxy"))
    .AddTransforms (builder =>
    {
        builder.RequestTransforms.Add (builder.Services.GetRequiredService<Red55.Mattermost.OpenId.Proxy.Transforms.Request> ());
    });
//.AddControllers();
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

//app.MapControllers();

app.MapReverseProxy ();

app.Run ();
