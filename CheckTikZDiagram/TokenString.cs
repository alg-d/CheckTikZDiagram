using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace CheckTikZDiagram
{
    /// <summary>
    /// TeXにおけるTokenの列を表す．
    /// (1)添え字(_ ^)の部分は必ず { } で囲う
    /// (2)改行は削除する
    /// (3)空白系は無視される
    /// </summary>
    public class TokenString : IEquatable<TokenString>
    {
        private readonly string _toOriginalString;
        private readonly ReadOnlyCollection<string> _values;

        public ReadOnlyCollection<Token> Tokens { get; }

        public bool IsEmpty => this.Tokens.Count == 0;

        public static TokenString Empty { get; } = new TokenString();

        private TokenString()
        {
            Tokens = new ReadOnlyCollection<Token>(Array.Empty<Token>());
            _values = new ReadOnlyCollection<string>(Array.Empty<string>());

            _toOriginalString = "";
        }

        public TokenString(Token token)
            : this(new[] { token })
        {
            if (token.IsEmpty) throw new ArgumentException($"{nameof(token)}が空です");
        }

        public TokenString(IList<Token> tokens)
        {
            if (tokens.Count == 0) throw new ArgumentException($"{nameof(tokens)}が空です");

            Tokens = new ReadOnlyCollection<Token>(tokens.Where(x => !x.IsEmpty).ToArray());
            _values = new ReadOnlyCollection<string>(Tokens.Select(x => x.Value).ToArray());

            _toOriginalString = CreateOriginalString();
        }

        private string CreateOriginalString()
        {
            var sb = new StringBuilder();
            var pre = false;

            foreach (var item in Tokens.Select(x => x.Origin))
            {
                if (pre && item.Length == 1)
                {
                    var x = item[0];
                    if (('a' <= x && x <= 'z') || ('A' <= x && x <= 'Z'))
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append(item);
                pre = (item.Trim().Length > 1);
            }

            return sb.ToString();
        }


        public TokenString Add(TokenString other)
        {
            if (other.Tokens.Count == 0)
            {
                return this;
            }

            var otherOrigins = other.Tokens.Select(x => x.Origin).ToArray();

            if (this.Tokens.Count > 0)
            {
                otherOrigins[0] = ' ' + otherOrigins[0];
            }

            var tokens = this._values.Concat(other._values).Zip(
                this.Tokens.Select(x => x.Origin).Concat(otherOrigins),
                (x, y) => new Token(x, y)
            ).ToArray();
            return new TokenString(tokens);
        }

        public TokenString Add(Token other)
        {
            return this.Add(other.ToTokenString());
        }

        public static TokenString Join(string separator, IEnumerable<TokenString> sequence)
        {
            var firstFlag = true;
            var tokens = new List<Token>();

            foreach (var tokenString in sequence)
            {
                if (firstFlag)
                {
                    tokens.AddRange(tokenString.Tokens);
                    firstFlag = false;
                }
                else
                {
                    if (!separator.IsNullOrEmpty())
                    {
                        tokens.Add(new Token(separator, separator));
                    }
                    tokens.AddRange(tokenString.Tokens);
                }
            }

            return new TokenString(tokens);
        }

        public bool Contains(string value)
        {
            return this._values.Any(x => x.Contains(value));
        }

        public bool EndsWith(char value)
        {
            if (this.Tokens.Count == 0)
            {
                return false;
            }

            var x = this.Tokens.Last().Value;
            if (x.Length == 0)
            {
                return false;
            }

            return x.Last() == value;
        }

        public bool Equals(string text)
        {
            return this.Equals(text.ToTokenString());
        }

        public string ToOriginalString() => _toOriginalString;

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals(obj as TokenString);
        }

        public bool Equals([AllowNull] TokenString other)
        {
            if (other == null || this._values.Count != other.Tokens.Count)
            {
                return false;
            }

            for (int i = 0; i < this._values.Count; i++)
            {
                if (this._values[i] != other._values[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = 0x2D2816FE;
            foreach (var x in this._values)
            {
                result = result * 31 + x.GetHashCode();
            }
            return result;
        }

        public override string ToString() => string.Join(" ", this._values);
    }
}
