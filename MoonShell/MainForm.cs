using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace MoonShell
{
    public partial class MainForm : Form
    {
        internal static string WorkingDirectory = string.Empty;
        int tabCounter = 0;

        public static bool IsAdmin
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        internal void AddTab()
        {
            ConsoleControl cc = new ConsoleControl(this);
            cc.BackColor = Options.CurrentOptions.BackgroundColor;
            cc.ForeColor = Options.CurrentOptions.ForegroundColor;
            cc.ErrorColor = Options.CurrentOptions.ErrorColor;
            cc.Font = Options.CurrentOptions.Font;
            cc.InternalRichTextBox.ContextMenuStrip = helperMenu;
            cc.InternalRichTextBox.AllowDrop = true;
            cc.InternalRichTextBox.DragEnter += MainForm_DragEnter;
            cc.InternalRichTextBox.DragDrop += MainForm_DragDrop;
            
            tabCounter++;
            TabPage tab = new TabPage();
            tab.Text = "Console " + tabCounter;
            tab.BackColor = Options.CurrentOptions.BackgroundColor;
            tab.ForeColor = Options.CurrentOptions.ForegroundColor;

            Tabs.TabPages.Add(tab);
            cc.Dock = DockStyle.Fill;
            tab.Controls.Add(cc);
            cc.Dock = DockStyle.Fill;

            Tabs.SelectedTab = tab;
            cc.Select();

            cc.StartProcess("cmd", WorkingDirectory);
        }

        internal void RemoveTab()
        {
            if (Tabs.TabPages.Count > 1)
            {
                var cc = Tabs.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();
                cc.StopProcess();
                Tabs.TabPages.Remove(Tabs.SelectedTab);
            }
        }

        internal void ClearTab()
        {
            var cc = Tabs.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();
            cc.ClearOutput();
        }

        //private void FixColor()
        //{
        //    foreach (ToolStripItem item2 in helperMenu.Items)
        //    {
        //        item2.ForeColor = Options.CurrentOptions.ForegroundColor;
        //    }
        //}

        private void ApplyOptions()
        {
            foreach (TabPage tab in Tabs.TabPages)
            {
                foreach (ConsoleControl cmd in tab.Controls.OfType<ConsoleControl>())
                {
                    cmd.Font = Options.CurrentOptions.Font;
                    cmd.ForeColor = Options.CurrentOptions.ForegroundColor;
                    cmd.BackColor = Options.CurrentOptions.BackgroundColor;
                }
            }

            //FixColor();
        }

        internal void NewWindow()
        {
            try
            {
                Process.Start(Path.Combine(Application.StartupPath + "\\" + AppDomain.CurrentDomain.FriendlyName));
            }
            catch { }
        }

        internal void NewWindowAdmin()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(Path.Combine(Application.StartupPath + "\\" + AppDomain.CurrentDomain.FriendlyName));
                info.UseShellExecute = true;
                info.Verb = "runas";
                Process.Start(info);
            }
            catch { }
        }

        private void Copy()
        {
            try
            {
                var cc = Tabs.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();
                Clipboard.SetText(cc.InternalRichTextBox.SelectedText);
            }
            catch { }
        }

        private void CopyAll()
        {
            try
            {
                var cc = Tabs.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();
                Clipboard.SetText(cc.InternalRichTextBox.Text);
            }
            catch { }
        }

        private void Paste()
        {
            try
            {
                var cc = Tabs.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();
                cc.InternalRichTextBox.AppendText(Clipboard.GetText());
            } 
            catch { }
        }

        internal void ShowOptions()
        {
            OptionsForm f = new OptionsForm();
            f.ShowDialog();

            ApplyOptions();
        }

        internal void ShowThemes()
        {
            ThemeForm f = new ThemeForm(this);
            f.ShowDialog(this);
        }

        internal void ShowAboutDialog()
        {
            AboutForm f = new AboutForm();
            f.ShowDialog(this);
        }

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Options.ApplyTheme(this);
            helperMenu.Renderer = new ToolStripRendererMaterial();

            this.KeyPreview = true;

            if (IsAdmin)
            {
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
                this.Text = "Administrator: MoonShell " + Program.GetCurrentVersionToString();
            }
            else
            {
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                this.Text = "MoonShell " + Program.GetCurrentVersionToString();
            }

            //FixColor();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AddTab();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddTab();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RemoveTab();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearTab();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            NewWindow();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            NewWindowAdmin();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ShowAboutDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ShowThemes();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowOptions();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Options.SaveSettings();
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            CopyAll();
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            ClearTab();
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            AddTab();
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            RemoveTab();
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            NewWindow();
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            NewWindowAdmin();
        }

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] sa = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            var cc = Tabs.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();

            try
            {
                if (Directory.Exists(sa[0]))
                {
                    cc.InternalRichTextBox.AppendText("\"" + sa[0] + "\"");
                }
                if (File.Exists(sa[0]))
                {
                    cc.InternalRichTextBox.AppendText(sa[0]);
                }
            }
            catch { }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.T) AddTab();
                if (e.KeyCode == Keys.W) RemoveTab();
            }
        }
    }
}
