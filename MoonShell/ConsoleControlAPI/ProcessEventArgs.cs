using System;

namespace MoonShell
{
    public class ProcessEventArgs : EventArgs
    {
        public ProcessEventArgs() { }

        public ProcessEventArgs(string content)
        {
            Content = content;
        }

        public ProcessEventArgs(int code)
        {
            Code = code;
        }

        public ProcessEventArgs(string content, int code)
        {
            Content = content;
            Code = code;
        }

        public string Content { get; private set; }

        public int? Code { get; private set; }
    }
}