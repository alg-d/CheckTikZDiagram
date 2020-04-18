using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TestTikZDiagram_Diagram
    {
        [TestMethod]
        public void Check通常()
        {
            var tex = @"
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$u$};
\node (b) at (0, 0) {$b$};   \node (y) at (1.2, 0) {$v$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (x) to node {$\scriptstyle p$} (y);
\draw[->] (a) to node[swap] {$\scriptstyle i$} (b);
\draw[->] (b) -- node[swap] {$\scriptstyle g$} (y);
\draw[<-] (a) to node {$\scriptstyle k$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "u", MorphismType.OneMorphism) },
                { "p".ToTokenString(), ToMorphismHelp("p", "u", "v", MorphismType.OneMorphism) },
                { "i".ToTokenString(), ToMorphismHelp("i", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "b", "v", MorphismType.OneMorphism) },
                { "k".ToTokenString(), ToMorphismHelp("k", "v", "a", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check折れ線()
        {
            var tex = @"
\node (a) at (0, 1) {$a$}; \node (x) at (1, 2) {$u$};
\coordinate (dummy) at (0, 0);
\draw[->] (a) to (0,2) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle p$} (0,2) to (x);
\draw[->] (a) to (a |- dummy) to node[swap] {$\scriptstyle i$} (dummy -| x) to (x);
\draw[->] (a) -| node {$\scriptstyle g$} (x);
\draw[->] (a) |- node {$\scriptstyle k$} (x);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "u", MorphismType.OneMorphism) },
                { "p".ToTokenString(), ToMorphismHelp("p", "a", "u", MorphismType.OneMorphism) },
                { "i".ToTokenString(), ToMorphismHelp("i", "a", "u", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "a", "u", MorphismType.OneMorphism) },
                { "k".ToTokenString(), ToMorphismHelp("k", "a", "u", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check射の定義がない()
        {
            var tex = @"
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$u$};
\node (b) at (0, 0) {$b$};   \node (y) at (1.2, 0) {$v$};
\draw[->] (a) to node {$\scriptstyle p_i$} (x);
\draw[->] (b) to node {$\scriptstyle q_i$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "p".ToTokenString(), ToMorphismHelp("p", "a", "u", MorphismType.OneMorphism) },
                { "q_{i}".ToTokenString(), ToMorphismHelp("q_{i}", "b", "v", MorphismType.OneMorphism) },
            };
            var xs = CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .Where(x => x.IsError)
                .ToArray();
            xs.Length.Is(1);
            xs[0].Message.Is("p_i の定義が存在しません");
        }

        [TestMethod]
        public void Check_nodeの定義がない_domain()
        {
            var tex = @"
\node (z) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$u$};
\node (b) at (0, 0) {$b$};   \node (y) at (1.2, 0) {$v$};
\draw[->] (a) to node {$\scriptstyle p_i$} (x);
\draw[->] (b) to node {$\scriptstyle q_i$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "p_{i}".ToTokenString(), ToMorphismHelp("p_i", "a", "u", MorphismType.OneMorphism) },
                { "q_{i}".ToTokenString(), ToMorphismHelp("q_{i}", "b", "v", MorphismType.OneMorphism) },
            };
            var xs = CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .Where(x => x.IsError)
                .ToArray();
            xs.Length.Is(1);
            xs[0].Message.Is("p_i: (a) → (x): domainのnodeが存在しません");
        }

        [TestMethod]
        public void Check_nodeの定義がない_codomain()
        {
            var tex = @"
\node (a) at (0, 1.2) {$a$}; \node (c) at (1.2, 1.2) {$u$};
\node (b) at (0, 0) {$b$};   \node (y) at (1.2, 0) {$v$};
\draw[->] (a) to node {$\scriptstyle p_i$} (x);
\draw[->] (b) to node {$\scriptstyle q_i$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "p_{i}".ToTokenString(), ToMorphismHelp("p_i", "a", "u", MorphismType.OneMorphism) },
                { "q_{i}".ToTokenString(), ToMorphismHelp("q_{i}", "b", "v", MorphismType.OneMorphism) },
            };
            var xs = CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .Where(x => x.IsError)
                .ToArray();
            xs.Length.Is(1);
            xs[0].Message.Is("p_i: (a) → (x): codomainのnodeが存在しません");
        }



        [TestMethod]
        public void Check恒等射()
        {
            var tex = @"
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$a$};
\node (b) at (0, 0) {$\cat{C}$};   \node (y) at (1.2, 0) {$\cat{C}$};
\node (c) at (0, 0) {$x$};   \node (z) at (1.2, 0) {$x$};
\draw[->] (a) to node {$\scriptstyle \id_a$} (x);
\draw[->] (b) to node[swap] {$\scriptstyle \id_{\cat{C}}$} (y);
\draw[->] (c) to node[swap] {$\scriptstyle \id$} (z);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check関手適用()
        {
            var tex = @"
\node (a) at (0, 1.2) {$F(a)$}; \node (x) at (1.2, 1.2) {$G(a)$};
\node (b) at (0, 0) {$K(F(b))$}; \node (y) at (1.2, 0) {$K(G(b))$};
\node (c) at (0, -1.2) {$Ka$}; \node (z) at (1.2, -1.2) {$Kb$};
\node (d) at (0, -3) {$K(a)$}; \node (w) at (1.2, -3) {$K(b)$};
\node (aaa) at (0, -4) {$H(a)$};      \node (xxx) at (1.2, -4) {$H(b)$};
\node (bbb) at (0, -5) {$a\times u$}; \node (yyy) at (1.2, -5) {$b\times u$};
\draw[->] (a) to node {$\scriptstyle \theta_a$} (x);
\draw[->] (b) to node {$\scriptstyle K\theta_b$} (y);
\draw[->] (c) to node {$\scriptstyle Kf$} (z);
\draw[->] (d) to node {$\scriptstyle Kf$} (w);
\draw[->] (aaa) to node {$\scriptstyle Hf$} (xxx);
\draw[->] (bbb) to node {$\scriptstyle Hf$} (yyy);
\draw[->] (aaa) to node {$\scriptstyle g$} (xxx);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { @"\theta".ToTokenString(), ToMorphismHelp(@"\theta", "F", "G", MorphismType.NaturalTransformation) },
                { "K".ToTokenString(), ToMorphismHelp(@"K", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "H".ToTokenString(), ToMorphismHelp(@"H", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "f".ToTokenString(), ToMorphismHelp(@"f", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp(@"g", @"a\times u", @"b\times u", MorphismType.OneMorphism) },
            };
            var func = ExtensionsInTest.CreateDefaultFunctors().ToList();
            func.Add(Functor.Create(@"Functor H#1, #1\times u"));
            new TikZDiagram(tex, -1, false, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), func)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check関手適用_2変数()
        {
            var tex = @"
\node (a) at (0, 1) {$T(a_0, b_0)$}; \node (x) at (1, 1) {$T(a_0, b_1)$};
\node (b) at (0, 0) {$T(a_1, b_0)$}; \node (y) at (1, 0) {$T(a_1, b_1)$};
\draw[->] (a) to node {$\scriptstyle T(a_0, g)$} (x);
\draw[->] (x) to node {$\scriptstyle T(f, b_1)$} (y);
\draw[->] (a) to node {$\scriptstyle T(f, b_0)$} (b);
\draw[->] (b) to node {$\scriptstyle T(a_1, g)$} (y);
\draw[->] (a) to node {$\scriptstyle T(f, g)$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "T".ToTokenString(), ToMorphismHelp("T", @"\cat{A}\times\cat{B}", @"\cat{C}", MorphismType.Bifunctor) },
                { "f".ToTokenString(), ToMorphismHelp("f", "a_0", "a_1", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "b_0", @"b_1", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check関手適用_添え字()
        {
            var tex = @"
\node (a) at (0, 1.2) {$Fa_0$}; \node (x) at (1.2, 1.2) {$Ga_0$};
\node (b) at (0, 0) {$F(a_1)$}; \node (y) at (1.2, 0) {$G(a_1)$};
\node (c) at (0, -1.2) {$Ka$}; \node (z) at (1.2, -1.2) {$Kb$};
\node (d) at (0, -3) {$K(a)$}; \node (w) at (1.2, -3) {$K(b)$};
\node (aaa) at (0, -4) {$H(a)$};      \node (xxx) at (1.2, -4) {$H(b)$};
\node (bbb) at (0, -5) {$a\times u$}; \node (yyy) at (1.2, -5) {$b\times u$};
\draw[->] (a) to node {$\scriptstyle \theta_{a_0}$} (x);
\draw[->] (b) to node {$\scriptstyle \theta_{a_1}$} (y);
\draw[->] (a) to node {$\scriptstyle Ff$} (b);
\draw[->] (x) to node {$\scriptstyle G(f)$} (y);
\draw[->] (a) to node {$\scriptstyle Fg_j$} (b);
\draw[->] (x) to node {$\scriptstyle G(g_j)$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { @"\theta".ToTokenString(), ToMorphismHelp(@"\theta", "F", "G", MorphismType.NaturalTransformation) },
                { "F".ToTokenString(), ToMorphismHelp(@"F", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "G".ToTokenString(), ToMorphismHelp(@"G", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "f".ToTokenString(), ToMorphismHelp(@"f", "a_0", "a_1", MorphismType.OneMorphism) },
                { "g_j".ToTokenString(), ToMorphismHelp(@"g_j", @"a_0", @"a_1", MorphismType.OneMorphism) },
            };
            var func = ExtensionsInTest.CreateDefaultFunctors().ToList();
            new TikZDiagram(tex, -1, false, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), func)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void CheckKan拡張()
        {
            var tex = @"
\node (D) at (0, 1.3) {$\cat{D}$};
\node (C) at (0, 0) {$\cat{C}$}; \node (M) at (2.2, 0) {$\cat{U}$};
\node at (0.4, 0.4) {$\scriptstyle \eta$};
\node[rotate=90] at (0.6, 0.4) {$\Longrightarrow$};
\draw[->] (C) to node {$\scriptstyle F$} (D);
\draw[->] (C) to node[swap] {$\scriptstyle E$} (M);
\draw[->] (D) to node[transform canvas={xshift=-6pt}] {$\scriptstyle \Lan{F}E$} (M);
\draw[->] (D) to node {$\scriptstyle \Ran{F}E$} (M);
\draw[->] (D) to node {$\scriptstyle \Lift{F}E$} (M);
\draw[->] (D) to node {$\scriptstyle \Rift{F}E$} (M);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), ToMorphismHelp("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "E".ToTokenString(), ToMorphismHelp("E", @"\cat{C}", @"\cat{U}", MorphismType.Functor) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void CheckKan拡張2()
        {
            var tex = @"
\node (D) at (0, 1.3) {$\cat{D}$};
\node (C) at (0, 0) {$\cat{C}$}; \node (M) at (1.6, 0) {$\cat{U}$}; \node (V) at (3.3, 0) {$\cat{V}$};
\draw[->] (C) to node {$\scriptstyle F$} (D);
\draw[->] (C) to node[swap] {$\scriptstyle E$} (M);
\draw[->] (D) to node[transform canvas={xshift=-6pt}] {$\scriptstyle K\circ (\Lan{F}E)$} (V);
\draw[->] (M) to node[swap] {$\scriptstyle K$} (V);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), ToMorphismHelp("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "E".ToTokenString(), ToMorphismHelp("E", @"\cat{C}", @"\cat{U}", MorphismType.Functor) },
                { "K".ToTokenString(), ToMorphismHelp("K", @"\cat{U}", @"\cat{V}", MorphismType.Functor) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check随伴()
        {
            var tex = @"
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$};
\draw[->] (a) to node {$\scriptstyle f$} (b);
\draw[->] (b) to node {$\scriptstyle g$} (a);
\draw[->] (a) to node[swap] {$\scriptstyle h$} (b);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            Morphism.Create(@"f\dashv g\dashv h\colon a\rightarrow b")
                .ForEach(mor => dic.Add(mor.Name.ToTokenString(), mor));
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check合成射()
        {
            var tex = @"
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) to node {$\scriptstyle f$} (b);
\draw[->] (b) to node {$\scriptstyle g$} (c);
\draw[->] (a) to node[swap] {$\scriptstyle \compo{fg}$} (c);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "b", "c", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check合成射2()
        {
            var tex = @"
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) to node {$\scriptstyle p_1$} (b);
\draw[->] (b) to node {$\scriptstyle q_2$} (c);
\draw[->] (a) to node[swap] {$\scriptstyle \compo{{p_1}{q_2}}$} (c);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "p_{1}".ToTokenString(), ToMorphismHelp("p_{1}", "a", "b", MorphismType.OneMorphism) },
                { "q_{2}".ToTokenString(), ToMorphismHelp("q_{2}", "b", "c", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check合成射3()
        {
            var tex = @"
\node (a) at (0, 0) {$a$}; \node (c) at (3, 0) {$c$};
\node[rotate=90] at (1.5, 0) {$\Leftarrow$}; \node at (1.85, 0) {$\scriptstyle \gamma\hcmp\beta$};
\draw[->, bend left=18] (a) to node {$\scriptstyle k\ocmp f$} (c);
\draw[->, bend right=18] (a) to node[swap] {$\scriptstyle l\ocmp g$} (c);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "a", "b", MorphismType.OneMorphism) },
                { "k".ToTokenString(), ToMorphismHelp("k", "b", "c", MorphismType.OneMorphism) },
                { "l".ToTokenString(), ToMorphismHelp("l", "b", "c", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check米田埋込1()
        {
            var tex = @"
\node (C) at (0, 0) {$\cat{C}$}; \node (C2) at (1, 1) {$\widehat{\cat{C}}$};
\node (D) at (2, 0) {$\cat{D}$}; \node (D2) at (3, 1) {$\widehat{\cat{D}}$};
\node (X) at (4, 0) {$\cat{X}$};
\draw[->] (C) to node {$\scriptstyle \yoneda$} (C2);
\draw[->] (D) to node {$\scriptstyle \yoneda$} (D2);
\draw[->] (C) to node[swap] {$\scriptstyle F$} (D);
\draw[->] (C2) to node[swap] {$\scriptstyle G$} (X);
\draw[->] (C) to node[swap] {$\scriptstyle \compo{{F}{\yoneda}}$} (D2);
\draw[->] (C) to node[swap] {$\scriptstyle \compo{{\yoneda}{G}}$} (X);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), ToMorphismHelp("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "G".ToTokenString(), ToMorphismHelp("G", @"\widehat{\cat{C}}", @"\cat{X}", MorphismType.Functor) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check米田埋込2()
        {
            var tex = @"
\node (C) at (0, 0) {$a$}; \node (C2) at (1, 1) {$\widehat{a}$};
\node (D) at (2, 0) {$b$}; \node (D2) at (3, 1) {$\widehat{b}$};
\draw[->] (C) to node {$\scriptstyle \yoneda_a$} (C2);
\draw[->] (D) to node {$\scriptstyle \yoneda_b$} (D2);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check普遍随伴()
        {
            var tex = @"
\node (D) at (0, 1) {$\widehat{\cat{C}}$}; 
\node (C) at (0, 0) {$\cat{C}$}; \node (U) at (1, 0) {$\cat{U}$};
\draw[->] (C) to node {$\scriptstyle \yoneda$} (D);
\draw[->] (C) to node[swap] {$\scriptstyle F$} (U);
\draw[->] (D) to node {$\scriptstyle \Lan{\yoneda}F$} (U);
\draw[->] (U) to node {$\scriptstyle \Lan{F}\yoneda$} (D);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), ToMorphismHelp("F", @"\cat{C}", @"\cat{U}", MorphismType.Functor) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void CheckEqual()
        {
            var tex = @"
\node (a) at (0, 0) {$a$}; \node (b) at (1, 0) {$b$};
\draw[->] (a) to node {$\scriptstyle f=g$} (b);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "a", "b", MorphismType.OneMorphism) },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check添え字()
        {
            var tex = @"
\node (a) at (0, 0) {$a$}; \node (b) at (1, 0) {$b$};
\draw[->] (a) to node {$\scriptstyle p_{j}$} (b);
\draw[->] (a) to node {$\scriptstyle q_i$} (b);
\end{tikzpicture
";

            var dic = Morphism.Create(@"p_j\colon a\rightarrow b")
                .Concat(Morphism.Create(@"q_{i}\colon a\rightarrow b"))
                .ToDictionary(x => x.Name.ToTokenString());
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check自然変換_成分()
        {
            var tex = @"
\node (a) at (0, 1.2) {$F(a)$}; \node (x) at (1.2, 1.2) {$G(a)$};
\node (b) at (0, 0) {$F(b)$}; \node (y) at (1.2, 0) {$G(b)$};
\node (c) at (0, -1.2) {$Fa$}; \node (z) at (1.2, -1.2) {$Ga$};
\node (d) at (0, -3) {$Fb$}; \node (w) at (1.2, -3) {$Gb$};
\draw[->] (a) to node {$\scriptstyle \theta_a$} (x);
\draw[->] (x) to node {$\scriptstyle Gf$} (y);
\draw[->] (a) to node {$\scriptstyle F(f)$} (b);
\draw[->] (b) to node {$\scriptstyle \theta_{b}$} (y);

\draw[->] (c) to node {$\scriptstyle \theta_a$} (z);
\draw[->] (z) to node {$\scriptstyle Gf$} (w);
\draw[->] (c) to node {$\scriptstyle F(f)$} (d);
\draw[->] (d) to node {$\scriptstyle \theta_{b}$} (w);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { @"\theta".ToTokenString(), ToMorphismHelp(@"\theta", "F", "G", MorphismType.NaturalTransformation) },
                { "F".ToTokenString(), ToMorphismHelp(@"F", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "G".ToTokenString(), ToMorphismHelp(@"G", @"\cat{C}", @"\cat{D}", MorphismType.Functor) },
                { "f".ToTokenString(), ToMorphismHelp(@"f", "a", "b", MorphismType.OneMorphism) },
            };
            var func = ExtensionsInTest.CreateDefaultFunctors().ToList();
            new TikZDiagram(tex, -1, false, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), func)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check自然変換_関手()
        {
            var tex = @"
\node (a) at (0, 3) {$FK$}; \node (x) at (1, 3) {$GK$};
\node (b) at (0, 2) {$UFK$}; \node (y) at (1, 2) {$UGK$};
\node (c) at (0, 1) {$F(K)$}; \node (z) at (1, 1) {$G(K)$};
\node (d) at (0, 0) {$UF(K)$}; \node (w) at (1, 0) {$U(GK)$};
\draw[->] (a) to node {$\scriptstyle \theta_K$} (x);
\draw[->] (b) to node {$\scriptstyle U\theta_K$} (y);
\draw[->] (c) to node {$\scriptstyle \theta_K$} (z);
\draw[->] (d) to node {$\scriptstyle U\theta_K$} (w);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), ToMorphismHelp("F", "\\cat{C}", "\\cat{D}", MorphismType.Functor) },
                { "G".ToTokenString(), ToMorphismHelp("G", "\\cat{C}", "\\cat{D}", MorphismType.Functor) },
                { "K".ToTokenString(), ToMorphismHelp("K", "\\cat{C}", "\\cat{D}", MorphismType.Functor) },
                { "U".ToTokenString(), ToMorphismHelp("U", "\\cat{C}", "\\cat{D}", MorphismType.Functor) },
                { "\\theta".ToTokenString(), ToMorphismHelp("\\theta", "F", "G", MorphismType.NaturalTransformation) },
            };
            var list = ExtensionsInTest.CreateDefaultFunctors().ToList();
            new TikZDiagram(tex, -1, false, true, dic, Array.Empty<Morphism>(), list)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check自然変換_2変数関手()
        {
            var tex = @"
\node (a) at (0, 1) {$S(a, b)$}; \node (x) at (1, 1) {$T(a, b)$};
\node (b) at (0, 0) {$S(a^i, b_j)$}; \node (y) at (1, 0) {$T(a^i, b_j)$};
\draw[->] (a) to node {$\scriptstyle \phi_{ab}$} (x);
\draw[->] (b) to node {$\scriptstyle \phi_{a^ib_j}$} (y);
\draw[->] (a) to node {$\scriptstyle S(f^i, g_j)$} (b);
\draw[->] (x) to node {$\scriptstyle T(f^i, g_j)$} (y);
\end{tikzpicture
";

            var mor1 = ToMorphismHelp(@"\phi", "S", "T", MorphismType.NaturalTransformation);
            var mor2 = ToMorphismHelp(@"S", @"\cat{A}\times\cat{B}", @"\cat{C}", MorphismType.Bifunctor);
            var mor3 = ToMorphismHelp(@"T", @"\cat{A}\times\cat{B}", @"\cat{C}", MorphismType.Bifunctor);
            var mor4 = ToMorphismHelp(@"f^{#1}", @"a", @"a^{#1}", MorphismType.OneMorphism);
            var mor5 = ToMorphismHelp(@"g_{#1}", @"b", @"b_{#1}", MorphismType.OneMorphism);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
            };
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(mor4);
            list.Add(mor5);

            new TikZDiagram(tex, -1, false, true, dic, list, ExtensionsInTest.CreateDefaultFunctors().ToList())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check自然変換id()
        {
            var tex = @"
\node (a) at (0, 1) {$a$}; \node (x) at (1, 1) {$GFa$};
\node (b) at (0, 0) {$a\amalg b$}; \node (y) at (1, 0) {$GF(a\amalg b)$};
\draw[->] (a) to node {$\scriptstyle \eta_a$} (x);
\draw[->] (b) to node {$\scriptstyle \eta_{a\amalg b}$} (y);
\draw[->] (a) to node {$\scriptstyle f$} (b);
\draw[->] (x) to node {$\scriptstyle GFf$} (y);
\end{tikzpicture
";

            var mor1 = ToMorphismHelp(@"\eta", @"\id_{\cat{C}}", "GF", MorphismType.NaturalTransformation);
            var mor2 = ToMorphismHelp(@"G", @"\cat{D}", @"\cat{C}", MorphismType.Functor);
            var mor3 = ToMorphismHelp(@"F", @"\cat{C}", @"\cat{D}", MorphismType.Functor);
            var mor4 = ToMorphismHelp(@"f", @"a", @"a\amalg b", MorphismType.OneMorphism);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
                { mor4.Name.ToTokenString(), mor4 },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Check自然変換Hom()
        {
            var tex = @"
\node (a) at (0, 1) {$\Hom_{\cat{C}}(b, a)$}; \node (x) at (1, 1) {$Pb$};
\node (b) at (0, 0) {$\Hom_{\cat{C}}(c, a)$}; \node (y) at (1, 0) {$Pc$};
\draw[->] (a) to node {$\scriptstyle \theta_b$} (x);
\draw[->] (y) to node {$\scriptstyle Pf$} (x);
\draw[<-] (x) to node {$\scriptstyle Pf$} (y);
\draw[->] (b) to node {$\scriptstyle \Hom_{\cat{C}}(f, a)$} (a);
\draw[<-] (a) to node {$\scriptstyle \Hom_{\cat{C}}(f, a)$} (b);
\draw[->] (b) to node {$\scriptstyle \theta_c$} (y);
\end{tikzpicture
";

            var mor1 = ToMorphismHelp(@"\theta", @"\Hom_{\cat{C}}(-, a)", "P", MorphismType.NaturalTransformation);
            var mor2 = ToMorphismHelp(@"P", @"\cat{C}", @"\Set", MorphismType.ContravariantFunctor);
            var mor3 = ToMorphismHelp(@"f", "b", "c", MorphismType.OneMorphism);
            var mor4 = ToMorphismHelp(@"\Hom_{\cat{C}}(-, a)", @"\cat{C}", @"\Set", MorphismType.ContravariantFunctor);

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { mor1.Name.ToTokenString(), mor1 },
                { mor2.Name.ToTokenString(), mor2 },
                { mor3.Name.ToTokenString(), mor3 },
                { mor4.Name.ToTokenString(), mor4 },
            };
            CreateTikZDiagram(tex, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void Definition()
        {
            var tex1 = @"
%CheckTikZDiagram
\node (a) at (0, 1) {$FK$}; \node (x) at (1, 1) {$GK$};
\node (b) at (0, 0) {$uvs$};  \node (s) at (1, 1) {$u$};
\draw[->] (a) to node {$\scriptstyle \theta_a$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (b);
\draw[->] (a) to node {$\scriptstyle p_0$} (s);
\end{tikzpicture
";
            var tex2 = @"
\node (x) at (0, 1) {$FK$}; \node (z) at (1, 1) {$GK$};
\node (y) at (0, 0) {$uvs$}; \node (u) at (1, 1) {$u$};
\draw[->] (x) to node {$\scriptstyle \theta_a$} (z);
\draw[->] (x) to node {$\scriptstyle g$} (y);
\draw[->] (x) to node {$\scriptstyle p_0$} (u);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            new TikZDiagram(tex1, -1, true, true, dic, Array.Empty<Morphism>(), Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
            CreateTikZDiagram(tex2, dic)
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターAssociatorBicat()
        {
            var tex = @"
\node (123) at (3, 2.8) {$((k\ocmp h)\ocmp g)\ocmp f$};
\node (213) at (0, 1.5) {$(k\ocmp (h\ocmp g))\ocmp f$}; \node (132) at (6, 1.5) {$(k\ocmp h)\ocmp (g\ocmp f)$};
\node (312) at (0.4, 0) {$k\ocmp ((h\ocmp g)\ocmp f)$}; \node (321) at (5.6, 0) {$k\ocmp (h\ocmp (g\ocmp f))$};
\draw[->] (123) to node[swap] {$\scriptstyle \alpha^{bcde}_{khg}\hcmp f$} (213);
\draw[->] (213) to node[swap, yshift=5pt] {$\scriptstyle \alpha^{abde}_{k, h\ocmp g, f}$} (312);
\draw[->] (312) to node[swap] {$\scriptstyle k\hcmp\alpha^{abcd}_{hgf}$} (321);

\draw[->] (123) to node {$\scriptstyle \alpha^{abce}_{k\ocmp h, g, f}$} (132);
\draw[->] (132) to node[yshift=5pt] {$\scriptstyle \alpha^{acde}_{k, h, g\ocmp f}$} (321);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "b", "c", MorphismType.OneMorphism) },
                { "h".ToTokenString(), ToMorphismHelp("h", "c", "d", MorphismType.OneMorphism) },
            };
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\alpha^{#1?}_{#2?#3?#4?}", @"(#2\ocmp #3)\ocmp #4", @"#2\ocmp (#3\ocmp #4)", MorphismType.OneMorphism)
            };
            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターAssociatorEncat()
        {
            var tex = @"
\node (abcd1) at (0, 2.2) {$\bigl(\encat{C}(c, d)\otimes\encat{C}(b, c)\bigr)\otimes\encat{C}(a, b)$};
\node (abcd2) at (5, 2.2) {$\encat{C}(c, d)\otimes\bigl(\encat{C}(b, c)\otimes\encat{C}(a, b)\bigr)$};
\node (abd) at (0, 1) {$\encat{C}(b, d)\otimes\encat{C}(a, b)$};
\node (acd) at (5, 1) {$\encat{C}(c, d)\otimes\encat{C}(a, c)$};
\node (ad) at (2.5, 0) {$\encat{C}(a, d)$};
\draw[->] (abcd1) to node[swap] {$\scriptstyle m_{bcd}\otimes\id$} (abd);
\draw[->] (abd) to node[swap] {$\scriptstyle m_{abd}$} (ad);
\draw[->] (abcd1) to node {$\scriptstyle \alpha$} (abcd2);
\draw[->] (abcd2) to node {$\scriptstyle \id\otimes m_{abc}$} (acd);
\draw[->] (acd) to node {$\scriptstyle m_{acd}$} (ad);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(Morphism.Create(@"\alpha^{#1?}_{#2?#3?#4?}\colon (#2\otimes #3)\otimes #4\rightarrow #2\otimes (#3\otimes #4)").TestSingle());
            list.Add(Morphism.Create(@"m_{#1?#2?#3?}\colon\encat{#4}(#2, #3)\otimes \encat{#4}(#1, #2)\rightarrow \encat{#4}(#1, #3)").TestSingle());

            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターUnitor()
        {
            var tex = @"
\node (g1) at (0, 1.2) {$(g\ocmp\id_b)\ocmp f$}; \node (1f) at (3.4, 1.2) {$g\ocmp(\id_b\ocmp f)$};
                                  \node (gf) at (1.7, 0) {$g\ocmp f$};
\draw[->] (g1) to node {$\scriptstyle \alpha^{abbc}_{g, \id_b, f}$} (1f);
\draw[->] (1f) to node {$\scriptstyle g\hcmp\lambda^{ab}_f$} (gf);
\draw[->] (g1) to node[swap] {$\scriptstyle \rho^{bc}_g\hcmp f$} (gf);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), ToMorphismHelp("f", "a", "b", MorphismType.OneMorphism) },
                { "g".ToTokenString(), ToMorphismHelp("g", "b", "c", MorphismType.OneMorphism) },
            };
            var list = new List<Morphism>()
            {
                ToMorphismHelp(@"\alpha^{#1?}_{#2?#3?#4?}", @"(#2\ocmp #3)\ocmp #4", @"#2\ocmp (#3\ocmp #4)", MorphismType.OneMorphism),
                ToMorphismHelp(@"\lambda^{#1?}_{#2?}", @"\id_{#3}\ocmp #2", @"#2", MorphismType.OneMorphism),
                ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\ocmp\id_{#3}", @"#2", MorphismType.OneMorphism)
            };
            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターev()
        {
            var tex = @"
\node (a) at (0, 2.4) {$[\Vunit, u]$};
\node (b) at (0, 1.2) {$[\Vunit, u]\otimes\Vunit$};
\node (c) at (0, 0) {$u$}; \node (x) at (3, 0) {$[\Vunit, u]$};
\draw[->] (a) to node {$\scriptstyle \id$} (x);

\draw[->] (a) to node[swap] {$\scriptstyle \rho^{-1}$} (b);
\draw[->] (b) to node[swap] {$\scriptstyle \ev$} (c);
\draw[->] (c) to node[swap] {$\scriptstyle i$} (x);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(ToMorphismHelp(@"\ev", @"[#1, #2]\otimes #1", @"#2", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"i", @"#1", @"[\Vunit, #1]", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"\rho^{#1?}_{#2?}", @"#2\otimes\Vunit", @"#2", MorphismType.OneMorphism));

            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターNull()
        {
            var tex = @"
\node (a) at (0, 1) {$\Hom_{\cat{C}}(a, b)$}; \node (x) at (0, 1) {$\Hom(a, b)$};
\draw[->] (a) to node[swap] {$\scriptstyle f_b$} (x);
\draw[->] (x) to node[swap] {$\scriptstyle f_b$} (a);
\draw[->] (a) to node[swap] {$\scriptstyle f$} (x);
\draw[->] (x) to node[swap] {$\scriptstyle f$} (a);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            var list = new List<Morphism>()
            {
                Morphism.Create(@"f_{#4?}\colon \Hom_{#1?}(#3, #4)\rightarrow \Hom_{#2?}(#3, #4)").Single()
            };

            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターst()
        {
            var tex = @"
\node (a) at (0, 1) {$\Hom_{\cat{C}}(v, a)$}; \node (x) at (0, 1) {$\Hom_{\cat{C}}(u, a)$};
\node (b) at (0, 0) {$\Hom_{\cat{C}}(v, b)$}; \node (y) at (0, 0) {$\Hom_{\cat{C}}(u, b)$};
\draw[->] (a) to node[swap] {$\scriptstyle f\circ -$} (b);
\draw[->] (x) to node[swap] {$\scriptstyle f\circ -$} (y);
\draw[->] (a) to node[swap] {$\scriptstyle -\circ g$} (x);
\draw[->] (b) to node[swap] {$\scriptstyle -\circ g$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), Morphism.Create(@"f\colon a\rightarrow b").Single() },
                { "g".ToTokenString(), Morphism.Create(@"g\colon u\rightarrow v").Single() }
            };
            var list = ExtensionsInTest.CreateDefaultMorphisms();

            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターst_Null()
        {
            var tex = @"
\node (a) at (0, 1) {$\Hom(v, a)$}; \node (x) at (0, 1) {$\Hom(u, a)$};
\node (b) at (0, 0) {$\Hom(v, b)$}; \node (y) at (0, 0) {$\Hom(u, b)$};
\draw[->] (a) to node[swap] {$\scriptstyle f\circ -$} (b);
\draw[->] (x) to node[swap] {$\scriptstyle f\circ -$} (y);
\draw[->] (a) to node[swap] {$\scriptstyle -\circ g$} (x);
\draw[->] (b) to node[swap] {$\scriptstyle -\circ g$} (y);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "f".ToTokenString(), Morphism.Create(@"f\colon a\rightarrow b").Single() },
                { "g".ToTokenString(), Morphism.Create(@"g\colon u\rightarrow v").Single() }
            };
            var list = ExtensionsInTest.CreateDefaultMorphisms();

            new TikZDiagram(tex, -1, false, true, dic, list, Array.Empty<Functor>())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void パラメーターst_自然変換()
        {
            var tex = @"
\node (a) at (0, 1) {$\Hom(Gj, Fi)$}; \node (x) at (0, 1) {$\Hom(Fj, Fi)$};
\node (b) at (0, 0) {$\Hom(Gj, Gi)$}; 
\draw[->] (a) to node[swap] {$\scriptstyle \theta_i\circ -$} (b);
\draw[->] (a) to node[swap] {$\scriptstyle -\circ \theta_j$} (x);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), Morphism.Create(@"F\colon \cat{C}\rightarrow \cat{D}").Single() },
                { "G".ToTokenString(), Morphism.Create(@"G\colon \cat{C}\rightarrow \cat{D}").Single() },
                { @"\theta".ToTokenString(), Morphism.Create(@"\theta\colon F\Rightarrow G").Single() },
            };
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            new TikZDiagram(tex, -1, false, true, dic, list, ExtensionsInTest.CreateDefaultFunctors().ToList())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void TEST1()
        {
            var tex = @"
\node (b1) at (0, 3) {$T(\dom f)$};\node (b2) at (4, 3) {$Tk$};
\node (cop1) at (0, 1.5) {$\dcoprod_{f\in\Mor(\cat{J})}T(\dom f)$};
                    \node (cop2) at (4, 1.5) {$\dcoprod_{j\in\Ob(\cat{J})}Tj$};
                    \node (coeq) at (6, 1.7) {$c$};
                    \node (c) at (8, 1.7) {$a$};
\node (c1) at (0, 0) {$T(\dom f)$};\node (c2) at (4, 0) {$Tj$};
\draw[->,bend left=15] (b2) to node {$\scriptstyle \sigma_k$} (c);
\draw[->] (b1) to node[swap] {$\scriptstyle \nu_f$} (cop1); \draw[->] (b2) to node[swap] {$\scriptstyle \mu_k$} (cop2);
\draw[->] (c1) to node {$\scriptstyle \nu_f$} (cop1); \draw[->] (c2) to node {$\scriptstyle \mu_j$} (cop2);
\draw[->] (b1) to node {$\scriptstyle Tf$} (b2);
\draw[->] (c1) to node[swap] {$\scriptstyle \id$} (c2);
\draw[->] (b2) to node {$\scriptstyle \eta_k$} (coeq);
\draw[->] (c2) to node[swap] {$\scriptstyle \eta_j$} (coeq);
\draw[->,bend right=15] (c2) to node[swap] {$\scriptstyle \sigma_j$} (c);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            dic.Add("f".ToTokenString(), new Morphism("f", @"\dom f", "k", MorphismType.OneMorphism, -1));
            dic.Add("T".ToTokenString(), new Morphism("T", @"\cat{J}", @"\cat{C}", MorphismType.OneMorphism, -1));
            dic.Add(@"\id".ToTokenString(), new Morphism(@"\id", @"T(\dom f)", @"Tj", MorphismType.OneMorphism, -1));
            dic.Add(@"\sigma".ToTokenString(), new Morphism(@"\sigma", @"T", @"\Delta a", MorphismType.NaturalTransformation, -1));
            var list = ExtensionsInTest.CreateDefaultMorphisms();
            list.Add(ToMorphismHelp(@"\eta_{#1}", @"T#1", @"c", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"\mu_{#1}", @"T#1", @"\dcoprod_{j\in\Ob(\cat{J})}Tj", MorphismType.OneMorphism));
            list.Add(ToMorphismHelp(@"\nu_f", @"T(\dom f)", @"\dcoprod_{f\in\Mor(\cat{J})}T(\dom f)", MorphismType.OneMorphism));

            new TikZDiagram(tex, -1, false, true, dic, list, ExtensionsInTest.CreateDefaultFunctors().ToList())
                .CheckDiagram()
                .TestError();
        }

        [TestMethod]
        public void TEST2()
        {
            var tex = @"
\node (Tj) at (0, 2) {$\Hom_{\cat{C}}(c, Tj)$};
                       \node (x) at (2.5, 1) {$\Hom_{\cat{C}}(c, \lim T)$};
\node (Tk) at (0, 0) {$\Hom_{\cat{C}}(c, Tk)$};
\node (a) at (0, -1) {$\lim T$}; \node (b) at (1, -1) {$Tj$};
\draw[->] (x) to node[swap] {$\scriptstyle \pi_j\circ-$} (Tj);
\draw[->] (Tj) to node[swap] {$\scriptstyle Tf\circ-$} (Tk);
\draw[->] (a) to node {$\scriptstyle \pi_j$} (b);
\draw[->] (x) to node {$\scriptstyle \pi_k\circ-$} (Tk);
\end{tikzpicture
";

            var dic = new Dictionary<TokenString, Morphism>();
            dic.Add("T".ToTokenString(), new Morphism("T", @"\cat{J}", @"\cat{C}", MorphismType.Functor, -1));
            dic.Add(@"\pi".ToTokenString(), new Morphism(@"\pi", @"\Delta(\lim T)", "T", MorphismType.NaturalTransformation, -1));
            dic.Add("f".ToTokenString(), new Morphism(@"f", "j", "k", MorphismType.OneMorphism, -1));
            var list = ExtensionsInTest.CreateDefaultMorphisms();

            new TikZDiagram(tex, -1, false, true, dic, list, ExtensionsInTest.CreateDefaultFunctors().ToList())
                .CheckDiagram()
                .TestError();
        }


        private TikZDiagram CreateTikZDiagram(string tex, Dictionary<TokenString, Morphism> dic)
            => new TikZDiagram(tex, -1, false, true, dic, ExtensionsInTest.CreateDefaultMorphisms(), ExtensionsInTest.CreateDefaultFunctors().ToList());

        private Morphism ToMorphismHelp(string name, string source, string target, MorphismType type)
            => new Morphism(name, source, target, type, -1);
    }
}
