namespace BloodShadow.GameCore.Localizations
{
    public class LocalizationPair<T>(string key, T value) : LocalizationPair(key, value ?? new object())
    {
        public override string Key { get; set; } = key;
        public override object Value { get; set; } = value ?? new object();
        public T TValue => (T)Value;
    }

    public class LocalizationPair(string key, object value)
    {
        public virtual string Key { get; set; } = key;
        public virtual object Value { get; set; } = value;
    }
}
