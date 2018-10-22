using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace MoonShell
{
    public delegate void ProcessEventHanlder(object sender, ProcessEventArgs args);

    public class ProcessInterface
    {
        public ProcessInterface()
        {
            outputWorker.WorkerReportsProgress = true;
            outputWorker.WorkerSupportsCancellation = true;
            outputWorker.DoWork += outputWorker_DoWork;
            outputWorker.ProgressChanged += outputWorker_ProgressChanged;

            errorWorker.WorkerReportsProgress = true;
            errorWorker.WorkerSupportsCancellation = true;
            errorWorker.DoWork += errorWorker_DoWork;
            errorWorker.ProgressChanged += errorWorker_ProgressChanged;
        }

        void outputWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                FireProcessOutputEvent(e.UserState as string);
            }
        }

        void outputWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (outputWorker.CancellationPending == false)
            {
                int count;
                var buffer = new char[1024];
                do
                {
                    var builder = new StringBuilder();
                    count = outputReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    outputWorker.ReportProgress(0, builder.ToString());
                } while (count > 0);

                System.Threading.Thread.Sleep(200);
            }
        }

        void errorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                FireProcessErrorEvent(e.UserState as string);
            }
        }

        void errorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (errorWorker.CancellationPending == false)
            {
                int count;
                var buffer = new char[1024];
                do
                {
                    var builder = new StringBuilder();
                    count = errorReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    errorWorker.ReportProgress(0, builder.ToString());
                } while (count > 0);

                System.Threading.Thread.Sleep(200);
            }
        }

        public void StartProcess(string fileName, string arguments)
        {
            var processStartInfo = new ProcessStartInfo(fileName, null);

            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = arguments;
            processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            processStartInfo.StandardOutputEncoding = Encoding.UTF8;

            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;

            process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = processStartInfo;
            process.Exited += currentProcess_Exited;

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to start process " + fileName + " with arguments '" + arguments + "'");
                Trace.WriteLine(e.ToString());
                return;
            }

            processFileName = fileName;
            processArguments = arguments;

            inputWriter = process.StandardInput;
            outputReader = TextReader.Synchronized(process.StandardOutput);
            errorReader = TextReader.Synchronized(process.StandardError);

            outputWorker.RunWorkerAsync();
            errorWorker.RunWorkerAsync();
        }

        public void StopProcess()
        {
            if (IsProcessRunning == false)
                return;

            process.Kill();
        }

        void currentProcess_Exited(object sender, EventArgs e)
        {
            FireProcessExitEvent(process.ExitCode);

            outputWorker.CancelAsync();
            errorWorker.CancelAsync();
            inputWriter = null;
            outputReader = null;
            errorReader = null;
            process = null;
            processFileName = null;
            processArguments = null;
        }

        private void FireProcessOutputEvent(string content)
        {
            var theEvent = OnProcessOutput;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(content));
        }
        
        private void FireProcessErrorEvent(string content)
        {
            var theEvent = OnProcessError;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(content));
        }

        private void FireProcessInputEvent(string content)
        {
            var theEvent = OnProcessInput;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(content));
        }
        
        private void FireProcessExitEvent(int code)
        {
            var theEvent = OnProcessExit;
            if (theEvent != null)
                theEvent(this, new ProcessEventArgs(code));
        }
        
        public void WriteInput(string input)
        {
            if (IsProcessRunning)
            {
                inputWriter.WriteLine(input);
                inputWriter.Flush();
            }
        }
        
        private Process process;
        
        private StreamWriter inputWriter;
        
        private TextReader outputReader;
        
        private TextReader errorReader;
        
        private BackgroundWorker outputWorker = new BackgroundWorker();
        
        private BackgroundWorker errorWorker = new BackgroundWorker();

        private string processFileName;

        private string processArguments;
 
        public event ProcessEventHanlder OnProcessOutput;

        public event ProcessEventHanlder OnProcessError;

        public event ProcessEventHanlder OnProcessInput;

        public event ProcessEventHanlder OnProcessExit;
  
        public bool IsProcessRunning
        {
            get
            {
                try
                {
                    return (process != null && process.HasExited == false);
                }
                catch
                {
                    return false;
                }
            }
        }

        public Process Process
        {
            get { return process; }
        }

        public string ProcessFileName
        {
            get { return processFileName; }
        }

        public string ProcessArguments
        {
            get { return processArguments; }
        }
    }
}
