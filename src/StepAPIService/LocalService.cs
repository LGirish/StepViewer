using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TessModel;

namespace StepAPIService
{
    public sealed class LocalService
    {
        private static readonly Lazy<LocalService> Instance = new (() => new LocalService());

        private IHost? host;

        private LocalService()
        {
        }

        public static bool IsStarted => !(!Instance.IsValueCreated || Instance.Value.host is null);

        public static ILogger? Logger
            => IsStarted
                ? Instance.Value.host?.Services.GetRequiredService<ILogger>() : null;

        public static IConfiguration? Configuration
            => IsStarted
                ? Instance.Value.host?.Services.GetRequiredService<IConfiguration>() : null;

        public static IStepTessellator? Tessellator
            => IsStarted
                ? Instance.Value.host?.Services.GetRequiredService<IStepTessellator>() : null;

        public static void Start()
        {
            if (IsStarted)
            {
                return;
            }

            Instance.Value.host ??= Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(configBuilder =>
                {
                }).ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSimpleConsole(options => options.SingleLine = true);
                    loggingBuilder.AddDebug();
                })
                .ConfigureServices((services) =>
                {
                    // Services to connect Tessellator to the application.
                    services.AddStepTessellator();
                }).Build();

            Instance.Value.host.StartAsync();
        }

        public static void Stop()
    {
        if (!IsStarted)
        {
            return;
        }

        Instance.Value.host?.StopAsync();
        Instance.Value.host?.WaitForShutdownAsync();
    }
}
}
