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

        string _logsDirectory = Application.StartupPath + "\\Logs\\";

        int _tabCounter = 0;

        ConsoleControl _currentTab;

        public static bool IsAdmin
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        private void RestoreWindowState()
        {
            this.WindowState = Options.CurrentOptions.WindowState;
            this.Size = Options.CurrentOptions.WindowSize;

            if (Options.CurrentOptions.WindowLocation != null)
            {
                this.Location = (Point)Options.CurrentOptions.WindowLocation;
            }
            else
            {
                this.CenterToScreen();
            }
        }

        private void SaveWindowState()
        {
            Options.CurrentOptions.WindowState = this.WindowState;

            if (this.WindowState == FormWindowState.Normal)
            {
                Options.CurrentOptions.WindowLocation = this.Location;
                Options.CurrentOptions.WindowSize = this.Size;
            }
            else
            {
                Options.CurrentOptions.WindowLocation = this.RestoreBounds.Location;
                Options.CurrentOptions.WindowSize = this.RestoreBounds.Size;
            }

        }

        internal void ExportLog()
        {
            if (_currentTab != null)
            {
                if (!Directory.Exists(_logsDirectory))
                {
                    Directory.CreateDirectory(_logsDirectory);
                }

                string fileName = string.Format("{0}-{1}-{2} - {3}.{4}.{5}.log", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                try
                {
                    File.WriteAllText(_logsDirectory + fileName, _currentTab.InternalRichTextBox.Text, Encoding.UTF8);
                    MessageBox.Show("Log file created in Logs folder!", "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Log file couldn't be created:\n\n" + ex.Message, "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        internal void AddTab(string placeDirectory = null)
        {
            _currentTab = new ConsoleControl(this);
            _currentTab.BackColor = Options.CurrentOptions.BackgroundColor;
            _currentTab.ForeColor = Options.CurrentOptions.ForegroundColor;
            _currentTab.ErrorColor = Options.CurrentOptions.ErrorColor;
            _currentTab.Font = Options.CurrentOptions.Font;
            
            _currentTab.InternalRichTextBox.ContextMenuStrip = helperMenu;
            _currentTab.InternalRichTextBox.AllowDrop = true;
            _currentTab.InternalRichTextBox.DragEnter += MainForm_DragEnter;
            _currentTab.InternalRichTextBox.DragDrop += MainForm_DragDrop;
            
            _tabCounter++;
            TabPage tab = new TabPage();
            tab.Text = "Console " + _tabCounter;

            tab.BackColor = Options.CurrentOptions.BackgroundColor;
            tab.ForeColor = Options.CurrentOptions.ForegroundColor;

            tabConsoles.TabPages.Add(tab);
            _currentTab.Dock = DockStyle.Fill;
            tab.Controls.Add(_currentTab);
            _currentTab.Dock = DockStyle.Fill;

            if (!string.IsNullOrEmpty(placeDirectory))
            {
                _currentTab.StartProcess("cmd", placeDirectory);
            }
            else
            {
                _currentTab.StartProcess("cmd", WorkingDirectory);
            }

            tabConsoles.SelectedTab = tab;
            _currentTab.Focus();
        }

        internal void RemoveTab()
        {
            if (tabConsoles.TabPages.Count > 1)
            {
                if (_currentTab != null)
                {
                    _currentTab.StopProcess();
                    tabConsoles.TabPages.Remove(tabConsoles.SelectedTab);
                }  
            }
        }

        internal void ClearTab()
        {
            if (_currentTab != null)
            {
                _currentTab.ClearOutput();
                _currentTab.Focus();
            }
        }

        private void ApplyOptions()
        {
            foreach (TabPage tab in tabConsoles.TabPages)
            {
                foreach (ConsoleControl cc in tab.Controls.OfType<ConsoleControl>())
                {
                    cc.Font = Options.CurrentOptions.Font;
                    cc.ForeColor = Options.CurrentOptions.ForegroundColor;
                    cc.BackColor = Options.CurrentOptions.BackgroundColor;
                }
            }
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
            if (_currentTab != null)
            {
                try
                {
                    Clipboard.SetText(_currentTab.InternalRichTextBox.SelectedText);
                }
                catch { }
            }
        }

        private void CopyAll()
        {
            if (_currentTab != null)
            {
                try
                {
                    Clipboard.SetText(_currentTab.InternalRichTextBox.Text);
                }
                catch { }
            }
        }

        private void Paste()
        {
            if (_currentTab != null)
            {
                try
                {
                    _currentTab.InternalRichTextBox.AppendText(Clipboard.GetText());
                }
                catch { }
            }
        }

        internal void ShowOptions()
        {
            OptionsForm f = new OptionsForm();
            f.ShowDialog();

            ApplyOptions();
        }

        internal void ShowThemes()
        {
            ThemesForm f = new ThemesForm(this);
            f.ShowDialog(this);
        }

        internal void ShowAboutDialog()
        {
            AboutForm f = new AboutForm();
            f.ShowDialog(this);
        }

        internal void LoadPlaces()
        {
            placesMenu.Items.Clear();

            foreach (string x in Options.CurrentOptions.Places)
            {
                ToolStripItem i = new ToolStripMenuItem();

                if (placesMenu.Items.Count <= 0)
                {
                    i.Text = "1) " + x;
                }
                else
                {
                    i.Text = string.Format("{0}) {1}", placesMenu.Items.Count + 1, x);
                }

                i.ForeColor = Color.White;
                i.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

                i.Click += Place_Click;

                placesMenu.Items.Add(i);
            }
        }

        private void Place_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            if (Directory.Exists(item.Text.Substring(3)))
            {
                AddTab(item.Text.Substring(3));
            }
            else
            {
                MessageBox.Show("This directory no longer exists!", "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public MainForm(string customDirectory = null)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            Options.ApplyTheme(this);

            helperMenu.Renderer = new ToolStripRendererMaterial();
            placesMenu.Renderer = new ToolStripRendererMaterial();

            this.KeyPreview = true;

            LoadPlaces();

            if (IsAdmin)
            {
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
                this.Text = "Administrator: MoonShell " + Program.GetCurrentVersion();
            }
            else
            {
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                this.Text = "MoonShell " + Program.GetCurrentVersion();
            }

            if (!string.IsNullOrEmpty(customDirectory))
            {
                WorkingDirectory = customDirectory;
            }
            else
            {
                if (!string.IsNullOrEmpty(Options.CurrentOptions.StartingDirectory))
                {
                    if (Directory.Exists(Options.CurrentOptions.StartingDirectory))
                    {
                        WorkingDirectory = Options.CurrentOptions.StartingDirectory;
                    }
                    else
                    {
                        MessageBox.Show("Custom working directory no longer exists!\nDefault directory will be set!", "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Options.CurrentOptions.StartingDirectory = string.Empty;
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RestoreWindowState();
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
            
            if (_currentTab != null)
            {
                _currentTab.Focus();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ShowThemes();
            
            if (_currentTab != null)
            {
                _currentTab.Focus();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowOptions();
            WorkingDirectory = Options.CurrentOptions.StartingDirectory;

            if (_currentTab != null)
            {
                _currentTab.Focus();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowState();
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

            if (_currentTab != null)
            {
                try
                {
                    if (Directory.Exists(sa[0]))
                    {
                        _currentTab.InternalRichTextBox.AppendText("\"" + sa[0] + "\"");
                    }
                    if (File.Exists(sa[0]))
                    {
                        _currentTab.InternalRichTextBox.AppendText(sa[0]);
                    }
                }
                catch { }
            }
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

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentTab = tabConsoles.SelectedTab.Controls.OfType<ConsoleControl>().FirstOrDefault();

            if (_currentTab != null)
            {
                _currentTab.Focus();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            placesMenu.Show(button8, button8.Location);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ExportLog();
        }
    }
}
