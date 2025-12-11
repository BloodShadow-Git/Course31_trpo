using Newtonsoft.Json;

namespace BloodShadow.GameCore.Localizations
{
    public class LocalizationData(string key)
    {
        public string LocalizationKey { get; set; } = key;
        public LocalizationPair[] Pairs { get; set; } = [];

        [JsonConstructor] public LocalizationData(string key, params LocalizationPair[] pairs) : this(key) { Pairs = pairs; }
    }
}
