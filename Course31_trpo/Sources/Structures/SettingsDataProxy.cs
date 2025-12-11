using ObservableCollections;
using R3;

namespace Course31_trpo.Sources.Structures
{
    public class SettingsDataProxy
    {
        public ReactiveProperty<string> DefaultPath { get; }
        public BindableReactiveProperty<AppTheme> CurrentTheme { get; }
        public ObservableDictionary<string, BindableReactiveProperty<bool>> AutorunDict { get; }
        public BindableReactiveProperty<string> CurrentLocalization { get; }

        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private const string NOLOC = "NO LOCALIZATION";
        private Subject<Unit> _onSaveSubj;
        private CompositeDisposable _cd;

        public SettingsDataProxy()
        {
            SettingsData Data = new();
            MauiProgram.SaveSystem.Load<SettingsData>(_filePath, data => { Data = data ?? new(); }, false, true);

            DefaultPath = new(Data.DefaultPath);
            CurrentTheme = new(Data.CurrentTheme);
            AutorunDict = [.. Data.Autorun.Select(x => new KeyValuePair<string, BindableReactiveProperty<bool>>(x.Key, new(x.Value)))];
            if (Data.CurrentLocalization.Equals(NOLOC)) { CurrentLocalization = new(MauiProgram.LocalizationManager.AvailableLocalizations.FirstOrDefault() ?? NOLOC); }
            else { CurrentLocalization = new(Data.CurrentLocalization); }
            _onSaveSubj = new();
            _cd = new();

            DefaultPath.Subscribe(_ => { if (!Directory.Exists(DefaultPath.CurrentValue)) { DefaultPath.Value = new SettingsData().DefaultPath; } });
            CurrentTheme.Subscribe(_ => { Application.Current?.UserAppTheme = CurrentTheme.CurrentValue; });
            MauiProgram.OnAppThemeChange.Skip(1).Subscribe(_ => CurrentTheme.Value = Application.Current?.UserAppTheme ?? AppTheme.Unspecified);
            CurrentLocalization.Subscribe(_ => { MauiProgram.LocalizationManager.SetLocalization(CurrentLocalization.CurrentValue); });
            MauiProgram.LocalizationManager.CurrentLocalization.Subscribe(_ => CurrentLocalization.Value = MauiProgram.LocalizationManager.CurrentLocalization.CurrentValue);

            AutorunDict.ObserveChanged().Subscribe(_ => UpdateDict());
            UpdateDict();

            DefaultPath.Select(_ => Unit.Default)
                .Merge(CurrentTheme.Select(_ => Unit.Default))
                .Merge(CurrentLocalization.Select(_ => Unit.Default))
                .Merge(_onSaveSubj)
                .Subscribe(_ => MauiProgram.SaveSystem.Save(_filePath, new SettingsData()
                {
                    DefaultPath = DefaultPath.CurrentValue,
                    CurrentTheme = CurrentTheme.CurrentValue,
                    Autorun = new(AutorunDict.Select(x => new KeyValuePair<string, bool>(x.Key, x.Value.CurrentValue))),
                    CurrentLocalization = CurrentLocalization.CurrentValue
                }, null, false, true));
        }

        private void UpdateDict()
        {
            _cd.Dispose();
            _cd = [];
            foreach (var item in AutorunDict) { _cd.Add(item.Value.Subscribe(_ => _onSaveSubj.OnNext(Unit.Default))); }
        }

        public class SettingsData
        {
            public string DefaultPath { get; set; }
            public AppTheme CurrentTheme { get; set; }
            public Dictionary<string, bool> Autorun { get; set; }
            public string CurrentLocalization { get; set; }

            public SettingsData()
            {
                DefaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "Reports");
                CurrentTheme = Application.Current?.PlatformAppTheme ?? AppTheme.Unspecified;
                Autorun = [];
                CurrentLocalization = MauiProgram.LocalizationManager.AvailableLocalizations.FirstOrDefault() ?? NOLOC;
            }
        }
    }
}
