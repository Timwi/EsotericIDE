using System;
using System.Linq;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Controls;
using RT.Util.Forms;

namespace EsotericIDE.Languages
{
    partial class Funciton
    {
        public override ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText)
        {
            ToolStripMenuItem includeFiles = null;

            var update = Ut.Lambda(() =>
            {
                includeFiles.Text = "Select &include files ({0})...".Fmt(_settings.AdditionalSourceFiles.Length);
            });

            return Ut.NewArray<ToolStripMenuItem>(
                new ToolStripMenuItem("F&unciton settings", null,
                    (includeFiles = new ToolStripMenuItem("Select &include files...", null, delegate
                    {
                        using (var dlg = new ManagedForm(_settings.FormSettings) { FormBorderStyle = FormBorderStyle.Sizable, Text = "Select include files", MinimizeBox = false, MaximizeBox = false, ShowIcon = false, ShowInTaskbar = false })
                        {
                            var layout = new TableLayoutPanel { Dock = DockStyle.Fill };
                            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                            var list = new ListBoxEx { SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false, Dock = DockStyle.Fill, Sorted = true };
                            list.Items.AddRange(_settings.AdditionalSourceFiles);
                            layout.Controls.Add(list, 0, 0);
                            layout.SetRowSpan(list, 3);

                            var add = new Button { Text = "&Add", Anchor = AnchorStyles.Left | AnchorStyles.Right };
                            layout.Controls.Add(add, 1, 0);
                            var remove = new Button { Text = "&Remove", Anchor = AnchorStyles.Left | AnchorStyles.Right };
                            layout.Controls.Add(remove, 1, 1);

                            var bottomLayout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
                            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                            bottomLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
                            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
                            bottomLayout.Controls.Add(ok, 1, 0);
                            bottomLayout.Controls.Add(cancel, 2, 0);

                            layout.Controls.Add(bottomLayout, 0, 3);
                            layout.SetColumnSpan(bottomLayout, 2);

                            dlg.AcceptButton = ok;
                            dlg.CancelButton = cancel;

                            dlg.Controls.Add(layout);

                            add.Click += delegate
                            {
                                using (var fileDlg = new OpenFileDialog { Multiselect = true, Filter = "Funciton source files|*.fnc|All files|*.*", FilterIndex = 0, DefaultExt = "fnc", CheckFileExists = true, Title = "Select file(s) to add" })
                                {
                                    var fileResult = fileDlg.ShowDialog();
                                    if (fileResult != DialogResult.Cancel)
                                    {
                                        var newFileNames = fileDlg.FileNames.Except(list.Items.Cast<string>()).ToArray();
                                        list.Items.AddRange(newFileNames);
                                        list.SelectedItems.Clear();
                                        foreach (var filename in newFileNames)
                                            list.SelectedItems.Add(filename);
                                    }
                                }
                            };
                            remove.Click += delegate
                            {
                                foreach (var filename in list.SelectedItems.Cast<string>().ToArray())
                                    list.Items.Remove(filename);
                            };

                            if (dlg.ShowDialog() == DialogResult.OK)
                                _settings.AdditionalSourceFiles = list.Items.Cast<string>().ToArray();
                        }
                    }))
                )
            );
        }
    }
}
