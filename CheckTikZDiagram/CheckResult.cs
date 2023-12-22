using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 検証結果を表すクラス
    /// </summary>
    public class CheckResult
    {
        /// <summary>
        /// 検証対象の行番号
        /// </summary>
        public int Line { get; }
        
        public string Message { get; }

        /// <summary>
        /// エラーならtrue
        /// </summary>
        public bool IsError { get; }

        /// <summary>
        /// 画面に表示する場合true
        /// </summary>
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
