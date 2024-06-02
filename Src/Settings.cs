using System.Collections.Generic;
using RT.Serialization;
using RT.Util.Forms;

namespace EsotericIDE
{
    sealed class Settings
    {
        public ManagedForm.Settings FormSettings = new ManagedForm.Settings();
        public string LastLanguageName;
        public FontSpec SourceFont;
        public FontSpec OutputFont;
        public FontSpec WatchFont;
        public string LastDirectory;
        public double SplitterPercent;
        public string DebugInput;
        public bool SaveWhenRun = true;
        public bool WordWrap = true;

        [ClassifyNotNull]
        public Dictionary<string, LanguageSettings> LanguageSettings = new Dictionary<string, LanguageSettings>();
    }

    public abstract class LanguageSettings { }
}