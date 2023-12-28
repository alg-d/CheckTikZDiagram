using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TokenStringの生成テスト
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

            TestHelper(new string[]
            {
                @"\alpha uv\gamma",
                @"\alpha u v \gamma",
                @"\alpha uv \gamma",
                @"\alpha  u   v  \gamma",
            }, @"\alpha u v \gamma");
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

            TestHelper(new string[]
            {
                "a^(x)",
                "a ^ ( x )",
                "a^{(}x)",
                "a ^ { ( } x )",
            }, "a ^ { ( } x )");
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
        public void 添え字_変数()
        {
            TestHelper(new string[]
            {
                @"f^#1",
                @"f^{#1}",
                @"f ^#1",
                @"f^ #1",
                @" f ^{ #1 } ",
            }, @"f ^ { #1 }");

            TestHelper(new string[]
            {
                @"g_#1",
                @"g_{#1}",
                @"g_{ #1}",
                @"g_{#1 }",
                @"g _#1",
                @"g_ #1",
                @" g _{ #1 } ",
            }, @"g _ { #1 }");
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

            TestHelper(new string[]
            {
                @"ab^{\test'}",
                @"ab ^{\test'}",
                @"ab^ {\test'}",
                @"ab^{ \test '}",
                @"ab ^ { \test ' } ",
                @"ab^ { \test^{\prime} }",
                @"ab^{ \test^{ \prime } }",
                @"a b^{ \test ^ { \prime } }",
            }, @"a b ^ { \test ^ { \prime } }");

            TestHelper(new string[]
            {
                @"a'_i",
                @"a ' _i",
                @"a '_ i",
                @"a ' _ i",
                @"a^{\prime}_i",
                @"a^{\prime}_{i}",
                @"a ^ { \prime }_i",
                @"a ^ { \prime } _ i",
            }, @"a ^ { \prime } _ { i }");

            TestHelper(new string[]
            {
                @"a''",
                @"a' '",
                @" a ''",
                @" a ' ' ",
                @"a^{\prime\prime}",
                @"a ^ { \prime \prime }",
            }, @"a ^ { \prime \prime }");
        }

        [TestMethod]
        public void Prime_添え字()
        {
            TestHelper(new string[]
            {
                @"a'^ij",
                @"a' ^ij",
                @"a ' ^ ij",
                @"a '^ij",
                @"a'^{i}j",
                @"a ' ^ { i }j",
                @"a^{\prime i}j",
                @"a ^ { \prime i }j",
            }, @"a ^ { \prime i } j");

            TestHelper(new string[]
            {
                @"a'^{ij}",
                @"a' ^{ij}",
                @"a ' ^ {ij}",
                @"a '^{ij}",
                @"a'^{ij}",
                @"a ' ^ { i j }",
                @"a^{\prime i j }",
                @"a ^ { \prime i j }",
            }, @"a ^ { \prime i j }");

            TestHelper(new string[]
            {
                @"a'^\test\test",
                @"a' ^\test\test",
                @"a ' ^ \test\test",
                @"a '^\test\test",
                @"a'^{\test}\test",
                @"a ' ^ { \test }\test",
                @"a^{\prime \test}\test",
                @"a ^ { \prime \test }\test",
            }, @"a ^ { \prime \test } \test");

            TestHelper(new string[]
            {
                @"a'^#1?\test",
                @"a' ^#1?\test",
                @"a ' ^ #1?\test",
                @"a '^#1?\test",
                @"a'^{#1?}\test",
                @"a ' ^ { #1? }\test",
                @"a^{\prime #1?}\test",
                @"a ^ { \prime #1? }\test",
            }, @"a ^ { \prime #1? } \test");
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
                @"#1\test#2",
                @"#1 \test#2",
                @"#1\test #2",
                @"#1 \test #2",
                @" #1\test#2 ",
                @" #1 \test #2 ",
            }, @"#1 \test #2");

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
        public void 変数abst()
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
                @"\test{#1ab}",
                @"\test {#1a b}",
                @"\test { #1a b}",
                @"\test { #1a b }",
                @"\test{#1a b }",
                @"\test{ #1a b }",
            }, @"\test { #1a b }");

            TestHelper(new string[]
            {
                @"ab^{#1s#2a}_{#4b#5t}",
                @"ab ^{#1s#2a} _{#4b#5t}",
                @"ab^{ #1s#2a }_{ #4b#5t }",
                @"ab^{ #1s #2a }_{ #4b #5t }",
                @"ab ^{ #1s #2a } _{ #4b #5t }",
            }, @"a b ^ { #1s #2a } _ { #4b #5t }");
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
