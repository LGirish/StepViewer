using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StepAPIService
{
    internal class PrioritySemaphore<TPriority>
    {
        private static readonly SemaphoreSlim Lock = new (1, 1);

        private readonly SortedList<(TPriority, long), TaskCompletionSource<int>> list;
        private readonly int maxCount;
        private int currentCount;
        private long indexSeed;

        public PrioritySemaphore(int initialCount, int maxCount, IComparer<TPriority>? comparer = default)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }

            if (maxCount <= 0 || maxCount < initialCount)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            }

            comparer ??= Comparer<TPriority>.Default;
            list = new SortedList<(TPriority, long), TaskCompletionSource<int>>(Comparer<(TPriority, long)>.Create((x, y) =>
                {
                    int result = comparer.Compare(x.Item1, y.Item1);
                    if (result == 0)
                    {
                        result = x.Item2.CompareTo(y.Item2);
                    }

                    return result;
                }));

            currentCount = initialCount;
            this.maxCount = maxCount;
        }

        public PrioritySemaphore(int initialCount, IComparer<TPriority>? comparer = default)
            : this(initialCount, int.MaxValue, comparer)
        {
        }

        public PrioritySemaphore(IComparer<TPriority>? comparer = default)
            : this(0, int.MaxValue, comparer)
        {
        }

        public async Task WaitAsync(TPriority priority)
        {
            TaskCompletionSource<int> tcs;

            await Lock.WaitAsync().ConfigureAwait(false);
            try
            {
                Debug.Assert((list.Count == 0) || (currentCount == 0), "Empty list");
                if (currentCount > 0)
                {
                    currentCount--;
                    return;
                }

                tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                list.Add((priority, ++indexSeed), tcs);
            }
            finally
            {
                Lock.Release();
            }

            await tcs.Task;
        }

        public async Task ReleaseAsync()
        {
            TaskCompletionSource<int> tcs;
            await Lock.WaitAsync().ConfigureAwait(false);
            try
            {
                Debug.Assert((list.Count == 0) || (currentCount == 0), "Empty list");
                if (list.Count == 0)
                {
                    if (currentCount >= maxCount)
                    {
                        throw new SemaphoreFullException();
                    }

                    currentCount++;
                    return;
                }

                tcs = list.First().Value;
                list.RemoveAt(0);
            }
            finally
            {
                Lock.Release();
            }

            tcs.SetResult(0);
        }
    }
}
