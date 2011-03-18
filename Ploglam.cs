using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using RT.Util;

[assembly: AssemblyTitle("Intelpletel")]
[assembly: AssemblyDescription("Intelplets the ploglamming Sclipting.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Intelpletel")]
[assembly: AssemblyCopyright("Copyright © CuteBits 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("1c40df55-4404-4c9e-8d71-99c6310999fd")]
[assembly: AssemblyVersion("1.0.9999.9999")]
[assembly: AssemblyFileVersion("1.0.9999.9999")]

namespace Intelpletel
{
    static class Ploglam
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

            SettingsUtil.SaveSettings(Settings, SettingsUtil.OnFailure.ShowRetryWithCancel);
            return 0;
        }
    }
}
