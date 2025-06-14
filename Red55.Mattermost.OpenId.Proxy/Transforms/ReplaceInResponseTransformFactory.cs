using Red55.Mattermost.OpenId.Proxy.Extensions;
using Red55.Mattermost.OpenId.Proxy.Transforms.Response;

using Yarp.ReverseProxy.Transforms.Builder;

namespace Red55.Mattermost.OpenId.Proxy.Transforms
{
    public class ReplaceInResponseTransformFactory(ILoggerFactory loggerFactory) : ITransformFactory
    {
        internal readonly string ReplaceInResponseTransformKey = "ReplaceInResponse";
        private ILoggerFactory LoggerFactory { get; } = EnsureArg.IsNotNull (loggerFactory, nameof (loggerFactory));

        private static (string what, string with) ExtractReplaceValues(IReadOnlyDictionary<string, string> transformValues)
        {
            if (transformValues.TryGetValue (nameof (ReplaceInResponseTransform.Match), out var match)
                && transformValues.TryGetValue (nameof (ReplaceInResponseTransform.Replacement), out var replacement))
            {
                return (match, replacement);
            }

            return (string.Empty, string.Empty);
        }

        public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            if (!transformValues.TryGetValue (ReplaceInResponseTransformKey, out _))
            {
                return false;
            }

            TransformHelpers.TryCheckTooManyParameters (context, transformValues, 3);

            var (match, replacement) = ExtractReplaceValues (transformValues);
            if (string.IsNullOrEmpty (match) || string.IsNullOrEmpty (replacement))
            {
                context.Errors.Add (new ArgumentException ("Both 'Match' and 'Replacement' parameters are required for ReplaceInResponse transform"));
            }

            if (!Regexp.IsValid (match))
            {
                context.Errors.Add (new ArgumentException ($"Invalid regular expression: {match}"));
            }

            return true;

        }

        public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
        {
            var (match, replacement) = ExtractReplaceValues (transformValues);

            if (!transformValues.TryGetValue (ReplaceInResponseTransformKey, out _))
            {
                return false;
            }

            context.ResponseTransforms.Add (new ReplaceInResponseTransform (match, replacement,
            LoggerFactory.CreateLogger<ReplaceInResponseTransform> ()));

            return true;
        }
    }
}
