using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TestMathObject
    {
        [TestMethod]
        public void Split()
        {
            var xs = new MathObjectFactory(@"u\otimes v").Create()
                .TestSingleMath()
                .Split(@"\otimes")
                .ToArray();
            xs.Length.Is(2);
            xs[0].IsMathToken("u");
            xs[1].IsMathToken("v");

            xs = new MathObjectFactory(@"(u\otimes v)").Create()
                .TestSingleMath()
                .Split(@"\otimes")
                .ToArray();
            xs.Length.Is(2);
            xs[0].IsMathToken("u");
            xs[1].IsMathToken("v");

            xs = new MathObjectFactory(@"(u\otimes v)^i").Create()
                .TestSingleMath()
                .Split(@"\otimes")
                .ToArray();
            xs.Length.Is(1);

            xs = new MathObjectFactory(@"u\otimes v").Create()
                .TestSingleMath()
                .Split(@",")
                .ToArray();
            xs.Length.Is(1);

            xs = new MathObjectFactory(@"(u\otimes v)\otimes w").Create()
                .TestSingleMath()
                .Split(@"\otimes")
                .ToArray();
            xs.Length.Is(2);
            xs[0].AsMathSequence().ToTokenString().TestString(@"(u\otimes v)");
            xs[1].IsMathToken("w");
        }

        [TestMethod]
        public void Add()
        {
            var t0 = new MathObjectFactory("a").Create().TestSingle();
            var t1 = new MathObjectFactory("b").Create().TestSingle();
            var seq0 = new MathObjectFactory("hi jk").Create().TestSingle();
            var seq1 = new MathObjectFactory("(x)(y z)").Create().TestSingle();

            var x = t0.Add(t1);
            x.List.Count.Is(2);
            x.List[0].IsMathToken("a");
            x.List[1].IsMathToken("b");
            x.ToTokenString().TestString("ab");
            x.OriginalText.Is("ab");

            x = t0.Add(seq0);
            x.List.Count.Is(5);
            x.List[0].IsMathToken("a");
            x.List[1].IsMathToken("h");
            x.List[2].IsMathToken("i");
            x.List[3].IsMathToken("j");
            x.List[4].IsMathToken("k");
            x.ToTokenString().TestString("ahijk");
            x.OriginalText.Is("ahi jk");

            seq0 = seq0.SetScript(Token.Circumflex, new Token("{", ""), t0, new Token("}", ""));
            x = t0.Add(seq0);
            x.List.Count.Is(2);
            x.List[0].IsMathToken("a");
            x.List[1].ToTokenString().TestString("hijk^a");
            x.ToTokenString().TestString("ahijk^a");
            x.OriginalText.Is("ahi jk^a");

            x = seq1.Add(t1);
            x.List.Count.Is(3);
            x.List[0].AsMathSequence().List.Count.Is(1);
            x.List[1].AsMathSequence().List.Count.Is(2);
            x.List[2].IsMathToken("b");
            x.ToTokenString().TestString("(x)(yz)b");
            x.OriginalText.Is("(x)(y z)b");
        }

        [TestMethod]
        public void Divide_通常()
        {
            var math = new MathObjectFactory(@"a\times b").Create().TestSingle();

            var center = new MathObjectFactory(@"\times").Create().TestSingle();
            var ar = math.Divide(center).ToArray();
            ar.Length.Is(1);
            ar[0].left.IsMathToken("a");
            ar[0].left.OriginalText.Is("a");
            ar[0].center.IsMathToken(@"\times");
            ar[0].right.IsMathToken("b");
            ar[0].right.OriginalText.Is("b");

            math = new MathObjectFactory(@"a\times b\times c").Create().TestSingle();
            ar = math.Divide(center).ToArray();
            ar.Length.Is(2);

            ar[0].left.IsMathToken("a");
            ar[0].left.OriginalText.Is("a");
            ar[0].center.IsMathToken(@"\times");
            var seq = (MathSequence)ar[0].right;
            seq.List.Count.Is(3);
            seq.List[0].IsMathToken("b");
            seq.List[1].IsMathToken(@"\times");
            seq.List[2].IsMathToken("c");
            seq.OriginalText.Is(@"b\times c");

            seq = (MathSequence)ar[1].left;
            seq.List.Count.Is(3);
            seq.List[0].IsMathToken("a");
            seq.List[1].IsMathToken(@"\times");
            seq.List[2].IsMathToken("b");
            seq.OriginalText.Is(@"a\times b");
            ar[1].center.IsMathToken(@"\times");
            ar[1].right.IsMathToken("c");
            ar[1].right.OriginalText.Is("c");

            math = new MathObjectFactory(@"a\times\times c").Create().TestSingle();
            ar = math.Divide(center).ToArray();
            ar.Length.Is(2);

            ar[0].left.IsMathToken("a");
            ar[0].center.IsMathToken(@"\times");
            ar[0].right.ToTokenString().TestString(@"\times c");

            ar[1].left.ToTokenString().TestString(@"a\times");
            ar[1].center.IsMathToken(@"\times");
            ar[1].right.IsMathToken(@"c");
        }

        [TestMethod]
        public void Divide_括弧()
        {
            var token = new MathObjectFactory(@"\otimes").Create().TestSingle();

            var math = new MathObjectFactory(@"\encat { A } \otimes (\encat {B} \otimes \encat{C})").Create().TestSingle();
            var ar = math.Divide(token).ToArray();
            ar.Length.Is(1);

            ar[0].left.AsMathSequence().List.Count.Is(2);
            ar[0].left.AsMathSequence().List[0].IsMathToken(@"\encat");
            ar[0].left.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            ar[0].left.OriginalText.Is(@"\encat { A }");

            ar[0].center.IsMathToken(@"\otimes");

            var seq = (MathSequence)ar[0].right;
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.List.Count.Is(5);
            seq.List[0].IsMathToken(@"\encat");
            seq.List[1].ToTokenString().TestString("{B}");
            seq.List[2].IsMathToken(@"\otimes");
            seq.List[3].IsMathToken(@"\encat");
            seq.List[4].ToTokenString().TestString("{C}");
            seq.OriginalText.Is(@"(\encat {B} \otimes \encat{C})");

            ar = seq.Divide(token).ToArray();
            ar.Length.Is(1);

            ar[0].left.AsMathSequence().List.Count.Is(2);
            ar[0].left.AsMathSequence().Main.TestString(@"\encat{B}");
            ar[0].left.AsMathSequence().ExistsBracket.IsFalse();
            ar[0].left.OriginalText.Is(@"\encat {B}");

            ar[0].center.IsMathToken(@"\otimes");

            ar[0].right.AsMathSequence().List.Count.Is(2);
            ar[0].right.AsMathSequence().Main.TestString(@"\encat{C}");
            ar[0].right.AsMathSequence().ExistsBracket.IsFalse();
            ar[0].right.OriginalText.Is(@"\encat{C}");
        }

        [TestMethod]
        public void Divide_返さない()
        {
            var token = new MathObjectFactory("F").Create().TestSingle();
            token.Divide(token).Count().Is(0);

            var math = new MathObjectFactory(@"a\times b").Create().TestSingle();
            math.Divide(token).Count().Is(0);

            token = new MathObjectFactory(@"\times").Create().TestSingle();

            math = new MathObjectFactory("F").Create().TestSingle();
            math.Divide(token).Count().Is(0);

            math = new MathObjectFactory(@"\times").Create().TestSingle();
            math.Divide(token).Count().Is(0);

            math = new MathObjectFactory(@"a\times").Create().TestSingle();
            math.Divide(token).Count().Is(0);

            math = new MathObjectFactory(@"\times b").Create().TestSingle();
            math.Divide(token).Count().Is(0);
        }

        [TestMethod]
        public void SetBracket()
        {
            var math = new MathObjectFactory(@"f").Create().TestSingle();
            var seq = math.SetBracket();
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken("f");
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.ToTokenString().TestString("(f)");
            seq.OriginalText.Is("(f)");

            math = new MathObjectFactory(@"f^{ i }").Create().TestSingle();
            seq = math.SetBracket();
            seq.List.Count.Is(1);
            seq.List[0].AsMathSequence().List.Count.Is(1);
            seq.List[0].AsMathSequence().List[0].IsMathToken("f");
            seq.List[0].AsMathSequence().Sup.IsMathToken("i");
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.ToTokenString().TestString("(f^i)");
            seq.OriginalText.Is("(f^{ i })");
        }

        [TestMethod]
        public void SetScriptSup()
        {
            var math = ExtensionsInTest.CreateSingleMathObject("f");
            var math2 = ExtensionsInTest.CreateSingleMathObject("a b c");
            var seq = math.SetScript(new Token("^", "^"), Token.LeftCurlyBracket, math2, Token.RightCurlyBracket);
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken("f");
            seq.Sup.Main.TestString("abc");
            seq.Sup.AsMathSequence().ExistsBracket.IsFalse();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString("f^{abc}");
            seq.OriginalText.Is("f^{a b c}");

            math = ExtensionsInTest.CreateSingleMathObject(@"\test AB");
            seq = math.SetScript(new Token("^", " ^"), new Token("{", " {"), math2, new Token("}", " }"));
            seq.List.Count.Is(3);
            seq.Main.TestString(@"\test AB");
            seq.Sup.Main.TestString(@"abc");
            seq.Sup.AsMathSequence().ExistsBracket.IsFalse();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"\test AB^{abc}");
            seq.OriginalText.Is(@"\test AB ^ {a b c }");

            math = ExtensionsInTest.CreateSingleMathObject(@"{ CD } _ { uv }");
            math2 = ExtensionsInTest.CreateSingleMathObject(@"a\times b");
            seq = math.SetScript(new Token("^", "^"), Token.LeftCurlyBracket, math2, Token.RightCurlyBracket);
            seq.List.Count.Is(2);
            seq.Main.TestString(@"CD");
            seq.LeftBracket.TestToken("{");
            seq.RightBracket.TestToken("}");
            seq.Sup.Main.TestString(@"a\times b");
            seq.Sup.AsMathSequence().ExistsBracket.IsFalse();
            seq.Sub.Main.TestString("uv");
            seq.Sub.AsMathSequence().ExistsBracket.IsFalse();
            seq.ToTokenString().TestString(@"{CD}_{uv}^{a\times b}");
            seq.OriginalText.Is(@"{ CD } _ { uv }^{a\times b}");
        }

        [TestMethod]
        public void SetScriptSub()
        {
            var math = ExtensionsInTest.CreateSingleMathObject("f");
            var math2 = ExtensionsInTest.CreateSingleMathObject("a b c");
            var seq = math.SetScript(new Token("_", "_"), Token.LeftCurlyBracket, math2, Token.RightCurlyBracket);
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken("f");
            seq.Sup.IsNull();
            seq.Sub.Main.TestString("abc");
            seq.Sub.AsMathSequence().ExistsBracket.IsFalse();
            seq.ToTokenString().TestString("f_{abc}");
            seq.OriginalText.Is("f_{a b c}");

            math = ExtensionsInTest.CreateSingleMathObject(@"\test AB");
            seq = math.SetScript(new Token("_", " _"), new Token("{", " {"), math2, new Token("}", " }"));
            seq.List.Count.Is(3);
            seq.Main.TestString(@"\test AB");
            seq.Sup.IsNull();
            seq.Sub.Main.TestString(@"abc");
            seq.Sub.AsMathSequence().ExistsBracket.IsFalse();
            seq.ToTokenString().TestString(@"\test AB_{abc}");
            seq.OriginalText.Is(@"\test AB _ {a b c }");

            math = ExtensionsInTest.CreateSingleMathObject(@"(F\theta) ^{ab}");
            math2 = ExtensionsInTest.CreateSingleMathObject(@"G(i)");
            seq = math.SetScript(new Token("_", "_"), Token.LeftCurlyBracket, math2, Token.RightCurlyBracket);
            seq.List.Count.Is(2);
            seq.Main.TestString(@"F\theta");
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.Sup.Main.TestString(@"ab");
            seq.Sup.AsMathSequence().ExistsBracket.IsFalse();
            seq.Sub.Main.TestString("G(i)");
            seq.Sub.AsMathSequence().ExistsBracket.IsFalse();
            seq.ToTokenString().TestString(@"(F\theta)^{ab}_{G(i)}");
            seq.OriginalText.Is(@"(F\theta) ^{ab}_{G(i)}");
        }

        [TestMethod]
        public void GetVariables()
        {
            var math = new MathObjectFactory(@"f").Create().TestSingle();
            math.GetVariables().Count().Is(0);

            math = new MathObjectFactory(@"#1").Create().TestSingle();
            var list = math.GetVariables().ToArray();
            list.Count().Is(1);
            list[0].Is("#1");

            math = new MathObjectFactory(@"#2?").Create().TestSingle();
            list = math.GetVariables().ToArray();
            list.Count().Is(1);
            list[0].Is("#2?");

            math = new MathObjectFactory(@"#2s").Create().TestSingle();
            list = math.GetVariables().ToArray();
            list.Count().Is(1);
            list[0].Is("#2s");

            math = new MathObjectFactory(@"#2t").Create().TestSingle();
            list = math.GetVariables().ToArray();
            list.Count().Is(1);
            list[0].Is("#2t");

            math = new MathObjectFactory(@"#2u").Create().TestSingle();
            list = math.GetVariables().ToArray();
            list.Count().Is(1);
            list[0].Is("#2");

            math = new MathObjectFactory(@"-").Create().TestSingle();
            list = math.GetVariables().ToArray();
            list.Count().Is(1);
            list[0].Is("-");

            math = new MathObjectFactory(@"(#1#2)").Create().TestSingle();
            math.GetVariables().Count().Is(2);

            math = new MathObjectFactory(@"\test^{#1#2}_{#3?#4?}").Create().TestSingle();
            math.GetVariables().Count().Is(4);

            math = new MathObjectFactory(@"\test^{#1#2}_{#1#3}").Create().TestSingle();
            math.GetVariables().Count().Is(4);
        }


        [DataTestMethod]
        [DataRow(@"\test", false)]
        [DataRow(@"\id_{\cat{C}}", false)]
        [DataRow(@"\Set", true)]
        [DataRow(@"\cat{C}", true)]
        [DataRow(@"\Cat[\moncat{V}]", true)]
        [DataRow(@"abc\Mod", true)]
        public void IsCategory(string text, bool result)
        {
            ExtensionsInTest.CreateSingleMathObject(text).IsCategory().Is(result);
        }
    }
}
