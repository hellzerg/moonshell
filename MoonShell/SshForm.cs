using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Renci.SshNet;

namespace MoonShell
{
    public partial class SshForm : Form
    {
        string _logsDirectory = Application.StartupPath + "\\Logs\\";

        SshClient _connectedClient;
        ShellStream _shellStream;

        public SshForm(SshClient client)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            Options.ApplyTheme(this);
            helperMenu.Renderer = new ToolStripRendererMaterial();

            _connectedClient = client;

            txtConsole.BackColor = Options.CurrentOptions.BackgroundColor;
            txtConsole.ForeColor = Options.CurrentOptions.ForegroundColor;
            txtConsole.Font = Options.CurrentOptions.Font;

            this.Text = string.Format("{0}@{1} : {2}", _connectedClient.ConnectionInfo.Username, _connectedClient.ConnectionInfo.Host, _connectedClient.ConnectionInfo.Port);

            _shellStream = _connectedClient.CreateShellStream("MoonShell_Session", 80, 60, 800, 600, 65536);
            _shellStream.DataReceived += _shellStream_DataReceived;
        }

        private void _shellStream_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
        {
            if (_shellStream != null)
            {
                if (_shellStream.DataAvailable)
                {
                    txtConsole.AppendText(Encoding.UTF8.GetString(e.Data));
                    txtConsole.ScrollToCaret();
                }
            }
        }   

        private void SshForm_Load(object sender, EventArgs e)
        {
            txtInput.Select();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(txtInput.Text))
                {
                    if (txtInput.Text == "clear")
                    {
                        txtConsole.Clear();
                        txtInput.Clear();
                        return;
                    }

                    //if (txtInput.Text == "exit")
                    //{
                    //    this.Close();
                    //}

                    if (_shellStream != null)
                    {
                        _shellStream.WriteLine(txtInput.Text);
                        _shellStream.Flush();
                    }
                }
            }
        }

        private void SshForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_connectedClient != null)
                {
                    _connectedClient.Disconnect();
                    _connectedClient.Dispose();
                }
            }
            catch { }
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtConsole.SelectedText);
            }
            catch { }
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtConsole.Text);
            }
            catch { }
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            try
            {
                txtInput.AppendText(Clipboard.GetText());
            }
            catch { }
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            txtConsole.Clear();
        }

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }

            string fileName = string.Format("{0}-{1}-{2} - {3}.{4}.{5}.log", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            string title = "SSH Session - " + this.Text + Environment.NewLine + Environment.NewLine;

            try
            {
                File.WriteAllText(_logsDirectory + fileName, title + txtConsole.Text, Encoding.UTF8);
                MessageBox.Show("Log file created in Logs folder!", "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log file couldn't be created:\n\n" + ex.Message, "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
