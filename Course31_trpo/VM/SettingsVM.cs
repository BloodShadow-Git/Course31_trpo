using CommunityToolkit.Maui.Storage;
using Course31_trpo.Sources.Structures;
using ObservableCollections;
using R3;

namespace Course31_trpo.VM
{
    public class SettingsVM
    {
        public IReadOnlyBindableReactiveProperty<SettingsDataProxy> SettingsData => _settingsData;
        public IReadOnlyBindableReactiveProperty<string> DefPathText => _defPathText;
        public IReadOnlyBindableReactiveProperty<string> SelDefPathText => _selDefPathText;
        public BindableReactiveProperty<string> DefaultPath { get; }
        public ReactiveCommand DefaultPathValid { get; }
        public IReadOnlyBindableReactiveProperty<string> ThemeText => _themeText;
        public IReadOnlyList<string> ThemesList => _themesList.ToNotifyCollectionChanged();
        public BindableReactiveProperty<int> SelectedThemeIndex { get; }
        public IReadOnlyBindableReactiveProperty<string> AutorunText => _autorunText;
        public NotifyCollectionChangedSynchronizedViewList<AutorunVM> Autorun =>
            _settingsData.CurrentValue.AutorunDict.CreateView(x => new AutorunVM(x.Key, x.Value)).ToNotifyCollectionChanged();
        public IReadOnlyBindableReactiveProperty<string> LanguageText => _languageText;
        public NotifyCollectionChangedSynchronizedViewList<string> AvailableLocalizations => _availableLocalizations.ToNotifyCollectionChanged();
        public BindableReactiveProperty<int> SelectedLanguageIndex { get; }
        public ReactiveCommand SelectDefPath { get; }

        private readonly BindableReactiveProperty<SettingsDataProxy> _settingsData;
        private readonly BindableReactiveProperty<string> _defPathText;
        private readonly BindableReactiveProperty<string> _selDefPathText;
        private readonly BindableReactiveProperty<string> _themeText;
        private readonly ObservableList<string> _themesList;
        private readonly BindableReactiveProperty<string> _autorunText;
        private readonly BindableReactiveProperty<string> _languageText;
        private readonly ObservableList<string> _availableLocalizations;

        public SettingsVM()
        {
            _settingsData = new(new());
            _defPathText = new();
            _selDefPathText = new();
            DefaultPath = new();
            DefaultPathValid = new();
            _themeText = new();
            _themesList = [];
            SelectedThemeIndex = new();
            _autorunText = new();
            _languageText = new();
            SelectedLanguageIndex = new();
            SelectDefPath = new();
            _availableLocalizations = [];

            MauiProgram.LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _defPathText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.DEFAULTPATH) ?? LocalizationKeys.DEFAULTPATH;
                _selDefPathText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.SELECTDEFPATH) ?? LocalizationKeys.SELECTDEFPATH;
                _themeText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.THEME) ?? LocalizationKeys.THEME;

                List<string> localizatedAppTheme = [.. Enum.GetNames<AppTheme>().Select(x => MauiProgram.LocalizationManager.Localize<string>(x) ?? x)];
                int minAppTheme = Math.Min(_themesList.Count, localizatedAppTheme.Count);
                for (int i = 0; i < minAppTheme; i++) { _themesList[i] = localizatedAppTheme[i]; }
                if (_themesList.Count > localizatedAppTheme.Count) { _themesList.RemoveRange(minAppTheme, _themesList.Count - minAppTheme); }
                else if (_themesList.Count < localizatedAppTheme.Count) { _themesList.AddRange(localizatedAppTheme.Skip(minAppTheme)); }

                _autorunText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.AUTORUN) ?? LocalizationKeys.AUTORUN;
                _languageText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.LANGUAGE) ?? LocalizationKeys.LANGUAGE;

                List<string> localizatedLang = [.. MauiProgram.LocalizationManager.AvailableLocalizations.Select(x => MauiProgram.LocalizationManager.Localize<string>(x) ?? x)];
                int minLang = Math.Min(_availableLocalizations.Count, localizatedLang.Count);
                for (int i = 0; i < minLang; i++) { _availableLocalizations[i] = localizatedLang[i]; }
                if (_availableLocalizations.Count > localizatedLang.Count) { _availableLocalizations.RemoveRange(minLang, _availableLocalizations.Count - minLang); }
                else if (_availableLocalizations.Count < localizatedLang.Count) { _availableLocalizations.AddRange(localizatedLang.Skip(minLang)); }
            });
            _settingsData.CurrentValue.DefaultPath.Subscribe(_ => DefaultPath.Value = _settingsData.CurrentValue.DefaultPath.CurrentValue);
            DefaultPathValid.Subscribe(_ =>
            {
                if (Directory.Exists(DefaultPath.CurrentValue)) { _settingsData.CurrentValue.DefaultPath.Value = DefaultPath.CurrentValue; }
                else { DefaultPath.Value = _settingsData.CurrentValue.DefaultPath.CurrentValue; }
            });
            SelectDefPath.Subscribe(async _ =>
            {
                FolderPickerResult res = await FolderPicker.PickAsync(AppDomain.CurrentDomain.BaseDirectory);
                if (res.IsSuccessful) { _settingsData.CurrentValue.DefaultPath.Value = res.Folder?.Path ?? AppDomain.CurrentDomain.BaseDirectory; }
            });
            _settingsData.CurrentValue.CurrentTheme.Subscribe(_ => SelectedThemeIndex.Value = (int)_settingsData.CurrentValue.CurrentTheme.CurrentValue);
            SelectedThemeIndex.Subscribe(_ =>
            {
                if (_themesList.Count <= 0)
                {
                    SelectedThemeIndex.Value = (int)_settingsData.CurrentValue.CurrentTheme.CurrentValue;
                    return;
                }
                _settingsData.CurrentValue.CurrentTheme.Value = (AppTheme)SelectedThemeIndex.CurrentValue;
            });
            _settingsData.CurrentValue.CurrentLocalization.Subscribe(_ =>
                SelectedLanguageIndex.Value = MauiProgram.LocalizationManager.AvailableLocalizations
                    .ToList().IndexOf(_settingsData.CurrentValue.CurrentLocalization.CurrentValue));
            SelectedLanguageIndex.Subscribe(_ =>
                _settingsData.CurrentValue.CurrentLocalization.Value = MauiProgram.LocalizationManager.AvailableLocalizations.ElementAt(SelectedLanguageIndex.CurrentValue));
        }

        public readonly struct AutorunVM(string key, BindableReactiveProperty<bool> value)
        {
            public string Key { get; } = key;
            public BindableReactiveProperty<bool> Value { get; } = value;
        }
    }
}
