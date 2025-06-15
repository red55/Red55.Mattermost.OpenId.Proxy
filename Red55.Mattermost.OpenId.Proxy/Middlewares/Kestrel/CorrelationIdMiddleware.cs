namespace Red55.Mattermost.OpenId.Proxy.Middlewares.Kestrel
{
    public class CorrelationIdMiddleware
    {
        private const string _headerName = "X-Correlation-ID";
        public static string HeaderName { get; private set; } = _headerName;
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = EnsureArg.IsNotNull (next, nameof (next));
        }

        public CorrelationIdMiddleware(RequestDelegate next, string headerName)
        {
            _next = EnsureArg.IsNotNull (next, nameof (next));
            HeaderName = EnsureArg.IsNotNullOrEmpty (headerName);
        }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue (HeaderName, out var correlationId))
            {
                if (string.IsNullOrEmpty (context.TraceIdentifier))
                {
                    correlationId = context.TraceIdentifier = Guid.CreateVersion7 ().ToString ();
                }
                else
                {
                    correlationId = context.TraceIdentifier;
                }

            }

            context.Request.Headers[HeaderName] = correlationId;
            context.Response.Headers.Append (HeaderName, correlationId);

            return _next (context);
        }
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
    }
}
