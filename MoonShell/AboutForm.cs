using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoonShell
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            Options.ApplyTheme(this);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void About_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            infoBox.AppendText("Type 'add' or 'remove' to add or remove tabs\n");
            infoBox.AppendText("Press CTRL+T or CTRL+W to add or remove tabs\n");
            infoBox.AppendText("Type 'clear' or 'cls' to clear current tab\n");
            infoBox.AppendText("Type '!!' to execute the last command\n");
            infoBox.AppendText("Type 'ssh connect' to configure your connection\n");
            infoBox.AppendText("Type 'ssh <username>@<host>' also works\n");
            infoBox.AppendText("Type 'addplace' to bookmark current directory\n");
            infoBox.AppendText("Type 'removeplace <ID>' to remove a specific place\n");
            infoBox.AppendText("Type 'verifyplaces' to remove invalid places\n");
            infoBox.AppendText("Type 'clearplaces' to remove all places\n");
            infoBox.AppendText("Type 'exportlog' to export a log file\n");
            infoBox.AppendText("Type 'options' to show options\n");
            infoBox.AppendText("Type 'themes' to show available themes\n");
            infoBox.AppendText("Type 'theme <theme>' to change theme\n");
            infoBox.AppendText("Type 'help' or 'about' to show available commands\n");
            infoBox.AppendText("Type 'new' to launch a new instance\n");
            infoBox.AppendText("Type 'admin' to launch a new privileged instance\n");
            infoBox.AppendText("Type 'exit' or 'quit' to close MoonShell");

            t1.Interval = 50;
            t2.Interval = 50;

            t1.Start();
        }

        private void t1_Tick(object sender, EventArgs e)
        {
            const string s0 = "";
            const string s1 = "M";
            const string s2 = "Mo";
            const string s3 = "Moo";
            const string s4 = "Moon";
            const string s5 = "MoonS";
            const string s6 = "MoonSh";
            const string s7 = "MoonShe";
            const string s8 = "MoonShel";
            const string s9 = "MoonShell";

            switch (l1.Text)
            {
                case s0:
                    l1.Text = s1;
                    break;
                case s1:
                    l1.Text = s2;
                    break;
                case s2:
                    l1.Text = s3;
                    break;
                case s3:
                    l1.Text = s4;
                    break;
                case s4:
                    l1.Text = s5;
                    break;
                case s5:
                    l1.Text = s6;
                    break;
                case s6:
                    l1.Text = s7;
                    
                    break;
                case s7:
                    l1.Text = s8;
                    break;
                case s8:
                    l1.Text = s9;
                    t1.Stop();
                    t2.Start();
                    break;
                case s9:
                    l1.Text = s0;
                    break;
            }
        }

        private void t2_Tick(object sender, EventArgs e)
        {
            string s0 = "";
            string s1 = "d";
            string s2 = "de";
            string s3 = "dea";
            string s4 = "dead";
            string s5 = "deadm";
            string s6 = "deadmo";
            string s7 = "deadmoo";
            string s8 = "deadmoon";
            string s9 = "deadmoon © ";
            string s10 = "deadmoon © 2";
            string s11 = "deadmoon © 20";
            string s12 = "deadmoon © 201";
            string s13 = "deadmoon © 2017";

            switch (l2.Text)
            {
                case "":
                    l2.Text = s1;
                    break;
                case "d":
                    l2.Text = s2;
                    break;
                case "de":
                    l2.Text = s3;
                    break;
                case "dea":
                    l2.Text = s4;
                    break;
                case "dead":
                    l2.Text = s5;
                    break;
                case "deadm":
                    l2.Text = s6;
                    break;
                case "deadmo":
                    l2.Text = s7;
                    break;
                case "deadmoo":
                    l2.Text = s8;
                    break;
                case "deadmoon":
                    l2.Text = s9;
                    break;
                case "deadmoon © ":
                    l2.Text = s10;
                    break;
                case "deadmoon © 2":
                    l2.Text = s11;
                    break;
                case "deadmoon © 20":
                    l2.Text = s12;
                    break;
                case "deadmoon © 201":
                    l2.Text = s13;
                    t2.Stop();
                    //t1.Start();
                    break;
                case "deadmoon © 2017":
                    l2.Text = s0;
                    break;
            }
        }

        private void l2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://carcinos.eu/deadmoon");
        }
    }
}
