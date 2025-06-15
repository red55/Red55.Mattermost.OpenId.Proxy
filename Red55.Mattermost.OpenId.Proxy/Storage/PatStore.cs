using Microsoft.Extensions.Options;

using Red55.Mattermost.OpenId.Proxy.Models;
using Red55.Mattermost.OpenId.Proxy.Models.Gitlab;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Red55.Mattermost.OpenId.Proxy.Storage
{
    public interface IPatStore
    {
        ValueTask<string> GetTokenAsync(CancellationToken cancellationToken);
        ValueTask UpdateAsync(PersonalAccessToken pat, CancellationToken cancellationToken);
    }
    public class PatStore(ILogger<PatStore> logger, IOptions<AppConfig> config) : IPatStore
    {
        const string PAT_FILE_NAME = "pat.yml";
        ILogger Log { get; } = EnsureArg.IsNotNull (logger, nameof (logger));
        AppConfig Config { get; } = EnsureArg.IsNotNull (EnsureArg.IsNotNull (config, nameof (config)).Value,
            nameof (config.Value));

        readonly ReaderWriterLockSlim _lock = new ();
        PersonalAccessToken? _pat;

        string StoreFileName
        {
            get
            {
                return Path.Join (Config.GitLab.PAT.StoreLocation, PAT_FILE_NAME);
            }
        }

        public ValueTask<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            _lock.EnterReadLock ();
            try
            {
                cancellationToken.ThrowIfCancellationRequested ();
                if (_pat is null && File.Exists (StoreFileName))
                {
                    var deserializer = new DeserializerBuilder ()
                        .WithNamingConvention (CamelCaseNamingConvention.Instance)
                        .WithTypeConverter (new DateTimeOffsetConverter ())
                        .WithTypeConverter (new DateOnlyConverter ())
                        .Build ();

                    using var reader = new StreamReader (StoreFileName);
                    _pat = deserializer.Deserialize<PersonalAccessToken> (reader);
                }
                return ValueTask.FromResult (_pat is null ? Config.GitLab.PAT.BootstrapToken : _pat.Token);
            }
            finally
            {
                _lock.ExitReadLock ();
            }
        }

        public async ValueTask UpdateAsync(PersonalAccessToken pat, CancellationToken cancellationToken)
        {
            _lock.EnterWriteLock ();
            try
            {
                _pat = EnsureArg.IsNotNull (pat, nameof (pat));

                cancellationToken.ThrowIfCancellationRequested ();

                Directory.CreateDirectory (Config.GitLab.PAT.StoreLocation);

                using var f = File.OpenWrite (StoreFileName);
                using var writer = new StreamWriter (f, System.Text.Encoding.UTF8);

                var serializer = new SerializerBuilder ()
                    .WithNamingConvention (CamelCaseNamingConvention.Instance)
                    .WithTypeConverter (new DateTimeOffsetConverter ())
                    .WithTypeConverter (new DateOnlyConverter ())
                    .Build ();

                cancellationToken.ThrowIfCancellationRequested ();
                await writer.WriteAsync (serializer.Serialize (pat));
            }
            catch (Exception e)
            {
                Log.LogError (e, "Error while saving PAT");
                throw;
            }
            finally
            {
                _lock.ExitWriteLock ();
            }
        }
    }
}
