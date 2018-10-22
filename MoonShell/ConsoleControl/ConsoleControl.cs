using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

namespace MoonShell
{
    public delegate void ConsoleEventHandler(object sender, ConsoleEventArgs args);

    public partial class ConsoleControl : UserControl
    {
        MainForm _mainForm;

        // Stores the current line.
        // Used while navigating the previous commands list.
        // static int? currentLine = null;

        // Stores all the used commands.
        // public static ArrayList History = new ArrayList();

        // Stores the last used command.
        string _lastCommand = string.Empty;

        // Helper for adding a place
        bool _addPlace = false;

        public ConsoleControl(MainForm mainForm)
        {
            InitializeComponent();

            _mainForm = mainForm;

            ShowDiagnostics = false;
            
            ForeColor = Color.Lime;
            
            BackColor = Color.Black;
            
            ErrorColor = Color.Tomato;
            
            DiagnosticsColor = Color.Yellow;
            
            IsInputEnabled = true;
            
            SendKeyboardCommandsToProcess = false;
            
            InitializeKeyMappings();
            
            processInterface.OnProcessOutput += processInterace_OnProcessOutput;
            processInterface.OnProcessError += processInterace_OnProcessError;
            processInterface.OnProcessInput += processInterace_OnProcessInput;
            processInterface.OnProcessExit += processInterace_OnProcessExit;

            richTextBoxConsole.KeyDown += richTextBoxConsole_KeyDown;
        }

        private void AddPlace()
        {
            if (_addPlace)
            {
                string place = InternalRichTextBox.Lines.Last().Replace(">", string.Empty).Trim();

                if (!string.IsNullOrEmpty(place))
                {
                    Options.CurrentOptions.Places.Add(place);
                    _mainForm.LoadPlaces();

                    _addPlace = false;
                }
            }
        }

        void processInterace_OnProcessError(object sender, ProcessEventArgs args)
        {
            WriteOutput(args.Content, ErrorColor);
            
            FireConsoleOutputEvent(args.Content);
        }
        
        void processInterace_OnProcessOutput(object sender, ProcessEventArgs args)
        {
            WriteOutput(args.Content, ForeColor);
            FireConsoleOutputEvent(args.Content);

            AddPlace();
        }

        void processInterace_OnProcessInput(object sender, ProcessEventArgs args)
        {
            throw new NotImplementedException();
        }

        void processInterace_OnProcessExit(object sender, ProcessEventArgs args)
        {
            if (ShowDiagnostics)
            {
                WriteOutput(Environment.NewLine + processInterface.ProcessFileName + " exited.", DiagnosticsColor);
            }
            
            if (!this.IsHandleCreated)
                return;
          
            Invoke((Action)(() =>
            {
                richTextBoxConsole.ReadOnly = true;
            }));
        }

        private void InitializeKeyMappings()
        {
            //  Map 'tab'.
            keyMappings.Add(new KeyMapping(false, false, false, Keys.Tab, "{TAB}", "\t"));

            //  Map 'Ctrl-C'.
            keyMappings.Add(new KeyMapping(true, false, false, Keys.C, "^(c)", "\x03\r\n"));
        }

