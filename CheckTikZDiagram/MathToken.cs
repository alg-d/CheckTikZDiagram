using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 一つの射or対象を表すクラス。ただ一つのTokenからなる (添え字も括弧も含まない)
    /// </summary>
    public class MathToken : MathObject
    {
        public override string OriginalText { get; }

        public override TokenString Main { get; }

        public override int Length => 1;

        public MathToken(Token token)
        {
            Main = TokenString.Create(token);
            OriginalText = token.Origin.Trim();
        }

        public override MathSequence SetBracket(Token left, Token right)
        {
            if (left.IsEmpty) throw new ArgumentException($"{nameof(left)}が空です");
            if (right.IsEmpty) throw new ArgumentException($"{nameof(right)}が空です");

            return new MathSequence(this, left, right);
        }

        public override MathSequence SetScript(Token supOrSub, Token left, MathObject math, Token right)
        {
            return new MathSequence(this, supOrSub, left, math, right);
        }

        private bool IsVariable()
        {
            var x = this.ToString();
            if (x == "-")
            {
                return true;
            }
            else if (x.StartsWith("#"))
            {
                if (x.Length == 2)
                {
                    return true;
                }
                else if (x.Length == 3 && x[2].AllowedCharacter())
                {
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<string> GetVariables()
        {
            if (this.IsVariable())
            {
                yield return this.ToString();
            }
        }

        public override bool IsCategory() => Config.Instance.Categories.Any(x => this.Main.Tokens.Contains(new Token(x, "")));

        public override bool IsSameType(MathObject other, IDictionary<string, MathObject> parameters)
        {
            if (this.IsVariable())
            {
                var x = this.ToString();
                if (x.Length == 3 && x[2] == '?')
                {
                    return IsSameTypeMain(other, parameters, x.Substring(0, 2));
                }
                else
                {
                    return IsSameTypeMain(other, parameters, x);
                }
            }
            else
            {
                return other is MathToken && this.Main.Equals(other.Main);
            }
        }

        public bool IsSameTypeMain(MathObject other, IDictionary<string, MathObject> parameters, string key)
        {
            if (other is MathToken)
            {
                if (parameters.ContainsKey(key) && !parameters[key].Equals(other))
                {
                    return false;
                }
                else
                {
                    parameters[key] = other;
                }
            }
            else if (other is MathSequence math)
            {
                if (math.Sup == null && math.Sub == null
                    && (math.LeftBracket.Value == "(" || math.LeftBracket.Value == "{"))
                {
                    math = new MathSequence(math.List, math.Separator, math.Main.ToOriginalString());
                }

                if (parameters.ContainsKey(key) && !parameters[key].Equals(math))
                {
                    return false;
                }
                else
                {
                    parameters[key] = math;
                }
            }

            return true;
        }

        public override IEnumerable<MathObject> ApplyParameters(IReadOnlyDictionary<string, MathObject> parameters, bool setNull)
        {
            var key = this.ToString();
            if (this.IsVariable())
            {
                if (key.Length > 2 && key[2] == '?')
                {
                    key = key.Substring(0, 2);
                }

                if (parameters.TryGetValue(key, out var value))
                {
                    yield return value;
                    yield break;
                }
                else if (setNull && this.Main.EndsWith('?'))
                {
                    yield break;
                }
            }

            yield return this;
        }

        public override IEnumerable<(MathObject left, MathObject center, MathObject right)> Divide(MathObject center)
        {
            yield break;
        }

        public override string ToString() => Main.ToString();

        public override TokenString ToTokenString() => Main;
    }
}
