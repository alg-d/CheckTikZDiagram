using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class TestTikZNode
    {

        [TestMethod]
        public void Create通常()
        {
            TikZNode.Create(@"\node (fst) at (0, 1.2) {$a$}").TestTikZNode("fst", "a");
            TikZNode.Create(@"\node (C) at (0, 1.2) {$\cat{C}$}").TestTikZNode("C", @"\cat {C}");
            TikZNode.Create(@"\node (x0) at (0, 1.2) {$x$}").TestTikZNode("x0", "x");
            TikZNode.Create(@"\node[rotate=90] at (0.6, 0.4) {$\Longrightarrow$};").IsNull();
        }
    }
}
