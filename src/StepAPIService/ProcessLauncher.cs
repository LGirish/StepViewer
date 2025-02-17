using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StepAPIService
{
    internal class ProcessLauncher : IDisposable
    {
        private readonly Process process = new ();
        private readonly TaskCompletionSource<bool> tcs = new (TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly string processName;
        private readonly ProgramOptions options;

        public ProcessLauncher(string processName, ProgramOptions options)
        {
            this.processName = processName;
            this.options = options;
        }

        public async Task<int> RunAsync(CancellationToken cancellationToken)
        {
            process.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, processName);
            process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            process.StartInfo.Arguments = TessModel.ModelSerializer.ConvertToBase64(options);
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.EnableRaisingEvents = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Exited += new EventHandler((_, _) => tcs.TrySetResult(true));
            try
            {
                var startResult = process.Start();
                if (startResult)
                {
                    using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                    {
                        await tcs.Task.ConfigureAwait(false);
                    }

                    return process.HasExited ? process.ExitCode : -1;
                }

            }
            catch (Exception ex)
            {
                // ReSharper disable once InvocationIsSkipped
                Debug.WriteLine(ex);
            }

            return -1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                process.Dispose();
            }
        }
    }
}
