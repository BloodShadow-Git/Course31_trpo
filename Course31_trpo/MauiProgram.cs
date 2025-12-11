using BloodShadow.Core.SaveSystem;
using BloodShadow.GameCore.Localizations;
using CommunityToolkit.Maui;
using Course31_trpo.Sources.LoadModules;
using Course31_trpo.VM;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using R3;
using R3.Maui;
using System.Globalization;

namespace Course31_trpo
{
    public static class MauiProgram
    {
        public const double NARROWWINDOWSIZE = 600;
        public const string FAKEPATH = "FAKEPATH";

        public static Observable<AppTheme> OnAppThemeChange => _onAppThemeChange ??= CreateAppThemeObservable().Result;
        private static Observable<AppTheme>? _onAppThemeChange;

        public static Observable<SizeF> OnAppSizeChange => _onAppSizeChange ??= CreateAppSizeObservable().Result;
        private static Observable<SizeF>? _onAppSizeChange;

        public readonly static SaveSystem SaveSystem = new JsonSaveSystem();
        public readonly static LocalizationManager LocalizationManager = new DefaultLocalizationManager();
        public readonly static CultureInfo CurrentCultureInfo = CultureInfo.CurrentCulture;

        public readonly static IImportModule[] ImportModules = [new ExcelImportModule()];

        public static ReactiveProperty<HomeVM> HomeVM { get; private set; } = new();
        public static ReactiveProperty<LoadedItemsVM> LoadedItemsVM { get; private set; } = new();
        public static ReactiveProperty<ReportVM> ReportVM { get; private set; } = new();
        public static ReactiveProperty<SettingsVM> SettingsVM { get; private set; } = new();

        public readonly static IReadOnlyDictionary<int, string> MonthLocKeys = new Dictionary<int, string>()
        {
            { 1, LocalizationKeys.JAN },
            { 2, LocalizationKeys.FEB },
            { 3, LocalizationKeys.MAR },
            { 4, LocalizationKeys.APR },
            { 5, LocalizationKeys.MAY },
            { 6, LocalizationKeys.JUNE },
            { 7, LocalizationKeys.JULE },
            { 8, LocalizationKeys.AUG },
            { 9, LocalizationKeys.SEPT },
            { 10, LocalizationKeys.OCT },
            { 11, LocalizationKeys.NOV },
            { 12, LocalizationKeys.DEC },
        };

        public static MauiApp CreateMauiApp()
        {
            ExcelPackage.License.SetNonCommercialPersonal("BloodShadow");
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }).UseR3();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            LoadLocalizations();
            InitVM();

            return builder.Build();
        }

        private static void LoadLocalizations()
        {
            string localizationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "Localizations");
            if (!Directory.Exists(localizationPath)) { Directory.CreateDirectory(localizationPath); }
            foreach (string path in Directory.EnumerateFiles(localizationPath, "*.json", SearchOption.AllDirectories))
            { SaveSystem.Load<LocalizationData>(path, data => { if (data != null) { LocalizationManager.AddLocalization(data); } }, false, false); }
        }

        private static async void InitVM()
        {
            while (Application.Current == null) { await Task.Yield(); }
            SettingsVM.Value = new();
            ReportVM.Value = new();
            LoadedItemsVM.Value = new();
            HomeVM.Value = new();
            List<Task> tasks = [];
            foreach (IImportModule module in ImportModules)
            {
                if (!SettingsVM.CurrentValue.SettingsData.Value.AutorunDict.TryGetValue(module.Name, out BindableReactiveProperty<bool>? value))
                {
                    value = new(true);
                    SettingsVM.CurrentValue.SettingsData.Value.AutorunDict.Add(module.Name, value);
                }
                if (value.CurrentValue) { tasks.Add(module.Load(true).Start()); }
            }
            await Task.WhenAll(tasks);
        }
        private static async Task<Observable<AppTheme>> CreateAppThemeObservable()
        {
            while (Application.Current == null) { await Task.Yield(); }
            return Observable.EveryValueChanged(Application.Current, x => x.UserAppTheme);
        }
        private static async Task<Observable<SizeF>> CreateAppSizeObservable()
        {
            while (Application.Current == null || Application.Current.Windows.Count < 1) { await Task.Yield(); }
            return Observable.Concat(
                Observable.EveryValueChanged((object)Application.Current.Windows[0].Width, _
                    => new SizeF((float)Application.Current.Windows[0].Width, (float)Application.Current.Windows[0].Height)),
                Observable.EveryValueChanged((object)Application.Current.Windows[0].Height, _
                    => new SizeF((float)Application.Current.Windows[0].Width, (float)Application.Current.Windows[0].Height)));
        }
    }

    public static class LocalizationKeys
    {
        public const string SELECTFILE = nameof(SELECTFILE);
        public const string HOME = nameof(HOME);
        public const string LOADED = nameof(LOADED);
        public const string REPORTS = nameof(REPORTS);
        public const string SETTINGS = nameof(SETTINGS);
        public const string DEFAULTPATH = nameof(DEFAULTPATH);
        public const string THEME = nameof(THEME);
        public const string AUTORUN = nameof(AUTORUN);
        public const string LANGUAGE = nameof(LANGUAGE);
        public const string INCOME = nameof(INCOME);
        public const string EXPENSES = nameof(EXPENSES);

        public const string JAN = nameof(JAN);
        public const string FEB = nameof(FEB);
        public const string MAR = nameof(MAR);
        public const string APR = nameof(APR);
        public const string MAY = nameof(MAY);
        public const string JUNE = nameof(JUNE);
        public const string JULE = nameof(JULE);
        public const string AUG = nameof(AUG);
        public const string SEPT = nameof(SEPT);
        public const string OCT = nameof(OCT);
        public const string NOV = nameof(NOV);
        public const string DEC = nameof(DEC);

        public const string SELECTDEFPATH = nameof(SELECTDEFPATH);
        public const string CLEARINCOME = nameof(CLEARINCOME);
        public const string LOAD = nameof(LOAD);
    }

    public static class Routes
    {
        public const string Home = nameof(Home);
        public const string Loaded = nameof(Loaded);
        public const string Report = nameof(Report);
        public const string Settings = nameof(Settings);
    }
}
