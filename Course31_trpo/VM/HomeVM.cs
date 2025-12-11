using Course31_trpo.Drawables;
using Course31_trpo.Sources.LoadModules;
using Course31_trpo.Sources.Structures;
using ObservableCollections;
using R3;

namespace Course31_trpo.VM
{
    public class HomeVM
    {
        public IReadOnlyBindableReactiveProperty<PieChart> YearChart => _yearChart;
        public IReadOnlyBindableReactiveProperty<Diogramma> MonthYearDio => _monthYearDio;
        public IReadOnlyBindableReactiveProperty<ChangeData> Income => _income;
        public IReadOnlyBindableReactiveProperty<ChangeData> Expenses => _expenses;
        public IReadOnlyBindableReactiveProperty<ChangeData> Summary => _summary;
        public IReadOnlyBindableReactiveProperty<string> IncomeText => _incomeText;
        public IReadOnlyBindableReactiveProperty<string> ExpensesText => _expensesText;
        public IReadOnlyBindableReactiveProperty<string> SummaryText => _summaryText;

        private readonly BindableReactiveProperty<PieChart> _yearChart;
        private readonly BindableReactiveProperty<Diogramma> _monthYearDio;
        private readonly BindableReactiveProperty<ChangeData> _income;
        private readonly BindableReactiveProperty<ChangeData> _expenses;
        private readonly BindableReactiveProperty<ChangeData> _summary;
        private readonly BindableReactiveProperty<string> _incomeText;
        private readonly BindableReactiveProperty<string> _expensesText;
        private readonly BindableReactiveProperty<string> _summaryText;

        private const string _downGreen = "angle_small_down_green.png";
        private const string _downRed = "angle_small_down_red.png";
        private const string _downWhite = "angle_small_down_white.png";

        private static readonly Color _badColor = new(1f, 0f, 0f, 1f);
        private static readonly Color _normColor = new(1f, 1f, 1f, 1f);
        private static readonly Color _goodColor = new(0f, 1f, 0f, 1f);

