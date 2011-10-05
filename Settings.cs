using System.Collections.Generic;
using RT.Util;
using RT.Util.Forms;

namespace EsotericIDE
{
    [Settings("EsotericIDE", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsBase
    {
        public ManagedForm.Settings FormSettings = new ManagedForm.Settings();
        public string LastLanguageName;
        public string SourceFontName;
        public float SourceFontSize;
        public string OutputFontName;
        public float OutputFontSize;
        public string ExecutionStateFontName;
        public float ExecutionStateFontSize;
        public string LastDirectory;
        public int SplitterDistance;
        public string DebugInput;
        public bool SaveWhenRun = true;

        public Dictionary<string, LanguageSettings> LanguageSettings;

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            SettingsUtil.PostBuildStep(rep, typeof(Settings));
        }
#endif
    }

    public abstract class LanguageSettings { }
}