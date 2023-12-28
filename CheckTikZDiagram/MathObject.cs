using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 一つの射or対象を表すクラス
    /// </summary>
    public abstract class MathObject : IEquatable<MathObject?>
    {
        /// <summary>
        /// 元となった文字列
        /// </summary>
        public abstract string OriginalText { get; }

        /// <summary>
        /// 添え字・括弧を除いた部分を表す文字列
        /// </summary>
        public abstract TokenString Main { get; }

        public abstract int Length { get; }


        /// <summary>
        /// 括弧を付ける
        /// </summary>
        /// <param name="left">左括弧</param>
        /// <param name="right">右括弧</param>
        /// <returns>付けた後のMathSequence</returns>
        public abstract MathSequence SetBracket(Token left, Token right);

        /// <summary>
        /// (, ) を付ける
        /// </summary>
        /// <returns></returns>
        public MathSequence SetBracket()
        {
            return this.SetBracket(Token.LeftParenthesis, Token.RightParenthesis);
        }

        /// <summary>
        /// 添え字を付ける
        /// </summary>
        /// <param name="supOrSub">^ もしくは _ を表すToken</param>
        /// <param name="left">左側の括弧となるToken</param>
        /// <param name="math">添え字となるMathObject</param>
        /// <param name="right">右側の括弧となるToken</param>
        /// <returns>付けた後のMathSequence</returns>
        public abstract MathSequence SetScript(Token supOrSub, Token left, MathObject math, Token right);

        /// <summary>
        /// 指定したMathObjectで二つに分割する(全てのパターンを返す)(添え字がある場合は何も返さない)
        /// </summary>
        /// <param name="center">分割で使用するMathObject</param>
        /// <returns>left: 左のMathObject  center: 指定したMathObject  right: 右のMathObject</returns>
        public abstract IEnumerable<(MathObject left, MathObject center, MathObject right)> Divide(MathObject center);

        /// <summary>
        /// 含む変数を全て返す(重複を含む)
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetVariables();

        /// <summary>
        /// 圏を表すMathObjectの場合true
        /// </summary>
        /// <returns></returns>
        public abstract bool IsCategory();

        /// <summary>
        /// MathObjectが同じ「型」であるかを調べる(otherには変数を含まない)(parametersの読み書きを行う)
        /// </summary>
        /// <param name="other">#1, #2, …, #9を含まないMathObject</param>
        /// <param name="parameters">#1, #2, …, #9に対応するパラメーターを取得し格納する辞書</param>
        /// <param name="equalsFunc">2つのMathObjectが等しいか判定する関数</param>
        /// <returns>同じ型ならtrue</returns>
        public abstract bool IsSameType(MathObject other, IDictionary<string, MathObject> parameters, Func<MathObject, MathObject, bool>? equalsFunc = null);

        /// <summary>
        /// #1, #2, …, #9 にパラメーターを代入したMathObjectを生成する。
        /// (#i が含まれない場合は自分自身を返す。)
        /// </summary>
        /// <param name="parameters">代入するパラメーター</param>
        /// <param name="setNull">省略可変数(#1?, #2?, …, #9?)を許す場合true</param>
        /// <returns></returns>
        public abstract IEnumerable<MathObject> ApplyParameters(IReadOnlyDictionary<string, MathObject> parameters, bool setNull);

        public abstract TokenString ToTokenString();

        /// <summary>
        /// 変数を持つ場合true
        /// </summary>
        /// <returns></returns>
        public bool HasVariables()
        {
            var x = this.ToString();
            if (x.Contains('#'))
            {
                // #は変数と見なす
                return true;
            }
            else if (x.Contains('-'))
            {
                // -は、(余)極限のコマンドが含まれていない場合変数と見なす
                foreach (var item in Config.Instance.Limits)
                {
                    if (x.Contains(item))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// このMathObjectをHomとみなして、domとcodを取得する
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>取得できた場合true</returns>
        public bool TryGetSourceAndTargetAsHom(out MathObject source, out MathObject target)
        {
            if (this is not MathSequence math)
            {
                source = this;
                target = this;
                return false;
            }

            if (Length == 2
                && math.List[0].Main.Equals(Config.Instance.Hom)
                && math.List[1] is MathSequence homMain
                && homMain.Length == 2
                && homMain.Separator == ",")
            {
                source = homMain.List[0];
                target = homMain.List[1];
                return true;
            }
            else if (math.Sup != null)
            {
                source = math.Sup;
                target = math.CopyWithoutSup();
                return true;
            }
            else if (math.List.Count > 0
                  && math.List.Last() is MathSequence last
                  && last.Sup != null)
            {
                var list = new List<MathObject>(math.List.Take(math.List.Count - 1));
                list.Add(last.CopyWithoutSup());
                source = last.Sup;
                target = new MathSequence(list);
                return true;
            }
            else
            {
                source = this;
                target = this;
                return false;
            }
        }

        public bool Contains(string value) => this.ToString().Contains(value);
        
        /// <summary>
        /// otherを加えた新しいMathSequenceを返す
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public MathSequence Add(MathObject other)
        {
            var list = new List<MathObject>();

            if (this is MathSequence s && s.IsSimple)
            {
                list.AddRange(s.List);
            }
            else
            {
                list.Add(this);
            }

            if (other is MathSequence t && t.IsSimple)
            {
                list.AddRange(t.List);
            }
            else
            {
                list.Add(other);
            }

            return new MathSequence(list);
        }


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

            return this.Equals(obj as MathObject);
        }


        public bool Equals(MathObject? other)
        {
            if (other == null)
            {
                return false;
            }

            return this.ToTokenString().Equals(other.ToTokenString());
        }

        public override int GetHashCode() => this.ToTokenString().GetHashCode();
        

        public override string ToString() => Main.ToString();
    }
}
