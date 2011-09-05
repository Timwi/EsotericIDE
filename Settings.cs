using RT.Util;
using RT.Util.Forms;
using RT.Util.Lingo;

namespace EsotericIDE
{
    [Settings("EsotericIDE", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsBase
    {
        public ManagedForm.Settings FormSettings = new ManagedForm.Settings();
        public Language Language;
        public string SourceFontName;
        public float SourceFontSize;
        public string OutputFontName;
        public float OutputFontSize;
        public string LastDirectory;
        public int SplitterDistance;
        public string DebugInput;
        public bool SaveWhenRun = true;

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            SettingsUtil.PostBuildStep(rep, typeof(Settings));
        }
#endif
    }
}