using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using RT.PostBuild;
using RT.Serialization;
using RT.Util;

[assembly: AssemblyTitle("Esoteric IDE")]
[assembly: AssemblyDescription("IDE (Interpreter/Debugger Engine) for esoteric programming languages")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Esoteric IDE")]
[assembly: AssemblyCopyright("Copyright © Timwi 2011–2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("1c40df55-4404-4c9e-8d71-99c6310999fd")]
[assembly: AssemblyVersion("1.1.2")]
[assembly: AssemblyFileVersion("1.1.2")]

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
                return PostBuildChecker.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());

            Classify.DefaultOptions.AddTypeSubstitution(new ClassifyColorSubstitute());

            SettingsUtil.LoadSettings(out Settings);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Mainform(Settings, args));

            Settings.Save(onFailure: SettingsOnFailure.ShowRetryWithCancel);
            return 0;
        }
    }
}
