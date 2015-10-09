using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;
using RT.Util.Serialization;

namespace EsotericIDE
{
    [Settings("EsotericIDE", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsBase
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

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            SettingsUtil.PostBuildStep<Settings>(rep);
        }
#endif
    }

    public abstract class LanguageSettings { }
}