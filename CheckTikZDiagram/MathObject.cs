using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 一つの射or対象を表すクラス
    /// </summary>
    public abstract class MathObject : IEquatable<MathObject>
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
        /// 指定したMathObjectで二つに分割する
        /// </summary>
        /// <param name="center">分割で使用するMathObject</param>
        /// <returns></returns>
        public abstract IEnumerable<(MathObject left, MathObject center, MathObject right)> Divide(MathObject center);

        /// <summary>
        /// 含むパラメーターを全て返す(重複を含む)
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetParameters();

        /// <summary>
        /// MathObjectが同じ「型」であるかを調べる
        /// </summary>
        /// <param name="other">#1, #2, …, #9を含まないMathObject</param>
        /// <param name="parameters">t#1, #2, …, #9に対応するパラメーターを取得し格納する辞書</param>
        /// <returns>同じ型ならtrue</returns>
        public abstract bool IsSameType(MathObject other, IDictionary<string, MathObject> parameters);

        /// <summary>
        /// #1, #2, …, #9 にパラメーターを代入したMathObjectを生成する。
        /// (#i が含まれない場合は自分自身を返す。)
        /// </summary>
        /// <param name="parameters">代入するパラメーター</param>
        /// <param name="setNull">省略可パラメーター(#!?, #2?, …, #9?)を許す場合true</param>
        /// <returns></returns>
        public abstract IEnumerable<MathObject> ApplyParameters(IReadOnlyDictionary<string, MathObject> parameters, bool setNull);

        public abstract TokenString ToTokenString();

        public bool HasParameter() => this.ToString().Contains("#") || this.ToString().Contains("-");

        public bool Contains(string value) => this.ToString().Contains(value);
        
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


        public bool Equals([AllowNull] MathObject other)
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
