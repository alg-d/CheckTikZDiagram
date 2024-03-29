﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class EqualsAsMathObjectのテスト
    {
        [TestMethod]
        public void Morphism_通常()
        {
            var dic = new Dictionary<TokenString, Morphism>
            {
                { "F".ToTokenString(), new Morphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor, -1) },
            };
            var diagram = CreateTikZDiagram(dic);

            var left = new MathObjectFactory("Fa").CreateSingle();
            var right = new MathObjectFactory("Fa").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("F(a)").CreateSingle();
            right = new MathObjectFactory("Fa").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("Fa").CreateSingle();
            right = new MathObjectFactory("F(a)").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("F(a)").CreateSingle();
            right = new MathObjectFactory("F(a)").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_変数無し()
        {
            var list = new List<Functor>()
            {
                Functor.Create("Fab", "Gba")
            };
            var diagram = CreateTikZDiagram(list);

            var right = new MathObjectFactory("Gba").CreateSingle();
            diagram.EqualsAsMathObject(new MathObjectFactory("Fab").CreateSingle(), right)
                .IsTrue();
            diagram.EqualsAsMathObject(new MathObjectFactory("Gba").CreateSingle(), right)
                .IsTrue();
            diagram.EqualsAsMathObject(new MathObjectFactory("F").CreateSingle(), right)
                .IsFalse();
        }

        [TestMethod]
        public void Functor_定数関手()
        {
            var list = new List<Functor>()
            {
                Functor.Create("F#1", "x")
            };
            var diagram = CreateTikZDiagram(list);

            var right = new MathObjectFactory("x").CreateSingle();
            diagram.EqualsAsMathObject(new MathObjectFactory("Fa").CreateSingle(), right)
                .IsTrue();
            diagram.EqualsAsMathObject(new MathObjectFactory("F(b)").CreateSingle(), right)
                .IsTrue();
            diagram.EqualsAsMathObject(new MathObjectFactory("F(uvw)").CreateSingle(), right)
                .IsTrue();
            diagram.EqualsAsMathObject(new MathObjectFactory("Fs").CreateSingle(), new MathObjectFactory("Ft").CreateSingle())
                .IsTrue();
        }

        [TestMethod]
        public void Functor_括弧あり()
        {
            var list = new List<Functor>()
            {
                Functor.Create("F#1", "G(#1)")
            };
            var dic = new Dictionary<TokenString, Morphism>
            {
                { "G".ToTokenString(), new Morphism("G", @"\cat{C}", @"\cat{D}", MorphismType.Functor, -1) },
            };
            var diagram = CreateTikZDiagram(dic, list);

            var left = new MathObjectFactory("Fa").CreateSingle();
            var right = new MathObjectFactory("G(a)").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("F(a)").CreateSingle();
            right = new MathObjectFactory("G(a)").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("Fa").CreateSingle();
            right = new MathObjectFactory("Ga").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("F(a)").CreateSingle();
            right = new MathObjectFactory("Ga").CreateSingle();
            diagram.EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_括弧なし()
        {
            var list = new List<Functor>()
            {
                Functor.Create("F#1", "G#1")
            };

            var left = new MathObjectFactory("Fa").CreateSingle();
            var right = new MathObjectFactory("Ga").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("F(a)").CreateSingle();
            right = new MathObjectFactory("Ga").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("Fa").CreateSingle();
            right = new MathObjectFactory("G(a)").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory("F(a)").CreateSingle();
            right = new MathObjectFactory("G(a)").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_恒等関手()
        {
            var list = new List<Functor>()
            {
                Functor.Create(@"\id_{#2?}#1", "#1")
            };

            var left = new MathObjectFactory(@"\id_{\cat{C}}(a)").CreateSingle();
            var right = new MathObjectFactory("a").CreateSingle();

            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"\id(x)").CreateSingle();
            right = new MathObjectFactory("x").CreateSingle();

            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"\id_{\cat{C}}(f\otimes g)").CreateSingle();
            right = new MathObjectFactory(@"f\otimes g").CreateSingle();

            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_対角関手1()
        {
            var list = new List<Functor>()
            {
                Functor.Create(@"\Diagonal #1(#2)", "#1"),
            };

            var left = new MathObjectFactory(@"\Diagonal a(x)").CreateSingle();
            var right = new MathObjectFactory("a").CreateSingle();

            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"\Diagonal (a+b)(x)").CreateSingle();
            right = new MathObjectFactory("a+b").CreateSingle();

            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"GF(\Diagonal a(x))").CreateSingle();
            right = new MathObjectFactory("GFa").CreateSingle();
            var dic = new Dictionary<TokenString, Morphism>
            {
                { "F".ToTokenString(), new Morphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor, -1) },
                { "G".ToTokenString(), new Morphism("G", @"\cat{C}", @"\cat{D}", MorphismType.Functor, -1) },
            };

            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"GF(\Diagonal a(x))").CreateSingle();
            right = new MathObjectFactory("GF(a)").CreateSingle();
            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"GF(\Diagonal a(x))").CreateSingle();
            right = new MathObjectFactory("G(Fa)").CreateSingle();
            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"GF(\Diagonal a(x))").CreateSingle();
            right = new MathObjectFactory("G(F(a))").CreateSingle();
            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"G(F(\Diagonal a(x)))").CreateSingle();
            right = new MathObjectFactory("GFa").CreateSingle();
            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_対角関手2()
        {
            var list = new List<Functor>()
            {
                Functor.Create(@"\Diagonal #1(#2)", "#1"),
                Functor.Create(@"\Hom(#1, #2)", @"\Hom(#1, #2)"),
                Functor.Create(@"\Hom_{#3}(#1, #2)", @"\Hom_{#3}(#1, #2)"),
            };
            var dic = new Dictionary<TokenString, Morphism>()
            {
                { "F".ToTokenString(), new Morphism("F", @"\cat{C}", @"\cat{D}", MorphismType.Functor, -1) }
            };

            var left = new MathObjectFactory(@"\Hom(\Diagonal(\lim T)(x), F(u))").CreateSingle();
            var right = new MathObjectFactory(@"\Hom(\lim T, Fu)").CreateSingle();

            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"\Hom_{\cat{C}}(\Diagonal(\lim T)(x), F(u))").CreateSingle();
            right = new MathObjectFactory(@"\Hom_{\cat{C}}(\lim T, Fu)").CreateSingle();
            new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list)
                .EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_米田1()
        {
            var list = new List<Functor>()
            {
                Functor.Create(@"\yoneda #2 #1", @"\Hom(#1, #2)"),
                Functor.Create("F#1", "G(#1)")
            };

            var left = new MathObjectFactory(@"\yoneda(a)(b)").CreateSingle();
            var right = new MathObjectFactory(@"\Hom(b, a)").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            //left = new MathObjectFactory(@"F(\yoneda(a)(b))").CreateSingle();
            //right = new MathObjectFactory(@"G(\Hom(b, a))").CreateSingle();
            //CreateTikZDiagram(list)
            //    .CheckNode(left, right)
            //    .IsTrue();

            left = new MathObjectFactory(@"F(\yoneda a b)").CreateSingle();
            right = new MathObjectFactory(@"G(\Hom(b, a))").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();
        }

        [TestMethod]
        public void Functor_米田2()
        {
            var list = new List<Functor>()
            {
                Functor.Create(@"\yoneda(#2)(#1)", @"\Hom(#1, #2)"),
                Functor.Create("F#1", "G(#1)")
            };

            var left = new MathObjectFactory(@"\yoneda(a)(b)").CreateSingle();
            var right = new MathObjectFactory(@"\Hom(b, a)").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"F(\yoneda(a)(b))").CreateSingle();
            right = new MathObjectFactory(@"G(\Hom(b, a))").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();

            left = new MathObjectFactory(@"F(\yoneda a b)").CreateSingle();
            right = new MathObjectFactory(@"G(\Hom(b, a))").CreateSingle();
            CreateTikZDiagram(list)
                .EqualsAsMathObject(left, right)
                .IsTrue();
        }


        private TikZDiagram CreateTikZDiagram(IDictionary<TokenString, Morphism> dic)
            => new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), Array.Empty<Functor>());

        private TikZDiagram CreateTikZDiagram(IList<Functor> list)
            => new TikZDiagram("", -1, false, false, true, new Dictionary<TokenString, Morphism>(), Array.Empty<Morphism>(), list);
        private TikZDiagram CreateTikZDiagram(IDictionary<TokenString, Morphism> dic, IList<Functor> list)
            => new TikZDiagram("", -1, false, false, true, dic, Array.Empty<Morphism>(), list);
    }
}