        void richTextBoxConsole_KeyDown(object sender, KeyEventArgs e)
        {
            _addPlace = false;

            if (SendKeyboardCommandsToProcess && IsProcessRunning)
            {
                var mappings = from k in keyMappings
                               where
                               (k.KeyCode == e.KeyCode &&
                               k.IsAltPressed == e.Alt &&
                               k.IsControlPressed == e.Control &&
                               k.IsShiftPressed == e.Shift)
                               select k;

                //foreach (var mapping in mappings)
                //{
                //    SendKeysEx.SendKeys(CurrentProcessHwnd, mapping.SendKeysMapping);
                //    inputWriter.WriteLine(mapping.StreamMapping);
                //    WriteInput("\x3", ForeColor, false);
                //}

                if (mappings.Any())
                {
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            if ((richTextBoxConsole.SelectionStart <= inputStart) && e.KeyCode == Keys.Back) e.SuppressKeyPress = true;

            if (richTextBoxConsole.SelectionStart < inputStart)
            {
                //  Allow arrows and Ctrl-C.
                if (!(e.KeyCode == Keys.Left ||
                    e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.Up ||
                    e.KeyCode == Keys.Down ||
                    (e.KeyCode == Keys.C && e.Control)))
                {
                    e.SuppressKeyPress = true;
                }
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                ((RichTextBox)sender).Paste(DataFormats.GetFormat("Text"));
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Return)
            {
                string input = string.Empty;

                try
                {
                    input = richTextBoxConsole.Text.Substring(inputStart, (richTextBoxConsole.SelectionStart) - inputStart).Trim();
                }
                catch
                {
                    e.SuppressKeyPress = true;
                    return;
                }

                // Custom command processing

                //if (!string.IsNullOrEmpty(input))
                //{
                //    History.Add(input);
                //}

                if (input == "!!")
                {
                    if (!string.IsNullOrEmpty(_lastCommand))
                    {
                        input = _lastCommand;
                    }
                    else
                    {
                        input = string.Empty;
                    }
                }

                if (input == "addplace")
                {
                    input = string.Empty;
                    _addPlace = true;
                }

                if (input.StartsWith("removeplace "))
                {
                    int i = -1;
                    int.TryParse(input.Replace("removeplace ", string.Empty), out i);

                    input = string.Empty;

                    if (i > 0 && Options.CurrentOptions.Places.Count > 0)
                    {
                        try
                        {
                            Options.CurrentOptions.Places.RemoveAt(i - 1);
                            _mainForm.LoadPlaces();
                        }
                        catch { } 
                    }
                }

                if (input == "verifyplaces")
                {
                    input = string.Empty;
                    Options.VerifyPlaces();
                    _mainForm.LoadPlaces();
                }

                if (input == "clearplaces")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Places.Clear();
                    _mainForm.LoadPlaces();
                }

                if (input == "exportlog")
                {
                    input = string.Empty;
                    _mainForm.ExportLog();
                }

                if (input == "ls")
                {
                    input = "dir";
                }

                if (input == "exit" || input == "quit")
                {
                    input = string.Empty;
                    Application.Exit();
                }

                if (input == "clear" || input == "cls")
                {
                    ClearOutput();
                    return;
                }

                if (input == "add")
                {
                    input = string.Empty;
                    _mainForm.AddTab();
                }

                if (input == "remove")
                {
                    input = string.Empty;
                    _mainForm.RemoveTab();
                }

                if (input == "new")
                {
                    input = string.Empty;
                    _mainForm.NewWindow();
                }

                if (input == "admin")
                {
                    input = string.Empty;
                    _mainForm.NewWindowAdmin();
                }

                if (input == "options")
                {
                    input = string.Empty;
                    _mainForm.ShowOptions();
                }

                if (input == "themes")
                {
                    input = string.Empty;
                    _mainForm.ShowThemes();
                }

                if (input == "help" || input == "about")
                {
                    input = string.Empty;
                    _mainForm.ShowAboutDialog();
                }

                if (input == "theme zerg")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Color = Theme.Zerg;
                    Options.ApplyTheme(_mainForm);
                }

                if (input == "theme ocean")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Color = Theme.Ocean;
                    Options.ApplyTheme(_mainForm);
                }

                if (input == "theme caramel")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Color = Theme.Caramel;
                    Options.ApplyTheme(_mainForm);
                }

                if (input == "theme lime")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Color = Theme.Lime;
                    Options.ApplyTheme(_mainForm);
                }

