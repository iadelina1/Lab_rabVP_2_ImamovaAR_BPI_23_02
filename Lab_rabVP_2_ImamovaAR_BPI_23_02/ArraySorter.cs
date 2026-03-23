using System;
using System.Diagnostics;
using System.Threading;

namespace Lab_rabVP_2_ImamovaAR_BPI_23_02
{
    public class ArraySorter
    {
        private long _totalComparisons;
        private readonly object _locker = new();

        public delegate void SortCompletedHandler(int[] sortedArray, long comparisons, double elapsedMilliseconds);
        public event SortCompletedHandler? BubbleSortCompleted;
        public event SortCompletedHandler? QuickSortCompleted;
        public event SortCompletedHandler? InsertionSortCompleted;
        public event SortCompletedHandler? ShakerSortCompleted;

        public event Action<string, double>? ProgressChanged;

        public long TotalComparisons => _totalComparisons;

        public int[] GenerateRandomArray(int size)
        {
            Random rand = new();
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = rand.Next(1000);
            return array;
        }

        private int[] CopyArray(int[] source)
        {
            int[] copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        public void BubbleSort(int[] originalArray, CancellationToken ct)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < array.Length - 1; i++)
            {
                if (ct.IsCancellationRequested) return;

                for (int j = 0; j < array.Length - 1 - i; j++)
                {
                    comparisons++;
                    if (array[j] > array[j + 1])
                    {
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    }
                }

                double progress = ((double)i / (array.Length - 1)) * 100;
                ProgressChanged?.Invoke("Bubble", progress);
            }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
            ProgressChanged?.Invoke("Bubble", 100);
        }

        public void QuickSort(int[] originalArray, CancellationToken ct)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                QuickSortRecursive(array, 0, array.Length - 1, ref comparisons, ct);
            }
            catch (OperationCanceledException) { return; }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            QuickSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
            ProgressChanged?.Invoke("Quick", 100);
        }

        private void QuickSortRecursive(int[] arr, int left, int right, ref long comparisons, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) throw new OperationCanceledException();

            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right, ref comparisons);
                QuickSortRecursive(arr, left, pivotIndex - 1, ref comparisons, ct);
                QuickSortRecursive(arr, pivotIndex + 1, right, ref comparisons, ct);

                ProgressChanged?.Invoke("Quick", 50);
            }
        }

        private int Partition(int[] arr, int left, int right, ref long comparisons)
        {
            int pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                comparisons++;
                if (arr[j] < pivot)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]);
            return i + 1;
        }

        public void InsertionSort(int[] originalArray, CancellationToken ct)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 1; i < array.Length; i++)
            {
                if (ct.IsCancellationRequested) return;

                int key = array[i];
                int j = i - 1;
                while (j >= 0 && array[j] > key)
                {
                    comparisons++;
                    array[j + 1] = array[j];
                    j--;
                }
                comparisons++;
                array[j + 1] = key;

                double progress = ((double)i / array.Length) * 100;
                ProgressChanged?.Invoke("Insertion", progress);
            }

            watch.Stop();
            lock (_locker) { _totalComparisons += comparisons; }
            InsertionSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
            ProgressChanged?.Invoke("Insertion", 100);
        }

        public void ShakerSort(int[] originalArray, CancellationToken ct)
        {
            int[] array = (int[])originalArray.Clone();
            long comps = 0;
            var sw = Stopwatch.StartNew();

            int left = 0, right = array.Length - 1;
            while (left <= right)
            {
                if (ct.IsCancellationRequested) return;

                for (int i = left; i < right; i++)
                {
                    comps++;
                    if (array[i] > array[i + 1]) (array[i], array[i + 1]) = (array[i + 1], array[i]);
                }
                right--;

                for (int i = right; i > left; i--)
                {
                    comps++;
                    if (array[i - 1] > array[i]) (array[i - 1], array[i]) = (array[i], array[i - 1]);
                }
                left++;

                double progress = ((double)(array.Length - (right - left)) / array.Length) * 100;
                ProgressChanged?.Invoke("Shaker", Math.Min(progress, 99));
            }

            sw.Stop();
            lock (_locker) { _totalComparisons += comps; }
            ShakerSortCompleted?.Invoke(array, comps, sw.Elapsed.TotalMilliseconds);
            ProgressChanged?.Invoke("Shaker", 100);
        }

        public void ResetTotal()
        { 
            lock (_locker) 
            
            { _totalComparisons = 0; } 
        }
    }
}