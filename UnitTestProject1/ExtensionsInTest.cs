﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckTikZDiagram;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    static class ExtensionsInTest
    {
        public static void TestMorphism(this Morphism m, string name, string source, string target, MorphismType type)
        {
            m.Name.ToTokenString().TestString(name);
            m.Source.ToTokenString().TestString(source);
            m.Target.ToTokenString().TestString(target);
            m.Type.Is(type);
        }

        public static void TestMorphism(this IEnumerable<Morphism> morphisms, string name, string source, string target, MorphismType type)
        {
            var array = morphisms.ToArray();
            if (array.Length == 1)
            {
                var m = array[0];
                m.Name.ToTokenString().TestString(name);
                m.Source.ToTokenString().TestString(source);
                m.Target.ToTokenString().TestString(target);
                m.Type.Is(type);
            }
            else
            {
                array.Any(m => 
                    m.Name.ToTokenString().Equals(name)
                    && m.Source.ToTokenString().Equals(source)
                    && m.Target.ToTokenString().Equals(target)
                    && m.Type.Equals(type)
                ).IsTrue($"array.Length == {array.Length}");
            }
        }

        public static void TestTikZNode(this TikZNode node, string name, string text)
        {
            node.NodeName.Is(name);
            node.MathObject.ToTokenString().TestString(text);
        }

        public static void TestTikZArrow(this TikZArrow arrow, string name, string source, string target)
        {
            arrow.Name.TestString(name);
            arrow.SourceNodeName.Is(source);
            arrow.TargetNodeName.Is(target);
        }

        public static MathSequence TestSingleMath(this IEnumerable<MathObject> source)
        {
            var list = source.ToList();
            list.Count.Is(1);
            return (MathSequence)list[0];
        }

        public static T TestSingle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            list.Count.Is(1);
            return list[0];
        }

        public static void TestString(this TokenString normalizedString, string text)
        {
            text.ToTokenString().Equals(normalizedString)
                .IsTrue($"[{text.ToTokenString()}] != [{normalizedString}]");
        }

        public static void TestToken(this Token token, string text)
        {
            token.Value.Is(text);
        }

        public static void TestError(this IEnumerable<CheckResult> source)
        {
            foreach (var item in source)
            {
                if (!item.Message.IsNullOrEmpty())
                {
                    Console.WriteLine(item.Message);
                }

                item.IsError.IsFalse();
            }
        }



        public static MathSequence AsMathSequence(this MathObject math)
        {
            return (MathSequence)math;
        }


        public static void IsMathToken(this MathObject mathObject, string main)
        {
            mathObject.IsInstanceOf<MathToken>();
            mathObject.Main.TestString(main);
        }


        public static IEnumerable<Morphism> CreateMorphism(this TikZDiagram tikz, string math, string source = null, string target = null)
        {
            return tikz.CreateMorphism(
                new MathObjectFactory(math).CreateSingle(),
                source == null ? null : new MathObjectFactory(source).CreateSingle(),
                target == null ? null : new MathObjectFactory(target).CreateSingle()
            );
        }

        public static List<Morphism> CreateDefaultMorphisms()
        {
            return new List<Morphism>()
            {
                Morphism.Create(@"\id_{#1?}\colon #1\rightarrow #1").TestSingle(),
                Morphism.Create(@"#1\circ -\colon \Hom_{#3?}(#2, #1s)\rightarrow \Hom_{#3?}(#2, #1t)").TestSingle(),
                Morphism.Create(@"-\circ #1\colon \Hom_{#3?}(#1t, #2)\rightarrow \Hom_{#3?}(#1s, #2)").TestSingle(),
                Morphism.Create(@"#1\ocmp -\colon \Hom_{#3?}(#2, #1s)\rightarrow \Hom_{#3?}(#2, #1t)").TestSingle(),
                Morphism.Create(@"-\ocmp #1\colon \Hom_{#3?}(#1t, #2)\rightarrow \Hom_{#3?}(#1s, #2)").TestSingle(),
                Morphism.Create(@"\yoneda_{#1?}\colon #1\rightarrow \widehat{#1}").TestSingle()
            };
        }

        public static IEnumerable<Functor> CreateDefaultFunctors()
        {
            yield return Functor.Create(@"\id_{#1?}#2", "#2");
            yield return Functor.Create(@"\Delta #1(#2)", "#1");
            yield return Functor.Create(@"\Hom(#1, #2)", @"\Hom(#1, #2)");
            yield return Functor.Create(@"\Hom_{\cat{C}}(#1, #2)", @"\Hom_{\cat{C}}(#1, #2)");
            yield return Functor.Create(@"\yoneda #2 #1", @"\Hom(#1, #2)");
        }
    }
}