                if (input == "theme magma")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Color = Theme.Magma;
                    Options.ApplyTheme(_mainForm);
                }

                if (input == "theme minimal")
                {
                    input = string.Empty;
                    Options.CurrentOptions.Color = Theme.Minimal;
                    Options.ApplyTheme(_mainForm);
                }

                WriteInput(input, ForeColor, false);

                if (input != "!!") _lastCommand = input;
            }
        }

        public void WriteOutput(string output, Color color)
        {
            if (string.IsNullOrEmpty(output)) return;

            if (string.IsNullOrEmpty(lastInput) == false && 
                (output == lastInput || output.Replace("\r\n", "") == lastInput))
                return;
                
            if (!this.IsHandleCreated)
                return;

            Invoke((Action)(() =>
            {
                richTextBoxConsole.SelectionColor = color;
                richTextBoxConsole.SelectedText += output;
                inputStart = richTextBoxConsole.SelectionStart;
            }));
        }

        public void ClearOutput()
        {
            richTextBoxConsole.Clear();
            WriteInput(string.Empty, ForeColor, false);
        }

        public void WriteInput(string input, Color color, bool echo)
        {
            Invoke((Action)(() =>
            {
                if (echo)
                {
                    richTextBoxConsole.SelectionColor = color;
                    richTextBoxConsole.SelectedText += input;
                    inputStart = richTextBoxConsole.SelectionStart;
                }

                lastInput = input;

                processInterface.WriteInput(input);
                FireConsoleInputEvent(input);
            }));
        }

        public void StartProcess(string fileName, string arguments)
        {
            if (ShowDiagnostics)
            {
                WriteOutput("Preparing to run " + fileName, DiagnosticsColor);
                if (!string.IsNullOrEmpty(arguments))
                    WriteOutput(" with arguments " + arguments + "." + Environment.NewLine, DiagnosticsColor);
                else
                    WriteOutput("." + Environment.NewLine, DiagnosticsColor);
            }

            processInterface.StartProcess(fileName, arguments);

            if (IsInputEnabled)
                richTextBoxConsole.ReadOnly = false;
        }

        public void StopProcess()
        {
            processInterface.StopProcess();
        }
     
        private void FireConsoleOutputEvent(string content)
        {
            var theEvent = OnConsoleOutput;
            if (theEvent != null)
                theEvent(this, new ConsoleEventArgs(content));
        }

        private void FireConsoleInputEvent(string content)
        {
            var theEvent = OnConsoleInput;
            if (theEvent != null)
                theEvent(this, new ConsoleEventArgs(content));
        }
        
        private readonly ProcessInterface processInterface = new ProcessInterface();
        
        int inputStart = -1;
        
        private bool isInputEnabled = true;
        
        private string lastInput;
        
        private List<KeyMapping> keyMappings = new List<KeyMapping>();
        
        public event ConsoleEventHandler OnConsoleOutput;
        
        public event ConsoleEventHandler OnConsoleInput;
        
        [Category("Console Control"), Description("Show diagnostic information, such as exceptions.")]
        public bool ShowDiagnostics
        {
            get;
            set;
        }

        [Category("Console Control"), Description("If true, the user can key in input.")]
        public bool IsInputEnabled
        {
            get { return isInputEnabled; }
            set
            {
                isInputEnabled = value;
                if (IsProcessRunning)
                    richTextBoxConsole.ReadOnly = !value;
            }
        }

        [Category("Console Control"), Description("If true, special keyboard commands like Ctrl-C and tab are sent to the process.")]
        public bool SendKeyboardCommandsToProcess
        {
            get;
            set;
        }

        [Category("Console Control"), Description("Sets the text color for errors.")]
        public Color ErrorColor
        {
            get;
            set;
        }

        [Category("Console Control"), Description("Sets the text color for diagnostics.")]
        public Color DiagnosticsColor
        {
            get;
            set;
        }

        [Browsable(false)]
        public bool IsProcessRunning
        {
            get { return processInterface.IsProcessRunning; }
        }

        [Browsable(false)]
        public RichTextBox InternalRichTextBox
        {
            get { return richTextBoxConsole; }
        }

        [Browsable(false)]
        public ProcessInterface ProcessInterface
        {
            get { return processInterface; }
        }

        [Browsable(false)]
        public List<KeyMapping> KeyMappings
        {
            get { return keyMappings; }
        }

        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                richTextBoxConsole.Font = value;
            }
        }

        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                richTextBoxConsole.BackColor = value;
            }
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }

            set
            {
                base.ForeColor = value;
                richTextBoxConsole.ForeColor = value;
            }
        }
    }

    public class Resfinder { }
}
