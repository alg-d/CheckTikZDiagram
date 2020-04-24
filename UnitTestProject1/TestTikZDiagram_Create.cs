using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TestTikZDiagram_Create
    {
        [TestMethod]
        public void MorphismDef()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "K".ToTokenString(), ToMorphismHelp(@"K", @"\cat{C}", @"\cat{D}", MorphismType.Functor)}
            };
            var tikz = CreateTikZDiagram(dic);
            tikz.CreateMorphism("K")
                .TestMorphism("K", @"\cat{C}", @"\cat{D}", MorphismType.Functor);

            tikz.CreateMorphism("K", @"\cat{C}", @"\cat{D}")
                .TestMorphism("K", @"\cat{C}", @"\cat{D}", MorphismType.Functor, true);
        }

        [TestMethod]
        public void Morphism逆射()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp(@"f", @"a", @"b", MorphismType.OneMorphism)},
                { @"\theta".ToTokenString(), ToMorphismHelp(@"\theta", @"F", @"G", MorphismType.NaturalTransformation)},
                { @"\beta_i".ToTokenString(), ToMorphismHelp(@"\beta_i", @"Ki", @"Hi", MorphismType.OneMorphism)},
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"f^{-1}")
                .TestMorphism(@"f^{-1}", "b", "a", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"f^{-1}", "b", "a")
                .TestMorphism(@"f^{-1}", "b", "a", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\theta_a^{-1}")
                .TestMorphism(@"\theta_a^{-1}", "G(a)", "F(a)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_a^{-1}", "G(a)", "F(a)")
                .TestMorphism(@"\theta_a^{-1}", "G(a)", "F(a)", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\beta_i^{-1}")
                .TestMorphism(@"\beta_i^{-1}", "Hi", "Ki", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void Morphism恒等射()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"\id_a")
                .TestMorphism(@"\id_a", "a", "a", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\id_a", "a", "a")
                .TestMorphism(@"\id_a", "a", "a", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\id_x", "a", "b")
                .TestMorphism(@"\id_x", "x", "x", MorphismType.OneMorphism, false);

            tikz.CreateMorphism(@"\id_{\cat{C}}")
                .TestMorphism(@"\id_{\cat{C}}", @"\cat{C}", @"\cat{C}", MorphismType.Functor);

            tikz.CreateMorphism(@"\id_{\cat{C}}", @"\cat{C}", @"\cat{C}")
                .TestMorphism(@"\id_{\cat{C}}", @"\cat{C}", @"\cat{C}", MorphismType.Functor, true);

            tikz.CreateMorphism(@"\id", "x", "x")
                .TestMorphism(@"\id", "x", "x", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\id").Count().Is(0);

            tikz.CreateMorphism(@"\id", "a", "b").Count().Is(0);
        }

        [TestMethod]
        public void Morphism射の合成()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism)},
                { "g".ToTokenString(), ToMorphismHelp("g", "b", "c", MorphismType.OneMorphism)},
                { "h".ToTokenString(), ToMorphismHelp("h", "c", "d", MorphismType.OneMorphism)}
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"\compo{fgh}")
                .TestMorphism(@"\compo{fgh}", "a", "d", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\compo{fgh}", "a", "d")
                .TestMorphism(@"\compo{fgh}", "a", "d", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\compo{{f}{g}{h}}")
                .TestMorphism(@"\compo{{f}{g}{h}}", "a", "d", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\compo{{f}{\id_b}}")
                .TestMorphism(@"\compo{{f}{\id_b}}", "a", "b", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\compo{{f}{\id_b}}", "a", "b")
                .TestMorphism(@"\compo{{f}{\id_b}}", "a", "b", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"h\ocmp g\ocmp f")
                .TestMorphism(@"h\ocmp g\ocmp f", "a", "d", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"h\ocmp g\ocmp f", "a", "d")
                .TestMorphism(@"h\ocmp g\ocmp f", "a", "d", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"(h\ocmp g)\ocmp f")
                .TestMorphism(@"(h\ocmp g)\ocmp f", "a", "d", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"(h\ocmp g)\ocmp f", "a", "d")
                .TestMorphism(@"(h\ocmp g)\ocmp f", "a", "d", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void Morphism自然変換の添え字()
        {
            var mor1 = ToMorphismHelp(@"\theta", "F", "G", MorphismType.NaturalTransformation);
            var mor2 = ToMorphismHelp(@"\theta_{k}", "u", "v", MorphismType.OneMorphism);
            var mor3 = ToMorphismHelp(@"\beta", @"\id_{\cat{C}}", "RL", MorphismType.TwoMorphism);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"\theta_i")
                .TestMorphism(@"\theta_{i}", "F(i)", "G(i)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_i", "F(i)", "G(i)")
                .TestMorphism(@"\theta_{i}", "F(i)", "G(i)", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\theta_i", "Fi", "Gi")
                .TestMorphism(@"\theta_{i}", "Fi", "Gi", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\theta_{Ka}")
                .TestMorphism(@"\theta_{Ka}", "F(Ka)", "G(Ka)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_{Ka}", "F(Ka)", "G(Ka)")
                .TestMorphism(@"\theta_{Ka}", "F(Ka)", "G(Ka)", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\theta_{Ka}", "FKa", "GKa")
                .TestMorphism(@"\theta_{Ka}", "F(Ka)", "G(Ka)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_k")
                .TestMorphism(@"\theta_{k}", "u", "v", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\beta_x")
                .TestMorphism(@"\beta_{x}", @"\id_{\cat{C}}(x)", "RL(x)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void Morphism自然変換の添え字2()
        {
            var mor1 = ToMorphismHelp(@"\theta", "T", "S", MorphismType.NaturalTransformation);
            var mor2 = ToMorphismHelp(@"T", @"\cat{C}\times\cat{D}", @"\Set", MorphismType.Bifunctor);
            var mor3 = ToMorphismHelp(@"S", @"\cat{C}\times\cat{D}", @"\Set", MorphismType.Bifunctor);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"\theta_{cd}")
                .TestMorphism(@"\theta_{cd}", "T(c, d)", "S(c, d)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_{cd}", "T(c, d)", "S(c, d)")
                .TestMorphism(@"\theta_{cd}", "T(c, d)", "S(c, d)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_{c_id_j}")
                .TestMorphism(@"\theta_{c_id_j}", "T(c_i, d_j)", "S(c_i, d_j)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\theta_{abc, def}")
                .TestMorphism(@"\theta_{abc, def}", "T(abc, def)", "S(abc, def)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void Morphism関手適用()
        {
            var nat = ToMorphismHelp(@"\theta", "F", "G", MorphismType.NaturalTransformation);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { nat.Name.ToTokenString(), nat },
                { "K".ToTokenString(), ToMorphismHelp("K", @"\cat{C}", @"\cat{D}", MorphismType.Functor)},
                { "E".ToTokenString(), ToMorphismHelp("E", @"\cat{C}", @"\cat{U}", MorphismType.Functor)},
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism)},
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism("Kf")
                .TestMorphism("Kf", "K(a)", "K(b)", MorphismType.OneMorphism);

            tikz.CreateMorphism("K(f)")
                .TestMorphism("K(f)", "K(a)", "K(b)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"K\theta")
                .TestMorphism(@"K\theta", "KF", "KG", MorphismType.NaturalTransformation);

            tikz.CreateMorphism(@"K\theta_i")
                .TestMorphism(@"K\theta_i", "K(F(i))", "K(G(i))", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\Lan{K}E(f)")
                .TestMorphism(@"\Lan{K}E(f)", @"\Lan{K}E(a)", @"\Lan{K}E(b)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void Morphism関手適用_2変数1()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "T".ToTokenString(), ToMorphismHelp("T", @"\cat{A}\times\cat{B}", @"\cat{C}", MorphismType.Bifunctor)},
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism)},
                { "g".ToTokenString(), ToMorphismHelp("g", "c", "d", MorphismType.OneMorphism)},
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism("T(f, g)")
                .TestMorphism("T(f, g)", "T(a, c)", "T(b, d)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"T(\id_a, \id_b)")
                .TestMorphism(@"T(\id_a, \id_b)", "T(a, b)", "T(a, b)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"T(\id, \id)", "T(a, b)", "T(a, b)")
                .TestMorphism(@"T(\id, \id)", "T(a, b)", "T(a, b)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"T(f, \id)", "T(a, b)", "T(b, b)")
                .TestMorphism(@"T(f, \id)", "T(a, b)", "T(b, b)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"T(\id, f)", "T(a, a)", "T(a, b)")
                .TestMorphism(@"T(\id, f)", "T(a, a)", "T(a, b)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void Morphism関手適用_2変数2()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "T".ToTokenString(), ToMorphismHelp("T", @"\cat{A}\times\cat{B}", @"\cat{C}", MorphismType.Bifunctor)},
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism)},
                { "g".ToTokenString(), ToMorphismHelp("g", @"\widehat{c}", @"\widehat{d}", MorphismType.OneMorphism)},
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism("T(f, g)")
                .TestMorphism("T(f, g)", @"T(a, \widehat{c})", @"T(b, \widehat{d})", MorphismType.OneMorphism);

            tikz.CreateMorphism("T(g, f)")
                .TestMorphism("T(g, f)", @"T(\widehat{c}, a)", @"T(\widehat{d}, b)", MorphismType.OneMorphism);

            tikz.CreateMorphism("T(f, x)", "T(a, x)", "T(b, x)")
                .TestMorphism("T(f, x)", "T(a, x)", "T(b, x)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"T(\test, f)", @"T(\test, a)", @"T(\test, b)")
                .TestMorphism(@"T(\test, f)", @"T(\test, a)", @"T(\test, b)", MorphismType.OneMorphism);

            tikz.CreateMorphism("T(g, uv)", @"T(\widehat{c}, uv)", @"T(\widehat{d}, uv)")
                .TestMorphism("T(g, uv)", @"T(\widehat{c}, uv)", @"T(\widehat{d}, uv)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void Morphism二項演算()
        {
            var mor1 = ToMorphismHelp(@"f", "a", "b", MorphismType.OneMorphism);
            var mor2 = ToMorphismHelp(@"g", "s", "t", MorphismType.OneMorphism);
            var mor3 = ToMorphismHelp(@"\beta_i", "Fi", "Gi", MorphismType.OneMorphism);
            var mor4 = ToMorphismHelp(@"\gamma_j", "Kj", "Lj", MorphismType.OneMorphism);
            var mor5 = ToMorphismHelp(@"\sigma", "P", "Q", MorphismType.NaturalTransformation);
            var mor6 = ToMorphismHelp(@"\tau", "R", "S", MorphismType.NaturalTransformation);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
                { mor4.Name.ToTokenString(), mor4 },
                { mor5.Name.ToTokenString(), mor5 },
                { mor6.Name.ToTokenString(), mor6 },
            };
            var tikz = CreateTikZDiagram(dic);

            var mor = tikz.CreateMorphism(@"f\otimes g");
            mor.TestMorphism(@"f\otimes g", @"a\otimes s", @"b\otimes t", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"f\otimes g", @"a\otimes s", @"b\otimes t");
            mor.TestMorphism(@"f\otimes g", @"a\otimes s", @"b\otimes t", MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"\beta_i\otimes\gamma_j");
            mor.TestMorphism(@"\beta_i\otimes\gamma_j", @"Fi\otimes Kj", @"Gi\otimes Lj", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"\sigma_a\otimes\tau_b");
            mor.TestMorphism(@"\sigma_a\otimes\tau_b", @"P(a)\otimes R(b)", @"Q(a)\otimes S(b)", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"\sigma_a\otimes(\tau_b\otimes\beta_i)");
            mor.TestMorphism(
                @"\sigma_a\otimes (\tau_b\otimes\beta_i)",
                @"P(a)\otimes (R(b)\otimes Fi)",
                @"Q(a)\otimes (S(b)\otimes Gi)",
                MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"(\sigma_a\otimes\tau_b)\otimes\beta_i");
            mor.TestMorphism(
                @"(\sigma_a\otimes\tau_b)\otimes\beta_i",
                @"(P(a)\otimes R(b))\otimes Fi",
                @"(Q(a)\otimes S(b))\otimes Gi",
                MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"\sigma_a\otimes(\tau_b\otimes\beta_i)", @"P(a)\otimes(R(b)\otimes Fi)", @"Q(a)\otimes (S(b)\otimes Gi)");
            mor.TestMorphism(
                @"\sigma_a\otimes (\tau_b\otimes\beta_i)",
                @"P(a)\otimes (R(b)\otimes Fi)",
                @"Q(a)\otimes (S(b)\otimes Gi)",
                MorphismType.OneMorphism,
                true);
        }

        [TestMethod]
        public void Morphism二項演算_省略()
        {
            var mor1 = ToMorphismHelp(@"f", "a", "b", MorphismType.OneMorphism);
            var mor2 = ToMorphismHelp(@"g", "s", "t", MorphismType.OneMorphism);
            var mor3 = ToMorphismHelp(@"X", @"\test{A}", @"\test{B}", MorphismType.OneMorphism);
            var mor4 = ToMorphismHelp(@"Y", @"\test{C}", @"\test{D}", MorphismType.OneMorphism);
            var mor5 = ToMorphismHelp(@"\beta_i", "Fi", "Gi", MorphismType.OneMorphism);
            var mor6 = ToMorphismHelp(@"\gamma_j", "Kj", "Lj", MorphismType.OneMorphism);
            var mor7 = ToMorphismHelp(@"\sigma", "P", "Q", MorphismType.NaturalTransformation);
            var mor8 = ToMorphismHelp(@"\tau", "R", "S", MorphismType.NaturalTransformation);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
                { mor4.Name.ToTokenString(), mor4 },
                { mor5.Name.ToTokenString(), mor5 },
                { mor6.Name.ToTokenString(), mor6 },
                { mor7.Name.ToTokenString(), mor7 },
                { mor8.Name.ToTokenString(), mor8 },
            };
            var tikz = new TikZDiagram("", -1, false, true, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), ExtensionsInTest.CreateDefaultFunctors().ToList());

            var mor = tikz.CreateMorphism(@"f\otimes g", @"as", @"bt");
            mor.TestMorphism(@"f\otimes g", @"as", @"bt", MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"X\otimes Y", @"(\test{A})(\test{C})", @"(\test{B})(\test{D})");
            mor.TestMorphism(@"X\otimes Y", @"(\test{A})(\test{C})", @"(\test{B})(\test{D})", MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"\beta_i\otimes\gamma_j", @"(Fi)(Kj)", @"(Gi)(Lj)");
            mor.TestMorphism(@"\beta_i\otimes\gamma_j", @"(Fi)(Kj)", @"(Gi)(Lj)", MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"\sigma_a\otimes\tau_b", @"(Pa)(Rb)", @"(Qa)(Sb)");
            mor.TestMorphism(@"\sigma_a\otimes\tau_b", @"(Pa)(Rb)", @"(Qa)(Sb)", MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"(\sigma_a\otimes\tau_b)\otimes\beta_i", @"((Pa)(Rb))(Fi)", @"((Qa)(Sb))(Gi)");
            mor.TestMorphism(@"(\sigma_a\otimes\tau_b)\otimes\beta_i", @"((Pa)(Rb))(Fi)", @"((Qa)(Sb))(Gi)",
                MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"\sigma_a\otimes(\tau_b\otimes\beta_i)", @"(Pa)((Rb)(Fi))", @"(Qa)((Sb)(Gi))");
            mor.TestMorphism(@"\sigma_a\otimes (\tau_b\otimes\beta_i)", @"(Pa)((Rb)(Fi))", @"(Qa)((Sb)(Gi))",
                MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void Morphismハイフン()
        {
            var mor1 = ToMorphismHelp(@"\Hom(x, -)", @"\cat{C}", @"\Set", MorphismType.Functor);
            var mor2 = ToMorphismHelp(@"\Hom_{\cat{D}}(-, x)", @"\cat{D}", @"\Set", MorphismType.ContravariantFunctor);
            var mor3 = ToMorphismHelp(@"f", "a", "b", MorphismType.OneMorphism);
            var mor4 = ToMorphismHelp(@"-\otimes v", @"\moncat{V}", @"\moncat{V}", MorphismType.Functor);
            var mor5 = ToMorphismHelp(@"u\otimes -", @"\moncat{V}", @"\moncat{V}", MorphismType.Functor);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
                { mor4.Name.ToTokenString(), mor4 },
                { mor5.Name.ToTokenString(), mor5 },
            };
            var tikz = CreateTikZDiagram(dic);

            var mor = tikz.CreateMorphism(@"\Hom(x, f)");
            mor.TestMorphism(@"\Hom(x, f)", @"\Hom(x, a)", @"\Hom(x, b)", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"\Hom(x, f)", @"\Hom(x, a)", @"\Hom(x, b)");
            mor.TestMorphism(@"\Hom(x, f)", @"\Hom(x, a)", @"\Hom(x, b)", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"\Hom_{\cat{D}}(f, x)");
            mor.TestMorphism(@"\Hom_{\cat{D}}(f, x)", @"\Hom_{\cat{D}}(b, x)", @"\Hom_{\cat{D}}(a, x)", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"\Hom_{\cat{D}}(f, x)", @"\Hom_{\cat{D}}(b, x)", @"\Hom_{\cat{D}}(a, x)");
            mor.TestMorphism(@"\Hom_{\cat{D}}(f, x)", @"\Hom_{\cat{D}}(b, x)", @"\Hom_{\cat{D}}(a, x)", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"f\otimes v");
            mor.TestMorphism(@"f\otimes v", @"a\otimes v", @"b\otimes v", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"f\otimes v", @"a\otimes v", @"b\otimes v");
            mor.TestMorphism(@"f\otimes v", @"a\otimes v", @"b\otimes v", MorphismType.OneMorphism, true);

            mor = tikz.CreateMorphism(@"u\otimes f");
            mor.TestMorphism(@"u\otimes f", @"u\otimes a", @"u\otimes b", MorphismType.OneMorphism);

            mor = tikz.CreateMorphism(@"u\otimes f", @"u\otimes a", @"u\otimes b");
            mor.TestMorphism(@"u\otimes f", @"u\otimes a", @"u\otimes b", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数1()
        {
            var mor = ToMorphismHelp(@"\test{#1}", "F(#1)", "G(#1)", MorphismType.OneMorphism);

            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                mor,
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\test a")
                .TestMorphism(@"\test a", "F(a)", "G(a)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\test{x}")
                .TestMorphism(@"\test{x}", "F(x)", "G(x)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\test{x^i_j}")
                .TestMorphism(@"\test{x^i_j}", "F(x^i_j)", "G(x^i_j)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\test{\alpha\beta}")
                .TestMorphism(@"\test{\alpha\beta}", @"F(\alpha\beta)", @"G(\alpha\beta)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void 変数2()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                Morphism.Create(@"m_{#1#2} \colon \encat{A}(#1, #2) \rightarrow \encat{B}(G#1, G#2)").TestSingle()
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"m_{ab}")
                .TestMorphism(@"m_{ab}", @"\encat{A}(a, b)", @"\encat{B}(Ga, Gb)", MorphismType.Functor);

            tikz.CreateMorphism(@"m_{ab}", @"\encat{A}(a, b)", @"\encat{B}(Ga, Gb)")
                .TestMorphism(@"m_{ab}", @"\encat{A}(a, b)", @"\encat{B}(Ga, Gb)", MorphismType.Functor, true);

            tikz.CreateMorphism(@"m_{{Fa}{Fb}}")
                .TestMorphism(@"m_{{Fa}{Fb}}", @"\encat{A}(Fa, Fb)", @"\encat{B}(GFa, GFb)", MorphismType.Functor);

            tikz.CreateMorphism(@"m_{{Fa}{Fb}}", @"\encat{A}(Fa, Fb)", @"\encat{B}(GFa, GFb)")
                .TestMorphism(@"m_{{Fa}{Fb}}", @"\encat{A}(Fa, Fb)", @"\encat{B}(GFa, GFb)", MorphismType.Functor, true);
        }

        [TestMethod]
        public void 変数3()
        {
            var mor = ToMorphismHelp(@"M^{#1#2#3}", @"\bicat{B}(#2, #3)\times\bicat{B}(#1, #2)", @"\bicat{B}(#1, #3)", MorphismType.Bifunctor);

            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                mor,
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"M^{abc}")
                .TestMorphism(@"M^{abc}", @"\bicat{B}(b, c)\times\bicat{B}(a, b)", @"\bicat{B}(a, c)", MorphismType.Bifunctor);

            tikz.CreateMorphism(@"M^{abc}", @"\bicat{B}(b, c)\times\bicat{B}(a, b)", @"\bicat{B}(a, c)")
                .TestMorphism(@"M^{abc}", @"\bicat{B}(b, c)\times\bicat{B}(a, b)", @"\bicat{B}(a, c)", MorphismType.Bifunctor, true);

            tikz.CreateMorphism(@"M^{ace}")
                .TestMorphism(@"M^{ace}", @"\bicat{B}(c, e)\times\bicat{B}(a, c)", @"\bicat{B}(a, e)", MorphismType.Bifunctor);

            tikz.CreateMorphism(@"M^{abb}")
                .TestMorphism(@"M^{abb}", @"\bicat{B}(b, b)\times\bicat{B}(a, b)", @"\bicat{B}(a, b)", MorphismType.Bifunctor);

            tikz.CreateMorphism(@"M^{Fa, Fb, Fc}")
                .TestMorphism(@"M^{Fa, Fb, Fc}", @"\bicat{B}(Fb, Fc)\times\bicat{B}(Fa, Fb)", @"\bicat{B}(Fa, Fc)", MorphismType.Bifunctor);
        }

        [TestMethod]
        public void 変数dom_codのみにある変数()
        {
            var mor = ToMorphismHelp(@"M^{#1#2#3}", @"\bicat{#4}(#2, #3)\times\bicat{#4}(#1, #2)", @"\bicat{#4}(#1, #3)", MorphismType.Bifunctor);

            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                mor,
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"M^{abc}", @"\bicat{B}(b, c)\times\bicat{B}(a, b)", @"\bicat{B}(a, c)")
                .TestMorphism(@"M^{abc}", @"\bicat{B}(b, c)\times\bicat{B}(a, b)", @"\bicat{B}(a, c)", MorphismType.Bifunctor, true);

            tikz.CreateMorphism(@"M^{ace}", @"\bicat{B}(c, e)\times\bicat{B}(a, c)", @"\bicat{B}(a, e)")
                .TestMorphism(@"M^{ace}", @"\bicat{B}(c, e)\times\bicat{B}(a, c)", @"\bicat{B}(a, e)", MorphismType.Bifunctor, true);

            tikz.CreateMorphism(@"M^{abb}", @"\bicat{B}(b, b)\times\bicat{B}(a, b)", @"\bicat{B}(a, b)")
                .TestMorphism(@"M^{abb}", @"\bicat{B}(b, b)\times\bicat{B}(a, b)", @"\bicat{B}(a, b)", MorphismType.Bifunctor, true);

            tikz.CreateMorphism(@"M^{Fa, Fb, Fc}", @"\bicat{C}(Fb, Fc)\times\bicat{C}(Fa, Fb)", @"\bicat{C}(Fa, Fc)")
                .TestMorphism(@"M^{Fa, Fb, Fc}", @"\bicat{C}(Fb, Fc)\times\bicat{C}(Fa, Fb)", @"\bicat{C}(Fa, Fc)", MorphismType.Bifunctor, true);
        }

        [TestMethod]
        public void 変数ab()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\ev", @"[#1a, #2]\otimes #1b", @"#2", MorphismType.OneMorphism)
            };
            var func = ExtensionsInTest.CreateDefaultFunctors().ToList();
            func.Add(Functor.Create("F#1", "G#1"));
            func.Add(Functor.Create("X", "Y"));
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, func);

            tikz.CreateMorphism(@"\ev", @"[u, v]\otimes u", @"v")
                .TestMorphism(@"\ev", @"[u, v]\otimes u", @"v", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\ev", @"[X, v]\otimes Y", @"v")
                .TestMorphism(@"\ev", @"[X, v]\otimes Y", @"v", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\ev", @"[Fu, v]\otimes Gu", @"v")
                .TestMorphism(@"\ev", @"[Fu, v]\otimes Gu", @"v", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数st()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), Morphism.Create(@"F\colon\encat{A}\rightarrow\encat{B}").TestSingle() }
            };
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"#1_{#2#3}", @"#1s(#2, #3)", @"#1t(#1#2, #1#3)", MorphismType.OneMorphism)
            };
            var func = ExtensionsInTest.CreateDefaultFunctors().ToList();

            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, func);

            tikz.CreateMorphism(@"F_{ab}")
                .TestMorphism(@"F_{ab}", @"\encat{A}(a, b)", @"\encat{B}(Fa, Fb)", MorphismType.Functor);

            tikz.CreateMorphism(@"F_{ab}", @"\encat{A}(a, b)", @"\encat{B}(Fa, Fb)")
                .TestMorphism(@"F_{ab}", @"\encat{A}(a, b)", @"\encat{B}(Fa, Fb)", MorphismType.Functor, true);
        }

        [TestMethod]
        public void 変数Associator_NotNull()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\alpha^{#1}_{#2#3#4}", @"(#2\otimes #3)\otimes #4", @"#2\otimes (#3\otimes #4)", MorphismType.OneMorphism)
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\alpha^{abcd}_{uvw}")
                .TestMorphism(@"\alpha^{abcd}_{uvw}", @"(u\otimes v)\otimes w", @"u\otimes(v\otimes w)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\alpha^x_{u, v\otimes w, x}")
                .TestMorphism(@"\alpha^x_{u, v\otimes w, x}", @"(u\otimes (v\otimes w))\otimes x", @"u\otimes((v\otimes w)\otimes x)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"(\alpha_{abc}^x)^{-1}")
                .TestMorphism(@"(\alpha_{abc}^x)^{-1}", @"a\otimes(b\otimes c)", @"(a\otimes b)\otimes c", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\alpha^x_{Fu, Fv, Fw}")
                .TestMorphism(@"\alpha^x_{Fu, Fv, Fw}", @"(Fu\otimes Fv)\otimes Fw", @"Fu\otimes(Fv\otimes Fw)", MorphismType.OneMorphism);
        }

        [TestMethod]
        public void 変数Associator_Null()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\alpha^{#1?}_{#2?#3?#4?}", @"(#2\otimes #3)\otimes #4", @"#2\otimes (#3\otimes #4)", MorphismType.OneMorphism)
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\alpha_{uvw}", @"(u\otimes v)\otimes w", null)
                .TestMorphism(@"\alpha_{uvw}", @"(u\otimes v)\otimes w", @"u\otimes(v\otimes w)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\alpha_{u, v\otimes w, x}")
                .TestMorphism(@"\alpha_{u, v\otimes w, x}", @"(u\otimes (v\otimes w))\otimes x", @"u\otimes((v\otimes w)\otimes x)", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\alpha_{abc}^{-1}", @"a\otimes(b\otimes c)")
                .TestMorphism(@"\alpha_{abc}^{-1}", @"a\otimes(b\otimes c)", @"(a\otimes b)\otimes c", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\alpha", @"(u\otimes v)\otimes w", @"u\otimes (v\otimes w)")
                .TestMorphism(@"\alpha", @"(u\otimes v)\otimes w", @"u\otimes(v\otimes w)", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\alpha", @"(\encat{A}\otimes \encat{B})\otimes \encat{C}", @"\encat{A}\otimes (\encat{B}\otimes \encat{C})")
                .TestMorphism(@"\alpha", @"(\encat{A}\otimes \encat{B})\otimes \encat{C}", @"\encat{A}\otimes(\encat{B}\otimes \encat{C})", MorphismType.Functor, true);
        }

        [TestMethod]
        public void 変数LeftUnitor()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\lambda^{#1?}_{#2?}", @"\Vunit\otimes #2", @"#2", MorphismType.OneMorphism)
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\lambda_x", @"\Vunit\otimes x", null)
                .TestMorphism(@"\lambda_x", @"\Vunit\otimes x", @"x", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\lambda_x^{-1}", "x", @"\Vunit\otimes x")
                .TestMorphism(@"\lambda_x^{-1}", "x", @"\Vunit\otimes x", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\lambda_{u\otimes v}", @"\Vunit\otimes (u\otimes v)", @"u\otimes v")
                .TestMorphism(@"\lambda_{u\otimes v}", @"\Vunit\otimes (u\otimes v)", @"u\otimes v", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\lambda", @"\Vunit\otimes x", "x")
                .TestMorphism(@"\lambda", @"\Vunit\otimes x", @"x", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数LeftUnitor2()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\lambda^{#1?}_{#2?}", @"\id_{#3?}\ocmp #2", @"#2", MorphismType.OneMorphism)
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\lambda_f", @"\id_b\ocmp f", "f")
                .TestMorphism(@"\lambda_f", @"\id_b\ocmp f", @"f", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\lambda_f", @"\id\ocmp f", "f")
                .TestMorphism(@"\lambda_f", @"\id\ocmp f", @"f", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数RightUnitor()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\otimes\Vunit", @"#2", MorphismType.OneMorphism)
            };
            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\rho_x", @"\Vunit\otimes x", null)
                .TestMorphism(@"\rho_x", @"x\otimes \Vunit", @"x", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"\rho_x^{-1}", "x", @"x\otimes\Vunit")
                .TestMorphism(@"\rho_x^{-1}", "x", @"x\otimes \Vunit", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\rho_{u\otimes v}", @"(u\otimes v)\otimes \Vunit", @"u\otimes v")
                .TestMorphism(@"\rho_{u\otimes v}", @"(u\otimes v)\otimes \Vunit", @"u\otimes v", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\rho", @"x\otimes \Vunit", "x")
                .TestMorphism(@"\rho", @"x\otimes \Vunit", @"x", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数二項演算()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\otimes\Vunit", @"#2", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\Vunit", @"#2", MorphismType.OneMorphism));

            var tikz = new TikZDiagram("", -1, false, false, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\rho^{ab}_x\otimes\rho_u", @"(x\otimes \Vunit)\otimes (u\otimes \Vunit)", @"x\otimes u")
                .TestMorphism(@"\rho^{ab}_x\otimes\rho_u", @"(x\otimes \Vunit)\otimes (u\otimes \Vunit)", @"x\otimes u", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\id\otimes\rho^{ab}_x", @"a\otimes (x\otimes \Vunit)", @"a \otimes x")
                .TestMorphism(@"\id\otimes\rho^{ab}_x", @"a\otimes (x\otimes \Vunit)", @"a \otimes x", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数二項演算_省略しない()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), Morphism.Create(@"f\colon u\rightarrow v").TestSingle()},
                { "g".ToTokenString(), Morphism.Create(@"g\colon s\rightarrow t").TestSingle()},
            };

            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"f\otimes g", @"us", @"vt")
                .All(m =>
                {
                    Console.WriteLine(m.ToString());
                    return !m.Item1.Name.ToTokenString().Equals(@"f\otimes g")
                            || !m.Item1.Source.ToTokenString().Equals(@"us")
                            || !m.Item1.Target.ToTokenString().Equals(@"vt");
                }).IsTrue();
        }

        [TestMethod]
        public void 変数二項演算_省略1()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), Morphism.Create(@"f\colon u\rightarrow v").TestSingle()},
                { "g".ToTokenString(), Morphism.Create(@"g\colon s\rightarrow t").TestSingle()},
            };

            var tikz = new TikZDiagram("", -1, false, true, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), Array.Empty<Functor>());

            tikz.CreateMorphism(@"f\otimes g")
                .TestMorphism(@"f\otimes g", @"u\otimes s", @"v\otimes t", MorphismType.OneMorphism);

            tikz.CreateMorphism(@"f\otimes g", @"u\otimes s", @"v\otimes t")
                .TestMorphism(@"f\otimes g", @"u\otimes s", @"v\otimes t", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"f\otimes g", @"us", @"vt")
                .TestMorphism(@"f\otimes g", @"us", @"vt", MorphismType.OneMorphism, true);
        }

        [TestMethod]
        public void 変数二項演算_省略2()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\otimes\Vunit", @"#2", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\Vunit", @"#2", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"\lambda^{#1?}_{#2?}", @"\Vunit #2", @"#2", MorphismType.OneMorphism));

            var tikz = new TikZDiagram("", -1, false, true, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\rho^{ab}_x\otimes\rho_u", @"(x\Vunit)(u\Vunit)", @"xu")
                .TestMorphism(@"\rho^{ab}_x\otimes\rho_u", @"(x\Vunit)(u\Vunit)", @"xu", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\id\otimes\rho^{ab}_x", @"a(x\Vunit)", @"ax")
                .TestMorphism(@"\id\otimes\rho^{ab}_x", @"a(x\Vunit)", @"ax", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\lambda\otimes(\id\otimes\rho)", @"(\Vunit a)(s(x\Vunit))", @"a(sx)")
                .TestMorphism(@"\lambda\otimes(\id\otimes\rho)", @"(\Vunit a)(s(x\Vunit))", @"a(sx)", MorphismType.OneMorphism, true);

            tikz.CreateMorphism(@"\lambda", @"\Vunit \encat{C}_{ab}", @"\encat{C}_{ab}")
                .TestMorphism(@"\lambda", @"\Vunit \encat{C}_{ab}", @"\encat{C}_{ab}", MorphismType.Functor, true);

            tikz.CreateMorphism(@"\id\otimes\rho",
                @"\encat{C}_{st}(\encat{C}_{xz}\Vunit)",
                @"\encat{C}_{st}\encat{C}_{xz}")
                .TestMorphism(@"\id\otimes\rho",
                @"\encat{C}_{st}(\encat{C}_{xz}\Vunit)",
                @"\encat{C}_{st}\encat{C}_{xz}", MorphismType.Functor, true);

            tikz.CreateMorphism(@"\lambda\otimes(\id\otimes\rho)",
                @"(\Vunit \encat{C}_{ab})(\encat{C}_{st}(\encat{C}_{xz}\Vunit))",
                @"\encat{C}_{ab}(\encat{C}_{st}\encat{C}_{xz})")
                .TestMorphism(@"\lambda\otimes(\id\otimes\rho)",
                @"(\Vunit \encat{C}_{ab})(\encat{C}_{st}(\encat{C}_{xz}\Vunit))",
                @"\encat{C}_{ab}(\encat{C}_{st}\encat{C}_{xz})", MorphismType.Functor, true);
        }

        [TestMethod]
        public void 変数二項演算_省略3()
        {
            var dic = new Dictionary<TokenString, Morphism>();
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(Morphism.Create(@"\alpha^{#1?}_{#2?#3?#4?}\colon (#2#3)#4\rightarrow #2(#3#4)").TestSingle());

            var tikz = new TikZDiagram("", -1, false, true, true, dic, list, Array.Empty<Functor>());

            tikz.CreateMorphism(@"\alpha",
                @"((\encat{C}_{cd}\encat{D}_{c'd'})(\encat{C}_{bc}\encat{D}_{b'c'}))(\encat{C}_{ab}\encat{D}_{a'b'})",
                @"(\encat{C}_{cd}\encat{D}_{c'd'})((\encat{C}_{bc}\encat{D}_{b'c'})(\encat{C}_{ab}\encat{D}_{a'b'}))"
            ).TestMorphism(@"\alpha",
                @"((\encat{C}_{cd}\encat{D}_{c'd'})(\encat{C}_{bc}\encat{D}_{b'c'}))(\encat{C}_{ab}\encat{D}_{a'b'})",
                @"(\encat{C}_{cd}\encat{D}_{c'd'})((\encat{C}_{bc}\encat{D}_{b'c'})(\encat{C}_{ab}\encat{D}_{a'b'}))",
                MorphismType.Functor,
                true
            );
        }

        [TestMethod]
        public void 変数米田()
        {
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), ToMorphismHelp("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor) }
            };
            var tikz = CreateTikZDiagram(dic);

            tikz.CreateMorphism(@"\yoneda", @"\cat{C}", null)
                .TestMorphism(@"\yoneda", @"\cat{C}", @"\widehat{\cat{C}}", MorphismType.Functor);

            tikz.CreateMorphism(@"\yoneda", null, @"\widehat{\cat{C}}")
                .TestMorphism(@"\yoneda", @"\cat{C}", @"\widehat{\cat{C}}", MorphismType.Functor);

            tikz.CreateMorphism(@"\yoneda", @"\cat{C}", @"\widehat{\cat{C}}")
                .TestMorphism(@"\yoneda", @"\cat{C}", @"\widehat{\cat{C}}", MorphismType.Functor, true);

            tikz.CreateMorphism(@"\yoneda").Count().Is(0);

            tikz.CreateMorphism(@"\yoneda", @"\cat{C}", @"\widehat{\cat{D}}").Count().Is(0);

            tikz.CreateMorphism(@"\Lan{F}\yoneda", null, @"\widehat{\cat{C}}")
                .TestMorphism(@"\Lan{F}\yoneda", @"\cat{D}", @"\widehat{\cat{C}}", MorphismType.Functor);

            tikz.CreateMorphism(@"\Lan{\yoneda}F", @"\widehat{\cat{C}}", null)
                .TestMorphism(@"\Lan{\yoneda}F", @"\widehat{\cat{C}}", @"\cat{D}", MorphismType.Functor);
        }


        private TikZDiagram CreateTikZDiagram(Dictionary<TokenString, Morphism> dic)
            => new TikZDiagram("", -1, false, false, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), ExtensionsInTest.CreateDefaultFunctors().ToList());

        private Morphism ToMorphismHelp(string name, string source, string target, MorphismType type)
            => new Morphism(name, source, target, type, -1);
    }
}
