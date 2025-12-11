namespace BloodShadow.Core.SaveSystem
{
    using System;
    using System.Threading.Tasks;

    public abstract class SaveSystem
    {
        public void Save(string key, object data) => Save(key, data, null, true, true);
        public void Save(string key, object data, Action<bool>? callback) => Save(key, data, callback, true, true);
        public abstract void Save(string key, object data, Action<bool>? callback, bool useBuildPath, bool useCheckPath);

        public Task SaveAsync(string key, object data) => SaveAsync(key, data, null, true, true);
        public Task SaveAsync(string key, object data, Action<bool>? callback) => SaveAsync(key, data, callback, true, true);
        public abstract Task SaveAsync(string key, object data, Action<bool>? callback, bool useBuildPath, bool useCheckPath);
        public abstract void SaveToString(object data, Action<bool, string>? callback);

        public void Load<T>(string key, Action<T> callback) => Load(key, callback, true, true);
        public abstract void Load<T>(string key, Action<T> callback, bool useBuildPath, bool useCheckPath);
        public Task LoadAsync<T>(string key, Action<T> callback) => LoadAsync(key, callback, true, true);
        public abstract Task LoadAsync<T>(string key, Action<T> callback, bool useBuildPath, bool useCheckPath);
        public abstract void LoadFromString<T>(string objectString, Action<T> callback);

        public abstract void CheckFile(string path);
        public abstract void CheckDirectory(string path);
    }
}