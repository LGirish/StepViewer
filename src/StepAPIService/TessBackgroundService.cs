using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

namespace StepAPIService
{
    internal sealed class TessBackgroundService : BackgroundService
    {
        private readonly TessProcessDispatcher exchangeProcessDispatcher;

        public TessBackgroundService(TessProcessDispatcher exchangeProcessDispatcher)
        {
            this.exchangeProcessDispatcher = exchangeProcessDispatcher;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
            Task.Run(
                () =>
                {
                    try
                    {
                        AsyncContext.Run(async () => await exchangeProcessDispatcher.StartAsync(cancellationToken).ConfigureAwait(true));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        throw;
                    }
                },
                cancellationToken);
    }
}
