using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MediatR;

namespace StepAPIService
{
    internal class TessProcessDispatcher
    {
        private const string DefaultProcessName = "StepProcessor.exe";
        private readonly PrioritySemaphore<long> semaphore;
        private readonly Channel<Job> queue;
        private readonly Dictionary<string, byte[]> responseDict = new();

        private int count;

        public Action<string, MemoryStream> AddToResponseDict
            => (key, stream) 
            => responseDict[key] = stream.ToArray();

        public TessProcessDispatcher()
        {
            var maxParallelProcessSetting = Environment.ProcessorCount;

            semaphore = new PrioritySemaphore<long>(maxParallelProcessSetting, maxParallelProcessSetting);

            var options = new BoundedChannelOptions(maxParallelProcessSetting)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            queue = Channel.CreateBounded<Job>(options);
        }

        public int PendingJobsCount => count;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Job currentJob = await queue.Reader.ReadAsync(cancellationToken);
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _ = ExecuteJobAsync(currentJob, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Log or handle the cancellation if needed
                // This is an expected exception during shutdown
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions
                Debug.WriteLine($"Unexpected error in StartAsync: {ex}");
            }
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request, long priority)
            where TRequest : IRequest<TResponse>
        {
            Interlocked.Increment(ref count);
            await semaphore.WaitAsync(priority).ConfigureAwait(false);
            TaskCompletionSource<TResponse> tcs = new();
            TResponse result;
            var currentJob = new Job(
                request,
                async output =>
                {
                    try
                    {
                        result = (TResponse)output;
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref count);
                        await semaphore.ReleaseAsync().ConfigureAwait(false);
                    }
                });
            await queue.Writer.WriteAsync(currentJob);

            return await tcs.Task.ConfigureAwait(false);
        }

        public byte[] GetProcessResponseForFileName(string fileName)
            => responseDict.ContainsKey(fileName) ? responseDict[fileName] : Array.Empty<byte>();

        private async Task ExecuteJobAsync(Job currentJob, CancellationToken cancellationToken)
        {
            int exitCode = -1;
            if (!cancellationToken.IsCancellationRequested)
            {
                ProgramOptions? options = ((ProcessRequest)currentJob.Input).Options;
                if (options != null)
                {
                    using var processLauncher = new ProcessLauncher(DefaultProcessName, options);

                    using var pipe = new Pipe(options, AddToResponseDict);
                    exitCode = await processLauncher.RunAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    exitCode = -1;
                }
            }

            Func<object, Task> callback = currentJob.Callback;
            await callback(exitCode);
        }

        private sealed class Pipe : IDisposable
        {
            public readonly ProgramOptions Options;
            public NamedPipeServerStream PipeServer { get; private set; }
            private readonly Action<string, MemoryStream> AddToResponseDictAction;

            public Pipe(ProgramOptions options,
                Action<string, MemoryStream> addToResponseDictAction)
            {
                Options = options;
                PipeServer = new NamedPipeServerStream(Options.NamedPipe, PipeDirection.In);
                AddToResponseDictAction = addToResponseDictAction;

                Connect();
            }

            private void Connect()
            {
                PipeServer.BeginWaitForConnection(new AsyncCallback(OnConnected), PipeServer);
            }

            private void OnConnected(IAsyncResult ar)
            {
                try
                {
                    var pipeStream = (NamedPipeServerStream)ar.AsyncState;

                    // End the asynchronous connection operation
                    pipeStream.EndWaitForConnection(ar);

                    // Read data from the client
                    using (var ms = new MemoryStream())
                    {
                        pipeStream.CopyTo(ms);
                        AddToResponseDictAction(Options.InputFile, ms);
                    }

                    // Close the pipe
                    pipeStream.Close();
                }
                catch (Exception ex)
                {
                }
            }

            public void Dispose()
            {
                PipeServer?.Dispose();
            }
        }

        private sealed class Job
        {
            public Job(object input, Func<object, Task> callback)
            {
                Input = input;
                Callback = callback;
            }

            public object Input { get; }

            public Func<object, Task> Callback { get; }
        }
    }
}
