namespace BloodShadow.Core.SaveSystem
{
    using BloodShadow.Core.Logger;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class JsonSaveSystem : SaveSystem
    {
        private readonly JsonSerializerSettings _settings;
        private readonly Logger? _logger = null;

        public JsonSaveSystem()
        {
            _settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };
        }

        public JsonSaveSystem(JsonSerializerSettings settings) : this() { _settings = settings; }
        public JsonSaveSystem(Logger logger) : this() { _logger = logger; }
        public JsonSaveSystem(JsonSerializerSettings settings, Logger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public override void Save(string key, object data, Action<bool>? callback, bool useBuildPath, bool useCheckPath)
        {
            try
            {
                string path = key;
                if (useBuildPath) { path = BuildPath(key); }
                if (useCheckPath) { CheckFile(path); }
                using (StreamWriter writer = new StreamWriter(path)) { writer.Write(JsonConvert.SerializeObject(data, _settings)); }
                callback?.Invoke(true);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.WriteLineWarning(ex);
                    _logger.WriteLineWarning("Save wrong");
                }
                else
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Save wrong");
                }
                callback?.Invoke(false);
            }
        }

        public override async Task SaveAsync(string key, object data, Action<bool>? callback, bool useBuildPath, bool useCheckPath)
        {
            try
            {
                string path = key;
                if (useBuildPath) { path = BuildPath(key); }
                if (useCheckPath) { CheckFile(path); }
                using (StreamWriter writer = new StreamWriter(path)) { await writer.WriteAsync(JsonConvert.SerializeObject(data, _settings)); }
                callback?.Invoke(true);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.WriteLineWarning(ex);
                    _logger.WriteLineWarning("Save wrong");
                }
                else
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Save wrong");
                }
                callback?.Invoke(false);
            }
        }

        public override void SaveToString(object data, Action<bool, string>? callback)
        {
            try { callback?.Invoke(true, JsonConvert.SerializeObject(data, _settings)); }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.WriteLineWarning(ex);
                    _logger.WriteLineWarning("Save wrong");
                }
                else
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Save wrong");
                }
                callback?.Invoke(false, "");
            }
        }

        public override void Load<T>(string key, Action<T> callback, bool useBuildPath, bool useCheckPath)
        {
            try
            {
                string path = key;
                if (useBuildPath) { path = BuildPath(key); }
                if (useCheckPath) { CheckFile(path); }
                using StreamReader fileStream = new StreamReader(path);
                callback?.Invoke(JsonConvert.DeserializeObject<T>(fileStream.ReadToEnd(), _settings));
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.WriteLineWarning(ex);
                    _logger.WriteLineWarning("Load wrong");
                }
                else
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Load wrong");
                }
                callback?.Invoke(default);
            }
        }

        public override async Task LoadAsync<T>(string key, Action<T> callback, bool useBuildPath, bool useCheckPath)
        {
            try
            {
                string path = key;
                if (useBuildPath) { path = BuildPath(key); }
                if (useCheckPath) { CheckFile(path); }
                using StreamReader fileStream = new StreamReader(path);
                string data = await fileStream.ReadToEndAsync();
                callback?.Invoke(JsonConvert.DeserializeObject<T>(data, _settings));
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.WriteLineWarning(ex);
                    _logger.WriteLineWarning("Load wrong");
                }
                else
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Load wrong");
                }
                callback?.Invoke(default);
            }
        }

        public override void LoadFromString<T>(string objectString, Action<T> callback)
        {
            try { callback?.Invoke(JsonConvert.DeserializeObject<T>(objectString)); }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.WriteLineWarning(ex);
                    _logger.WriteLineWarning("Load wrong");
                }
                else
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Load wrong");
                }
                callback?.Invoke(default);
            }
        }

        public override void CheckFile(string path)
        {
            path = BuildPath(path);
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists) { fileInfo.Create().Close(); }
        }

        public override void CheckDirectory(string path)
        {
            path = BuildPath(path);
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists) { dirInfo.Create(); }
        }

        protected virtual string BuildPath(string key) => Path.Combine("./", key);
    }
}