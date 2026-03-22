using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;

namespace Lab_rabVP_2_ImamovaAR_BPI_23_02
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private readonly SynchronizationContext _uiContext;
        private int[]? _originalArray;

        [ObservableProperty] private int _arraySize = 1000;
        [ObservableProperty] private string _originalArrayString = "Массив не сгенерирован";
        [ObservableProperty] private string _bubbleSortResult = "Ожидание...";
        [ObservableProperty] private string _quickSortResult = "Ожидание...";
        [ObservableProperty] private string _insertionSortResult = "Ожидание...";
        [ObservableProperty] private string _totalComparisons = "Общее число сравнений: 0";

        public MainViewModel()
        {
            _sorter = new ArraySorter();

            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

            _sorter.BubbleSortCompleted += OnBubbleSortCompleted;
            _sorter.QuickSortCompleted += OnQuickSortCompleted;
            _sorter.InsertionSortCompleted += OnInsertionSortCompleted;
        }

        [RelayCommand]
        private void GenerateArray()
        {
            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            OriginalArrayString = $"Сгенерирован массив на {ArraySize} эл. (первые: {string.Join(", ", _originalArray[..Math.Min(10, ArraySize)])}...)";

            BubbleSortResult = QuickSortResult = InsertionSortResult = "Готов к работе";

            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
        }

        private bool CanSort() => _originalArray != null;

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void BubbleSort()
        {
            BubbleSortResult = "Сортируется...";
            BubbleSortCommand.NotifyCanExecuteChanged();

            Thread thread = new(() => _sorter.BubbleSort(_originalArray!)) { IsBackground = true };
            thread.Start();
        }

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void QuickSort()
        {
            QuickSortResult = "Сортируется...";
            QuickSortCommand.NotifyCanExecuteChanged();

            Thread thread = new(() => _sorter.QuickSort(_originalArray!)) { IsBackground = true };
            thread.Start();
        }

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void InsertionSort()
        {
            InsertionSortResult = "Сортируется...";
            InsertionSortCommand.NotifyCanExecuteChanged();

            Thread thread = new(() => _sorter.InsertionSort(_originalArray!)) { IsBackground = true };
            thread.Start();
        }

        private void OnBubbleSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ => {
                BubbleSortResult = $"Результат: {elapsedMs:F2} мс, {comparisons} сравнений";
                UpdateTotal();
                BubbleSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnQuickSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ => {
                QuickSortResult = $"Результат: {elapsedMs:F2} мс, {comparisons} сравнений";
                UpdateTotal();
                QuickSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnInsertionSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ => {
                InsertionSortResult = $"Результат: {elapsedMs:F2} мс, {comparisons} сравнений";
                UpdateTotal();
                InsertionSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void UpdateTotal()
        {
            TotalComparisons = $"Общее число сравнений (всех потоков): {_sorter.TotalComparisons}";
        }
    }
}