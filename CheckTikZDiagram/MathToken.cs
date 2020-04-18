using System;
using System.Collections.Generic;
using System.Text;

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
            Main = new TokenString(token);
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

        public override IEnumerable<string> GetParameters()
        {
            var x = this.ToString();
            if (x == "-")
            {
                yield return x;
            }
            else if (x.StartsWith("#"))
            {
                if (x.Length == 2)
                {
                    yield return x;
                }
                else if (x.Length == 3 && (x[2] == '?' || x[2] == 's' || x[2] == 't'))
                {
                    yield return x;
                }
            }
        }

        public override bool IsSameType(MathObject other, IDictionary<string, MathObject> parameters)
        {
            var x = this.ToString();
            if (x.StartsWith('#') && (x.Length == 2 || (x.Length == 3 && x[2] == '?')))
            {
                return IsSameTypeMain(other, parameters, x.Substring(0, 2));
            }
            else if (x == "-" || (x.StartsWith('#') && x.Length == 3 && (x[2] == 's' || x[2] == 't')))
            {
                return IsSameTypeMain(other, parameters, x);
            }
            else
            {
                return other is MathToken && this.Main.Equals(other.Main);
            }
        }

        public bool IsSameTypeMain(MathObject other, IDictionary<string, MathObject> parameters, string index)
        {
            if (other is MathToken)
            {
                if (parameters.ContainsKey(index) && !parameters[index].Equals(other))
                {
                    return false;
                }
                else
                {
                    parameters[index] = other;
                }
            }
            else if (other is MathSequence math)
            {
                if (math.Sup == null && math.Sub == null
                    && (!math.ExistsBracket || math.LeftBracket.Value == "(" || math.LeftBracket.Value == "{"))
                {
                    math = new MathSequence(math.List, math.Separator, math.Main.ToOriginalString());
                }

                if (parameters.ContainsKey(index) && !parameters[index].Equals(math))
                {
                    return false;
                }
                else
                {
                    parameters[index] = math;
                }
            }

            return true;
        }

        public override IEnumerable<MathObject> ApplyParameters(IReadOnlyDictionary<string, MathObject> parameters, bool setNull)
        {
            var key = this.ToString();
            if (key == "-" || key.StartsWith('#'))
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
