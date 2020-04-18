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
            var math = CreateSingleSequence(@"a\times b");
            math = math.CopyWithoutSup();
            math.List.Count.Is(3);
            math.List[0].IsMathToken("a");
            math.List[1].IsMathToken(@"\times");
            math.List[2].IsMathToken("b");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.ToTokenString().TestString(@"a\times b");
            math.OriginalText.Is(@"a\times b");

            math = CreateSingleSequence(@"(\alpha \beta)");
            math = math.CopyWithoutSup();
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\alpha");
            math.List[1].IsMathToken(@"\beta");
            math.LeftBracket.TestToken("(");
            math.RightBracket.TestToken(")");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.ToTokenString().TestString(@"(\alpha\beta)");
            math.OriginalText.Is(@"(\alpha \beta)");

            math = CreateSingleSequence(@"f_i");
            math = math.CopyWithoutSup();
            math.List.Count.Is(1);
            math.List[0].IsMathToken("f");
            math.Sup.IsNull();
            math.Sub.IsMathToken("i");
            math.ToTokenString().TestString("f_i");

            math = CreateSingleSequence(@"\theta^i_a");
            math = math.CopyWithoutSup();
            math.List.Count.Is(1);
            math.List[0].IsMathToken(@"\theta");
            math.Sup.IsNull();
            math.Sub.IsMathToken("a");
            math.ToTokenString().TestString(@"\theta_a");
            math.OriginalText.Is(@"\theta_{a}");

            math = CreateSingleSequence(@"( abc )^{ ijk }_{ uvw }");
            math = math.CopyWithoutSup();
            math.List.Count.Is(3);
            math.Main.TestString("abc");
            math.Sup.IsNull();
            math.Sub.ToTokenString().TestString("uvw");
            math.ToTokenString().TestString(@"(abc)_{uvw}");
            math.OriginalText.Is(@"( abc )_{ uvw}");
        }

        [TestMethod]
        public void CopyWithoutSub()
        {
            var math = CreateSingleSequence(@"a \times b");
            math = math.CopyWithoutSub();
            math.List.Count.Is(3);
            math.List[0].IsMathToken("a");
            math.List[1].IsMathToken(@"\times");
            math.List[2].IsMathToken("b");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.ToTokenString().TestString(@"a \times b");

            math = CreateSingleSequence(@"(\alpha\beta)");
            math = math.CopyWithoutSub();
            math.List.Count.Is(2);
            math.List[0].IsMathToken(@"\alpha");
            math.List[1].IsMathToken(@"\beta");
            math.LeftBracket.TestToken("(");
            math.RightBracket.TestToken(")");
            math.Sup.IsNull();
            math.Sub.IsNull();
            math.ToTokenString().TestString(@"(\alpha\beta)");

            math = CreateSingleSequence(@"f_i");
            math = math.CopyWithoutSub();
            math.List.Count.Is(1);
            math.List[0].IsMathToken("f");
            math.Sub.IsNull();
            math.ToTokenString().TestString("f");
            math.OriginalText.Is(@"f");

            math = CreateSingleSequence(@"\theta^i_a");
            math = math.CopyWithoutSub();
            math.List.Count.Is(1);
            math.List[0].IsMathToken(@"\theta");
            math.Sup.IsMathToken("i");
            math.Sub.IsNull();
            math.ToTokenString().TestString(@"\theta^i");
            math.OriginalText.Is(@"\theta^{i}");

            math = CreateSingleSequence(@"(a b c) ^ { i j k } _ { u v w }");
            math = math.CopyWithoutSub();
            math.List.Count.Is(3);
            math.Main.TestString("abc");
            math.Sup.ToTokenString().TestString("ijk");
            math.Sub.IsNull();
            math.ToTokenString().TestString(@"(abc)^{ijk}");
            math.OriginalText.Is(@"(a b c)^{ i j k}");
        }

        [TestMethod]
        public void Divide_通常()
        {
            var token = new MathObjectFactory("F").Create().TestSingle();
            token.Divide(token).Count().Is(0);

            var math = new MathObjectFactory(@"a\times b").Create().TestSingle();
            math.Divide(token).Count().Is(0);

            token = new MathObjectFactory(@"\times").Create().TestSingle();
            var ar = math.Divide(token).ToArray();
            ar.Length.Is(1);
            ar[0].left.IsMathToken("a");
            ar[0].left.OriginalText.Is("a");
            ar[0].center.IsMathToken(@"\times");
            ar[0].right.IsMathToken("b");
            ar[0].right.OriginalText.Is("b");

            math = CreateSingleSequence(@"a\times b\times c");
            ar = math.Divide(token).ToArray();
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
        }

        [TestMethod]
        public void Divide_括弧()
        {
            var token = new MathObjectFactory(@"\otimes").Create().TestSingle();

            var math = CreateSingleSequence(@"\encat { A } \otimes (\encat {B} \otimes \encat{C})");
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



        private MathSequence CreateSingleSequence(string text)
        {
            var math = new MathObjectFactory(text).Create().TestSingleMath();
            math.OriginalText.Is(text);
            return math;
        }
    }
}
