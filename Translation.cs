using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.Lingo;

namespace EsotericIDE
{
    enum TranslationGroup
    {
        [LingoGroup("Confirmation: Save changes", "Strings pertaining to the “Save changes?” prompt.")]
        ConfirmationSaveChanges
    }

    sealed class Translation : TranslationBase
    {
        public static readonly Language DefaultLanguage = Language.EnglishUK;

        public Translation() : base(DefaultLanguage) { }

        public MainformTranslation Mainform = new MainformTranslation();
        public AboutBoxTranslation AboutBox = new AboutBoxTranslation();

        [LingoInGroup(TranslationGroup.ConfirmationSaveChanges), LingoNotes("Confirmation dialog. Appears when user tries to close a file when there are unsaved changes.")]
        public TrString SaveChanges = "Would you like to save your changes to this file?";
        [LingoInGroup(TranslationGroup.ConfirmationSaveChanges)]
        public TrString SaveChangesSave = "&Save";
        [LingoInGroup(TranslationGroup.ConfirmationSaveChanges)]
        public TrString SaveChangesDiscard = "&Discard";
        [LingoInGroup(TranslationGroup.ConfirmationSaveChanges)]
        public TrString SaveChangesCancel = "&Cancel";

        [LingoNotes("Confirmation dialog. Appears when user tries to close a file while the program is currently being debugged.")]
        public TrString CancelDebugging = "Cancel debugging?";
        [LingoNotes("Used in yes/no confirmation dialogs.")]
        public TrString Yes = "&Yes";
        [LingoNotes("Used in yes/no confirmation dialogs.")]
        public TrString No = "&No";
        [LingoNotes("Used in message dialogs and in OK/Cancel prompts.")]
        public TrString Ok = "&OK";
        [LingoNotes("Used in OK/Cancel prompts.")]
        public TrString Cancel = "&Cancel";

        [LingoNotes("Titlebar for the Save dialog.")]
        public TrString SaveFile = "Save file";
        [LingoNotes("Titlebar for the Open dialog.")]
        public TrString OpenFile = "Open file";

        [LingoNotes("When compilation of a program returns an error, the error message is prefixed with this.")]
        public TrString CompilationFailed = "Compilation failed:";

        [LingoNotes("Indicates that the user aborted the program while debugging and that it didn’t run to completion.")]
        public TrString ExecutionStopped = "Execution stopped.";

        [LingoNotes("Prompts the user to provide input to the program when they run it.")]
        public TrString Input = "Please type the input to the program:";
    }

    sealed partial class MainformTranslation
    {
        [LingoNotes("Used in the window titlebar to indicate that the current file has not been saved under any filename yet.")]
        public TrString UnnamedFile = "(unnamed)";
        [LingoNotes("Used in the window titlebar to indicate that the program is currently being debugged.")]
        public TrString Running = "(running)";
    }

    sealed partial class AboutBoxTranslation
    {
        public TrString Version = "Version {0}";
    }
}
