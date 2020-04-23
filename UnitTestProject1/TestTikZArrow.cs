using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class TestTikZArrow
    {
        [TestMethod]
        public void Create通常()
        {
            TikZArrow.Create(@"\draw[->] (1) to node {$\scriptstyle 0$} (D)").TestSingle().TestTikZArrow("0", "1", "D");
            TikZArrow.Create(@"\draw[<-] (u) to node {$\scriptstyle F$} (v)").TestSingle().TestTikZArrow("F", "v", "u");
            TikZArrow.Create(@"\draw[<-right hook] (x) to node {$\scriptstyle inc $} (y)").TestSingle().TestTikZArrow("inc", "y", "x");
            TikZArrow.Create(@"\draw[dsh] (x0) to node[swap, xshift=2pt] {$\scriptstyle f$} (y1)").TestSingle().TestTikZArrow(@"f", "x0", "y1");

            TikZArrow.Create(@"\draw[->] (a) to node {$\scriptstyle \theta_i$} (b)").TestSingle().TestTikZArrow(@"\theta_i", "a", "b");
            TikZArrow.Create(@"\draw[->] (x) to node {$\scriptstyle \alpha\otimes\beta$} (y)").TestSingle().TestTikZArrow(@"\alpha\otimes\beta", "x", "y");

            TikZArrow.Create(@"\draw[->] (aaa) -- node {$\scriptstyle f$} (bbb)").TestSingle().TestTikZArrow("f", "aaa", "bbb");
            TikZArrow.Create(@"\draw[->] (aaa) -| node {$\scriptstyle f$} (bbb)").TestSingle().TestTikZArrow("f", "aaa", "bbb");
            TikZArrow.Create(@"\draw[->] (x0) to (x1) to node {$\scriptstyle f$} (x2) to (x3)").TestSingle().TestTikZArrow("f", "x0", "x3");
            TikZArrow.Create(@"\draw[->] (a) to (a -| dummy) to node {$\scriptstyle f$} (dummy |- b) to (b)").TestSingle().TestTikZArrow("f", "a", "b");

            TikZArrow.Create(@"\draw[->, bend left=30] (1) to node {$\scriptstyle 0$} (D)").TestSingle().TestTikZArrow("0", "1", "D");
            TikZArrow.Create(@"\draw[->, bend right=30] (1) to node {$\scriptstyle 0$} (D)").TestSingle().TestTikZArrow("0", "1", "D");
            TikZArrow.Create(@"\draw[->, transform canvas={xshift=14pt,yshift=20pt}] (1) to node {$\scriptstyle 0$} (D)")
                .TestSingle().TestTikZArrow("0", "1", "D");
            TikZArrow.Create(@"\draw[->] (1) to node[transform canvas={xshift=14pt,yshift=20pt}] {$\scriptstyle 0$} (D)")
                .TestSingle().TestTikZArrow("0", "1", "D");
        }

        [TestMethod]
        public void Create複数()
        {
            var xs = TikZArrow.Create(@"\draw[->] (a) to node {$\scriptstyle f, g$} (b)").ToList();
            xs.Count.Is(2);
            xs[0].TestTikZArrow("f", "a", "b");
            xs[1].TestTikZArrow("g", "a", "b");

            xs = TikZArrow.Create(@"\draw[->] (a) to node {$\scriptstyle f=g$} (b)").ToList();
            xs.Count.Is(2);
            xs[0].TestTikZArrow("f", "a", "b");
            xs[1].TestTikZArrow("g", "a", "b");

            xs = TikZArrow.Create(@"\draw[->] (a) to node {$\scriptstyle \sigma\cong\tau$} (b)").ToList();
            xs.Count.Is(2);
            xs[0].TestTikZArrow(@"\sigma", "a", "b");
            xs[1].TestTikZArrow(@"\tau", "a", "b");

            xs = TikZArrow.Create(@"\draw[->] (a) to node {$\scriptstyle \Lan{f}g\,,\ \Ran{f}g$} (b)").ToList();
            xs.Count.Is(2);
            xs[0].TestTikZArrow(@"\Lan{f}g", "a", "b");
            xs[1].TestTikZArrow(@"\Ran{f}g", "a", "b");
        }

        [TestMethod]
        public void Create無し()
        {
            TikZArrow.Create(@"\draw[->] (a) to (b)").Count().Is(0);
            TikZArrow.Create(@"\draw[->] (a) -- (b)").Count().Is(0);
            TikZArrow.Create(@"\draw[->] (a) to node {$\scriptstyle $} (b)").Count().Is(0);
            TikZArrow.Create(@"\draw[|->] (s) to node {$\scriptstyle f$} (t)").Count().Is(0);
        }
    }
}
