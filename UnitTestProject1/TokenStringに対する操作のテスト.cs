using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TokenStringに対する操作のテスト
    {
        [DataTestMethod]
        [DataRow(@"abcde", @"abcde")]
        [DataRow(@"a bc d e", @"a bc d e")]
        [DataRow(@" abc de ", @"abc de")]
        [DataRow(@"\text", @"\text")]
        [DataRow(@"\text\test", @"\text\test")]
        [DataRow(@"  \text  \test  ", @"\text  \test")]
        [DataRow(@"\cat{C}", @"\cat{C}")]
        [DataRow(@"\cat{ C }", @"\cat{ C }")]
        [DataRow(@"x^i", @"x^i")]
        [DataRow(@"x^ i", @"x^ i")]
        [DataRow(@"x_j", @"x_j")]
        [DataRow(@"x^{i}", @"x^{i}")]
        [DataRow(@"x_{ j }", @"x_{ j }")]
        [DataRow(@"x^i y_j", @"x^i y_j")]
        [DataRow(@"\cat{C}^{\test}", @"\cat{C}^{\test}")]
        [DataRow(@"\cat{C}^\test", @"\cat{C}^\test")]
        [DataRow(@"\cat {\Z } ^{\test \test }", @"\cat {\Z } ^{\test \test }")]
        [DataRow(@"a'b'", @"a'b'")]
        [DataRow(@"a ' b '", @"a ' b '")]
        [DataRow(@"\alpha_i'", @"\alpha_i'")]
        [DataRow(@"\beta'_i", @"\beta'_i")]
        [DataRow(@"x ' '", @"x ' '")]
        [DataRow(@"u'^{ab}", @"u'^{ab}")]
        [DataRow(@"u ' ^ x", @"u ' ^ x")]
        [DataRow(@"u '_ x", @"u '_ x")]
        public void ToOriginalString(string value, string result)
        {
            value.ToTokenString().ToOriginalString().Is(result);
        }

        [DataTestMethod]
        [DataRow(@"abc", @"de", "a b c d e", "abc de")]
        [DataRow(@"a bc ", @"d e", "a b c d e", "a bc d e")]
        [DataRow(@" ab c", @" de", "a b c d e", "ab c de")]
        [DataRow(@"\test", @"A", @"\test A", @"\test A")]
        [DataRow(@"\test", @"\cat", @"\test \cat", @"\test \cat")]
        [DataRow(@"a^i_j", @"\theta_p^q", @"a ^ { i } _ { j } \theta _ { p } ^ { q }", @"a^i_j \theta_p^q")]
        public void Add(string text1, string text2, string result, string origin)
        {
            var t = text1.ToTokenString().Add(text2.ToTokenString());
            t.ToString().Is(result);
            t.ToOriginalString().Is(origin);
        }

        [TestMethod]
        public void Empty()
        {
            TokenString.Empty.IsEmpty.IsTrue();
            "a".ToTokenString().IsEmpty.IsFalse();
            @"\test".ToTokenString().IsEmpty.IsFalse();
            TokenString.Empty.ToString().Is("");
            TokenString.Empty.ToOriginalString().Is("");

            var ts = " abc def ".ToTokenString();
            ts.IsEmpty.IsFalse();
            ts.ToOriginalString().Is("abc def");
            ts.Add(TokenString.Empty).ToString().Is("a b c d e f");
            ts.Add(TokenString.Empty).ToOriginalString().Is("abc def");
            TokenString.Empty.Add(ts).ToString().Is("a b c d e f");
            TokenString.Empty.Add(ts).ToOriginalString().Is("abc def");
            TokenString.Empty.Add(TokenString.Empty).ToString().Is("");
        }

        [DataTestMethod]
        [DataRow(@"?", true)]
        [DataRow(@"a?", true)]
        [DataRow(@"abc?", true)]
        [DataRow(@"\test?", true)]
        [DataRow(@"\test\cat?", true)]
        [DataRow(@"???", true)]
        [DataRow(@"#1?", true)]
        [DataRow(@"#1#2?", true)]
        [DataRow(@"#1?#2?", true)]
        [DataRow(@"#1 ?", true)]
        [DataRow(@"", false)]
        [DataRow(@"?a", false)]
        [DataRow(@"?abc", false)]
        [DataRow(@"?\test", false)]
        [DataRow(@"#1?#2", false)]
        [DataRow(@"#1#2", false)]
        [DataRow(@"#1#2?#3", false)]
        public void EndsWith(string text, bool result)
        {
            text.ToTokenString().EndsWith('?').Is(result);
        }


        [DataTestMethod]
        [DataRow(@"#")]
        [DataRow(@"#a")]
        [DataRow(@"\test#a")]
        [DataRow(@"\test#")]
        [DataRow(@"#1#2#3#a#4")]
        [DataRow(@"#\test")]
        [DataRow(@"abc^#")]
        [DataRow(@"abc_#")]
        [DataRow(@"abc'#")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void 例外(string text)
        {
            text.ToTokenString();
        }
    }
}
