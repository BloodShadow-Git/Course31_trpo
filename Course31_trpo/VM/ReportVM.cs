using Course31_trpo.Sources.LoadModules;
using Course31_trpo.Sources.Structures;
using ObservableCollections;
using R3;

namespace Course31_trpo.VM
{
    public class ReportVM
    {
        public Selecter<int> Year { get; }
        public UpdateableSelecter<string> Month { get; }
        public NotifyCollectionChangedSynchronizedViewList<GroupCollection<int, Transfer>> Transfers => _transfers.ToNotifyCollectionChanged();
        public NotifyCollectionChangedSynchronizedViewList<ModuleLoad> ModulesList => _modulesList.ToNotifyCollectionChanged();

        private ObservableList<GroupCollection<int, Transfer>> _transfers;
        private ObservableList<ModuleLoad> _modulesList;
        private Dictionary<int, int[]> _avaiableMonth;
        private Dictionary<(int, int), Report[]> _reportsDict;

        public ReportVM()
        {
            Year = new();
            Month = new();
            Month.Values.AddRange(MauiProgram.MonthLocKeys.Values.Select(x => new UpdateableSelecterItem<string>(x)));
            _transfers = [];
            _modulesList = [];
            _avaiableMonth = [];
            _reportsDict = [];
            MauiProgram.LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                for (int i = 0; i < Month.Values.Count; i++)
                {
                    UpdateableSelecterItem<string> cache = Month.Values[i];
                    cache.Item = MauiProgram.LocalizationManager.Localize<string>(cache.Item) ?? cache.Item;
                    Month.Values[i] = cache;
                }
            });
            foreach (IImportModule module in MauiProgram.ImportModules)
            {
                module.LoadedItems.ObserveChanged().Subscribe(_ => UpdateYear());
                _modulesList.Add(new(module));
            }
            UpdateYear();
            Year.DisplayValueIndex.AsObservable().Subscribe(_ => UpdateAvaiableMonth());
            Month.DisplayValueIndex.AsObservable().Subscribe(_ => UpdateReports());
        }

        private void UpdateYear()
        {
            IEnumerable<Report> repSum = MauiProgram.ImportModules.SelectMany(x => x.LoadedItems).OrderBy(x => x.DateOfSale);
            _avaiableMonth.Clear();
            _reportsDict.Clear();
            IEnumerable<IGrouping<int, Report>> grouppedReports = repSum.GroupBy(x => x.DateOfSale.Year);
            foreach (IGrouping<int, Report> reportGroup in grouppedReports)
            {
                IEnumerable<IGrouping<int, Report>> month = reportGroup.GroupBy(x => x.DateOfSale.Month);
                foreach (IGrouping<int, Report> reportMonth in month) { _reportsDict.Add((reportGroup.Key, reportMonth.Key), [.. reportMonth]); }
                _avaiableMonth.Add(reportGroup.Key, [.. month.Select(x => x.Key)]);
            }
            Year.Values.Clear();
            Year.Values.AddRange(_avaiableMonth.Keys);

            Year.DisplayValueIndex.Value = Year.Values.Count - 1;
            Month.SetMin(Month.Values.Count - 1);
        }

        private void UpdateAvaiableMonth()
        {
            if (Year.DisplayValueIndex.Value == -1) { return; }
            for (int i = 0; i < Month.Values.Count; i++) { Month.Values[i].Avaiable.Value = _avaiableMonth[Year.Values.ElementAt(Year.DisplayValueIndex.Value)].Contains(i + 1); }
            UpdateReports();
        }

        private void UpdateReports()
        {
            if (Year.DisplayValueIndex.Value == -1) { return; }
            _transfers.Clear();
            if (!_reportsDict.TryGetValue((Year.Values.ElementAt(Year.DisplayValueIndex.Value), Month.DisplayValueIndex.Value + 1), out Report[] reports)) { return; }
            _transfers.AddRange(reports.SelectMany(x => x.Transfers).OrderBy(x => x.Day).GroupBy(x => x.Day).Select(x => new GroupCollection<int, Transfer>(x.Key, x)));
        }
    }

    public class ModuleLoad
    {
        public IReadOnlyBindableReactiveProperty<string> ModuleName => _moduleName;
        public ReactiveCommand Load { get; }
        public IReadOnlyBindableReactiveProperty<string> LoadText => _loadText;

        private BindableReactiveProperty<string> _moduleName { get; }
        private BindableReactiveProperty<string> _loadText { get; }

        public ModuleLoad(IImportModule module)
        {
            _moduleName = new(module.Name);
            _loadText = new();
            MauiProgram.LocalizationManager.CurrentLocalization.Subscribe(_ =>
                _loadText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.LOAD) ?? LocalizationKeys.LOAD);
            Load = new();
            Load.Subscribe(_ => module.Load());
        }
    }
}
