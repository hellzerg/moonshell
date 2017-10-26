using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoonShell
{
    public class SettingsJson
    {
        public Theme Color { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color ErrorColor { get; set; }
        public Font Font { get; set; }

        //public int X { get; set; }
        //public int Y { get; set; }

        //public ArrayList History { get; set; }
    }

    public static class Options
    {
        internal static Color ForegroundColor = Color.MediumOrchid;
        internal static Color ForegroundAccentColor = Color.DarkOrchid;

        readonly static string _settingsFile = Application.StartupPath + "\\MoonShell.json";
        internal static SettingsJson CurrentOptions = new SettingsJson();
        internal readonly static string ThemeFlag = "themeable";

        // use this to determine if changes have been made
        static SettingsJson _flag = new SettingsJson();

        internal static void ApplyTheme(Form f)
        {
            switch (CurrentOptions.Color)
            {
                case Theme.Caramel:
                    SetTheme(f, Color.DarkOrange, Color.Chocolate);
                    break;
                case Theme.Lime:
                    SetTheme(f, Color.LimeGreen, Color.ForestGreen);
                    break;
                case Theme.Magma:
                    SetTheme(f, Color.Tomato, Color.Red);
                    break;
                case Theme.Minimal:
                    SetTheme(f, Color.Gray, Color.DimGray);
                    break;
                case Theme.Ocean:
                    SetTheme(f, Color.DodgerBlue, Color.RoyalBlue);
                    break;
                case Theme.Zerg:
                    SetTheme(f, Color.MediumOrchid, Color.DarkOrchid);
                    break;
            }
        }

        private static void SetTheme(Form f, Color c1, Color c2)
        {
            ForegroundColor = c1;
            ForegroundAccentColor = c2;

            Utilities.GetSelfAndChildrenRecursive(f).OfType<Button>().ToList().ForEach(b => b.BackColor = c1);
            Utilities.GetSelfAndChildrenRecursive(f).OfType<Button>().ToList().ForEach(b => b.FlatAppearance.BorderColor = c1);
            Utilities.GetSelfAndChildrenRecursive(f).OfType<Button>().ToList().ForEach(b => b.FlatAppearance.MouseDownBackColor = c2);
            Utilities.GetSelfAndChildrenRecursive(f).OfType<Button>().ToList().ForEach(b => b.FlatAppearance.MouseOverBackColor = c2);

            foreach (Label tmp in Utilities.GetSelfAndChildrenRecursive(f).OfType<Label>().ToList())
            {
                if ((string)tmp.Tag == ThemeFlag)
                {
                    tmp.ForeColor = c1;
                }
            }
            foreach (LinkLabel tmp in Utilities.GetSelfAndChildrenRecursive(f).OfType<LinkLabel>().ToList())
            {
                if ((string)tmp.Tag == ThemeFlag)
                {
                    tmp.LinkColor = c1;
                    tmp.VisitedLinkColor = c1;
                    tmp.ActiveLinkColor = c2;
                }
            }
            foreach (CheckBox tmp in Utilities.GetSelfAndChildrenRecursive(f).OfType<CheckBox>().ToList())
            {
                if ((string)tmp.Tag == ThemeFlag)
                {
                    tmp.ForeColor = c1;
                }
            }
        }

        internal static void SaveSettings()
        {
            if (File.Exists(_settingsFile))
            {
                File.WriteAllText(_settingsFile, string.Empty);

                if ((_flag.BackgroundColor != CurrentOptions.BackgroundColor) || (_flag.ForegroundColor != CurrentOptions.ForegroundColor) || (_flag.Font != CurrentOptions.Font) || _flag.Color != CurrentOptions.Color || CurrentOptions.ErrorColor != _flag.ErrorColor)
                {
                    //CurrentOptions.History = ConsoleControl.History;

                    using (FileStream fs = File.Open(_settingsFile, FileMode.OpenOrCreate))
                    using (StreamWriter sw = new StreamWriter(fs))
                    using (JsonWriter jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;

                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(jw, CurrentOptions);
                    }
                }
            }
        }

        internal static void LoadSettings()
        {
            if (!File.Exists(_settingsFile))
            {
                // default settings
                CurrentOptions.Font = new Font("Consolas", 10.8F);
                CurrentOptions.BackgroundColor = Color.Black;
                CurrentOptions.ForegroundColor = Color.Lime;
                CurrentOptions.ErrorColor = Color.Tomato;
                CurrentOptions.Color = Theme.Zerg;

                //if (CurrentOptions.History != null)
                //{
                //    CurrentOptions.History = ConsoleControl.History;
                //}

                using (FileStream fs = File.Open(_settingsFile, FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, CurrentOptions);
                }
            }
            else
            {
                CurrentOptions = JsonConvert.DeserializeObject<SettingsJson>(File.ReadAllText(_settingsFile));

                //if (CurrentOptions.History != null)
                //{
                //    ConsoleControl.History = CurrentOptions.History;
                //}

                // initialize flag with default settings
                _flag.Font = new Font("Consolas", 10.8F);
                _flag.BackgroundColor = Color.Black;
                _flag.ForegroundColor = Color.Lime;
                _flag.ErrorColor = Color.Tomato;
                _flag.Color = Theme.Zerg;
            }
        }
    }
}
