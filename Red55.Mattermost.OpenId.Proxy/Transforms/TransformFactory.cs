

// Ignore Spelling: Mattermost

using Microsoft.Extensions.Logging;

using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Red55.Mattermost.OpenId.Proxy.Transforms
{
    using Red55.Mattermost.OpenId.Proxy.Transforms.Response;

    public class TransformFactory : ITransformFactory
    {

        internal readonly string DisableSecureCookiesKeys = "DisableSecureCookies";
        internal readonly string InvalidDisableSecureCookiesValue =
                            "Unexpected value for {0}: {1}. Expected 'true' or 'false'";

        ILoggerFactory LoggerFactory { get; }
        public TransformFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
        }
        public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.TryGetValue (DisableSecureCookiesKeys, out var disableSecureCookiesValue))
            {

                if (!bool.TryParse (disableSecureCookiesValue, out var disableSecureCookies))
                {
                    throw new NotSupportedException (string.Format(InvalidDisableSecureCookiesValue, 
                        DisableSecureCookiesKeys, disableSecureCookiesValue));
                }
                if (disableSecureCookies)
                {
                    context.ResponseTransforms
                    .Add (new DisableSecureCookies (LoggerFactory.CreateLogger<DisableSecureCookies> ()));

                    return true;
                }
                
            }

            return false;
        }

        public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            
            if (transformValues.TryGetValue(DisableSecureCookiesKeys, out var disableSecureCookies))
            {
                TransformHelpers.TryCheckTooManyParameters (context, transformValues, 1);

                if (!bool.TryParse(disableSecureCookies, out var _))
                {
                    context.Errors.Add (
                        new ArgumentException (
                            $"Unexpected value for {DisableSecureCookiesKeys}: {disableSecureCookies}. Expected 'true' or 'false'"
                            )
                    );
                }
            }

            return context.Errors.Count == 0;
        }
    }
}