        public HomeVM()
        {
            _yearChart = new(new() { Animate = false });
            _monthYearDio = new(new() { Animate = false });
            _income = new(new());
            _expenses = new(new());
            _summary = new(new());
            _income.CurrentValue.ChangeImage.Value = _downWhite;
            _expenses.CurrentValue.ChangeImage.Value = _downWhite;
            _summary.CurrentValue.ChangeImage.Value = _downWhite;
            _incomeText = new();
            _expensesText = new();
            _summaryText = new();
            MauiProgram.LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _monthYearDio.CurrentValue.ScaleNames.Value = [.. MauiProgram.MonthLocKeys.Values.Select(x => MauiProgram.LocalizationManager.Localize<string>(x))];
                _incomeText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.INCOME);
                _expensesText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.EXPENSES);
                _summaryText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.CLEARINCOME);
            });
            foreach (IImportModule module in MauiProgram.ImportModules) { module.LoadedItems.ObserveChanged().Subscribe(_ => { UpdateCharts(); }); }
            UpdateCharts();
        }

        private void UpdateCharts()
        {
            IEnumerable<Report> allValidReports = MauiProgram.ImportModules.SelectMany(x => x.LoadedItems);
            if (!allValidReports.Any()) { return; }
            IList<Report> reports = FixReports(allValidReports);
            IEnumerable<IGrouping<int, Report>> groupedByYear = reports.GroupBy(x => x.DateOfSale.Year);

            _yearChart.CurrentValue.Values.Clear();
            _yearChart.CurrentValue.Values.AddRange(groupedByYear.Select(x =>
                new PieChartData(x.SelectMany(y => y.Transfers).Where(x => x.Amount >= 0).Sum(y => y.Amount), x.Key)));
            _monthYearDio.CurrentValue.Values.Clear();
            _monthYearDio.CurrentValue.Values.AddRange(groupedByYear.Select(x =>
                new DiogrammaValue(x.Key.ToString(), [.. x.GroupBy(y => y.DateOfSale.Month).Select(y => (float)y.SelectMany(z => z.Transfers).Sum(z => z.Amount))])));

            IOrderedEnumerable<Report> ordered = allValidReports.OrderBy(x => x.DateOfSale);
            DateTime currDate = DateTime.UtcNow;
            Report lastMonth = ordered.Count() switch
            {
                0 => new(new DateOnly(currDate.Year, currDate.Month, 1), [], MauiProgram.FAKEPATH),
                _ => ordered.Last(),
            };
            Dictionary<DateOnly, Report> dict = reports.ToDictionary(x => x.DateOfSale);
            DateOnly lastDO = lastMonth.DateOfSale.AddMonths(-1);
            if (!dict.TryGetValue(lastDO, out Report prevMonth)) { prevMonth = new(lastDO, [], MauiProgram.FAKEPATH); }

            (int lastISum, int lastESum) = GetIESums(lastMonth);
            (int prevISum, int prevESum) = GetIESums(prevMonth);

            _income.CurrentValue.PrevValue.Value.Value.Value = prevISum;
            _income.CurrentValue.NewValue.Value.Value.Value = lastISum;
            _expenses.CurrentValue.PrevValue.Value.Value.Value = -prevESum;
            _expenses.CurrentValue.NewValue.Value.Value.Value = -lastESum;
            _summary.CurrentValue.PrevValue.Value.Value.Value = prevISum + prevESum;
            _summary.CurrentValue.NewValue.Value.Value.Value = lastISum + lastESum;

            FillChanges(_income);
            FillChanges(_expenses, true);
            FillChanges(_summary);
            //if (prevISum < lastISum)
            //{
            //    _income.CurrentValue.PrevValue.Value.Color.Value = _badColor;
            //    _income.CurrentValue.NewValue.Value.Color.Value = _goodColor;
            //    _income.CurrentValue.ChangeImage.Value = _downGreen;
            //}
            //else if (prevISum > lastISum)
            //{
            //    _income.CurrentValue.PrevValue.Value.Color.Value = _goodColor;
            //    _income.CurrentValue.NewValue.Value.Color.Value = _badColor;
            //    _income.CurrentValue.ChangeImage.Value = _downRed;
            //}
            //else
            //{
            //    _income.CurrentValue.PrevValue.Value.Color.Value = _normColor;
            //    _income.CurrentValue.NewValue.Value.Color.Value = _normColor;
            //    _income.CurrentValue.ChangeImage.Value = _downWhite;
            //}

            //if (prevESum < lastESum)
            //{
            //    _expenses.CurrentValue.PrevValue.Value.Color.Value = _badColor;
            //    _expenses.CurrentValue.NewValue.Value.Color.Value = _goodColor;
            //    _expenses.CurrentValue.ChangeImage.Value = _downGreen;
            //}
            //else if (prevESum > lastESum)
            //{
            //    _expenses.CurrentValue.PrevValue.Value.Color.Value = _goodColor;
            //    _expenses.CurrentValue.NewValue.Value.Color.Value = _badColor;
            //    _expenses.CurrentValue.ChangeImage.Value = _downRed;
            //}
            //else
            //{
            //    _expenses.CurrentValue.PrevValue.Value.Color.Value = _normColor;
            //    _expenses.CurrentValue.NewValue.Value.Color.Value = _normColor;
            //    _expenses.CurrentValue.ChangeImage.Value = _downWhite;
            //}
        }

        private static (int, int) GetIESums(Report source)
            => (source.Transfers.Where(x => x.Amount >= 0).Sum(x => x.Amount), source.Transfers.Where(x => x.Amount < 0).Sum(x => x.Amount));

        private static void FillChanges(BindableReactiveProperty<ChangeData> change, bool invert = false)
        {
            if (invert)
            {
                if (change.CurrentValue.PrevValue.Value.Value.Value > change.CurrentValue.NewValue.Value.Value.Value)
                {
                    change.CurrentValue.PrevValue.Value.Color.Value = _badColor;
                    change.CurrentValue.NewValue.Value.Color.Value = _goodColor;
                    change.CurrentValue.ChangeImage.Value = _downGreen;
                }
                else if (change.CurrentValue.PrevValue.Value.Value.Value < change.CurrentValue.NewValue.Value.Value.Value)
                {
                    change.CurrentValue.PrevValue.Value.Color.Value = _goodColor;
                    change.CurrentValue.NewValue.Value.Color.Value = _badColor;
                    change.CurrentValue.ChangeImage.Value = _downRed;
                }
                else
                {
                    change.CurrentValue.PrevValue.Value.Color.Value = _normColor;
                    change.CurrentValue.NewValue.Value.Color.Value = _normColor;
                    change.CurrentValue.ChangeImage.Value = _downWhite;
                }
            }
            else
            {
                if (change.CurrentValue.PrevValue.Value.Value.Value < change.CurrentValue.NewValue.Value.Value.Value)
                {
                    change.CurrentValue.PrevValue.Value.Color.Value = _badColor;
                    change.CurrentValue.NewValue.Value.Color.Value = _goodColor;
                    change.CurrentValue.ChangeImage.Value = _downGreen;
                }
                else if (change.CurrentValue.PrevValue.Value.Value.Value > change.CurrentValue.NewValue.Value.Value.Value)
                {
                    change.CurrentValue.PrevValue.Value.Color.Value = _goodColor;
                    change.CurrentValue.NewValue.Value.Color.Value = _badColor;
                    change.CurrentValue.ChangeImage.Value = _downRed;
                }
                else
                {
                    change.CurrentValue.PrevValue.Value.Color.Value = _normColor;
                    change.CurrentValue.NewValue.Value.Color.Value = _normColor;
                    change.CurrentValue.ChangeImage.Value = _downWhite;
                }
            }
        }

        private static IList<Report> FixReports(IEnumerable<Report> allValidReports)
        {
            Dictionary<DateOnly, Report> dict = allValidReports.GroupBy(x => x.DateOfSale)
                .ToDictionary(x => x.Key, x => new Report(x.Key, [.. x.SelectMany(y => y.Transfers)], string.Join(":_:", x.Select(y => y.FilePath))));

            DateOnly startDO = new(dict.Keys.Min().Year, 1, 1);
            DateOnly endDO = new(dict.Keys.Max().Year, 12, 1);
            for (DateOnly date = startDO; date <= endDO; date = date.AddMonths(1))
            {
                if (!dict.TryGetValue(date, out Report value))
                {
                    value = new(date, [], MauiProgram.FAKEPATH);
                    dict[date] = value;
                }
            }
            return [.. dict.Values];
        }

        public struct ChangeData()
        {
            public readonly IReadOnlyBindableReactiveProperty<ValueData> PrevValue => _prevValue;
            public readonly IReadOnlyBindableReactiveProperty<ValueData> NewValue => _newValue;
            public BindableReactiveProperty<string> ChangeImage { get; set; } = new();

            private readonly BindableReactiveProperty<ValueData> _prevValue = new(new());
            private readonly BindableReactiveProperty<ValueData> _newValue = new(new());

            public struct ValueData()
            {
                public BindableReactiveProperty<int> Value { get; set; } = new();
                public BindableReactiveProperty<Color> Color { get; set; } = new();
            }
        }
    }
}
