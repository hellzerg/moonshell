using System.Windows.Forms;

namespace MoonShell
{
    public class KeyMapping
    {
        public KeyMapping() { }

        public KeyMapping(bool control, bool alt, bool shift, Keys keyCode, string sendKeysMapping, string streamMapping)
        {
            IsControlPressed = control;
            IsAltPressed = alt;
            IsShiftPressed = shift;
            KeyCode = keyCode;
            SendKeysMapping = sendKeysMapping;
            StreamMapping = streamMapping;
        }

        public bool IsControlPressed
        {
            get;
            set;
        }

        public bool IsAltPressed
        {
            get;
            set;
        }

        public bool IsShiftPressed
        {
            get;
            set;
        }

        public Keys KeyCode
        {
            get;
            set;
        }

        public string SendKeysMapping
        {
            get;
            set;
        }

        public string StreamMapping
        {
            get;
            set;
        }
    }
}