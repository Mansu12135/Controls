using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace TextRedactor
{
    class SettingsManager
    {
        private readonly string Path = "Setting.stg";
        private BinaryFormatter formatter = new BinaryFormatter();

        public Dictionary<string, string> Value = new Dictionary<string, string>();
        public void Save()
        {
            using (var stream = new FileStream(Path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(stream, Value);
            }
        }

        public void Load()
        {
            if (File.Exists(Path))
            {
                using (var stream = new FileStream(Path, FileMode.Open))
                {
                    var notes = formatter.Deserialize(stream) as Dictionary<string, string>;
                    if (notes != null)
                    {
                        Value = notes;
                    }
                }
            }
            if (!Value.Any())
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            Value.Add("CurrentStateProjectBrowserProjects", "");
            Value.Add("SettingPathToProjectFolder", "");
            Value.Add("SettingMarginWight", "800");
            Value.Add("SettingFontsList", "");
            Value.Add("SettingMarginsList", "");
            Value.Add("SettingFont", "14");
            Value.Add("SettingLineSpacing", "1");
        }
    }
}
