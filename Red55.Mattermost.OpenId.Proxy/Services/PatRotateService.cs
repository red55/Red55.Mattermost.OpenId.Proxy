using Microsoft.Extensions.Options;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;
using Red55.Mattermost.OpenId.Proxy.Models;
using Red55.Mattermost.OpenId.Proxy.Storage;

namespace Red55.Mattermost.OpenId.Proxy.Services
{
    public class PatRotateService(
        IPersonalAccessTokens patApi,
        IPatStore patStore,
        IOptions<AppConfig> config,
        IHostApplicationLifetime applicationLifetime,
        ILogger<PatRotateService> logger) : IHostedService
    {
        AppConfig Config { get; } = EnsureArg.IsNotNull (EnsureArg.IsNotNull (config, nameof (config)).Value,
            nameof (config.Value));
        ILogger Log { get; } = EnsureArg.IsNotNull (logger, nameof (logger));
        IPersonalAccessTokens PatApi { get; } = EnsureArg.IsNotNull (patApi, nameof (patApi));
        IPatStore PatStore { get; } = EnsureArg.IsNotNull (patStore, nameof (patStore));
        IHostApplicationLifetime ApplicationLifetime { get; } =
            EnsureArg.IsNotNull (applicationLifetime, nameof (applicationLifetime));
        Task? _myWork;
        private CancellationTokenSource? _cts;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.LogInformation ("Starting PAT rotation service");
            _cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
            _myWork = Task.Run (() => DoWork (_cts.Token), _cts.Token);
            return Task.CompletedTask;
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var apiResponse = await PatApi.GetSelfAsync (cancellationToken);
                    if (!apiResponse.IsSuccessStatusCode || apiResponse.Content is null)
                    {
                        Log.LogWarning ("Get Self Info for PAT failed {Status}. Will try again later.",
                            apiResponse.StatusCode);
                        await Task.Delay (10_000, cancellationToken);
                        continue;
                    }

                    var token = await PatStore.GetTokenAsync (cancellationToken);
                    await PatStore.UpdateAsync (apiResponse.Content with { Token = token }, cancellationToken);

                    Log.LogInformation ("PAT retrieved successfully: {Id}, {Name}, {ExpiresAt}",
                        apiResponse.Content.Id, apiResponse.Content.Name, apiResponse.Content.ExpiresAt);
                    var nextRotation = apiResponse.Content.ExpiresAt.ToDateTime (TimeOnly.MinValue)
                        - DateTimeOffset.UtcNow - Config.GitLab.PAT.GracePeriod;

                    Log.LogDebug ("Sleeping until next PAT rotation: {GracePeriod}, {NextRotation}",
                        Config.GitLab.PAT.GracePeriod, nextRotation);

                    await SafeDelay (nextRotation, cancellationToken);

                    apiResponse = await PatApi.RotateSelfAsync (cancellationToken);
                    if (!apiResponse.IsSuccessStatusCode || apiResponse.Content is null)
                    {
                        Log.LogError ("Failed to rotate PAT: {StatusCode} - {ReasonPhrase}",
                            apiResponse.StatusCode, apiResponse.ReasonPhrase);
                    }
                    else
                    {
                        await PatStore.UpdateAsync (apiResponse.Content, cancellationToken);
                        Log.LogInformation ("PAT rotated successfully: {Id}, {Name}, {ExpiresAt}",
                            apiResponse.Content.Id, apiResponse.Content.Name, apiResponse.Content.ExpiresAt);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Log.LogInformation ("PAT rotation service was cancelled");
            }
            catch (Exception e)
            {
                Log.LogError (e, "An error occurred in the PAT rotation service");
                ApplicationLifetime.StopApplication (); // Stop the application if an unhandled exception occurs
                throw;
            }

        }
        public static async Task SafeDelay(TimeSpan totalDelay, CancellationToken cancellationToken)
        {
            TimeSpan maxDelay = TimeSpan.FromMilliseconds (int.MaxValue);
            while (totalDelay > TimeSpan.Zero)
            {
                var delay = totalDelay > maxDelay ? maxDelay : totalDelay;
                await Task.Delay (delay, cancellationToken);
                totalDelay -= delay;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.LogInformation ("Stopping PAT rotation service");
            _cts?.Cancel ();
            return _myWork ?? Task.CompletedTask;
        }
    }
}
