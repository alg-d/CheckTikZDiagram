using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    public class CheckResult
    {
        public int Line { get; }
        
        public string Message { get; }

        public bool IsError { get; }

        public bool DisplayFlag { get; }

        public CheckResult(int line)
            : this(line, "", false, false)
        {
        }

        public CheckResult(int line, string message, bool error, bool display)
        {
            Line = line;
            Message = message;
            IsError = error;
            DisplayFlag = display;
        }
    }
}
