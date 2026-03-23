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
        [ObservableProperty] private int _maxThreads = 2;
        [ObservableProperty] private bool _useSharedArray = false;

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
            _sorter.BubbleSortCompleted += (arr, c, ms) => UpdateUI("Bubble", ms);
            _sorter.QuickSortCompleted += (arr, c, ms) => UpdateUI("Quick", ms);
            _sorter.InsertionSortCompleted += (arr, c, ms) => UpdateUI("Insertion", ms);
            _sorter.ShakerSortCompleted += (arr, c, ms) => UpdateUI("Shaker", ms);
            _sorter.ProgressChanged += OnProgressChanged;
        }

        [RelayCommand]
        private void GenerateArray()
        {
            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            OriginalArrayString = $"Сгенерирован массив ({ArraySize} эл.)";
            BubbleSortResult = QuickSortResult = InsertionSortResult = ShakerSortResult = "Готов";
            BubbleProgress = QuickProgress = InsertionProgress = ShakerProgress = 0;
            NotifyCommands();
        }

        [RelayCommand]
        private void CancelAll() { _cts.Cancel(); _cts = new CancellationTokenSource(); NotifyCommands(); }

        [RelayCommand]
        private void ResetAll()
        {
            CancelAll(); _originalArray = null; _sorter.ResetTotal();
            BubbleSortResult = QuickSortResult = InsertionSortResult = ShakerSortResult = "Ожидание...";
            BubbleProgress = QuickProgress = InsertionProgress = ShakerProgress = 0;
            UpdateTotal(); NotifyCommands();
        }

        private bool CanSort() => _originalArray != null;

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void RunBubble() => StartSort(() => _sorter.BubbleSort(_originalArray!, _cts.Token, UseSharedArray), "Bubble");

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void RunQuick() => StartSort(() => _sorter.QuickSort(_originalArray!, _cts.Token, UseSharedArray), "Quick");

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void RunInsertion() => StartSort(() => _sorter.InsertionSort(_originalArray!, _cts.Token, UseSharedArray), "Insertion");

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void RunShaker() => StartSort(() => _sorter.ShakerSort(_originalArray!, _cts.Token, UseSharedArray), "Shaker");

        [RelayCommand(CanExecute = nameof(CanSort))]
        private void RunAll()
        {
            RunBubble();
            RunQuick();
            RunInsertion();
            RunShaker();
        }

        private void StartSort(Action action, string alg)
        {
            _sorter.UpdateThreadLimit(MaxThreads);
            SetStatus(alg, "Работает...");
            new Thread(() => action()) { IsBackground = true }.Start();
        }

        private void OnProgressChanged(string alg, double val) => _uiContext.Post(_ => {
            if (alg == "Bubble") BubbleProgress = val;
            else if (alg == "Quick") QuickProgress = val;
            else if (alg == "Insertion") InsertionProgress = val;
            else if (alg == "Shaker") ShakerProgress = val;
        }, null);

        private void UpdateUI(string alg, double ms) => _uiContext.Post(_ => {
            SetStatus(alg, $"{ms:F2} мс");
            UpdateTotal();
        }, null);

        private void UpdateTotal() => TotalComparisons = $"Общее число сравнений: {_sorter.TotalComparisons}";

        private void SetStatus(string alg, string status)
        {
            if (alg == "Bubble") BubbleSortResult = status;
            else if (alg == "Quick") QuickSortResult = status;
            else if (alg == "Insertion") InsertionSortResult = status;
            else if (alg == "Shaker") ShakerSortResult = status;
        }

        private void NotifyCommands()
        {
            RunBubbleCommand.NotifyCanExecuteChanged();
            RunQuickCommand.NotifyCanExecuteChanged();
            RunInsertionCommand.NotifyCanExecuteChanged();
            RunShakerCommand.NotifyCanExecuteChanged();
            RunAllCommand.NotifyCanExecuteChanged();
        }
    }
}