using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MoonShell
{
    static class Program
    {
        /* VERSION PROPERTIES */
        /* DO NOT LEAVE THEM EMPTY */

        // Enter current version here
        internal readonly static float Major = 2;
        internal readonly static float Minor = 2;

        /* END OF VERSION PROPERTIES */

        internal static string GetCurrentVersionTostring()
        {
            return Major.ToString() + "." + Minor.ToString();
        }

        internal static float GetCurrentVersion()
        {
            return float.Parse(GetCurrentVersionTostring());
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string resource = "MoonShell.Newtonsoft.Json.dll";
            EmbeddedAssembly.Load(resource, resource.Replace("MoonShell.", string.Empty));

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Options.LoadSettings();

            //if (args.Length > 0)
            //{
            //    for (int i = 0; i < args.Length; i++)
            //    {
            //        args[i] = args[i].Trim();
            //    }
            //}

            if (args.Length == 0)
            {
                Application.Run(new MainForm());
            }
            else if (args.Length == 1)
            {
                args[0] = args[0].Trim();

                if (!string.IsNullOrEmpty(args[0]))
                {
                    if (Directory.Exists(args[0]))
                    {
                        Application.Run(new MainForm(args[0]));
                    }
                    else
                    {
                        MessageBox.Show("This directory does not exist!", "MoonShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
