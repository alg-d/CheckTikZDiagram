using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckTikZDiagram
{
    /// <summary>
    /// TeXにおけるTokenを表す。
    /// </summary>
    public class Token
    {
        /// <summary>
        /// 空Token
        /// </summary>
        public static Token Empty { get; } = new Token("", "");

        /// <summary>
        /// 左小括弧 (
        /// </summary>
        public static Token LeftParenthesis { get; } = new Token("(", "(");

        /// <summary>
        /// 右小括弧 )
        /// </summary>
        public static Token RightParenthesis { get; } = new Token(")", ")");

        /// <summary>
        /// 左中括弧 {
        /// </summary>
        public static Token LeftCurlyBracket { get; } = new Token("{", "{");

        /// <summary>
        /// 右中括弧 }
        /// </summary>
        public static Token RightCurlyBracket { get; } = new Token("}", "}");

        /// <summary>
        /// 曲折アクセント ^
        /// </summary>
        public static Token Circumflex { get; } = new Token("^", "^");

        /// <summary>
        /// 下線符号 _
        /// </summary>
        public static Token LowLine { get; } = new Token("_", "_");

        /// <summary>
        /// Tokenの値
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Tokenの元となった文字列(空の場合はプログラム側で生成したToken( { と } のこと))
        /// </summary>
        public string Origin { get; }

        public bool IsEmpty => this.Value.IsNullOrEmpty();

        public Token(string value, string origin)
        {
            Value = value;
            Origin = origin;
        }

        public bool IsOpenBracket()
        {
            return Config.Instance.OpenBrackets.Contains(this.Value);
        }

        public bool IsCloseBracket()
        {
            return Config.Instance.CloseBrackets.Contains(this.Value);
        }

        public bool IsSeparator()
        {
            return Config.Instance.Separators.Contains(this.Value);
        }


        public TokenString ToTokenString()
        {
            if (this.IsEmpty)
            {
                return TokenString.Empty;
            }
            else
            {
                return new TokenString(this);
            }
        }

        public override string ToString() => this.Value;
    }
}
