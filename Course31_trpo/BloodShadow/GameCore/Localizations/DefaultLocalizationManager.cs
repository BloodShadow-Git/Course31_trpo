namespace BloodShadow.GameCore.Localizations
{
    public class DefaultLocalizationManager : LocalizationManager
    {
        private readonly Dictionary<(string, string, Type), object> _localizations;

        public DefaultLocalizationManager() : base() { _localizations = []; }

        public DefaultLocalizationManager(params LocalizationData[] datas) : this()
        {
            AddLocalization(datas);
            if (_availableLocalizations.Count > 0) { _currentLocalization.Value = _availableLocalizations[0]; }
        }

        public override void AddLocalization(LocalizationData data)
        {
            if (!_availableLocalizations.Contains(data.LocalizationKey)) { _availableLocalizations.Add(data.LocalizationKey); }
            foreach (LocalizationPair pair in data.Pairs) { _localizations[(data.LocalizationKey, pair.Key, pair.Value.GetType())] = pair.Value; }
        }

        public override T Localize<T>(string key)
        {
            if (_availableLocalizations.Count <= 0) { return default; }
            if (_availableLocalizations[0] == _currentLocalization.CurrentValue) { return Localize<T>(_currentLocalization.CurrentValue, key); }
            return Localize<T>(_currentLocalization.CurrentValue, key) ?? Localize<T>(_availableLocalizations[0], key);
        }

        private T Localize<T>(string lang, string key)
        {
            if (_availableLocalizations.Count <= 0) { return default; }
            if (_localizations.TryGetValue((lang, key, typeof(T)), out object value)) { return (T)value; }
            else { return default; }
        }
    }
}
