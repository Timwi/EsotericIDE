using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;

namespace EsotericIDE
{
    sealed partial class AboutBox : Form
    {
        /// <summary>Main constructor.</summary>
        public AboutBox()
        {
            InitializeComponent();

            lblProductName.Text = AssemblyProduct;
            lblDescription.Text = AssemblyDescription;
            lblVersion.Text = "Version {0}".Fmt(Ut.VersionOfExe());
            lblCopyright.Text = AssemblyCopyright;
            btnOK.Text = "&OK";
            btnOK.BackColor = Color.FromKnownColor(KnownColor.ButtonFace);
            ctLogo.Image = Resources.EsotericIDELogo;
            Icon = Resources.EsotericIDEIcon;
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

        private Color ?_previousColor ;
        private Color? _targetColor;
        private int _countdown = 60;

        private void ctTimer_Tick(object sender, EventArgs e)
        {
            if (_previousColor == null)
                _previousColor = ctLogo.BackColor;

            if (_targetColor != null)
            {
                if (_countdown == 0)
                {
                    _previousColor = ctLogo.BackColor = _targetColor.Value;
                    _targetColor = null;
                    _countdown = 60;
                }
                else
                {
                    ctLogo.BackColor = GraphicsUtil.ColorBlend(_targetColor.Value, _previousColor.Value, (60 - _countdown) / 60d);
                    _countdown--;
                }
            }
            else
            {
                _countdown--;
                if (_countdown == 0)
                {
                    _countdown = 60;
                    _targetColor = Color.FromArgb(Rnd.Next(0, 255), Rnd.Next(0, 255), Rnd.Next(0, 255));
                }
            }
        }
    }
}
