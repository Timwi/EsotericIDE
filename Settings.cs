using RT.Util;
using RT.Util.Forms;

namespace EsotericIDE
{
    [Settings("EsotericIDE", SettingsKind.UserSpecific)]
    sealed class Settings
    {
        public ManagedForm.Settings FormSettings = new ManagedForm.Settings();
        public string SourceFontName;
        public float SourceFontSize;
        public string OutputFontName;
        public float OutputFontSize;
        public string LastDirectory;
        public int SplitterDistance;
    }
}
