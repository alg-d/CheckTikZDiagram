using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckTikZDiagram
{
    /// <summary>
    /// TeXにおけるTokenを表す。
    /// </summary>
    public class Token : IEquatable<Token?>
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
        /// Tokenの元となった文字列(Originが空の場合はプログラム側で生成したToken( { と } のこと)になる)
        /// </summary>
        public string Origin { get; }

        /// <summary>
        /// 空Tokenならtrue
        /// </summary>
        public bool IsEmpty => this.Value.IsNullOrEmpty();

        public Token(string value, string origin)
        {
            Value = value;
            Origin = origin;
        }

        /// <summary>
        /// 開き括弧を表すTokenならtrue
        /// </summary>
        public bool IsOpenBracket()
        {
            return Config.Instance.OpenBrackets.Contains(this.Value);
        }

        /// <summary>
        /// 閉じ括弧を表すTokenならtrue
        /// </summary>
        public bool IsCloseBracket()
        {
            return Config.Instance.CloseBrackets.Contains(this.Value);
        }

        /// <summary>
        /// Separatorを表すTokenならtrue
        /// </summary>
        public bool IsSeparator()
        {
            return Config.Instance.Separators.Contains(this.Value);
        }

        /// <summary>
        /// 無視するTokenならtrue
        /// </summary>
        public bool IsIgnored()
        {
            return Config.Instance.IgnoreCommands.Contains(this.Value);
        }

        /// <summary>
        /// このTokenのみを含むTokenStringを生成する
        /// </summary>
        /// <returns></returns>
        public TokenString ToTokenString()
        {
            if (this.IsEmpty)
            {
                return TokenString.Empty;
            }
            else
            {
                return TokenString.Create(this);
            }
        }

        public override string ToString() => this.Value;

        public override bool Equals(object? obj)
        {
            return Equals(obj as Token);
        }

        public bool Equals(Token? other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
