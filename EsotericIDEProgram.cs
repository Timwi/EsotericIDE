using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using RT.Util;

[assembly: AssemblyTitle("Esoteric IDE")]
[assembly: AssemblyDescription("IDE (Interpreter/Debugger Engine) for esoteric programming languages")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("CuteBits")]
[assembly: AssemblyProduct("Esoteric IDE")]
[assembly: AssemblyCopyright("Copyright © CuteBits 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("1c40df55-4404-4c9e-8d71-99c6310999fd")]
[assembly: AssemblyVersion("1.0.0.34")]
[assembly: AssemblyFileVersion("1.0.0.34")]

namespace EsotericIDE
{
    static class EsotericIDEProgram
    {
        public static Settings Settings;

        [STAThread]
        static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; }
            catch { }
            if (args.Length == 2 && args[0] == "--post-build-check")
                return Ut.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());

            SettingsUtil.LoadSettings(out Settings);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Mainform(Settings));

            Settings.Save(onFailure: SettingsOnFailure.ShowRetryWithCancel);
            return 0;
        }
    }
}
