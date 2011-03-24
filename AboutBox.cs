using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Lingo;

namespace EsotericIDE
{
    sealed partial class AboutBox : Form
    {
        /// <summary>Main constructor.</summary>
        public AboutBox()
        {
            InitializeComponent();
            Lingo.TranslateControl(this, Program.Tr.AboutBox);

            lblProductName.Text = AssemblyProduct;
            lblDescription.Text = AssemblyDescription;
            lblVersion.Text = Program.Tr.AboutBox.Version.Fmt(Ut.VersionOfExe());
            lblCopyright.Text = AssemblyCopyright;
            lblCompanyName.Text = AssemblyCompany;
            btnOK.Text = Program.Tr.Ok;
        }

#if DEBUG
        public AboutBox(bool dummy)
        {
            InitializeComponent();
        }
#endif

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public Version AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((AssemblyCompanyAttribute) attributes[0]).Company;
            }
        }
        #endregion

        private void clickUrl(object sender, EventArgs e)
        {
            Process.Start(lblURL.Text);
        }
    }
}
