using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TestTokenString
    {
        [TestMethod]
        public void TeXコマンド()
        {
            TestHelper(new string[]
            {
                @"\test",
                @"\test ",
                @"\test  ",
                @" \test",
                @"  \test",
                @" \test ",
                @"  \test  "
            }, @"\test");

            TestHelper(new string[] 
            { 
                @"\alpha\beta",
                @"\alpha \beta",
                @"\alpha  \beta",
                @"\alpha\beta ",
                @"\alpha\beta  ",
                @"\alpha \beta ",
                @" \alpha\beta",
                @"  \alpha\beta"
            }, @"\alpha \beta");

            TestHelper(new string[]
            {
                @"\alpha\beta\gamma",
                @"\alpha \beta\gamma",
                @"\alpha\beta \gamma",
                @"\alpha \beta \gamma",
                @" \alpha \beta \gamma "
            }, @"\alpha \beta \gamma");
        }

        [TestMethod]
        public void 括弧()
        {
            TestHelper(new string[]
            {
                @"\cat{C}",
                @"\cat {C}",
                @"\cat { C }",
                @" \cat { C } ",
            }, @"\cat { C }");

            TestHelper(new string[]
            {
                @"\compo{fgh}",
                @"\compo {fgh}",
                @"\compo { fgh }",
                @"\compo { fg h }",
                @"\compo { f gh }",
                @"\compo { f g h }",
                @" \compo { f g h } ",
            }, @"\compo { f g h }");

            TestHelper(new string[]
            {
                @"(i)",
                @"( i )",
                @"(i) ",
                @"( i ) ",
            }, "( i )");

            TestHelper(new string[]
            {
                @"(a,-)",
                @"(a, -)",
                @"( a, -) ",
                @"(a , - )",
            }, "( a , - )");

            TestHelper(new string[]
            {
                "(XYZ)^a_b",
                "(XYZ)^{a}_{b}",
                "( XYZ ) ^ a _b",
                "(X Y Z) ^{a} _ {b}",
                "( X Y Z ) ^ { a } _ { b }",
            }, "( X Y Z ) ^ { a } _ { b }");
        }

        [TestMethod]
        public void 添え字()
        {
            TestHelper(new string[]
            {
                @"f^i",
                @"f^{i}",
                @"f ^i",
                @"f^ i",
                @" f ^{ i } ",
            }, "f ^ { i }");

            TestHelper(new string[]
            {
                @"g_j",
                @"g_{j}",
                @"g_{ j}",
                @"g_{j }",
                @"g _j",
                @"g_ j",
                @" g _{ j } ",
            }, "g _ { j }");

            TestHelper(new string[]
            {
                @"\theta^i",
                @"\theta^{i}",
                @"\theta ^i",
                @"\theta ^{ i } ",
            }, @"\theta ^ { i }");

            TestHelper(new string[]
            {
                @"\sigma_j",
                @"\sigma_{j}",
                @"\sigma _j",
                @"\sigma _{ j } ",
            }, @"\sigma _ { j }");

            TestHelper(new string[]
            {
                @"i_{j_k}",
                @"i _{j _k}",
                @"i _{ j _{k}}",
                @"i _{ j _{ k } } ",
            }, "i _ { j _ { k } }");

            TestHelper(new string[]
            {
                @"x^i_j",
                @"x ^i_j",
                @"x^i _j",
                @"x ^i _j",
                @"x^{i}_{j}",
                @"x ^{i}_{j}",
                @"x^{i} _{j}",
                @"x ^{i} _{j}",
            }, "x ^ { i } _ { j }");

            TestHelper(new string[]
            {
                @"x^{ab}_{uv}",
                @"x ^{ab}_{uv}",
                @"x^{ab} _{uv}",
                @"x ^{ab} _{uv}",
                @"x^{a b}_{uv}",
                @"x ^{ ab }_{uv}",
                @"x^{ab} _{u v}",
                @"x ^{ab} _{ uv }",
            }, "x ^ { a b } _ { u v }");
        }

        [TestMethod]
        public void 添え字_TeXコマンド()
        {
            TestHelper(new string[]
            {
                @"f^\test",
                @"f^{\test}",
                @"f ^\test",
                @"f^ \test",
                @" f ^{ \test } ",
            }, @"f ^ { \test }");

            TestHelper(new string[]
            {
                @"g_\test",
                @"g_{\test}",
                @"g_{ \test}",
                @"g_{\test }",
                @"g _\test",
                @"g_ \test",
                @" g _{ \test } ",
            }, @"g _ { \test }");
        }

        [TestMethod]
        public void 改行()
        {
            TestHelper(new string[]
            {
                @"a b",
                @"a   b",
                @"a
b",
                @"a


b",
                @"a   
   
  b",
            }, "a b");
        }

        [TestMethod]
        public void Prime()
        {
            TestHelper(new string[]
            {
                @"a'",
                @"a '",
                @" a ' ",
                @"a^{\prime}",
                @"a ^ { \prime }",
            }, @"a ^ { \prime }");
            TestHelper(new string[]
            {
                @"a'b'",
                @"a ' b '",
                @"a^{\prime}b'",
                @"a'b^{ \prime }",
                @"a^{\prime}b^{ \prime }",
            }, @"a ^ { \prime } b ^ { \prime }");
        }

        [TestMethod]
        public void 変数()
        {
            TestHelper(new string[]
            {
                @"\test{#1}",
                @"\test {#1}",
                @"\test { #1}",
                @"\test { #1 }",
                @"\test{#1 }",
                @"\test{ #1 }",
            }, @"\test { #1 }");

            TestHelper(new string[]
            {
                @"M^{#1#2#3}_{#4#5}",
                @"M ^{#1#2#3} _{#4#5}",
                @"M^{ #1#2#3 }_{ #4#5 }",
                @"M^{ #1 #2 #3 }_{ #4 #5 }",
                @"M ^{ #1 #2 #3 } _{ #4 #5 }",
            }, @"M ^ { #1 #2 #3 } _ { #4 #5 }");

            TestHelper(new string[]
            {
                @"ab^{#1?#2?}_{#4?#5?}",
                @"ab ^{#1?#2?} _{#4?#5?}",
                @"ab^{ #1?#2? }_{ #4?#5? }",
                @"ab^{ #1? #2? }_{ #4? #5? }",
                @"ab ^{ #1? #2? } _{ #4? #5? }",
            }, @"a b ^ { #1? #2? } _ { #4? #5? }");
        }

        [TestMethod]
        public void 変数st()
        {
            TestHelper(new string[]
            {
                @"\test{#1s}",
                @"\test {#1s}",
                @"\test { #1s}",
                @"\test { #1s }",
                @"\test{#1s }",
                @"\test{ #1s }",
            }, @"\test { #1s }");

            TestHelper(new string[]
            {
                @"\test{#1 s}",
                @"\test {#1 s}",
                @"\test { #1 s}",
                @"\test { #1 s }",
                @"\test{#1 s }",
                @"\test{ #1 s }",
            }, @"\test { #1 s }");

            TestHelper(new string[]
            {
                @"\test{#1t}",
                @"\test {#1t}",
                @"\test { #1t}",
                @"\test { #1t }",
                @"\test{#1t }",
                @"\test{ #1t }",
            }, @"\test { #1t }");

            TestHelper(new string[]
            {
                @"\test{#1 t}",
                @"\test {#1 t}",
                @"\test { #1 t}",
                @"\test { #1 t }",
                @"\test{#1 t }",
                @"\test{ #1 t }",
            }, @"\test { #1 t }");

            TestHelper(new string[]
            {
                @"\test{#1u}",
                @"\test{#1 u}",
                @"\test {#1u}",
                @"\test { #1u}",
                @"\test { #1u }",
                @"\test {#1 u}",
                @"\test{#1u }",
                @"\test{ #1u }",
                @"\test{ #1 u }",
            }, @"\test { #1 u }");

            TestHelper(new string[]
            {
                @"ab^{#1s#2a}_{#4b#5t}",
                @"ab ^{#1s#2a} _{#4b#5t}",
                @"ab^{ #1s#2a }_{ #4b#5t }",
                @"ab^{ #1s #2a }_{ #4b #5t }",
                @"ab ^{ #1s #2a } _{ #4b #5t }",
            }, @"a b ^ { #1s #2 a } _ { #4 b #5t }");
        }

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


        private void TestHelper(string[] list, string value)
        {
            var nslist = list.Select(x => x.ToTokenString()).ToArray();

            foreach (var a in nslist)
            {
                a.ToString().Is(value);

                foreach (var b in nslist)
                {
                    a.Equals(b).IsTrue($"[{a}] != [{b}]");
                }
            }
        }
    }
}
