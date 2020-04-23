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

        [TestMethod]
        public void IsSameType1MathToken()
        {
            var def = new MathObjectFactory(@"#1").Create().TestSingle();
            def.IsMathToken(@"#1");

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"a").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].IsMathToken("a");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"abc").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"(abc)").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[u, v]").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("[u, v]");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"{abc}{def}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("{abc}{def}");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"#2").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].IsMathToken("#2");
        }

        [TestMethod]
        public void IsSameType1MathSequence()
        {
            var def = new MathObjectFactory(@"xy#1").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"xyz").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("z");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"xyzzz").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("zzz");


            def = new MathObjectFactory(@"x#1z").Create().TestSingle();

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"xyz").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("y");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"xyyyz").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("yyy");


            def = new MathObjectFactory(@"#1yz").Create().TestSingle();

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"xyz").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("x");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"xxxyz").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("xxx");
        }

        [TestMethod]
        public void IsSameType1小括弧1()
        {
            var def = new MathObjectFactory(@"(#1)").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"a").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].IsMathToken("a");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"abc").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"(abc)").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[u, v]").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("[u, v]");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"{abc}{def}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("{abc}{def}");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"#2").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].IsMathToken("#2");
        }

        [TestMethod]
        public void IsSameType1小括弧2()
        {
            var def = new MathObjectFactory(@"F(#1)").Create().TestSingleMath();
            def.List.Count.Is(2);
            def.List[0].IsMathToken(@"F");
            def.List[1].AsMathSequence().List.Count.Is(1);
            def.List[1].AsMathSequence().List[0].IsMathToken("#1");
            def.List[1].AsMathSequence().LeftBracket.TestToken("(");
            def.List[1].AsMathSequence().RightBracket.TestToken(")");
            def.List[1].AsMathSequence().Main.TestString("#1");
            def.List[1].AsMathSequence().ToTokenString().TestString("(#1)");

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"F(a)").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("a");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"Fx").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("x");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"F(abc)").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"F(\theta^i_x)").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"\theta^i_x");
        }

        [TestMethod]
        public void IsSameType1中括弧()
        {
            var def = new MathObjectFactory(@"\test{#1}").Create().TestSingleMath();
            def.List.Count.Is(2);
            def.List[0].IsMathToken(@"\test");
            def.List[1].AsMathSequence().List.Count.Is(1);
            def.List[1].AsMathSequence().List[0].IsMathToken("#1");
            def.List[1].AsMathSequence().LeftBracket.TestToken("{");
            def.List[1].AsMathSequence().RightBracket.TestToken("}");
            def.List[1].AsMathSequence().Main.TestString("#1");
            def.List[1].AsMathSequence().ToTokenString().TestString("{#1}");

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"\test{a}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("a");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{abc}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test C").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("C");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\theta^i_x}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"\theta^i_x");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{f\ocmp\theta^i_x}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"f\ocmp\theta^i_x");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\cat{C}}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"\cat{C}");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{}").Create().TestSingle();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test").Create().TestSingle();
            def.IsSameType(math, parameters).IsFalse();

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{f}{\theta^i_x}").Create().TestSingle();
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameType1添え字()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\test^{#1}");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\test^{a}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("a");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test^{abc}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test^C");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("C");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test^{a^y}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("a^y");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test");
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameType1添え字の添え字()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\alpha_{#3 }");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\alpha_{\id^b}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#3"].ToTokenString().TestString(@"\id^b");
        }

        [TestMethod]
        public void IsSameTypeハイフン1()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\Hom(-, x)");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\Hom(a, x)");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["-"].ToTokenString().TestString("a");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\Hom(a+b, x)");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["-"].ToTokenString().TestString("a+b");
        }

        [TestMethod]
        public void IsSameTypeハイフン2()
        {
            var def = new MathObjectFactory(@"\Hom(-, F(x))").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"\Hom(a, Fx)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["-"].ToTokenString().TestString("a");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\Hom(a+b, F(x))").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["-"].ToTokenString().TestString("a+b");
        }

        [TestMethod]
        public void IsSameType2A()
        {
            var def = new MathObjectFactory(@"\test{#1#2}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"\test{ab}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].IsMathToken("a");
            parameters["#2"].IsMathToken("b");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test(ab)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].IsMathToken("a");
            parameters["#2"].IsMathToken("b");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\alpha\beta}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].IsMathToken(@"\alpha");
            parameters["#2"].IsMathToken(@"\beta");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\encat{A}\encat{B}}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"\encat{A}");
            parameters["#2"].ToTokenString().TestString(@"\encat{B}");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\alpha^i\beta_j}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"\alpha^i");
            parameters["#2"].ToTokenString().TestString(@"\beta_j");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{aa, bb}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString("aa");
            parameters["#2"].ToTokenString().TestString("bb");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{abc}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"a");
            parameters["#2"].ToTokenString().TestString(@"bc");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test ab").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsFalse();

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{a}{b}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameType2B()
        {
            var def = new MathObjectFactory(@"\test{#1\times #2}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"\test{a\times b}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString("a");
            parameters["#2"].ToTokenString().TestString("b");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{ \alpha \times \beta }").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"\alpha");
            parameters["#2"].ToTokenString().TestString(@"\beta");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\alpha a\times \beta b}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"\alpha a");
            parameters["#2"].ToTokenString().TestString(@"\beta b");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\test{\alpha \Set\times \beta \Cat}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"\alpha \Set");
            parameters["#2"].ToTokenString().TestString(@"\beta \Cat");
        }

        [TestMethod]
        public void IsSameType2C()
        {
            var def = new MathObjectFactory(@"\Hom_{\cat{C}}(#1, #2)").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"\Hom_{\cat{C}}(a, b)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString("a");
            parameters["#2"].ToTokenString().TestString("b");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\Hom_{\cat{C}}(\alpha, \beta)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"\alpha");
            parameters["#2"].ToTokenString().TestString(@"\beta");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"\Hom_{\cat{C}}(\alpha\beta)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameType3()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"M^{#1#2#3}");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"M^{abc}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(3);
            parameters["#1"].ToTokenString().TestString("a");
            parameters["#2"].ToTokenString().TestString("b");
            parameters["#3"].ToTokenString().TestString("c");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"M^{Fa, Fb, Fc}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(3);
            parameters["#1"].ToTokenString().TestString("Fa");
            parameters["#2"].ToTokenString().TestString("Fb");
            parameters["#3"].ToTokenString().TestString("Fc");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"M^{ab}");
            def.IsSameType(math, parameters).IsFalse();

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"M^{abc}_x");
            def.IsSameType(math, parameters).IsFalse();


            def = ExtensionsInTest.CreateSingleMathObject(@"(#1#2)#3");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"((\encat{C}_{cd}\encat{D}_{c'd'})(\encat{C}_{bc}\encat{D}_{b'c'}))(\encat{C}_{ab}\encat{D}_{a'b'})");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(3);
            parameters["#1"].ToTokenString().TestString(@"\encat{C}_{cd}\encat{D}_{c'd'}");
            parameters["#2"].ToTokenString().TestString(@"\encat{C}_{bc}\encat{D}_{b'c'}");
            parameters["#3"].ToTokenString().TestString(@"\encat{C}_{ab}\encat{D}_{a'b'}");
        }

        [TestMethod]
        public void IsSameTypeNull通常()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\test {#1?}");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\test{a}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("a");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test");
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameTypeNull添え字上()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\test^{#1?}_{#2#3}");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\test^{a}_{ c d }");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(3);
            parameters["#1"].ToTokenString().TestString("a");
            parameters["#2"].ToTokenString().TestString("c");
            parameters["#3"].ToTokenString().TestString("d");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test _{cd}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#2"].ToTokenString().TestString("c");
            parameters["#3"].ToTokenString().TestString("d");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test^{c d }");
            def.IsSameType(math, parameters).IsFalse();

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test");
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameTypeNull添え字両方()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\test^{#1?}_{#2?#3?}#4");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\test^{a}_{ c d }xyz");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(4);
            parameters["#1"].ToTokenString().TestString("a");
            parameters["#2"].ToTokenString().TestString("c");
            parameters["#3"].ToTokenString().TestString("d");
            parameters["#4"].ToTokenString().TestString("xyz");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test _{cd}xyz");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(3);
            parameters["#2"].ToTokenString().TestString("c");
            parameters["#3"].ToTokenString().TestString("d");
            parameters["#4"].ToTokenString().TestString("xyz");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test^ {cd}xyz");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString("cd");
            parameters["#4"].ToTokenString().TestString("xyz");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\test xyz");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#4"].ToTokenString().TestString("xyz");
        }

        [TestMethod]
        public void IsSameType複数同じもの()
        {
            var def = new MathObjectFactory(@"[#1, #2]\otimes #1").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"[u, v]\otimes u").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].IsMathToken("u");
            parameters["#2"].IsMathToken("v");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[u\otimes v, w\otimes v]\otimes (u\otimes v)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].AsMathSequence().List.Count.Is(3);
            parameters["#1"].AsMathSequence().List[0].IsMathToken("u");
            parameters["#1"].AsMathSequence().List[1].IsMathToken(@"\otimes");
            parameters["#1"].AsMathSequence().List[2].IsMathToken("v");
            parameters["#2"].AsMathSequence().List.Count.Is(3);
            parameters["#2"].AsMathSequence().List[0].IsMathToken("w");
            parameters["#2"].AsMathSequence().List[1].IsMathToken(@"\otimes");
            parameters["#2"].AsMathSequence().List[2].IsMathToken("v");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[u\otimes (v\otimes w), x]\otimes (u\otimes (v\otimes w))").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].AsMathSequence().List.Count.Is(3);
            parameters["#1"].ToTokenString().TestString(@"u\otimes (v\otimes w)");
            parameters["#2"].IsMathToken("x");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[\Vunit, u]\otimes \Vunit").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].IsMathToken(@"\Vunit");
            parameters["#2"].IsMathToken("u");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[[\Vunit, u], v]\otimes [\Vunit, u]").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString(@"[\Vunit, u]");
            parameters["#2"].IsMathToken("v");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[u, v]\otimes w").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsFalse();

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"[u\otimes (v\otimes w), x]\otimes ((u\otimes v)\otimes w)").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsFalse();
        }

        [TestMethod]
        public void IsSameType添え字括弧()
        {
            var def = new MathObjectFactory(@"(\test_{#1})_{#2}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>();
            var math = new MathObjectFactory(@"(\test_a)_b").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].IsMathToken("a");
            parameters["#2"].IsMathToken("b");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"(\test_{ab})_{cd}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString("ab");
            parameters["#2"].ToTokenString().TestString("cd");

            parameters = new Dictionary<string, MathObject>();
            math = new MathObjectFactory(@"(\test_{a_i})_{b_j}").Create().TestSingleMath();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(2);
            parameters["#1"].ToTokenString().TestString("a_i");
            parameters["#2"].ToTokenString().TestString("b_j");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_トークン1(bool setNull)
        {
            var def = new MathObjectFactory(@"#1").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("a").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals("a"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                if (!item.ToTokenString().Equals("a")) continue;

                item.IsMathToken("a");
            }


            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\test").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                if (!item.ToTokenString().Equals(@"\test")) continue;

                item.IsMathToken(@"\test");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_トークン2(bool setNull)
        {
            var def = new MathObjectFactory(@"#1").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abc").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals("abc"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(3);
                math.List[0].IsMathToken(@"a");
                math.List[1].IsMathToken(@"b");
                math.List[2].IsMathToken(@"c");
                math.Main.TestString(@"abc");
                math.ToTokenString().TestString("abc");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\theta_a").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\theta_a"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\theta");
                math.Main.TestString(@"\theta");
                math.Sup.IsNull();
                math.Sub.IsMathToken("a");
                math.ToTokenString().TestString(@"\theta_a");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_通常(bool setNull)
        {
            var def = new MathObjectFactory(@"#1#2#3").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abc").CreateSingle() },
                { "#2", new MathObjectFactory("def").CreateSingle() },
                { "#3", new MathObjectFactory("ghi").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"abcdefghi"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(3);
                math.List[0].ToTokenString().TestString("abc");
                math.List[1].ToTokenString().TestString("def");
                math.List[2].ToTokenString().TestString("ghi");
                math.Main.TestString(@"abcdefghi");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\alpha a").CreateSingle() },
                { "#2", new MathObjectFactory(@"\beta b").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\alpha a\beta b #3"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(3);
                math.List[0].ToTokenString().TestString(@"\alpha a");
                math.List[1].ToTokenString().TestString(@"\beta b");
                math.List[2].ToTokenString().TestString(@"#3");
                math.Main.TestString(@"\alpha a\beta b #3");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_添え字1(bool setNull)
        {
            var def = new MathObjectFactory(@"\test^{#1}_{#2#3}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{ab}_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("a");
                s.List[1].IsMathToken("b");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{#1}_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsMathToken("#1");
                var s = (MathSequence)math.Sub;
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_添え字2(bool setNull)
        {
            var def = new MathObjectFactory(@"\test^{#1}_{#2#3}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abcd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{abcd}_{#2#3}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(4);
                math.Sup.Main.TestString("abcd");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("#2");
                s.List[1].IsMathToken("#3");
                math.Main.TestString(@"\test");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab^k").CreateSingle() },
                { "#2", new MathObjectFactory("c^i").CreateSingle() },
                { "#3", new MathObjectFactory("d_j").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{ab^k}_{c^id_j}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("a");
                s.List[1].ToTokenString().TestString("b^k");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].ToTokenString().TestString("c^i");
                s.List[1].ToTokenString().TestString("d_j");
                math.Main.TestString(@"\test");
            }
        }
        [TestMethod]
        public void SetParameters_ハテナ1()
        {
            var def = new MathObjectFactory(@"#1?").Create().TestSingle();
            def.IsMathToken("#1?");

            var parameters = new Dictionary<string, MathObject>();
            var list = def.ApplyParameters(parameters, true).ToArray();
            list.Length.Is(0);

            list = def.ApplyParameters(parameters, false)
                .Where(x => x.ToTokenString().Equals(@"#1?"))
                .ToArray();
            list.Length.Is(1);
            list[0].IsMathToken(@"#1?");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_ハテナ2(bool setNull)
        {
            var def = new MathObjectFactory(@"\test^{#1?}_{#2?#3?}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{ab}_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("a");
                s.List[1].IsMathToken("b");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }


            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .Where(x => x.ToTokenString().Equals(@"\test_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsNull();
                var s = (MathSequence)math.Sub;
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }

            list = def.ApplyParameters(parameters, false)
                .Where(x => x.ToTokenString().Equals(@"\test^{#1?}_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsMathToken("#1?");
                var s = (MathSequence)math.Sub;
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }
        }

        [TestMethod]
        public void SetParameters_ハテナ3()
        {
            var def = new MathObjectFactory(@"\test^{#1?}_{#2?#3?}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abcd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true)
                .Where(x => x.ToTokenString().Equals(@"\test^{abcd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(4);
                math.Sup.Main.TestString("abcd");
                math.Sub.IsNull();
                math.Main.TestString(@"\test");
            }

            list = def.ApplyParameters(parameters, false)
                .Where(x => x.ToTokenString().Equals(@"\test^{abcd}_{#2?#3?}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(4);
                s.Main.TestString("abcd");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.Main.TestString("#2?#3?");
                math.Main.TestString(@"\test");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetParameters_カンマ(bool setNull)
        {
            var def = new MathObjectFactory(@"\test{#1, #2}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test{ab, cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(2);
                math.List[0].IsMathToken(@"\test");
                math.List[1].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken("a");
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[1].IsMathToken("b");
                math.List[1].AsMathSequence().List[1].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("c");
                math.List[1].AsMathSequence().List[1].AsMathSequence().List[1].IsMathToken("d");
                math.List[1].AsMathSequence().LeftBracket.Value.Is("{");
                math.List[1].AsMathSequence().RightBracket.Value.Is("}");
                math.List[1].AsMathSequence().Separator.Is(",");
                math.Main.TestString(@"\test{ab, cd}");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test{ab, #2}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(2);
                math.List[0].IsMathToken(@"\test");
                math.List[1].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken("a");
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[1].IsMathToken("b");
                math.List[1].AsMathSequence().List[1].IsMathToken("#2");
                math.List[1].AsMathSequence().LeftBracket.Value.Is("{");
                math.List[1].AsMathSequence().RightBracket.Value.Is("}");
                math.List[1].AsMathSequence().Separator.Is(",");
                math.Main.TestString(@"\test{ab, #2}");
            }
        }

        [DataTestMethod]
        [DataRow(@"\test^{#1?^{#2}}")]
        [DataRow(@"\test^{#1?^{#2?}}")]
        public void SetParameters_添え字二重1(string text)
        {
            var def = new MathObjectFactory(text).Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab^{cd}"))
                {
                    s.Main.TestString("ab");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
                else if (s.ToTokenString().Equals("(ab)^{cd}"))
                {
                    s.Main.TestString("(ab)");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab"))
                {
                    s.Main.TestString("ab");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("ab");
                    s.ToTokenString().TestString("ab");
                }
                else if (s.ToTokenString().Equals("(ab)"))
                {
                    s.Main.TestString("(ab)");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("(ab)");
                    s.ToTokenString().TestString("(ab)");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsNull();
                math.Sub.IsNull();
                math.Main.TestString(@"\test");
                math.ToTokenString().TestString(@"\test");
            }
        }


        [TestMethod]
        public void SetParameters_添え字二重2()
        {
            var def = new MathObjectFactory(@"\test^{#1^{#2?}}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab^{cd}"))
                {
                    s.Main.TestString("ab");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
                else if (s.ToTokenString().Equals("(ab)^{cd}"))
                {
                    s.Main.TestString("(ab)");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab"))
                {
                    s.Main.TestString("ab");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("ab");
                    s.ToTokenString().TestString("ab");
                }
                else if (s.ToTokenString().Equals("(ab)"))
                {
                    s.Main.TestString("(ab)");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("(ab)");
                    s.ToTokenString().TestString("(ab)");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                s.Main.TestString("#1");
                s.ToTokenString().TestString("#1^{cd}");
                s = (MathSequence)s.Sup;
                s.List.Count.Is(2);
                s.Main.TestString("cd");

                math.ToTokenString().TestString(@"\test^{#1^{cd}}");
            }
        }
    }
}
