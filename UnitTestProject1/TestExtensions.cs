using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;

namespace UnitTestProject1
{
    [TestClass]
    public class TestExtensions
    {
        [TestMethod]
        public void TestTokenString()
        {
            "A_B".ToTokenString().ToString().Is("A _ { B }");
            "A^j".ToTokenString().ToString().Is("A ^ { j }");
            "A_{cd}".ToTokenString().ToString().Is("A _ { c d }");
            "A^{ij}".ToTokenString().ToString().Is("A ^ { i j }");
            "XYZ^UVW".ToTokenString().ToString().Is("X Y Z ^ { U } V W");
            "XYZ_UVW".ToTokenString().ToString().Is("X Y Z _ { U } V W");
            "\\mu".ToTokenString().ToString().Is("\\mu");
            "\\cat{A}".ToTokenString().ToString().Is("\\cat { A }");
            "\\cat\\Set".ToTokenString().ToString().Is("\\cat \\Set");
            "\\mu_i".ToTokenString().ToString().Is("\\mu _ { i }");
            "\\theta^s".ToTokenString().ToString().Is("\\theta ^ { s }");
            "\\mu_{i}".ToTokenString().ToString().Is("\\mu _ { i }");
            "\\theta^{s}".ToTokenString().ToString().Is("\\theta ^ { s }");
        }
    }
}
