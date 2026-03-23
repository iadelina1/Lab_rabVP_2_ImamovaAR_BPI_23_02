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
        private CancellationTokenSource _cts = new();

        [ObservableProperty] private int _arraySize = 1000;
        [ObservableProperty] private string _originalArrayString = "Массив не сгенерирован";
        [ObservableProperty] private string _bubbleSortResult = "Ожидание...";
        [ObservableProperty] private string _quickSortResult = "Ожидание...";
        [ObservableProperty] private string _insertionSortResult = "Ожидание...";
        [ObservableProperty] private string _shakerSortResult = "Ожидание...";

        [ObservableProperty] private string _totalComparisons = "Общее число сравнений: 0";

        [ObservableProperty] private double _bubbleProgress;
        [ObservableProperty] private double _quickProgress;
        [ObservableProperty] private double _insertionProgress;
        [ObservableProperty] private double _shakerProgress;

        public MainViewModel()
        {
            _sorter = new ArraySorter();
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

            _sorter.BubbleSortCompleted += OnBubbleSortCompleted;
            _sorter.QuickSortCompleted += OnQuickSortCompleted;
            _sorter.InsertionSortCompleted += OnInsertionSortCompleted;
            _sorter.ShakerSortCompleted += OnShakerSortCompleted;
            _sorter.ProgressChanged += OnProgressChanged;
        }

        [RelayCommand]
        private void GenerateArray()
        {
            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            OriginalArrayString = $"Сгенерирован массив на {ArraySize} эл.";

            BubbleSortResult = QuickSortResult = InsertionSortResult = ShakerSortResult = "Готов к работе";
            BubbleProgress = QuickProgress = InsertionProgress = ShakerProgress = 0;

            NotifyCommands();
        }

        [RelayCommand]
        private void CancelAll()
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            if (BubbleSortResult == "Сортируется...") BubbleSortResult = "Отменено";
            if (QuickSortResult == "Сортируется...") QuickSortResult = "Отменено";
            if (InsertionSortResult == "Сортируется...") InsertionSortResult = "Отменено";
            if (ShakerSortResult == "Сортируется...") ShakerSortResult = "Отменено";

            NotifyCommands();
        }

        private bool CanSort() => _originalArray != null;

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void BubbleSort()
        {
            BubbleSortResult = "Сортируется...";
            BubbleProgress = 0;
            BubbleSortCommand.NotifyCanExecuteChanged();
            new Thread(() => _sorter.BubbleSort(_originalArray!, _cts.Token)) { IsBackground = true }.Start();
        }

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void QuickSort()
        {
            QuickSortResult = "Сортируется...";
            QuickProgress = 0;
            QuickSortCommand.NotifyCanExecuteChanged();
            new Thread(() => _sorter.QuickSort(_originalArray!, _cts.Token)) { IsBackground = true }.Start();
        }

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void InsertionSort()
        {
            InsertionSortResult = "Сортируется...";
            InsertionProgress = 0;
            InsertionSortCommand.NotifyCanExecuteChanged();
            new Thread(() => _sorter.InsertionSort(_originalArray!, _cts.Token)) { IsBackground = true }.Start();
        }

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void ShakerSort()
        {
            ShakerSortResult = "Сортируется...";
            ShakerProgress = 0;
            ShakerSortCommand.NotifyCanExecuteChanged();
            new Thread(() => _sorter.ShakerSort(_originalArray!, _cts.Token)) { IsBackground = true }.Start();
        }

        [RelayCommand]
        private void ResetAll()
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _originalArray = null;
            _sorter.ResetTotal();

            ArraySize = 1000;
            OriginalArrayString = "Массив не сгенерирован";
            BubbleSortResult = QuickSortResult = InsertionSortResult = ShakerSortResult = "Ожидание...";

            BubbleProgress = QuickProgress = InsertionProgress = ShakerProgress = 0;

            UpdateTotal();

            NotifyCommands();
        }

        private void OnProgressChanged(string algorithm, double value)
        {
            _uiContext.Post(_ =>
            {
                switch (algorithm)
                {
                    case "Bubble": BubbleProgress = value; break;
                    case "Quick": QuickProgress = value; break;
                    case "Insertion": InsertionProgress = value; break;
                    case "Shaker": ShakerProgress = value; break;
                }
            }, null);
        }

        private void OnBubbleSortCompleted(int[] arr, long comps, double ms) =>
            _uiContext.Post(_ => { BubbleSortResult = $"Рез: {ms:F2} мс"; UpdateTotal(); BubbleSortCommand.NotifyCanExecuteChanged(); }, null);

        private void OnQuickSortCompleted(int[] arr, long comps, double ms) =>
            _uiContext.Post(_ => { QuickSortResult = $"Рез: {ms:F2} мс"; UpdateTotal(); QuickSortCommand.NotifyCanExecuteChanged(); }, null);

        private void OnInsertionSortCompleted(int[] arr, long comps, double ms) =>
            _uiContext.Post(_ => { InsertionSortResult = $"Рез: {ms:F2} мс"; UpdateTotal(); InsertionSortCommand.NotifyCanExecuteChanged(); }, null);

        private void OnShakerSortCompleted(int[] arr, long comps, double ms) =>
            _uiContext.Post(_ => { ShakerSortResult = $"Рез: {ms:F2} мс"; UpdateTotal(); ShakerSortCommand.NotifyCanExecuteChanged(); }, null);

        private void UpdateTotal() =>
            TotalComparisons = $"Общее число сравнений: {_sorter.TotalComparisons}";

        private void NotifyCommands()
        {
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            ShakerSortCommand.NotifyCanExecuteChanged();
        }
    }
}