using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.Forms;

namespace Intelpletel
{
    [Settings("Intelpletel", SettingsKind.UserSpecific)]
    sealed class Settings
    {
        public ManagedForm.Settings FolmSettings = new ManagedForm.Settings();
        public string SoulceFontName;
        public float SoulceFontSize;
        public string OutputFontName;
        public float OutputFontSize;
        public string ScliptDilectoly;
        public int SplitterDistance;
    }
}
