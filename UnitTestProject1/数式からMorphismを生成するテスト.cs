using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class 数式からMorphismを生成するテスト
    {
        [TestMethod]
        public void 通常の射()
        {
            Morphism.Create(@"f\colon a\rightarrow b").TestSingle().TestMorphism("f", "a", "b", MorphismType.OneMorphism);

            Morphism.Create(@" g \colon u \rightarrow v ").TestSingle().TestMorphism("g", "u", "v", MorphismType.OneMorphism);

            Morphism.Create(@"\test\colon \test\rightarrow\testtest").TestSingle()
                .TestMorphism(@"\test", @"\test", @"\testtest", MorphismType.OneMorphism);

            Morphism.Create(@"\test^{\test}\colon \test\test\rightarrow\testtest\test").TestSingle()
                .TestMorphism(@"\test ^{\test }", @"\test\test", @"\testtest\test", MorphismType.OneMorphism);

            Morphism.Create(@"\eta_a\colon \id_{\cat{C}}(a)\rightarrow GF(a)").TestSingle()
                .TestMorphism(@"\eta_a", @"\id_{\cat{C}}(a)", "GF(a)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void 複数の射を生成()
        {
            var xs = Morphism.Create(@"f, g\colon a\rightarrow b").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism("f", "a", "b", MorphismType.OneMorphism);
            xs[1].TestMorphism("g", "a", "b", MorphismType.OneMorphism);

            xs = Morphism.Create(@"F\cong G\colon\cat{C}\rightarrow\cat{D}").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            xs[1].TestMorphism("G", @"\cat{C}", @"\cat{D}", MorphismType.Functor);

            xs = Morphism.Create(@"F:=LM\colon\cat{C}\rightarrow\cat{D}").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            xs[1].TestMorphism("LM", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
        }

        [TestMethod]
        public void TwoMorphismの生成()
        {
            Morphism.Create(@"\theta\colon F\Rightarrow G").TestSingle().TestMorphism(@"\theta", "F", "G", MorphismType.TwoMorphism);
            Morphism.Create(@"\Gamma\colon\theta\Rrightarrow\rho").TestSingle().TestMorphism(@"\Gamma", @"\theta", @"\rho", MorphismType.ThreeMorphism);

            var xs = Morphism.Create(@"\alpha, \beta, \gamma\colon F\Rightarrow G\colon A\rightarrow B").ToList();
            xs.Count.Is(5);
            xs[0].TestMorphism("F", "A", "B", MorphismType.OneMorphism);
            xs[1].TestMorphism("G", "A", "B", MorphismType.OneMorphism);
            xs[2].TestMorphism(@"\alpha", "F", "G", MorphismType.TwoMorphism);
            xs[3].TestMorphism(@"\beta", "F", "G", MorphismType.TwoMorphism);
            xs[4].TestMorphism(@"\gamma", "F", "G", MorphismType.TwoMorphism);

            xs = Morphism.Create(@"\theta\colon F\Rightarrow G\colon C\rightarrow D").ToList();
            xs.Count.Is(3);
            xs[0].TestMorphism("F", @"C", @"D", MorphismType.OneMorphism);
            xs[1].TestMorphism("G", @"C", @"D", MorphismType.OneMorphism);
            xs[2].TestMorphism(@"\theta", "F", "G", MorphismType.TwoMorphism);
        }

        [TestMethod]
        public void ThreeMorphismの生成()
        { 
            var xs = Morphism.Create(@"\Gamma, \Sigma\colon\theta\Rrightarrow\rho\colon f\Rightarrow g\colon a\rightarrow b").ToList();
            xs.Count.Is(8);
            xs[0].TestMorphism("f", "a", "b", MorphismType.OneMorphism);
            xs[1].TestMorphism("g", "a", "b", MorphismType.OneMorphism);
            xs[2].TestMorphism(@"\theta", "f", "g", MorphismType.TwoMorphism);
            xs[3].TestMorphism("f", "a", "b", MorphismType.OneMorphism);
            xs[4].TestMorphism("g", "a", "b", MorphismType.OneMorphism);
            xs[5].TestMorphism(@"\rho", "f", "g", MorphismType.TwoMorphism);
            xs[6].TestMorphism(@"\Gamma", @"\theta", @"\rho", MorphismType.ThreeMorphism);
            xs[7].TestMorphism(@"\Sigma", @"\theta", @"\rho", MorphismType.ThreeMorphism);
        }

        [TestMethod]
        public void 関手()
        {
            Morphism.Create(@"F\colon\cat{C}\rightarrow\cat{D}").TestSingle().TestMorphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            Morphism.Create(@"U\colon\Ab\rightarrow\Set").TestSingle().TestMorphism("U", @"\Ab", @"\Set", MorphismType.Functor);
            Morphism.Create(@"U\colon\Mod[R]\rightarrow\Ab").TestSingle().TestMorphism("U", @"\Mod[R]", @"\Ab", MorphismType.Functor);

            var xs = Morphism.Create(@"\theta\colon F\Rightarrow G\colon\cat{C}\rightarrow\cat{D}").ToList();
            xs.Count.Is(3);
            xs[0].TestMorphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            xs[1].TestMorphism("G", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            xs[2].TestMorphism(@"\theta", "F", "G", MorphismType.TwoMorphism);

            Morphism.Create(@"F\colon\opp{\cat{C}}\rightarrow\cat{D}").TestSingle().TestMorphism("F", @"\opp{\cat{C}}", @"\cat{D}", MorphismType.ContravariantFunctor);
            Morphism.Create(@"G\colon\opp{\Set}\rightarrow\opp{\Cat}").TestSingle().TestMorphism("G", @"\opp{\Set}", @"\opp{\Cat}", MorphismType.Functor);

            Morphism.Create(@"\Hom_{\cat{C}}(a, -)\colon \cat{C}\rightarrow\Set").TestSingle()
                .TestMorphism(@"\Hom_{\cat{C}}(a,-)", @"\cat{C}", @"\Set", MorphismType.Functor);
        }

        [TestMethod]
        public void Morphismでないときは生成しない()
        {
            Morphism.Create(@"a").Count().Is(0);
            Morphism.Create(@"\cat{C}").Count().Is(0);
        }

        [TestMethod]
        public void 随伴()
        {
            var xs = Morphism.Create(@"F\dashv G\colon\cat{C}\rightarrow\cat{D}").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            xs[1].TestMorphism("G", @"\cat{D}", @"\cat{C}", MorphismType.Functor);

            xs = Morphism.Create(@"f\dashv g\dashv h\dashv k\colon a\rightarrow b").ToList();
            xs.Count.Is(4);
            xs[0].TestMorphism("f", "a", "b", MorphismType.OneMorphism);
            xs[1].TestMorphism("g", "b", "a", MorphismType.OneMorphism);
            xs[2].TestMorphism("h", "a", "b", MorphismType.OneMorphism);
            xs[3].TestMorphism("k", "b", "a", MorphismType.OneMorphism);

            xs = Morphism.Create(@"L_0\dashv R^1\colon\cat{C}\rightarrow\cat{D}").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism("L_{0}", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            xs[1].TestMorphism("R^{1}", @"\cat{D}", @"\cat{C}", MorphismType.Functor);
        }

        [TestMethod]
        public void 変数付き射()
        {
            var mor = Morphism.Create(@"\theta^{#1}\colon F(#1)\rightarrow G#1").TestSingle();
            mor.Name.AsMathSequence().List.Count.Is(1);
            mor.Name.AsMathSequence().List[0].IsMathToken(@"\theta");
            mor.Name.AsMathSequence().Sup.IsMathToken("#1");
            mor.Source.AsMathSequence().List.Count.Is(2);
            mor.Source.AsMathSequence().List[0].IsMathToken("F");
            mor.Source.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            mor.Source.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("#1");
            mor.Source.AsMathSequence().List[1].AsMathSequence().LeftBracket.TestToken("(");
            mor.Source.AsMathSequence().List[1].AsMathSequence().RightBracket.TestToken(")");
            mor.Target.AsMathSequence().List.Count.Is(2);
            mor.Target.AsMathSequence().List[0].IsMathToken("G");
            mor.Target.AsMathSequence().List[1].IsMathToken("#1");

            var xs = Morphism.Create(@"\test{#1}{#2s}\colon F#1\rightarrow G#1").ToArray();
            xs.Length.Is(0);
        }

        [TestMethod]
        public void ApplyParameter_通常()
        {
            var mor = Morphism.Create(@"\theta^{#1}\colon F(#1)\rightarrow G#1").TestSingle();
            mor.Name.AsMathSequence().List.Count.Is(1);
            mor.Name.AsMathSequence().List[0].IsMathToken(@"\theta");
            mor.Name.AsMathSequence().Sup.IsMathToken("#1");
            mor.Source.AsMathSequence().List.Count.Is(2);
            mor.Source.AsMathSequence().List[0].IsMathToken("F");
            mor.Source.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            mor.Source.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("#1");
            mor.Source.AsMathSequence().List[1].AsMathSequence().LeftBracket.TestToken("(");
            mor.Source.AsMathSequence().List[1].AsMathSequence().RightBracket.TestToken(")");
            mor.Target.AsMathSequence().List.Count.Is(2);
            mor.Target.AsMathSequence().List[0].IsMathToken("G");
            mor.Target.AsMathSequence().List[1].IsMathToken("#1");

            var f = new MathObjectFactory(@"\theta^i").CreateSingle();
            var parameters = new Dictionary<string, MathObject>();
            mor.Name.IsSameType(f, parameters).IsTrue();
            mor = mor.ApplyParameter(f, parameters, false).TestSingle();
            mor.Name.AsMathSequence().List.Count.Is(1);
            mor.Name.AsMathSequence().List[0].IsMathToken(@"\theta");
            mor.Name.AsMathSequence().Sup.IsMathToken("i");
            mor.Source.AsMathSequence().List.Count.Is(2);
            mor.Source.AsMathSequence().List[0].IsMathToken("F");
            mor.Source.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            mor.Source.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("i");
            mor.Source.AsMathSequence().List[1].AsMathSequence().LeftBracket.TestToken("(");
            mor.Source.AsMathSequence().List[1].AsMathSequence().RightBracket.TestToken(")");
            mor.Target.AsMathSequence().List.Count.Is(2);
            mor.Target.AsMathSequence().List[0].IsMathToken("G");
            mor.Target.AsMathSequence().List[1].IsMathToken("i");

            f = new MathObjectFactory(@"\theta^{i}").CreateSingle();
            parameters = new Dictionary<string, MathObject>();
            mor.Name.IsSameType(f, parameters).IsTrue();
            mor = mor.ApplyParameter(f, parameters, false).TestSingle();
            mor.Name.AsMathSequence().List.Count.Is(1);
            mor.Name.AsMathSequence().List[0].IsMathToken(@"\theta");
            mor.Name.AsMathSequence().Sup.IsMathToken("i");
            mor.Source.AsMathSequence().List.Count.Is(2);
            mor.Source.AsMathSequence().List[0].IsMathToken("F");
            mor.Source.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            mor.Source.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("i");
            mor.Source.AsMathSequence().List[1].AsMathSequence().LeftBracket.TestToken("(");
            mor.Source.AsMathSequence().List[1].AsMathSequence().RightBracket.TestToken(")");
            mor.Target.AsMathSequence().List.Count.Is(2);
            mor.Target.AsMathSequence().List[0].IsMathToken("G");
            mor.Target.AsMathSequence().List[1].IsMathToken("i");
        }

        [TestMethod]
        public void ApplyParameter_米田()
        {
            var mor = Morphism.Create(@"\yoneda_{#1?}\colon #1\rightarrow \widehat{#1}").TestSingle();
            mor.Name.AsMathSequence().List.Count.Is(1);
            mor.Name.AsMathSequence().List[0].IsMathToken(@"\yoneda");
            mor.Name.AsMathSequence().Sub.IsMathToken("#1?");
            mor.Source.IsMathToken("#1");
            mor.Target.AsMathSequence().List.Count.Is(2);
            mor.Target.AsMathSequence().List[0].IsMathToken(@"\widehat");
            mor.Target.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            mor.Target.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("#1");
            mor.Target.AsMathSequence().List[1].AsMathSequence().LeftBracket.TestToken("{");
            mor.Target.AsMathSequence().List[1].AsMathSequence().RightBracket.TestToken("}");

            var f = new MathObjectFactory(@"\yoneda_a").CreateSingle();
            var parameters = new Dictionary<string, MathObject>();
            mor.Name.IsSameType(f, parameters).IsTrue();
            mor = mor.ApplyParameter(f, parameters, false).TestSingle();
            mor.Name.AsMathSequence().List.Count.Is(1);
            mor.Name.AsMathSequence().List[0].IsMathToken(@"\yoneda");
            mor.Name.AsMathSequence().Sub.IsMathToken("a");
            mor.Source.IsMathToken("a");
            mor.Target.AsMathSequence().List.Count.Is(2);
            mor.Target.AsMathSequence().List[0].IsMathToken(@"\widehat");
            mor.Target.AsMathSequence().List[1].AsMathSequence().List.Count.Is(1);
            mor.Target.AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("a");
            mor.Target.AsMathSequence().List[1].AsMathSequence().LeftBracket.TestToken("{");
            mor.Target.AsMathSequence().List[1].AsMathSequence().RightBracket.TestToken("}");
            mor.Target.ToTokenString().TestString(@"\widehat{a}");
        }

        [TestMethod]
        public void Homから元を取ったとき()
        {
            Morphism.Create(@"f\in\Hom(a,b)").TestSingle().TestMorphism("f", "a", "b", MorphismType.OneMorphism);
            Morphism.Create(@"f\in\Hom_{\cat{C}}(a,b)").TestSingle().TestMorphism("f", "a", "b", MorphismType.OneMorphism);
            Morphism.Create(@" g \in \Hom _{\cat{C}} ( c , d )").TestSingle().TestMorphism("g", "c", "d", MorphismType.OneMorphism);

            Morphism.Create(@"\test\in\Hom(a, b)").TestSingle().TestMorphism(@"\test", "a", "b", MorphismType.OneMorphism);
            Morphism.Create(@"\test\test\in\Hom_{\cat{C}}(a, b)").TestSingle().TestMorphism(@"\test \test", "a", "b", MorphismType.OneMorphism);
            Morphism.Create(@"\mor{f}\in\Hom(a, b)").TestSingle().TestMorphism(@"\mor {f}", "a", "b", MorphismType.OneMorphism);

            var xs = Morphism.Create(@"K, L\in\Hom_{\Cat[\moncat{V}]}(\encat{C}, \encat{D})").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism(@"K", @"\encat {C}", @"\encat {D}", MorphismType.Functor);
            xs[1].TestMorphism(@"L", @"\encat {C}", @"\encat {D}", MorphismType.Functor);

            xs = Morphism.Create(@"s, t\in\Hom_{\Cat}(\Set, \Mod[R])").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism("s", @"\Set", @"\Mod [R]", MorphismType.Functor);
            xs[1].TestMorphism("t", @"\Set", @"\Mod [R]", MorphismType.Functor);

            Morphism.Create(@"f\in\Hom(a_i, c^b)").TestSingle().TestMorphism("f", "a_{i}", "c^{b}", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void 関手圏から元を取ったとき()
        {
            Morphism.Create(@"F\in\cat{D}^{\cat{C}}").TestSingle().TestMorphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);

            var xs = Morphism.Create(@"K, L\in\encat{M}^{\encat{C}}").ToList();
            xs.Count.Is(2);
            xs[0].TestMorphism(@"K", @"\encat {C}", @"\encat {M}", MorphismType.Functor);
            xs[1].TestMorphism(@"L", @"\encat {C}", @"\encat {M}", MorphismType.Functor);
        }
    }
}
