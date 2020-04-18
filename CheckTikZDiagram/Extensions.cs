using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CheckTikZDiagram
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            foreach (var item in source)
            {
                action(item);
            }
        }


        public static bool IsNullOrEmpty(this string? text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNullOrWhiteSpace(this string? text)
        {
            return string.IsNullOrWhiteSpace(text);
        }




        public static string Remove(this string main, string value)
        {
            return main.Replace(value, "", StringComparison.Ordinal);
        }

        /// <summary>
        /// 数式をトークン列にします。
        /// </summary>
        /// <param name="main">数式</param>
        /// <returns></returns>
        public static TokenString ToTokenString(this string text)
        {
            return new TokenStringFactory(text).Create();
        }

        ///// <summary>
        ///// 一番外側についている括弧は全て外した文字列を返す
        ///// </summary>
        ///// <param name="text"></param>
        ///// <returns></returns>
        //public static string RemoveBrackets(this string text)
        //{
        //    if (text.IsNullOrEmpty())
        //    {
        //        return text;
        //    }

        //    while (text.First().ToString().IsOpenBracket() && text.Last().ToString().IsCloseBracket())
        //    {
        //        text = text[1..^1];
        //    }

        //    return text;
        //}

        ///// <summary>
        ///// 一番外側についている括弧は全て外した文字列を返す
        ///// </summary>
        ///// <param name="text"></param>
        ///// <returns></returns>
        //public static string RemoveBracketsToString(this TokenString text)
        //{
        //    return text.ToString().RemoveBrackets();
        //}

        ///// <summary>
        ///// 一番外側についている括弧は全て外したNormalizedStringを返す
        ///// </summary>
        ///// <param name="text"></param>
        ///// <returns></returns>
        //public static TokenString RemoveBrackets(this TokenString text)
        //{
        //    return text.RemoveBracketsToString().ToTokenString();
        //}
    }
}
