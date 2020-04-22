using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class TestMathSequence
    {
        [TestMethod]
        public void コンストラクタ()
        {
            var list = new[] { new MathObjectFactory("a").CreateSingle(), new MathObjectFactory("b").CreateSingle(), new MathObjectFactory("c").CreateSingle() };
            var seq = new MathSequence(list);
            seq.List.Count.Is(3);
            seq.IsSimple.IsTrue();
            seq.ToTokenString().TestString("abc");
            seq.OriginalText.Is("abc");

            list = new[] { new MathObjectFactory("a^i").CreateSingle(), new MathObjectFactory("b^ x").CreateSingle(), new MathObjectFactory("c ^*").CreateSingle() };
            seq = new MathSequence(list);
            seq.List.Count.Is(3);
            seq.IsSimple.IsTrue();
            seq.ToTokenString().TestString("a^ib^xc^*");
            seq.OriginalText.Is("a^ib^ xc ^*");
        }

        [TestMethod]
        public void CopyWithoutSup()
        {
            var seq = CreateSingleSequence(@"a\times b");
            seq = seq.CopyWithoutSup().AsMathSequence();
            seq.List.Count.Is(3);
            seq.List[0].IsMathToken("a");
            seq.List[1].IsMathToken(@"\times");
            seq.List[2].IsMathToken("b");
            seq.Sup.IsNull();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"a\times b");
            seq.OriginalText.Is(@"a\times b");

            seq = CreateSingleSequence(@"( \alpha )");
            seq = seq.CopyWithoutSup().AsMathSequence();
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken(@"\alpha");
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.Sup.IsNull();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"(\alpha)");
            seq.OriginalText.Is(@"( \alpha )");

            seq = CreateSingleSequence(@"(\alpha \beta)");
            seq = seq.CopyWithoutSup().AsMathSequence();
            seq.List.Count.Is(2);
            seq.List[0].IsMathToken(@"\alpha");
            seq.List[1].IsMathToken(@"\beta");
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.Sup.IsNull();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"(\alpha\beta)");
            seq.OriginalText.Is(@"(\alpha \beta)");

            seq = CreateSingleSequence(@"\rho^i");
            var math = seq.CopyWithoutSup();
            math.IsMathToken(@"\rho");
            math.ToTokenString().TestString(@"\rho");
            math.OriginalText.Is(@"\rho");

            seq = CreateSingleSequence(@"f_i");
            seq = seq.CopyWithoutSup().AsMathSequence();
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken("f");
            seq.Sup.IsNull();
            seq.Sub.IsMathToken("i");
            seq.ToTokenString().TestString("f_i");

            seq = CreateSingleSequence(@"\theta^i_a");
            seq = seq.CopyWithoutSup().AsMathSequence();
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken(@"\theta");
            seq.Sup.IsNull();
            seq.Sub.IsMathToken("a");
            seq.ToTokenString().TestString(@"\theta_a");
            seq.OriginalText.Is(@"\theta_{a}");

            seq = CreateSingleSequence(@"( abc )^{ ijk }_{ uvw }");
            seq = seq.CopyWithoutSup().AsMathSequence();
            seq.List.Count.Is(3);
            seq.Main.TestString("abc");
            seq.Sup.IsNull();
            seq.Sub.ToTokenString().TestString("uvw");
            seq.ToTokenString().TestString(@"(abc)_{uvw}");
            seq.OriginalText.Is(@"( abc )_{ uvw}");
        }

        [TestMethod]
        public void CopyWithoutSub()
        {
            var seq = CreateSingleSequence(@"a \times b");
            seq = seq.CopyWithoutSub().AsMathSequence();
            seq.List.Count.Is(3);
            seq.List[0].IsMathToken("a");
            seq.List[1].IsMathToken(@"\times");
            seq.List[2].IsMathToken("b");
            seq.Sup.IsNull();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"a \times b");

            seq = CreateSingleSequence(@"(\alpha\beta)");
            seq = seq.CopyWithoutSub().AsMathSequence();
            seq.List.Count.Is(2);
            seq.List[0].IsMathToken(@"\alpha");
            seq.List[1].IsMathToken(@"\beta");
            seq.LeftBracket.TestToken("(");
            seq.RightBracket.TestToken(")");
            seq.Sup.IsNull();
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"(\alpha\beta)");

            seq = CreateSingleSequence(@"f_i");
            var math = seq.CopyWithoutSub();
            math.IsMathToken("f");
            math.ToTokenString().TestString("f");
            math.OriginalText.Is(@"f");

            seq = CreateSingleSequence(@"\theta^i_a");
            seq = seq.CopyWithoutSub().AsMathSequence();
            seq.List.Count.Is(1);
            seq.List[0].IsMathToken(@"\theta");
            seq.Sup.IsMathToken("i");
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"\theta^i");
            seq.OriginalText.Is(@"\theta^{i}");

            seq = CreateSingleSequence(@"(a b c) ^ { i j k } _ { u v w }");
            seq = seq.CopyWithoutSub().AsMathSequence();
            seq.List.Count.Is(3);
            seq.Main.TestString("abc");
            seq.Sup.ToTokenString().TestString("ijk");
            seq.Sub.IsNull();
            seq.ToTokenString().TestString(@"(abc)^{ijk}");
            seq.OriginalText.Is(@"(a b c)^{ i j k}");
        }

        [TestMethod]
        public void SubSequence()
        {
            var seq = CreateSingleSequence("abcde");
            var x = seq.SubSequence(3, 1);
            x.IsMathToken("d");

            var y = seq.SubSequence(2).AsMathSequence();
            y.List.Count.Is(3);
            y.List[0].IsMathToken("c");
            y.List[1].IsMathToken("d");
            y.List[2].IsMathToken("e");

            y = seq.SubSequence(1, 3).AsMathSequence();
            y.List.Count.Is(3);
            y.List[0].IsMathToken("b");
            y.List[1].IsMathToken("c");
            y.List[2].IsMathToken("d");


            seq = CreateSingleSequence("(abc){def}(ghi)");
            y = seq.SubSequence(2).AsMathSequence();
            y.List.Count.Is(3);
            y.LeftBracket.TestToken("(");
            y.RightBracket.TestToken(")");
            y.Main.TestString("ghi");

            y = seq.SubSequence(0, 2).AsMathSequence();
            y.List.Count.Is(2);
            y.List[0].AsMathSequence().List.Count.Is(3);
            y.List[0].AsMathSequence().LeftBracket.TestToken("(");
            y.List[0].AsMathSequence().RightBracket.TestToken(")");
            y.List[0].AsMathSequence().Main.TestString("abc");
            y.List[1].AsMathSequence().List.Count.Is(3);
            y.List[1].AsMathSequence().LeftBracket.TestToken("{");
            y.List[1].AsMathSequence().RightBracket.TestToken("}");
            y.List[1].AsMathSequence().Main.TestString("def");
        }



        private MathSequence CreateSingleSequence(string text)
        {
            var seq = new MathObjectFactory(text).Create().TestSingleMath();
            seq.OriginalText.Is(text);
            return seq;
        }
    }
}
