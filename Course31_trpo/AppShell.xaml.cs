using R3;

namespace Course31_trpo
{
    public partial class AppShell : Shell
    {
        public IReadOnlyBindableReactiveProperty<string> HomeTitleText => _homeTitleText;
        public IReadOnlyBindableReactiveProperty<string> LoadedItemsTitleText => _loadedItemsTitleText;
        public IReadOnlyBindableReactiveProperty<string> ReportTitleText => _reportTitleText;
        public IReadOnlyBindableReactiveProperty<string> SettingsTitleText => _settingsTitleText;

        private BindableReactiveProperty<string> _homeTitleText;
        private BindableReactiveProperty<string> _loadedItemsTitleText;
        private BindableReactiveProperty<string> _reportTitleText;
        private BindableReactiveProperty<string> _settingsTitleText;

        public AppShell()
        {
            _homeTitleText = new();
            _loadedItemsTitleText = new();
            _reportTitleText = new();
            _settingsTitleText = new();

            BindingContext = this;
            InitializeComponent();

            MauiProgram.LocalizationManager.CurrentLocalization.Subscribe(_ =>
            {
                _homeTitleText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.HOME) ?? LocalizationKeys.HOME;
                _loadedItemsTitleText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.LOADED) ?? LocalizationKeys.LOADED;
                _reportTitleText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.REPORTS) ?? LocalizationKeys.REPORTS;
                _settingsTitleText.Value = MauiProgram.LocalizationManager.Localize<string>(LocalizationKeys.SETTINGS) ?? LocalizationKeys.SETTINGS;
            });
        }
    }
}
