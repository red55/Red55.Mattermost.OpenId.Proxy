using Red55.Mattermost.OpenId.Proxy.Storage;

namespace Red55.Mattermost.OpenId.Proxy.Middlewares.HttpClient
{
    public class HttpRequestOptionsHandler : DelegatingHandler
    {
        public HttpRequestOptionsHandler(IPatStore patStore, ILogger<HttpRequestOptionsHandler> logger)
        {
            PatStore = EnsureArg.IsNotNull (patStore, nameof (patStore));
            Log = EnsureArg.IsNotNull (logger, nameof (logger));

        }


        public HttpRequestOptionsHandler(IPatStore patStore, ILogger<HttpRequestOptionsHandler> logger,
            HttpMessageHandler innerHandler) : base (innerHandler)
        {
            PatStore = EnsureArg.IsNotNull (patStore, nameof (patStore));
            Log = EnsureArg.IsNotNull (logger, nameof (logger));

        }

        IPatStore PatStore { get; }
        ILogger<HttpRequestOptionsHandler> Log { get; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Set an option (example key/value)
            request.Options.Set (new HttpRequestOptionsKey<IPatStore> (nameof (IPatStore)), PatStore);
            return base.SendAsync (request, cancellationToken);
        }
    }
}