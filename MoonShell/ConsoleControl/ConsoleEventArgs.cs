using System;

namespace MoonShell
{
  public class ConsoleEventArgs : EventArgs
  {
    
    public ConsoleEventArgs() { }

    public ConsoleEventArgs(string content)
    {
      Content = content;
    }

    public string Content
    {
      get;
      private set;
    }
  }
}
