using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class IsSameTypeのテスト
    {
        [TestMethod]
        public void 変数単体()
        {
            var def = new MathObjectFactory(@"#1").Create().TestSingle();
            def.IsMathToken(@"#1");

            var math = new MathObjectFactory(@"a").Create().TestSingle();
            var parameters = new Dictionary<string, MathObject>();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].IsMathToken("a");

            math = new MathObjectFactory(@"abc").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            math = new MathObjectFactory(@"(abc)").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("abc");

            math = new MathObjectFactory(@"[u, v]").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("[u, v]");

            math = new MathObjectFactory(@"{abc}{def}").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("{abc}{def}");
        }

        [TestMethod]
        public void 変数と文字()
        {
            var def = new MathObjectFactory(@"xy#1").Create().TestSingle();

            var math = new MathObjectFactory(@"xyz").Create().TestSingle();
            var parameters = new Dictionary<string, MathObject>();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("z");

            math = new MathObjectFactory(@"xyzzz").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("zzz");

            def = new MathObjectFactory(@"x#1z").Create().TestSingle();
            math = new MathObjectFactory(@"xyz").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("y");

            math = new MathObjectFactory(@"xyyyz").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("yyy");

            def = new MathObjectFactory(@"#1yz").Create().TestSingle();
            math = new MathObjectFactory(@"xyz").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("x");

            math = new MathObjectFactory(@"xxxyz").Create().TestSingle();
            parameters.Clear();
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("xxx");
        }

        [TestMethod]
        public void 小括弧1()
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
        public void 小括弧2()
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
        public void 中括弧()
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
        public void 添え字が変数()
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
        public void 変数に添え字()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"#1_a");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"x_a");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("x");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"xyz_a");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString("xyz");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\cat{C}_a");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"\cat{C}");

            parameters = new Dictionary<string, MathObject>();
            math = ExtensionsInTest.CreateSingleMathObject(@"\cat{C}^2_a");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#1"].ToTokenString().TestString(@"\cat{C}^2");
        }

        [TestMethod]
        public void 添え字の添え字()
        {
            var def = ExtensionsInTest.CreateSingleMathObject(@"\alpha_{#3 }");

            var parameters = new Dictionary<string, MathObject>();
            var math = ExtensionsInTest.CreateSingleMathObject(@"\alpha_{\id^b}");
            def.IsSameType(math, parameters).IsTrue();
            parameters.Count.Is(1);
            parameters["#3"].ToTokenString().TestString(@"\id^b");
        }

        [TestMethod]
        public void ハイフン1()
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
        public void ハイフン2()
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
        public void 複数変数()
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
        public void 複数変数2()
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
        public void 複数変数3()
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
        public void 複数変数_3変数()
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
        public void 省略可変数_通常()
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
        public void 省略可変数_添え字上()
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
        public void 省略可変数_添え字上下()
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
        public void 複数同じ変数()
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
        public void 変数に添え字括弧()
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
    }
}
