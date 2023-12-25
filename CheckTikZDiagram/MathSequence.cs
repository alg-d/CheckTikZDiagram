using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    /// <summary>
    /// いくつかの対象、射、関手などから作られた、一つの射or対象を表すクラス(MathObjectの非空有限列からなる)
    /// </summary>
    public class MathSequence : MathObject
    {
        private readonly string _toString;
        private readonly TokenString _toTokenString;
        private readonly bool _supFirst = false;

        public override string OriginalText { get; }

        public override TokenString Main { get; }

        /// <summary>
        /// 左括弧
        /// </summary>
        public Token LeftBracket { get; } = Token.Empty;

        /// <summary>
        /// 右括弧
        /// </summary>
        public Token RightBracket { get; } = Token.Empty;

        /// <summary>
        /// 左・右どちらかに括弧が存在する場合true
        /// </summary>
        public bool ExistsBracket => !this.LeftBracket.IsEmpty || !this.RightBracket.IsEmpty;

        /// <summary>
        /// 上付き添え字を表すMathObject(括弧の外側に付く)
        /// </summary>
        public MathObject? Sup { get; }

        /// <summary>
        /// 下付き添え字を表すMathObject(括弧の外側に付く)
        /// </summary>
        public MathObject? Sub { get; }

        /// <summary>
        /// MathSequenceを構成する要素
        /// </summary>
        public ReadOnlyCollection<MathObject> List { get; }

        /// <summary>
        /// MathSequenceを構成する要素の数
        /// </summary>
        public override int Length => List.Count;

        /// <summary>
        /// Sequenceを区切る文字
        /// </summary>
        public string Separator { get; }

        /// <summary>
        /// 括弧も添え字もなければtrue
        /// </summary>
        public bool IsSimple => !this.ExistsBracket && this.Sup == null && this.Sub == null;

        /// <summary>
        /// 通常のコンストラクタ
        /// </summary>
        /// <param name="list">MathObjectを構成するMathObject列</param>
        public MathSequence(IList<MathObject> list, string separator = "", string originalText = "")
        {
            List = new ReadOnlyCollection<MathObject>(list);
            Separator = separator;
            Main = CreateMainTokenString(List, separator);

            _toTokenString = Main;
            _toString = Main.ToString();
            OriginalText = originalText.IsNullOrEmpty() ? Main.ToOriginalString().Trim() : originalText.Trim();
        }


        #region MathObjectから新しいMathObjectを生成するためのprivateコンストラクタ
        private MathSequence(MathSequence seq, Token supOrSub, Token left, MathObject math, Token right)
            : this(seq)
        {
            if (supOrSub.Value == "^")
            {
                Sup = math;
                if (Sub == null)
                {
                    _supFirst = true;
                }
            }
            else
            {
                Sub = math;
            }

            _toTokenString = ConstructorHelperSupSub(_toTokenString, supOrSub, left, math.ToTokenString(), right);
            _toString = _toTokenString.ToString();
            OriginalText = _toTokenString.ToOriginalString().Trim();
        }

        private MathSequence(MathSequence seq, Token leftBracket, Token rightBracket)
            : this(seq)
        {
            LeftBracket = leftBracket;
            RightBracket = rightBracket;

            _toTokenString = ConstructorHelperBracket(leftBracket, Main, rightBracket);
            _toString = _toTokenString.ToString();
            OriginalText = _toTokenString.ToOriginalString().Trim();
        }

        private MathSequence(MathSequence seq)
        {
            List = seq.List;
            LeftBracket = seq.LeftBracket;
            RightBracket = seq.RightBracket;
            Main = seq.Main;
            Separator = seq.Separator;
            Sub = seq.Sub;
            Sup = seq.Sup;
            _supFirst = seq._supFirst;

            _toTokenString = seq._toTokenString;
            _toString = seq._toString;
            OriginalText = seq.OriginalText;
        }

        private MathSequence(MathSequence seq, IList<MathObject> list, MathObject? sup, MathObject? sub)
            : this(seq)
        {
            if (list.Count == 0) throw new InvalidOperationException($"{nameof(list)}が空です。");

            List = new ReadOnlyCollection<MathObject>(list);
            Main = CreateMainTokenString(List, seq.Separator);
            _toTokenString = ConstructorHelperBracket(seq.LeftBracket, Main, seq.RightBracket);

            Sup = sup;
            Sub = sub;

            if (seq._supFirst)
            {
                if (sup != null)
                {
                    _toTokenString = ConstructorHelperSupSub(_toTokenString, Token.Circumflex, sup.ToTokenString());
                }
                if (sub != null)
                {
                    _toTokenString = ConstructorHelperSupSub(_toTokenString, Token.LowLine, sub.ToTokenString());
                }
            }
            else
            {
                if (sub != null)
                {
                    _toTokenString = ConstructorHelperSupSub(_toTokenString, Token.LowLine, sub.ToTokenString());
                }
                if (sup != null)
                {
                    _toTokenString = ConstructorHelperSupSub(_toTokenString, Token.Circumflex, sup.ToTokenString());
                }
            }

            _toString = _toTokenString.ToString();
            OriginalText = _toTokenString.ToOriginalString().Trim();
        }
        #endregion

        #region MathTokenからMathObjectを生成するためのコンストラクタ
        public MathSequence(MathToken token, Token supOrSub, Token left, MathObject math, Token right)
            : this(token)
        {
            if (supOrSub.Value == "^")
            {
                Sup = math;
                _supFirst = true;
            }
            else
            {
                Sub = math;
            }

            _toTokenString = ConstructorHelperSupSub(_toTokenString, supOrSub, left, math.ToTokenString(), right);
            _toString = _toTokenString.ToString();
            OriginalText = _toTokenString.ToOriginalString().Trim();
        }

        public MathSequence(MathToken token, Token leftBracket, Token rightBracket)
            : this(token)
        {
            LeftBracket = leftBracket;
            RightBracket = rightBracket;
            _toTokenString = ConstructorHelperBracket(leftBracket, Main, rightBracket);
            _toString = _toTokenString.ToString();
            OriginalText = _toTokenString.ToOriginalString().Trim();
        }

        private MathSequence(MathToken token)
        {
            List = new ReadOnlyCollection<MathObject>(new[] { token });
            Main = token.ToTokenString();
            Separator = "";
            _toTokenString = Main;
            _toString = _toTokenString.ToString();
            OriginalText = token.OriginalText;
        }
        #endregion

        private TokenString CreateMainTokenString(ReadOnlyCollection<MathObject> list, string separator)
        {
            if (List.Count == 0)
            {
                return TokenString.Empty;
            }
            else
            {
                return TokenString.Join(separator, list.Select(x => x.ToTokenString()));
            }
        }

        private TokenString ConstructorHelperSupSub(TokenString main, Token supOrSub, Token left, TokenString inBracket, Token right)
        {
            var list = new List<Token>();

            list.AddRange(main.Tokens);
            list.Add(supOrSub);
            list.Add(left);
            list.AddRange(inBracket.Tokens);
            list.Add(right);

            return list.ToTokenString();
        }

        private TokenString ConstructorHelperSupSub(TokenString main, Token supOrSub, TokenString inBracket)
        {
            return this.ConstructorHelperSupSub(main, supOrSub, Token.LeftCurlyBracket, inBracket, Token.RightCurlyBracket);
        }

        private TokenString ConstructorHelperBracket(Token left, TokenString main, Token right)
        {
            var list = new List<Token>();

            list.Add(left);
            list.AddRange(main.Tokens);
            list.Add(right);

            return list.ToTokenString();
        }

        public override MathSequence SetBracket(Token left, Token right)
        {
            if (left.IsEmpty) throw new ArgumentException($"{nameof(left)}が空です");
            if (right.IsEmpty) throw new ArgumentException($"{nameof(right)}が空です");

            if (this.IsSimple)
            {
                return new MathSequence(this, left, right);
            }
            else
            {
                return new MathSequence(new[] { this }).SetBracket(left, right);
            }
        }


        public override MathSequence SetScript(Token supOrSub, Token left, MathObject math, Token right)
        {
            return new MathSequence(this, supOrSub, left, math, right);
        }

        public MathObject SubSequence(int startIndex)
        {
            return SubSequence(this.List.Skip(startIndex).ToArray());
        }

        public MathObject SubSequence(int startIndex, int length)
        {
            return SubSequence(this.List.Skip(startIndex).Take(length).ToArray());
        }

        private MathObject SubSequence(IList<MathObject> seq)
        {
            if (seq.Count == 0)
            {
                throw new InvalidOperationException();
            }
            else if (seq.Count == 1)
            {
                return seq[0];
            }
            else
            {
                return new MathSequence(seq);
            }
        }

        public MathObject CopyWithoutSup()
        {
            var result = this.List.Count == 1 ? this.List[0] : new MathSequence(this.List, this.Separator);
            if (this.ExistsBracket)
            {
                result = result.SetBracket(this.LeftBracket, this.RightBracket);
            }
            if (this.Sub != null)
            {
                result = result.SetScript(Token.LowLine, Token.LeftCurlyBracket, this.Sub, Token.RightCurlyBracket);
            }
            return result;
        }

        public MathObject CopyWithoutSub()
        {
            var result = this.List.Count == 1 ? this.List[0] : new MathSequence(this.List, this.Separator);
            if (this.ExistsBracket)
            {
                result = result.SetBracket(this.LeftBracket, this.RightBracket);
            }
            if (this.Sup != null)
            {
                result = result.SetScript(Token.Circumflex, Token.LeftCurlyBracket, this.Sup, Token.RightCurlyBracket);
            }
            return result;
        }

        public MathObject CopyWithoutScriptAndBracket()
        {
            var result = this.List.Count == 1 ? this.List[0] : new MathSequence(this.List, this.Separator);
            return result;
        }

        public MathObject CopyWithoutScript()
        {
            var result = this.List.Count == 1 ? this.List[0] : new MathSequence(this.List, this.Separator);
            if (this.ExistsBracket)
            {
                result = result.SetBracket(this.LeftBracket, this.RightBracket);
            }
            return result;
        }

        public override IEnumerable<string> GetVariables()
        {
            var list = this.List.SelectMany(x => x.GetVariables());

            if (this.Sup != null)
            {
                list = list.Concat(this.Sup.GetVariables());
            }

            if (this.Sub != null)
            {
                list = list.Concat(this.Sub.GetVariables());
            }

            return list;
        }

        public override bool IsCategory() => this.List.Any(m => m.IsCategory());

        public override bool IsSameType(MathObject other, IDictionary<string, MathObject> parameters)
        {
            return other switch
            {
                MathSequence seq => this.List.Count == 1
                                  ? IsSameTypeSingle(seq, parameters)
                                  : IsSameTypeMulti(seq, parameters),
                MathToken token => IsSameTypeToken(token, parameters),
                _ => false,
            };
        }

        private bool IsSameTypeSingle(MathSequence other, IDictionary<string, MathObject> parameters)
        {
            // 変数単体(+添え字)の場合の処理
            if (this.List[0] is MathToken token && token.HasVariables())
            {
                if (this.Sup != null || this.Sub != null)
                {
                    if (other.Sup != null || other.Sub != null || other.ExistsBracket)
                    {
                        // otherに添え字や括弧がある場合は、添え字と本体をそれぞれ比較
                        return IsSameTypeScript(this.Sup, other.Sup, parameters)
                            && IsSameTypeScript(this.Sub, other.Sub, parameters)
                            && token.IsSameType(other.CopyWithoutScriptAndBracket(), parameters);
                    }
                    else
                    {
                        // そうでない場合は、最後の添え字と比較する
                        if (other.List.Count > 0
                            && other.List.Last() is MathSequence last)
                        {
                            var list = new List<MathObject>(other.List.Take(other.List.Count - 1));
                            if (this.Sup == null)
                            {
                                list.Add(last.CopyWithoutSub());
                                return IsSameTypeScript(this.Sub, last.Sub, parameters)
                                    && token.IsSameType(new MathSequence(list), parameters);
                            }
                            else if (this.Sup == null)
                            {
                                list.Add(last.CopyWithoutSup());
                                return IsSameTypeScript(this.Sup, last.Sup, parameters)
                                    && token.IsSameType(new MathSequence(list), parameters);
                            }
                            else
                            {
                                list.Add(last.CopyWithoutScript());
                                return IsSameTypeScript(this.Sup, last.Sup, parameters)
                                    && IsSameTypeScript(this.Sub, last.Sub, parameters)
                                    && token.IsSameType(new MathSequence(list), parameters);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return token.IsSameType(other, parameters);
                }
            }

            // そうでない場合は、添え字と本体をそれぞれ比較
            return IsSameTypeScript(this.Sup, other.Sup, parameters)
                && IsSameTypeScript(this.Sub, other.Sub, parameters)
                && this.List[0].IsSameType(other.CopyWithoutScriptAndBracket(), parameters);
        }


        private bool IsSameTypeMulti(MathSequence other, IDictionary<string, MathObject> parameters)
        {
            // 上付き添え字についての判定
            if (!IsSameTypeScript(this.Sup, other.Sup, parameters)) return false;

            // 下付き添え字についての判定
            if (!IsSameTypeScript(this.Sub, other.Sub, parameters)) return false;

            // 括弧についての判定
            if (this.ExistsBracket && !RemovableBracket(this.LeftBracket, this.RightBracket))
            {
                if (!this.LeftBracket.Equals(other.LeftBracket) || !this.RightBracket.Equals(other.RightBracket)) return false;
            }

            // 本体についての判定
            if (!this.Separator.IsNullOrEmpty() && this.Separator != other.Separator) return false;

            if (this.List.Count == 1 && this.List[0].IsSameType(other.CopyWithoutScriptAndBracket(), parameters)) return true;
            
            if (this.List.Count > other.List.Count) return false;

            var temp_j = 0;
            for (int i = 0; i < this.List.Count; i++)
            {
                // other側の長さが足りなくなった場合
                if (temp_j >= other.List.Count) return false;

                // i番目がMathSequenceの場合
                if (this.List[i] is MathSequence)
                {
                    if (!this.List[i].IsSameType(other.List[temp_j], parameters)) return false;
                    temp_j++;
                    continue;
                }

                // i番目がMathTokenでパラメーター無しの場合
                if (!this.List[i].HasVariables())
                {
                    if (this.List[i].Equals(other.List[temp_j]))
                    {
                        temp_j++;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                // 以降はi番目がMathTokenでパラメーター有りの場合の処理

                // i番目が最後の場合は残りと比較する
                if (i == this.List.Count - 1)
                {
                    return this.List[i].IsSameType(other.SubSequence(temp_j), parameters);
                }

                // 次がパラメーターの場合
                if (this.List[i + 1].HasVariables())
                {
                    // 次のTeXコマンドのところを探す
                    for (int j = temp_j + 1; j < other.List.Count; j++)
                    {
                        if (other.List[j].Main.ToString().StartsWith(@"\"))
                        {
                            if (!this.List[i].IsSameType(other.SubSequence(temp_j, j - temp_j), parameters)) return false;
                            temp_j = j;
                            goto NEXT_LOOP;
                        }
                    }

                    // なければ次のMathObjectを使用する
                    if (!this.List[i].IsSameType(other.List[temp_j], parameters)) return false;
                    temp_j++;
                    continue;
                }

                // 次がパラメーターでない場合、「次」が一致する場所を探す
                for (int j = temp_j + 1; j < other.List.Count; j++)
                {
                    var dummy = new Dictionary<string, MathObject>();
                    if (this.List[i + 1].IsSameType(other.List[j], dummy))
                    {
                        if (!this.List[i].IsSameType(other.SubSequence(temp_j, j - temp_j), parameters)) return false;
                        temp_j = j;
                        goto NEXT_LOOP;
                    }
                }

                // 一致する場所がなかった場合はfalse
                return false;

            NEXT_LOOP:;
            }

            return temp_j == other.List.Count;

            static bool RemovableBracket(Token left, Token right)
            {
                return (left.Equals(Token.LeftParenthesis) && right.Equals(Token.RightParenthesis))
                    || (left.Equals(Token.LeftCurlyBracket) && right.Equals(Token.RightCurlyBracket));
            }
        }

        private bool IsSameTypeToken(MathToken other, IDictionary<string, MathObject> parameters)
        {
            // 上付き添え字についての判定
            if (this.Sup != null && !this.Sup.Main.EndsWith('?'))
            {
                return false;
            }

            // 下付き添え字についての判定
            if (this.Sub != null && !this.Sub.Main.EndsWith('?'))
            {
                return false;
            }

            // 本体の判定
            if (!this.Separator.IsNullOrEmpty())
            {
                return false;
            }

            return this.List.Count == 1 && this.List[0].IsSameType(other, parameters);
        }


        static bool IsSameTypeScript(MathObject? thisScript, MathObject? otherScript, IDictionary<string, MathObject> parameters)
        {
            if (thisScript == null)
            {
                if (otherScript != null) return false;
            }
            else
            {
                if (otherScript == null)
                {
                    if (!thisScript.Main.EndsWith('?'))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!thisScript.IsSameType(otherScript, parameters)) return false;
                }
            }

            return true;
        }

        public override IEnumerable<MathObject> ApplyParameters(IReadOnlyDictionary<string, MathObject> parameters, bool setNull)
        {
            var change = false;

            // this.Listにパラメーターを適用
            var lists = ApplyAggregate(this.List, parameters, setNull);
            if (lists.Count != 1 || lists[0].Count != this.List.Count)
            {
                change = true;
            }
            else
            {
                for (int i = 0; i < lists[0].Count; i++)
                {
                    if (!lists[0][i].ToTokenString().Equals(this.List[i].ToTokenString()))
                    {
                        change = true;
                    }
                }
            }

            if (this.Main.EndsWith('?') && List.Count == 0)
            {
                yield break;
            }

            // this.Supにパラメーターを適用
            var sups = this.Sup?.ApplyParameters(parameters, setNull).ToArray() ?? Array.Empty<MathObject>();
            if (Changed(sups, this.Sup))
            {
                change = true;
            }

            // this.Subにパラメーターを適用
            var subs = this.Sub?.ApplyParameters(parameters, setNull).ToArray() ?? Array.Empty<MathObject>();
            if (Changed(subs, this.Sub))
            {
                change = true;
            }

            // 変更がなければ、thisを返す
            if (change)
            {
                if (lists.Count != 0)
                {
                    foreach (var seq in lists)
                    {
                        if (sups.Length == 0)
                        {
                            if (subs.Length == 0)
                            {
                                var math = new MathSequence(this, seq, null, null);
                                yield return math;
                            }
                            else
                            {
                                foreach (var sub in subs)
                                {
                                    yield return new MathSequence(this, seq, null, sub);
                                }
                            }
                        }
                        else
                        {
                            foreach (var sup in sups)
                            {
                                if (subs.Length == 0)
                                {
                                    yield return new MathSequence(this, seq, sup, null);
                                }
                                else
                                {
                                    foreach (var sub in subs)
                                    {
                                        yield return new MathSequence(this, seq, sup, sub);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                yield return this;
            }
        }

        private bool Changed(IList<MathObject> list, MathObject? math)
        {
            return math != null & (list.Count != 1 || list[0] != math);
        }

        private List<List<MathObject>> ApplyAggregate(IReadOnlyCollection<MathObject> sequence, IReadOnlyDictionary<string, MathObject> parameters, bool setNull)
        {
            var result = new List<List<MathObject>>
            {
                new List<MathObject>()
            };

            foreach (var math in sequence)
            {
                var old = result;
                result = new List<List<MathObject>>();

                foreach (var x in math.ApplyParameters(parameters, setNull))
                {
                    foreach (var list in old)
                    {
                        var newList = new List<MathObject>(list);
                        newList.Add(x);
                        result.Add(newList);

                        if (!x.ToTokenString().Equals(math.ToTokenString())
                            && x is MathSequence seq
                            && seq.List.Count > 1
                            && seq.IsSimple)
                        {
                            newList = new List<MathObject>(list);
                            newList.Add(x.SetBracket());
                            result.Add(newList);
                        }
                    }
                }
            }

            return result;
        }

        public override IEnumerable<(MathObject left, MathObject center, MathObject right)> Divide(MathObject center)
        {
            if (this.Sup != null || this.Sub != null)
            {
                yield break;
            }

            for (int i = 1; i < this.List.Count - 1; i++)
            {
                if (this.List[i].Equals(center))
                {
                    yield return (this.SubSequence(0, i), center, this.SubSequence(i + 1));
                }
            }
        }

        public IEnumerable<MathObject> Split(string text)
        {
            if (this.Sup != null || this.Sub != null || this.List.Count == 0)
            {
                yield return this;
                yield break;
            }

            var list = new List<MathObject>();
            foreach (var item in this.List)
            {
                if (item.Main.Equals(text))
                {
                    if (list.Count == 1)
                    {
                        yield return list[0];
                        list.Clear();
                    }
                    else if (list.Count >= 2)
                    {
                        yield return new MathSequence(list);
                        list.Clear();
                    }
                }
                else
                {
                    list.Add(item);
                }
            }

            if (list.Count == 1)
            {
                yield return list[0];
            }
            else if (list.Count >= 2)
            {
                yield return new MathSequence(list);
            }
        }

        public override string ToString() => _toString;

        public override TokenString ToTokenString() => _toTokenString;
    }
}
