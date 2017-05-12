﻿using Bit.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Bit.Core.Services
{
    public class SettingsService
    {
        private static SettingsService _instance;
        private static object _locker = new object();
        private static string _baseStoragePath = string.Concat(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "\\bitwarden\\DirectoryConnector");

        private SettingsModel _settings;

        private SettingsService() { }

        public static SettingsService Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new SettingsService();
                }

                return _instance;
            }
        }

        public SettingsModel Settings
        {
            get
            {
                var filePath = $"{_baseStoragePath}\\settings.json";
                if(_settings == null && File.Exists(filePath))
                {
                    var serializer = new JsonSerializer();
                    using(var s = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using(var sr = new StreamReader(s, Encoding.UTF8))
                    using(var jsonTextReader = new JsonTextReader(sr))
                    {
                        _settings = serializer.Deserialize<SettingsModel>(jsonTextReader);
                    }
                }

                return _settings == null ? new SettingsModel() : _settings;
            }
        }

        private void SaveSettings()
        {
            lock(_locker)
            {
                if(!Directory.Exists(_baseStoragePath))
                {
                    Directory.CreateDirectory(_baseStoragePath);
                }

                _settings = Settings;
                var filePath = $"{_baseStoragePath}\\settings.json";
                using(var s = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using(var sw = new StreamWriter(s, Encoding.UTF8))
                {
                    var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                    sw.Write(json);
                }
            }
        }

        public EncryptedData AccessToken
        {
            get
            {
                return Settings.AccessToken;
            }
            set
            {
                Settings.AccessToken = value;
                SaveSettings();
            }
        }

        public EncryptedData RefreshToken
        {
            get
            {
                return Settings.RefreshToken;
            }
            set
            {
                Settings.RefreshToken = value;
                SaveSettings();
            }
        }

        public class SettingsModel
        {
            public EncryptedData AccessToken { get; set; }
            public EncryptedData RefreshToken { get; set; }
        }
    }
}