using System;
using System.Diagnostics;
using System.Threading;

namespace Lab_rabVP_2_ImamovaAR_BPI_23_02
{
    public class ArraySorter
    {
        private long _totalComparisons;
        private readonly object _locker = new();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(2);

        public delegate void SortCompletedHandler(int[] sortedArray, long comparisons, double elapsedMilliseconds);
        public event SortCompletedHandler? BubbleSortCompleted;
        public event SortCompletedHandler? QuickSortCompleted;
        public event SortCompletedHandler? InsertionSortCompleted;
        public event SortCompletedHandler? ShakerSortCompleted;
        public event Action<string, double>? ProgressChanged;

        public long TotalComparisons => _totalComparisons;

        public void UpdateThreadLimit(int maxThreads)
        {
            _semaphore = new SemaphoreSlim(maxThreads > 0 ? maxThreads : 1);
        }

        public int[] GenerateRandomArray(int size)
        {
            Random rand = new();
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = rand.Next(1000);
            return array;
        }

        public void BubbleSort(int[] originalArray, CancellationToken ct, bool useShared)
        {
            _semaphore.Wait();
            try
            {
                int[] array = useShared ? originalArray : (int[])originalArray.Clone();
                long comparisons = 0;
                Stopwatch watch = Stopwatch.StartNew();

                for (int i = 0; i < array.Length - 1; i++)
                {
                    if (ct.IsCancellationRequested) return;
                    for (int j = 0; j < array.Length - 1 - i; j++)
                    {
                        if (useShared)
                        {
                            lock (_locker)
                            {
                                comparisons++;
                                if (array[j] > array[j + 1]) (array[j], array[j + 1]) = (array[j + 1], array[j]);
                            }
                        }
                        else
                        {
                            comparisons++;
                            if (array[j] > array[j + 1]) (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        }
                    }
                    ProgressChanged?.Invoke("Bubble", ((double)i / (array.Length - 1)) * 100);
                }
                watch.Stop();
                lock (_locker) { _totalComparisons += comparisons; }
                BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
                ProgressChanged?.Invoke("Bubble", 100);
            }
            finally { _semaphore.Release(); }
        }

        public void QuickSort(int[] originalArray, CancellationToken ct, bool useShared)
        {
            _semaphore.Wait();
            try
            {
                int[] array = useShared ? originalArray : (int[])originalArray.Clone();
                long comparisons = 0;
                Stopwatch watch = Stopwatch.StartNew();
                QuickSortRecursive(array, 0, array.Length - 1, ref comparisons, ct, useShared);
                watch.Stop();
                lock (_locker) { _totalComparisons += comparisons; }
                QuickSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
                ProgressChanged?.Invoke("Quick", 100);
            }
            finally { _semaphore.Release(); }
        }

        private void QuickSortRecursive(int[] arr, int left, int right, ref long comparisons, CancellationToken ct, bool useShared)
        {
            if (ct.IsCancellationRequested || left >= right) return;
            int pivotIndex = Partition(arr, left, right, ref comparisons, useShared);
            QuickSortRecursive(arr, left, pivotIndex - 1, ref comparisons, ct, useShared);
            QuickSortRecursive(arr, pivotIndex + 1, right, ref comparisons, ct, useShared);
            ProgressChanged?.Invoke("Quick", 50);
        }

        private int Partition(int[] arr, int left, int right, ref long comps, bool useShared)
        {
            int pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                if (useShared) lock (_locker)
                    {
                        comps++; if (arr[j] < pivot) { i++; (arr[i], arr[j]) = (arr[j], arr[i]); }
                    }
                else { comps++; if (arr[j] < pivot) { i++; (arr[i], arr[j]) = (arr[j], arr[i]); } }
            }
            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]);
            return i + 1;
        }

        public void InsertionSort(int[] originalArray, CancellationToken ct, bool useShared)
        {
            _semaphore.Wait();
            try
            {
                int[] array = useShared ? originalArray : (int[])originalArray.Clone();
                long comps = 0;
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 1; i < array.Length; i++)
                {
                    if (ct.IsCancellationRequested) return;
                    int key = array[i]; int j = i - 1;
                    while (j >= 0)
                    {
                        if (useShared) lock (_locker)
                            {
                                comps++; if (array[j] > key) { array[j + 1] = array[j]; j--; } else break;
                            }
                        else { comps++; if (array[j] > key) { array[j + 1] = array[j]; j--; } else break; }
                    }
                    array[j + 1] = key;
                    ProgressChanged?.Invoke("Insertion", ((double)i / array.Length) * 100);
                }
                sw.Stop();
                lock (_locker) { _totalComparisons += comps; }
                InsertionSortCompleted?.Invoke(array, comps, sw.Elapsed.TotalMilliseconds);
                ProgressChanged?.Invoke("Insertion", 100);
            }
            finally { _semaphore.Release(); }
        }

        public void ShakerSort(int[] originalArray, CancellationToken ct, bool useShared)
        {
            _semaphore.Wait();
            try
            {
                int[] array = useShared ? originalArray : (int[])originalArray.Clone();
                long comps = 0; Stopwatch sw = Stopwatch.StartNew();
                int left = 0, right = array.Length - 1;
                while (left <= right)
                {
                    if (ct.IsCancellationRequested) return;
                    for (int i = left; i < right; i++)
                    {
                        if (useShared) lock (_locker) { comps++; if (array[i] > array[i + 1]) (array[i], array[i + 1]) = (array[i + 1], array[i]); }
                        else { comps++; if (array[i] > array[i + 1]) (array[i], array[i + 1]) = (array[i + 1], array[i]); }
                    }
                    right--;
                    for (int i = right; i > left; i--)
                    {
                        if (useShared) lock (_locker) { comps++; if (array[i - 1] > array[i]) (array[i - 1], array[i]) = (array[i], array[i - 1]); }
                        else { comps++; if (array[i - 1] > array[i]) (array[i - 1], array[i]) = (array[i], array[i - 1]); }
                    }
                    left++;
                    ProgressChanged?.Invoke("Shaker", ((double)(array.Length - (right - left)) / array.Length) * 100);
                }
                sw.Stop();
                lock (_locker) { _totalComparisons += comps; }
                ShakerSortCompleted?.Invoke(array, comps, sw.Elapsed.TotalMilliseconds);
                ProgressChanged?.Invoke("Shaker", 100);
            }
            finally { _semaphore.Release(); }
        }

        public void ResetTotal() { lock (_locker) { _totalComparisons = 0; } }
    }
}