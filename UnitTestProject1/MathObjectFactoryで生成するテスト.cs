using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class MathObjectFactoryで生成するテスト
    {
        [TestMethod]
        public void 通常()
        {
            var token = new MathObjectFactory("F").Create().TestSingle();
            token.IsMathToken("F");
            token.ToTokenString().TestString("F");
            token.OriginalText.Is("F");

            var math = CreateSingleSequence("Fg");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("F");
            math.List[1].IsMathToken("g");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.Main.TestString("Fg");
            math.ToTokenString().TestString("Fg");

            math = CreateSingleSequence("abcdefg");
            math.List.Count.Is(7);
            math.List[0].IsMathToken("a");
            math.List[1].IsMathToken("b");
            math.List[2].IsMathToken("c");
            math.List[3].IsMathToken("d");
            math.List[4].IsMathToken("e");
            math.List[5].IsMathToken("f");
            math.List[6].IsMathToken("g");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.Main.TestString("abcdefg");
            math.ToTokenString().TestString("abcdefg");
            math.OriginalText.Is("abcdefg");
        }

        [TestMethod]
        public void TeXコマンド()
        {
            var token = new MathObjectFactory(@"\theta").Create().TestSingle();
            token.IsMathToken(@"\theta");
            token.ToTokenString().TestString(@"\theta");
            token.OriginalText.Is(@"\theta");

            var math = CreateSingleSequence(@"F\theta");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("F");
            math.List[1].IsMathToken(@"\theta");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.Main.TestString(@"F\theta");
            math.ToTokenString().TestString(@"F\theta");
            math.OriginalText.Is(@"F\theta");

            math = CreateSingleSequence(@"\Diagonal a");
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\Diagonal");
            math.List[1].IsMathToken("a");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.Main.TestString(@"\Diagonal a");
            math.ToTokenString().TestString(@"\Diagonal a");
            math.OriginalText.Is(@"\Diagonal a");

            math = CreateSingleSequence(@"\cat{C}");
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\cat");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("C");
            math.List[1].AsMathSequence().LeftBracket.TestToken("{");
            math.List[1].AsMathSequence().RightBracket.TestToken("}");
            math.List[1].ToTokenString().TestString("{C}");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.Main.TestString(@"\cat{C}");
            math.ToTokenString().TestString(@"\cat{C}");
            math.OriginalText.Is(@"\cat{C}");

            math = CreateSingleSequence(@"\encat{C}\otimes\encat{D}");
            math.List.Count.Is(5);
            math.List[0].IsMathToken("\\encat");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("C");
            math.List[1].AsMathSequence().LeftBracket.TestToken("{");
            math.List[1].AsMathSequence().RightBracket.TestToken("}");
            math.List[2].IsMathToken("\\otimes");
            math.List[3].IsMathToken("\\encat");
            math.List[4].AsMathSequence().List.Count.Is(1);
            math.List[4].AsMathSequence().List[0].IsMathToken("D");
            math.List[4].AsMathSequence().LeftBracket.TestToken("{");
            math.List[4].AsMathSequence().RightBracket.TestToken("}");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.Main.TestString(@"\encat{C}\otimes\encat{D}");
            math.ToTokenString().TestString(@"\encat{C}\otimes\encat{D}");
            math.OriginalText.Is(@"\encat{C}\otimes\encat{D}");
        }

        [TestMethod]
        public void スペース()
        {
            var math = CreateSingleSequence(@"A BC");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("A");
            math.List[1].IsMathToken("B");
            math.List[2].IsMathToken("C");
            math.Main.TestString("ABC");
            math.ToTokenString().TestString("ABC");
            math.OriginalText.Is(@"A BC");

            math = CreateSingleSequence(@"\cat { C }");
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\cat");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("C");
            math.List[1].AsMathSequence().LeftBracket.TestToken("{");
            math.List[1].AsMathSequence().RightBracket.TestToken("}");
            math.List[1].ToTokenString().TestString("{C}");
            math.Main.TestString(@"\cat{C}");
            math.ToTokenString().TestString(@"\cat{C}");
            math.OriginalText.Is(@"\cat { C }");

            math = CreateSingleSequence(@" U V W ");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("U");
            math.List[1].IsMathToken("V");
            math.List[2].IsMathToken("W");
            math.Main.TestString("UVW");
            math.ToTokenString().TestString("U V W");
            math.OriginalText.Is(@"U V W");
        }

        [TestMethod]
        public void 無視するコマンド()
        {
            var math = CreateSingleSequence(@"F\,\downarrow\,G");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("F");
            math.List[1].IsMathToken("\\downarrow");
            math.List[2].IsMathToken("G");
            math.Main.TestString(@"F\downarrow G");
            math.ToTokenString().TestString(@"F\downarrow G");
            math.OriginalText.Is(@"F\,\downarrow\,G");

            math = CreateSingleSequence(@"A\ BC");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("A");
            math.List[1].IsMathToken("B");
            math.Main.TestString("ABC");
            math.ToTokenString().TestString("ABC");
            math.OriginalText.Is(@"A\ BC");

            math = CreateSingleSequence(@"AX\quad B");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("A");
            math.List[1].IsMathToken("X");
            math.List[2].IsMathToken("B");
            math.Main.TestString("AXB");
            math.ToTokenString().TestString("AXB");
            math.OriginalText.Is(@"AX\quad B");

            math = CreateSingleSequence(@"UVW\qquad");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("U");
            math.List[1].IsMathToken("V");
            math.List[2].IsMathToken("W");
            math.Main.TestString("UVW");
            math.ToTokenString().TestString("UVW");
            math.OriginalText.Is(@"UVW");

            math = CreateSingleSequence(@"\displaystyle XYZ");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("X");
            math.List[1].IsMathToken("Y");
            math.List[2].IsMathToken("Z");
            math.Main.TestString("XYZ");
            math.ToTokenString().TestString("XYZ");
            math.OriginalText.Is(@"XYZ");
        }

        [TestMethod]
        public void 空()
        {
            var math = new MathObjectFactory("").CreateSingle();
            math.OriginalText.Is("");
            math.AsMathSequence().List.Count.Is(0);
            math.Main.TestString("");
            math.ToTokenString().TestString("");

            math = new MathObjectFactory("{}").CreateSingle();
            math.OriginalText.Is("{}");
            math.AsMathSequence().List.Count.Is(0);
            math.Main.TestString("");
            math.AsMathSequence().LeftBracket.TestToken("{");
            math.AsMathSequence().RightBracket.TestToken("}");
            math.ToTokenString().TestString("{}");

            math = new MathObjectFactory("{}^i").CreateSingle();
            math.OriginalText.Is("{}^i");
            math.AsMathSequence().List.Count.Is(0);
            math.Main.TestString("");
            math.AsMathSequence().LeftBracket.TestToken("{");
            math.AsMathSequence().RightBracket.TestToken("}");
            math.AsMathSequence().Sup.IsMathToken("i");
            math.AsMathSequence().Sub.IsNull();
            math.ToTokenString().TestString("{}^i");

            math = new MathObjectFactory("{}_j").CreateSingle();
            math.OriginalText.Is("{}_j");
            math.AsMathSequence().List.Count.Is(0);
            math.Main.TestString("");
            math.AsMathSequence().LeftBracket.TestToken("{");
            math.AsMathSequence().RightBracket.TestToken("}");
            math.AsMathSequence().Sup.IsNull();
            math.AsMathSequence().Sub.IsMathToken("j");
            math.ToTokenString().TestString("{}_j");

            math = new MathObjectFactory("{}^i_j").CreateSingle();
            math.OriginalText.Is("{}^i_j");
            math.AsMathSequence().List.Count.Is(0);
            math.Main.TestString("");
            math.AsMathSequence().LeftBracket.TestToken("{");
            math.AsMathSequence().RightBracket.TestToken("}");
            math.AsMathSequence().Sup.IsMathToken("i");
            math.AsMathSequence().Sub.IsMathToken("j");
            math.ToTokenString().TestString("{}^i_j");
        }
        [TestMethod]
        public void Prime()
        {
            var math = CreateSingleSequence(@"a'");
            math.List.Count.Is(1);
            math.Sup.IsMathToken(@"\prime");
            math.Sub.IsNull();
            math.Main.TestString("a");
            math.ToTokenString().TestString(@"a^{\prime}");

            math = CreateSingleSequence(@"a''");
            math.List.Count.Is(1);
            math.Sup.AsMathSequence().List.Count.Is(2);
            math.Sup.AsMathSequence().List[0].IsMathToken(@"\prime");
            math.Sup.AsMathSequence().List[1].IsMathToken(@"\prime");
            math.Sub.IsNull();
            math.Main.TestString("a");
            math.ToTokenString().TestString(@"a^{\prime\prime}");

            math = CreateSingleSequence(@"\alpha'_{ij}");
            math.List.Count.Is(1);
            math.Sup.IsMathToken(@"\prime");
            math.Sub.AsMathSequence().List.Count.Is(2);
            math.Sub.AsMathSequence().Sup.IsNull();
            math.Sub.AsMathSequence().Sub.IsNull();
            math.Sub.AsMathSequence().ToTokenString().TestString("ij");
            math.Main.TestString(@"\alpha");
            math.ToTokenString().TestString(@"\alpha^{\prime}_{ij}");

            math = CreateSingleSequence(@"\alpha'^{ij}");
            math.List.Count.Is(1);
            math.Sup.AsMathSequence().List.Count.Is(3);
            math.Sup.AsMathSequence().List[0].IsMathToken(@"\prime");
            math.Sup.AsMathSequence().List[1].IsMathToken(@"i");
            math.Sup.AsMathSequence().List[2].IsMathToken(@"j");
            math.Sup.AsMathSequence().Sup.IsNull();
            math.Sup.AsMathSequence().Sub.IsNull();
            math.Sup.AsMathSequence().ToTokenString().TestString(@"\prime ij");
            math.Sub.IsNull();
            math.Main.TestString(@"\alpha");
            math.ToTokenString().TestString(@"\alpha^{\prime ij}");
        }

        [TestMethod]
        public void 複数生成()
        {
            var math = new MathObjectFactory("f, gh, ijk").Create().ToArray();
            math.Length.Is(3);
            math[0].IsMathToken("f");
            math[0].ToTokenString().TestString("f");
            math[0].OriginalText.Is("f");
            math[1].AsMathSequence().List.Count.Is(2);
            math[1].AsMathSequence().List[0].IsMathToken("g");
            math[1].AsMathSequence().List[1].IsMathToken("h");
            math[1].Main.TestString("gh");
            math[1].ToTokenString().TestString("gh");
            math[1].OriginalText.Is("gh");
            math[2].AsMathSequence().List.Count.Is(3);
            math[2].AsMathSequence().List[0].IsMathToken("i");
            math[2].AsMathSequence().List[1].IsMathToken("j");
            math[2].AsMathSequence().List[2].IsMathToken("k");
            math[2].Main.TestString("ijk");
            math[2].ToTokenString().TestString("ijk");
            math[2].OriginalText.Is("ijk");


            math = new MathObjectFactory(@"\alpha , \beta").Create().ToArray();
            math.Length.Is(2);
            math[0].IsMathToken(@"\alpha");
            math[0].ToTokenString().TestString(@"\alpha");
            math[0].OriginalText.Is(@"\alpha");
            math[1].IsMathToken(@"\beta");
            math[1].ToTokenString().TestString(@"\beta");
            math[1].OriginalText.Is(@"\beta");

            math = new MathObjectFactory("u=v").Create().ToArray();
            math.Length.Is(2);
            math[0].IsMathToken("u");
            math[0].ToTokenString().TestString("u");
            math[0].OriginalText.Is("u");
            math[1].IsMathToken("v");
            math[1].ToTokenString().TestString("v");
            math[1].OriginalText.Is("v");

            math = new MathObjectFactory(@"abc\, = \, def").Create().ToArray();
            math.Length.Is(2);
            math[0].AsMathSequence().List.Count.Is(3);
            math[0].Main.TestString("abc");
            math[0].ToTokenString().TestString("abc");
            math[0].OriginalText.Is("abc");
            math[1].AsMathSequence().List.Count.Is(3);
            math[1].Main.TestString("def");
            math[1].ToTokenString().TestString("def");
            math[1].OriginalText.Is(@"\, def");

            math = new MathObjectFactory(@"\beta_i\cong \gamma_j").Create().ToArray();
            math.Length.Is(2);
            math[0].AsMathSequence().List.Count.Is(1);
            math[0].Main.TestString(@"\beta");
            math[0].ToTokenString().TestString(@"\beta_i");
            math[0].OriginalText.Is(@"\beta_i");
            math[1].AsMathSequence().List.Count.Is(1);
            math[1].Main.TestString(@"\gamma");
            math[1].ToTokenString().TestString(@"\gamma_j");
            math[1].OriginalText.Is(@"\gamma_j");
        }

        [TestMethod]
        public void 複数生成されない()
        {
            var math = CreateSingleSequence(@"\pair{f, g}");
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\pair");
            math.List[1].AsMathSequence().List.Count.Is(2);
            math.List[1].AsMathSequence().List[0].IsMathToken("f");
            math.List[1].AsMathSequence().List[1].IsMathToken("g");
            math.List[1].Main.TestString("f,g");
            math.Main.TestString(@"\pair{f,g}");
            math.ToTokenString().TestString(@"\pair{f,g}");
            math.OriginalText.Is(@"\pair{f, g}");

            math = CreateSingleSequence("(f, g)");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("f");
            math.List[1].IsMathToken("g");
            math.Main.TestString("f,g");
            math.ToTokenString().TestString("(f,g)");
            math.OriginalText.Is("(f, g)");

            math = CreateSingleSequence(@"\Hom_{\cat{C}}(Fa, Ga)");
            math.List.Count.Is(2);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].IsMathToken(@"\Hom");
            math.List[0].AsMathSequence().Sub.AsMathSequence().List.Count.Is(2);
            math.List[0].AsMathSequence().Sub.AsMathSequence().List[0].IsMathToken("\\cat");
            math.List[0].AsMathSequence().Sub.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().Sub.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("C");
            math.List[0].AsMathSequence().Sub.AsMathSequence().List[1].ToTokenString().TestString("{C}");
            math.List[0].AsMathSequence().Sub.AsMathSequence().LeftBracket.TestToken("");
            math.List[0].AsMathSequence().Sub.AsMathSequence().RightBracket.TestToken("");
            math.List[0].AsMathSequence().Sub.Main.TestString(@"\cat{C}");
            math.List[0].ToTokenString().TestString(@"\Hom_{\cat{C}}");
            math.List[1].AsMathSequence().List.Count.Is(2);
            math.List[1].AsMathSequence().List[0].AsMathSequence().List.Count.Is(2);
            math.List[1].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken("F");
            math.List[1].AsMathSequence().List[0].AsMathSequence().List[1].IsMathToken("a");
            math.List[1].AsMathSequence().List[1].AsMathSequence().List.Count.Is(2);
            math.List[1].AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("G");
            math.List[1].AsMathSequence().List[1].AsMathSequence().List[1].IsMathToken("a");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].Main.TestString("Fa,Ga");
            math.List[1].ToTokenString().TestString("(Fa,Ga)");
            math.Main.TestString(@"\Hom_{\cat{C}}(Fa,Ga)");
            math.ToTokenString().TestString(@"\Hom_{\cat{C}}(Fa,Ga)");
            math.OriginalText.Is(@"\Hom_{\cat{C}}(Fa, Ga)");
        }

        [TestMethod]
        public void 添え字()
        {
            var seq = CreateSingleSequence("p_j");
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken("p");
            seq.Sup.IsNull();
            seq.Sub.IsMathToken("j");
            seq.Main.TestString("p");
            seq.ToTokenString().TestString("p_{j}");

            seq = CreateSingleSequence("F^*");
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken("F");
            seq.Sup.IsMathToken("*");
            seq.Sub.IsNull();
            seq.Main.TestString("F");
            seq.ToTokenString().TestString("F^*");

            seq = CreateSingleSequence(@"\theta_ i");
            seq.List.Count.Is(1);
            seq.Sup.IsNull();
            seq.Sub.IsMathToken("i");
            seq.Main.TestString(@"\theta");
            seq.ToTokenString().TestString(@"\theta_{i}");

            seq = CreateSingleSequence(@"\theta_{abc}{de}");
            seq.List.Count.Is(2);
            var x = seq.List[0].AsMathSequence();
            x.Sup.IsNull();
            x.Sub.AsMathSequence().List.Count.Is(3);
            x.Sub.Main.TestString("abc");
            x.Sub.AsMathSequence().LeftBracket.TestToken("");
            x.Sub.AsMathSequence().RightBracket.TestToken("");
            x.Main.TestString(@"\theta");
            x.ToTokenString().TestString(@"\theta_{abc}");
            seq.List[1].ToTokenString().TestString("{de}");
            seq.ToTokenString().TestString(@"\theta_{abc}{de}");

            seq = CreateSingleSequence(@"\theta _abc");
            seq.List.Count.Is(3);
            seq.List[0].Main.TestString(@"\theta");
            seq.List[0].AsMathSequence().Sup.IsNull();
            seq.List[0].AsMathSequence().Sub.IsMathToken("a");
            seq.List[1].IsMathToken("b");
            seq.List[2].IsMathToken("c");
            seq.Main.TestString(@"\theta_{a}bc");
            seq.ToTokenString().TestString(@"\theta_{a}bc");

            seq = CreateSingleSequence(@"G\theta _\index");
            seq.List.Count.Is(2);
            seq.List[0].IsMathToken("G");
            seq.List[1].Main.TestString(@"\theta");
            seq.List[1].AsMathSequence().Sub.IsMathToken(@"\index");
            seq.Main.TestString(@"G\theta_{\index}");
            seq.ToTokenString().TestString(@"G\theta_{\index}");

            seq = CreateSingleSequence(@"G\theta_{ \index }");
            seq.List.Count.Is(2);
            seq.List[0].IsMathToken("G");
            seq.List[1].Main.TestString(@"\theta");
            seq.List[1].AsMathSequence().Sub.IsMathToken(@"\index");
            seq.Main.TestString(@"G\theta_{\index}");
            seq.ToTokenString().TestString(@"G\theta_{\index}");

            seq = CreateSingleSequence(@"{f}{p _i}{\alpha^k}");
            seq.List.Count.Is(3);
            seq.List[0].AsMathSequence().List.Count.Is(1);
            seq.List[0].AsMathSequence().List[0].IsMathToken("f");
            seq.List[0].ToTokenString().TestString("{f}");
            seq.List[1].AsMathSequence().List.Count.Is(1);
            seq.List[1].AsMathSequence().List[0].AsMathSequence().List.Count.Is(1);
            seq.List[1].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken("p");
            seq.List[1].AsMathSequence().List[0].AsMathSequence().Sup.IsNull();
            seq.List[1].AsMathSequence().List[0].AsMathSequence().Sub.IsMathToken("i");
            seq.List[1].Main.TestString("p_i");
            seq.List[1].AsMathSequence().Sup.IsNull();
            seq.List[1].AsMathSequence().Sub.IsNull();
            seq.List[2].AsMathSequence().List.Count.Is(1);
            seq.List[2].AsMathSequence().List[0].AsMathSequence().List.Count.Is(1);
            seq.List[2].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken(@"\alpha");
            seq.List[2].AsMathSequence().List[0].AsMathSequence().Sup.IsMathToken("k");
            seq.List[2].AsMathSequence().List[0].AsMathSequence().Sub.IsNull();
            seq.List[2].Main.TestString(@"\alpha^{k}");
            seq.List[2].AsMathSequence().Sup.IsNull();
            seq.List[2].AsMathSequence().Sub.IsNull();
            seq.Main.TestString(@"{f}{p_{i}}{\alpha^{k}}");
            seq.ToTokenString().TestString(@"{f}{p_{i}}{\alpha^{k}}");
        }

        [TestMethod]
        public void 添え字_複数()
        {
            var math = CreateSingleSequence("A_j^{(i)}");
            math.List.Count.Is(1);
            math.List[0].IsMathToken("A");
            math.Sup.AsMathSequence().List.Count.Is(1);
            math.Sup.AsMathSequence().List[0].IsMathToken("i");
            math.Sup.AsMathSequence().LeftBracket.TestToken("(");
            math.Sup.AsMathSequence().RightBracket.TestToken(")");
            math.Sup.Main.TestString("i");
            math.Sup.ToTokenString().TestString("(i)");
            math.Sub.IsMathToken("j");
            math.Main.TestString("A");
            math.ToTokenString().TestString("A_{j}^{(i)}");

            math = CreateSingleSequence("A^{(i)}_j");
            math.List.Count.Is(1);
            math.List[0].IsMathToken("A");
            math.Sup.AsMathSequence().List.Count.Is(1);
            math.Sup.AsMathSequence().List[0].IsMathToken("i");
            math.Sup.AsMathSequence().LeftBracket.TestToken("(");
            math.Sup.AsMathSequence().RightBracket.TestToken(")");
            math.Sup.Main.TestString("i");
            math.Sup.ToTokenString().TestString("(i)");
            math.Sub.Main.TestString("j");
            math.Main.TestString("A");
            math.ToTokenString().TestString("A^{(i)}_{j}");

            math = CreateSingleSequence("U_{i_j}");
            math.List.Count.Is(1);
            math.List[0].IsMathToken("U");
            math.Sup.IsNull();
            math.Sub.AsMathSequence().List.Count.Is(1);
            math.Sub.AsMathSequence().List[0].IsMathToken("i");
            math.Sub.AsMathSequence().Sup.IsNull();
            math.Sub.AsMathSequence().Sub.IsMathToken("j");
            math.Sub.AsMathSequence().LeftBracket.TestToken("");
            math.Sub.AsMathSequence().RightBracket.TestToken("");
            math.Sub.Main.TestString("i");
            math.Sub.ToTokenString().TestString("i_j");
            math.Main.TestString("U");
            math.ToTokenString().TestString("U_{i_{j}}");

            math = CreateSingleSequence(@"(\Gamma_i)_x");
            math.List.Count.Is(1);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].IsMathToken(@"\Gamma");
            math.List[0].AsMathSequence().Sup.IsNull();
            math.List[0].AsMathSequence().Sub.Main.TestString("i");
            math.Sup.IsNull();
            math.Sub.IsMathToken("x");
            math.LeftBracket.TestToken("(");
            math.RightBracket.TestToken(")");
            math.Main.TestString(@"\Gamma_i");
            math.ToTokenString().TestString(@"(\Gamma_i)_x");

            math = CreateSingleSequence(@"(\mu_{j_0})_{i_1}");
            math.List.Count.Is(1);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].IsMathToken(@"\mu");
            math.List[0].AsMathSequence().Sup.IsNull();
            math.List[0].AsMathSequence().Sub.Main.TestString("j");
            math.List[0].AsMathSequence().Sub.ToTokenString().TestString("j_0");
            math.Sup.IsNull();
            math.Sub.ToTokenString().TestString("i_1");
            math.LeftBracket.TestToken("(");
            math.RightBracket.TestToken(")");
            math.Main.TestString(@"\mu_{j_0}");
            math.ToTokenString().TestString(@"(\mu_{j_0})_{i_1}");

            math = CreateSingleSequence(@"((\phi_{gf})_{\sigma})_d");
            math.List.Count.Is(1);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken(@"\phi");
            math.List[0].AsMathSequence().List[0].AsMathSequence().Sup.IsNull();
            math.List[0].AsMathSequence().List[0].AsMathSequence().Sub.Main.TestString("gf");
            math.List[0].AsMathSequence().List[0].ToTokenString().TestString(@"\phi_{gf}");
            math.List[0].AsMathSequence().Sup.IsNull();
            math.List[0].AsMathSequence().Sub.IsMathToken(@"\sigma");
            math.List[0].Main.TestString(@"\phi_{gf}");
            math.List[0].ToTokenString().TestString(@"(\phi_{gf})_{\sigma}");
            math.Sup.IsNull();
            math.Sub.IsMathToken("d");
            math.LeftBracket.TestToken("(");
            math.RightBracket.TestToken(")");
            math.Main.TestString(@"(\phi_{gf})_{\sigma}");
            math.ToTokenString().TestString(@"((\phi_{gf})_{\sigma})_d");

            math = CreateSingleSequence(@"((\phi_{gf})^{\sigma})_d");
            math.List.Count.Is(1);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken(@"\phi");
            math.List[0].AsMathSequence().List[0].AsMathSequence().Sup.IsNull();
            math.List[0].AsMathSequence().List[0].AsMathSequence().Sub.Main.TestString("gf");
            math.List[0].AsMathSequence().List[0].ToTokenString().TestString(@"\phi_{gf}");
            math.List[0].AsMathSequence().Sup.IsMathToken(@"\sigma");
            math.List[0].AsMathSequence().Sub.IsNull();
            math.List[0].Main.TestString(@"\phi_{gf}");
            math.List[0].ToTokenString().TestString(@"(\phi_{gf})^{\sigma}");
            math.Sup.IsNull();
            math.Sub.IsMathToken("d");
            math.LeftBracket.TestToken("(");
            math.RightBracket.TestToken(")");
            math.Main.TestString(@"(\phi_{gf})^{\sigma}");
            math.ToTokenString().TestString(@"((\phi_{gf})^{\sigma})_d");
        }

        [TestMethod]
        public void 添え字で開始()
        {
            var math = CreateSingleSequence("^i");
            math.List.Count.Is(0);
            math.Main.TestString("");
            math.Sup.IsMathToken("i");
            math.Sub.IsNull();
            math.ToTokenString().TestString("^i");

            math = CreateSingleSequence(@"_ \test");
            math.List.Count.Is(0);
            math.Main.TestString("");
            math.Sup.IsNull();
            math.Sub.IsMathToken(@"\test");
            math.ToTokenString().TestString(@"_\test");

            math = CreateSingleSequence("^ { a b c }_j");
            math.List.Count.Is(0);
            math.Main.TestString("");
            math.Sup.ToTokenString().TestString("abc");
            math.Sub.IsMathToken("j");
            math.ToTokenString().TestString("^{abc}_j");
        }

        [TestMethod]
        public void 添え字が空()
        {
            var token = new MathObjectFactory("p_{}").Create().TestSingle();
            token.IsMathToken("p");
            token.ToTokenString().TestString("p");
            token.OriginalText.Is("p");

            var seq = CreateSingleSequence(@"\cat{C}_{}");
            seq.List.Count.Is(2);
            seq.List[0].IsMathToken(@"\cat");
            seq.List[1].AsMathSequence().List.Count.Is(1);
            seq.List[1].AsMathSequence().List[0].IsMathToken("C");
            seq.List[1].AsMathSequence().LeftBracket.TestToken("{");
            seq.List[1].AsMathSequence().RightBracket.TestToken("}");
            seq.List[1].ToTokenString().TestString("{C}");
            seq.Sup.IsNull();
            seq.Sub.IsNull();
            seq.Main.TestString(@"\cat{C}");
            seq.ToTokenString().TestString(@"\cat{C}");

            token = new MathObjectFactory(@"p_{\mathstrut}").Create().TestSingle();
            token.IsMathToken("p");
            token.ToTokenString().TestString("p");
            token.OriginalText.Is("p");
        }

            [TestMethod]
        public void 括弧()
        {
            var math = CreateSingleSequence("F(f)");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("F");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("f");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].ToTokenString().TestString("(f)");
            math.Main.TestString("F(f)");
            math.ToTokenString().TestString("F(f)");

            math = CreateSingleSequence(@"\theta_a\otimes(\sigma_b\otimes\tau_c)");
            math.List.Count.Is(3);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].ToTokenString().TestString(@"\theta_a");
            math.List[1].IsMathToken(@"\otimes");
            math.List[2].AsMathSequence().List.Count.Is(3);
            math.List[2].AsMathSequence().List[0].ToTokenString().TestString(@"\sigma_b");
            math.List[2].AsMathSequence().List[1].IsMathToken(@"\otimes");
            math.List[2].AsMathSequence().List[2].ToTokenString().TestString(@"\tau_c");
            math.List[2].AsMathSequence().LeftBracket.TestToken("(");
            math.List[2].AsMathSequence().RightBracket.TestToken(")");
            math.List[2].Main.TestString(@"\sigma_b\otimes\tau_c");
            math.List[2].ToTokenString().TestString(@"(\sigma_b\otimes\tau_c)");
            math.Main.TestString(@"\theta_a\otimes(\sigma_b\otimes\tau_c)");
            math.ToTokenString().TestString(@"\theta_a\otimes(\sigma_b\otimes\tau_c)");

            math = CreateSingleSequence("(F)(f)");
            math.List.Count.Is(2);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].IsMathToken("F");
            math.List[0].AsMathSequence().LeftBracket.TestToken("(");
            math.List[0].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("f");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.Main.TestString("(F)(f)");
            math.ToTokenString().TestString("(F)(f)");

            math = CreateSingleSequence("{F}{f}");
            math.List.Count.Is(2);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].IsMathToken("F");
            math.List[0].AsMathSequence().LeftBracket.TestToken("{");
            math.List[0].AsMathSequence().RightBracket.TestToken("}");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("f");
            math.List[1].AsMathSequence().LeftBracket.TestToken("{");
            math.List[1].AsMathSequence().RightBracket.TestToken("}");
            math.Main.TestString("{F}{f}");
            math.ToTokenString().TestString("{F}{f}");
        }

        [TestMethod]
        public void 括弧_添え字()
        {
            var math = CreateSingleSequence("F(f)^2");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("F");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("f");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].AsMathSequence().Sup.IsMathToken("2");
            math.List[1].ToTokenString().TestString("(f)^2");
            math.Main.TestString("F(f)^2");

            math = CreateSingleSequence("F(f^i)^j");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("F");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].Main.TestString("f");
            math.List[1].AsMathSequence().List[0].AsMathSequence().Sup.IsMathToken("i");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].AsMathSequence().Sup.IsMathToken("j");
            math.List[1].ToTokenString().TestString("(f^i)^j");
            math.Main.TestString("F(f^i)^j");

            math = CreateSingleSequence("F(f^i_j)^a_b");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("F");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].Main.TestString("f");
            math.List[1].AsMathSequence().List[0].AsMathSequence().Sup.IsMathToken("i");
            math.List[1].AsMathSequence().List[0].AsMathSequence().Sub.IsMathToken("j");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].AsMathSequence().Sup.IsMathToken("a");
            math.List[1].AsMathSequence().Sub.IsMathToken("b");
            math.List[1].ToTokenString().TestString("(f^i_j)^a_b");
            math.Main.TestString("F(f^i_j)^a_b");

            math = CreateSingleSequence(@"GF(\theta_i\rho^i)^{abc}_{de}");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("G");
            math.List[1].IsMathToken("F");
            math.List[2].AsMathSequence().List.Count.Is(2);
            math.List[2].AsMathSequence().List[0].ToTokenString().TestString(@"\theta_i");
            math.List[2].AsMathSequence().List[1].ToTokenString().TestString(@"\rho^i");
            math.List[2].AsMathSequence().LeftBracket.TestToken("(");
            math.List[2].AsMathSequence().RightBracket.TestToken(")");
            math.List[2].AsMathSequence().Sup.Main.TestString("abc");
            math.List[2].AsMathSequence().Sub.Main.TestString("de");
            math.Main.TestString(@"GF(\theta_i\rho^i)^{abc}_{de}");

        }

        [TestMethod]
        public void 変数1()
        {
            var token = new MathObjectFactory("#1").Create().TestSingle();
            token.IsMathToken("#1");
            token.OriginalText.Is("#1");

            token = new MathObjectFactory("#1?").Create().TestSingle();
            token.IsMathToken("#1?");
            token.OriginalText.Is("#1?");

            token = new MathObjectFactory("#1s").Create().TestSingle();
            token.IsMathToken("#1s");
            token.OriginalText.Is("#1s");

            token = new MathObjectFactory("#1t").Create().TestSingle();
            token.IsMathToken("#1t");
            token.OriginalText.Is("#1t");

            var math = CreateSingleSequence("#1#2");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("#1");
            math.List[1].IsMathToken("#2");
            math.Main.TestString("#1#2");
            math.ToTokenString().TestString("#1#2");

            math = CreateSingleSequence("#1#2?");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("#1");
            math.List[1].IsMathToken("#2?");
            math.Main.TestString("#1#2?");
            math.ToTokenString().TestString("#1#2?");

            math = CreateSingleSequence("#1u");
            math.List.Count.Is(2);
            math.List[0].IsMathToken("#1");
            math.List[1].IsMathToken("u");
            math.Main.TestString("#1 u");
            math.ToTokenString().TestString("#1u");

            math = CreateSingleSequence("abc_{#1?}");
            math.List.Count.Is(3);
            math.List[0].IsMathToken("a");
            math.List[1].IsMathToken("b");
            math.List[2].Main.TestString("c");
            math.List[2].AsMathSequence().Sup.IsNull();
            math.List[2].AsMathSequence().Sub.IsMathToken("#1?");
            math.Main.TestString("abc_{#1?}");
            math.ToTokenString().TestString("abc_{#1?}");
        }

        [TestMethod]
        public void 変数2()
        {
            var math = CreateSingleSequence(@"\test{#1}");
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\test");
            math.List[1].AsMathSequence().List.Count.Is(1);
            math.List[1].AsMathSequence().List[0].IsMathToken("#1");
            math.List[1].AsMathSequence().LeftBracket.TestToken("{");
            math.List[1].AsMathSequence().RightBracket.TestToken("}");
            math.List[1].ToTokenString().TestString(@"{#1}");
            math.Main.TestString(@"\test{#1 }");
            math.ToTokenString().TestString(@"\test{#1 }");

            math = CreateSingleSequence(@"\Hom_{\cat{C}}(#1, #2)");
            math.List.Count.Is(2);
            math.List[0].AsMathSequence().List.Count.Is(1);
            math.List[0].AsMathSequence().List[0].IsMathToken(@"\Hom");
            math.List[0].AsMathSequence().Sub.Main.TestString(@"\cat{C}");
            math.List[1].AsMathSequence().List.Count.Is(2);
            math.List[1].AsMathSequence().List[0].IsMathToken("#1");
            math.List[1].AsMathSequence().List[1].IsMathToken("#2");
            math.List[1].AsMathSequence().LeftBracket.TestToken("(");
            math.List[1].AsMathSequence().RightBracket.TestToken(")");
            math.List[1].AsMathSequence().Separator.Is(",");
            math.List[1].Main.TestString(@"#1 , #2 ");
            math.Main.TestString(@"\Hom_{\cat{C}}(#1,#2)");
            math.ToTokenString().TestString(@"\Hom_{\cat{C}}(#1,#2)");

            math = CreateSingleSequence(@"M^{#1#2#3}");
            math.List.Count.Is(1);
            math.List[0].IsMathToken("M");
            math.Sup.AsMathSequence().List.Count.Is(3);
            math.Sup.AsMathSequence().List[0].IsMathToken("#1");
            math.Sup.AsMathSequence().List[1].IsMathToken("#2");
            math.Sup.AsMathSequence().List[2].IsMathToken("#3");
            math.Main.TestString("M");
            math.ToTokenString().TestString(@"M ^{ #1 #2 #3 }");
        }

        private MathSequence CreateSingleSequence(string text)
        {
            var math = new MathObjectFactory(text).Create().TestSingleMath();
            return math;
        }
    }
}
