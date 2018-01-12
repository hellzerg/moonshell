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
    /// <summary>
    /// The console event handler is used for console events.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ConsoleEventArgs"/> instance containing the event data.</param>
    public delegate void ConsoleEventHandler(object sender, ConsoleEventArgs args);

    /// <summary>
    /// The Console Control allows you to embed a basic console in your application.
    /// </summary>
    public partial class ConsoleControl : UserControl
    {
        // The Main form to handle custom command processing.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleControl"/> class.
        /// </summary>
        public ConsoleControl(MainForm mainForm)
        {
            //  Initialise the component.
            InitializeComponent();

            // Set Main form.
            _mainForm = mainForm;

            //  Show diagnostics disabled by default.
            ShowDiagnostics = false;

            // Fore color is Lime by default.
            ForeColor = Color.Lime;

            // Back color is Black by default.
            BackColor = Color.Black;

            // Error color is Tomato by default.
            ErrorColor = Color.Tomato;

            // Diagnostics color is Yellow by default.
            DiagnosticsColor = Color.Yellow;

            //  Input enabled by default.
            IsInputEnabled = true;

            //  Disable special commands by default.
            SendKeyboardCommandsToProcess = false;

            //  Initialise the keymappings.
            InitializeKeyMappings();

            //  Handle process events.
            processInterface.OnProcessOutput += processInterace_OnProcessOutput;
            processInterface.OnProcessError += processInterace_OnProcessError;
            processInterface.OnProcessInput += processInterace_OnProcessInput;
            processInterface.OnProcessExit += processInterace_OnProcessExit;

            //  Wait for key down messages on the rich text box.
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

        /// <summary>
        /// Handles the OnProcessError event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void processInterace_OnProcessError(object sender, ProcessEventArgs args)
        {
            //  Write the output, in red
            WriteOutput(args.Content, ErrorColor);

            //  Fire the output event.
            FireConsoleOutputEvent(args.Content);
        }

        /// <summary>
        /// Handles the OnProcessOutput event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void processInterace_OnProcessOutput(object sender, ProcessEventArgs args)
        {
            //  Write the output, in white
            WriteOutput(args.Content, ForeColor);

            //  Fire the output event.
            FireConsoleOutputEvent(args.Content);

            AddPlace();
        }

        /// <summary>
        /// Handles the OnProcessInput event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void processInterace_OnProcessInput(object sender, ProcessEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the OnProcessExit event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void processInterace_OnProcessExit(object sender, ProcessEventArgs args)
        {
            //  Are we showing diagnostics?
            if (ShowDiagnostics)
            {
                WriteOutput(Environment.NewLine + processInterface.ProcessFileName + " exited.", DiagnosticsColor);
            }
            
            if (!this.IsHandleCreated)
                return;
            //  Read only again.
            Invoke((Action)(() =>
            {
                richTextBoxConsole.ReadOnly = true;
            }));
        }

        /// <summary>
        /// Initialises the key mappings.
        /// </summary>
        private void InitializeKeyMappings()
        {
            //  Map 'tab'.
            keyMappings.Add(new KeyMapping(false, false, false, Keys.Tab, "{TAB}", "\t"));

            //  Map 'Ctrl-C'.
            keyMappings.Add(new KeyMapping(true, false, false, Keys.C, "^(c)", "\x03\r\n"));
        }

        /// <summary>
        /// Handles the KeyDown event of the richTextBoxConsole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        void richTextBoxConsole_KeyDown(object sender, KeyEventArgs e)
        {
            _addPlace = false;

            //  Are we sending keyboard commands to the process?
            if (SendKeyboardCommandsToProcess && IsProcessRunning)
            {
                //  Get key mappings for this key event?
                var mappings = from k in keyMappings
                               where
                               (k.KeyCode == e.KeyCode &&
                               k.IsAltPressed == e.Alt &&
                               k.IsControlPressed == e.Control &&
                               k.IsShiftPressed == e.Shift)
                               select k;

                //  Go through each mapping, send the message.
                //foreach (var mapping in mappings)
                //{
                //    SendKeysEx.SendKeys(CurrentProcessHwnd, mapping.SendKeysMapping);
                //    inputWriter.WriteLine(mapping.StreamMapping);
                //    WriteInput("\x3", ForeColor, false);
                //}

                //  If we handled a mapping, we're done here.
                if (mappings.Any())
                {
                    e.SuppressKeyPress = true;
                    return;
                }
            }

                //  If we're at the input point and it's backspace, bail.
            if ((richTextBoxConsole.SelectionStart <= inputStart) && e.KeyCode == Keys.Back) e.SuppressKeyPress = true;

            //  Are we in the read-only zone?
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

            // Paste only text (without formatting) when CTRL+V
            if (e.Control && e.KeyCode == Keys.V)
            {
                ((RichTextBox)sender).Paste(DataFormats.GetFormat("Text"));
                e.Handled = true;
            }

            //  Is it the return key?
            if (e.KeyCode == Keys.Return)
            {
                //  Get the input.
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

                if (input == "ssh connect")
                {
                    input = string.Empty;

                    ConnectForm f = new ConnectForm(Options.CurrentOptions.SSHHost, Options.CurrentOptions.SSHUsername, Options.CurrentOptions.SSHPort);
                    f.ShowDialog();
                }

                if (input.StartsWith("ssh "))
                {
                    string[] tmp2 = input.Replace("ssh ", string.Empty).Split('@');
                    input = string.Empty;

                    try
                    {
                        if (!string.IsNullOrEmpty(tmp2[0]) && !string.IsNullOrEmpty(tmp2[1]))
                        {
                            ConnectForm f = new ConnectForm(tmp2[1], tmp2[0]);
                            f.ShowDialog();
                        }
                    }
                    catch { }
                }

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

                //  Write the input (without echoing).
                WriteInput(input, ForeColor, false);

                if (input != "!!") _lastCommand = input;
            }
        }

        /// <summary>
        /// Writes the output to the console control.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="color">The color.</param>
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
                //  Write the output.
                richTextBoxConsole.SelectionColor = color;
                richTextBoxConsole.SelectedText += output;
                inputStart = richTextBoxConsole.SelectionStart;
            }));
        }

        /// <summary>
        /// Clears the output.
        /// </summary>
        public void ClearOutput()
        {
            richTextBoxConsole.Clear();
            WriteInput(string.Empty, ForeColor, false);
        }

        /// <summary>
        /// Writes the input to the console control.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="color">The color.</param>
        /// <param name="echo">if set to <c>true</c> echo the input.</param>
        public void WriteInput(string input, Color color, bool echo)
        {
            Invoke((Action)(() =>
            {
                //  Are we echoing?
                if (echo)
                {
                    richTextBoxConsole.SelectionColor = color;
                    richTextBoxConsole.SelectedText += input;
                    inputStart = richTextBoxConsole.SelectionStart;
                }

                lastInput = input;

                //  Write the input.
                processInterface.WriteInput(input);

                //  Fire the event.
                FireConsoleInputEvent(input);
            }));
        }

        

        /// <summary>
        /// Runs a process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        public void StartProcess(string fileName, string arguments)
        {
            //  Are we showing diagnostics?
            if (ShowDiagnostics)
            {
                WriteOutput("Preparing to run " + fileName, DiagnosticsColor);
                if (!string.IsNullOrEmpty(arguments))
                    WriteOutput(" with arguments " + arguments + "." + Environment.NewLine, DiagnosticsColor);
                else
                    WriteOutput("." + Environment.NewLine, DiagnosticsColor);
            }

            //  Start the process.
            processInterface.StartProcess(fileName, arguments);

            //  If we enable input, make the control not read only.
            if (IsInputEnabled)
                richTextBoxConsole.ReadOnly = false;
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        public void StopProcess()
        {
            //  Stop the interface.
            processInterface.StopProcess();
        }
        
        /// <summary>
        /// Fires the console output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleOutputEvent(string content)
        {
            //  Get the event.
            var theEvent = OnConsoleOutput;
            if (theEvent != null)
                theEvent(this, new ConsoleEventArgs(content));
        }

        /// <summary>
        /// Fires the console input event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleInputEvent(string content)
        {
            //  Get the event.
            var theEvent = OnConsoleInput;
            if (theEvent != null)
                theEvent(this, new ConsoleEventArgs(content));
        }

        /// <summary>
        /// The internal process interface used to interface with the process.
        /// </summary>
        private readonly ProcessInterface processInterface = new ProcessInterface();
        
        /// <summary>
        /// Current position that input starts at.
        /// </summary>
        int inputStart = -1;

        /// <summary>
        /// The is input enabled flag.
        /// </summary>
        private bool isInputEnabled = true;

        /// <summary>
        /// The last input string (used so that we can make sure we don't echo input twice).
        /// </summary>
        private string lastInput;

        /// <summary>
        /// The key mappings.
        /// </summary>
        private List<KeyMapping> keyMappings = new List<KeyMapping>();

        /// <summary>
        /// Occurs when console output is produced.
        /// </summary>
        public event ConsoleEventHandler OnConsoleOutput;

        /// <summary>
        /// Occurs when console input is produced.
        /// </summary>
        public event ConsoleEventHandler OnConsoleInput;

        /// <summary>
        /// Gets or sets a value indicating whether to show diagnostics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if show diagnostics; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("Show diagnostic information, such as exceptions.")]
        public bool ShowDiagnostics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is input enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is input enabled; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether [send keyboard commands to process].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [send keyboard commands to process]; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets a value indicating whether this instance is process running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is process running; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool IsProcessRunning
        {
            get { return processInterface.IsProcessRunning; }
        }

        /// <summary>
        /// Gets the internal rich text box.
        /// </summary>
        [Browsable(false)]
        public RichTextBox InternalRichTextBox
        {
            get { return richTextBoxConsole; }
        }

        /// <summary>
        /// Gets the process interface.
        /// </summary>
        [Browsable(false)]
        public ProcessInterface ProcessInterface
        {
            get { return processInterface; }
        }

        /// <summary>
        /// Gets the key mappings.
        /// </summary>
        [Browsable(false)]
        public List<KeyMapping> KeyMappings
        {
            get { return keyMappings; }
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        /// <returns>The <see cref="T:System.Drawing.Font" /> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont" /> property.</returns>
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
                //  Return the base class font.
                return base.Font;
            }
            set
            {
                //  Set the base class font...
                base.Font = value;

                //  ...and the internal control font.
                richTextBoxConsole.Font = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor" /> property.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Color BackColor
        {
            get
            {
                //  Return the base class background.
                return base.BackColor;
            }
            set
            {
                //  Set the base class background...
                base.BackColor = value;

                //  ...and the internal control background.
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

    /// <summary>
    /// Used to allow us to find resources properly.
    /// </summary>
    public class Resfinder { }
}
