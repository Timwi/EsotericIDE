﻿using System.Collections.Generic;
using RT.Util;
using RT.Util.Forms;
using RT.Util.Serialization;

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
        public string WatchFontName;
        public float WatchFontSize;
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