using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class TikZからTikZNodeを生成するテスト
    {
        [TestMethod]
        public void Create通常()
        {
            TikZNode.Create(@"\node (fst) at (0, 1.2) {$a$}").TestTikZNode("fst", "a");
            TikZNode.Create(@"\node (C) at (0, 1.2) {$\cat{C}$}").TestTikZNode("C", @"\cat {C}");
            TikZNode.Create(@"\node (x0) at (0, 1.2) {$x$}").TestTikZNode("x0", "x");
            TikZNode.Create(@"\node (a) at (3, 1.2) {$(u, v)$}").TestTikZNode("a", "(u, v)");
        }

        [TestMethod]
        public void Create名前無し()
        {
            TikZNode.Create(@"\node[rotate=90] at (0.6, 0.4) {$\Longrightarrow$};").IsNull();
        }
    }
}
