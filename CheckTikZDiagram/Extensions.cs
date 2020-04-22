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

        /// <summary>
        /// Tokenのリストをトークン列にします。
        /// </summary>
        /// <param name="list">Tokenのリスト</param>
        /// <returns></returns>
        public static TokenString ToTokenString(this IList<Token> tokens)
        {
            return TokenString.Create(tokens);
        }
    }
}
