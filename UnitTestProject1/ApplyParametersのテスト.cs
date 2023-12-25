using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace UnitTestProject1
{
    [TestClass]
    public class ApplyParametersのテスト
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void トークン1個の代入(bool setNull)
        {
            var def = new MathObjectFactory(@"#1").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("a").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals("a"))
                .ToArray();
            list.Length.Is(1);
            list[0].IsMathToken("a");


            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\test").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test"))
                .ToArray();
            list.Length.Is(1);
            list[0].IsMathToken(@"\test");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void トークン複数の代入(bool setNull)
        {
            var def = new MathObjectFactory(@"#1").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abc").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull).ToArray();
            list.Length.Is(1);
            var math = (MathSequence)list[0];
            math.List.Count.Is(3);
            math.List[0].IsMathToken(@"a");
            math.List[1].IsMathToken(@"b");
            math.List[2].IsMathToken(@"c");
            math.Main.TestString(@"abc");
            math.ToTokenString().TestString("abc");

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\theta_a").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull).ToArray();
            list.Length.Is(1);
            math = (MathSequence)list[0];
            math.List.Count.Is(1);
            math.List[0].IsMathToken(@"\theta");
            math.Main.TestString(@"\theta");
            math.Sup.IsNull();
            math.Sub.IsMathToken("a");
            math.ToTokenString().TestString(@"\theta_a");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void 複数変数の代入(bool setNull)
        {
            var def = new MathObjectFactory(@"#1#2#3").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abc").CreateSingle() },
                { "#2", new MathObjectFactory("def").CreateSingle() },
                { "#3", new MathObjectFactory("ghi").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull).ToArray();
            list.Length.Is(8);
            foreach (var item in list)
            {
                if (item.ToTokenString().Equals(@"abcdefghi"))
                {
                    var math = (MathSequence)item;
                    math.List.Count.Is(3);
                    math.List[0].ToTokenString().TestString("abc");
                    math.List[1].ToTokenString().TestString("def");
                    math.List[2].ToTokenString().TestString("ghi");
                    math.Main.TestString(@"abcdefghi");
                }
                else if (item.ToTokenString().Equals(@"(abc)(def)(ghi)"))
                {
                    var math = (MathSequence)item;
                    math.List.Count.Is(3);
                    math.List[0].ToTokenString().TestString("(abc)");
                    math.List[1].ToTokenString().TestString("(def)");
                    math.List[2].ToTokenString().TestString("(ghi)");
                    math.Main.TestString(@"(abc)(def)(ghi)");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\alpha a").CreateSingle() },
                { "#2", new MathObjectFactory(@"\beta b").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull).ToArray();
            list.Length.Is(4);
            foreach (var item in list)
            {
                if (item.ToTokenString().Equals(@"\alpha a\beta b #3"))
                {
                    var math = (MathSequence)item;
                    math.List.Count.Is(3);
                    math.List[0].ToTokenString().TestString(@"\alpha a");
                    math.List[1].ToTokenString().TestString(@"\beta b");
                    math.List[2].ToTokenString().TestString(@"#3");
                    math.Main.TestString(@"\alpha a\beta b #3");
                }
                else if (item.ToTokenString().Equals(@"(\alpha a)(\beta b) #3"))
                {
                    var math = (MathSequence)item;
                    math.List.Count.Is(3);
                    math.List[0].ToTokenString().TestString(@"(\alpha a)");
                    math.List[1].ToTokenString().TestString(@"(\beta b)");
                    math.List[2].ToTokenString().TestString(@"#3");
                    math.Main.TestString(@"(\alpha a)(\beta b) #3");
                }
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void 添え字1(bool setNull)
        {
            var def = new MathObjectFactory(@"\test^{#1}_{#2#3}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull).ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("a");
                s.List[1].IsMathToken("b");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull).ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsMathToken("#1");
                var s = (MathSequence)math.Sub;
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void 添え字2(bool setNull)
        {
            var def = new MathObjectFactory(@"\test^{#1}_{#2#3}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abcd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{abcd}_{#2#3}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(4);
                math.Sup.Main.TestString("abcd");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("#2");
                s.List[1].IsMathToken("#3");
                math.Main.TestString(@"\test");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab^k").CreateSingle() },
                { "#2", new MathObjectFactory("c^i").CreateSingle() },
                { "#3", new MathObjectFactory("d_j").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{ab^k}_{c^id_j}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("a");
                s.List[1].ToTokenString().TestString("b^k");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].ToTokenString().TestString("c^i");
                s.List[1].ToTokenString().TestString("d_j");
                math.Main.TestString(@"\test");
            }
        }

        [TestMethod]
        public void 省略可変数_通常()
        {
            var def = new MathObjectFactory(@"#1?").Create().TestSingle();
            def.IsMathToken("#1?");

            var parameters = new Dictionary<string, MathObject>();
            var list = def.ApplyParameters(parameters, true).ToArray();
            list.Length.Is(0);

            list = def.ApplyParameters(parameters, false)
                .Where(x => x.ToTokenString().Equals(@"#1?"))
                .ToArray();
            list.Length.Is(1);
            list[0].IsMathToken(@"#1?");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void 省略可変数_添え字(bool setNull)
        {
            var def = new MathObjectFactory(@"\test^{#1?}_{#2?#3?}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test^{ab}_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("a");
                s.List[1].IsMathToken("b");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }


            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("c").CreateSingle() },
                { "#3", new MathObjectFactory("d").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .Where(x => x.ToTokenString().Equals(@"\test_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsNull();
                var s = (MathSequence)math.Sub;
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }

            list = def.ApplyParameters(parameters, false)
                .Where(x => x.ToTokenString().Equals(@"\test^{#1?}_{cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsMathToken("#1?");
                var s = (MathSequence)math.Sub;
                s.List[0].IsMathToken("c");
                s.List[1].IsMathToken("d");
                math.Main.TestString(@"\test");
            }
        }

        [TestMethod]
        public void 省略可変数_添え字2()
        {
            var def = new MathObjectFactory(@"\test^{#1?}_{#2?#3?}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("abcd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true)
                .Where(x => x.ToTokenString().Equals(@"\test^{abcd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(4);
                math.Sup.Main.TestString("abcd");
                math.Sub.IsNull();
                math.Main.TestString(@"\test");
            }

            list = def.ApplyParameters(parameters, false)
                .Where(x => x.ToTokenString().Equals(@"\test^{abcd}_{#2?#3?}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                var s = (MathSequence)math.Sup;
                s.List.Count.Is(4);
                s.Main.TestString("abcd");
                s = (MathSequence)math.Sub;
                s.List.Count.Is(2);
                s.Main.TestString("#2?#3?");
                math.Main.TestString(@"\test");
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void カンマ(bool setNull)
        {
            var def = new MathObjectFactory(@"\test{#1, #2}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test{ab, cd}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(2);
                math.List[0].IsMathToken(@"\test");
                math.List[1].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken("a");
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[1].IsMathToken("b");
                math.List[1].AsMathSequence().List[1].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[1].AsMathSequence().List[0].IsMathToken("c");
                math.List[1].AsMathSequence().List[1].AsMathSequence().List[1].IsMathToken("d");
                math.List[1].AsMathSequence().LeftBracket.Value.Is("{");
                math.List[1].AsMathSequence().RightBracket.Value.Is("}");
                math.List[1].AsMathSequence().Separator.Is(",");
                math.Main.TestString(@"\test{ab, cd}");
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, setNull)
                .Where(x => x.ToTokenString().Equals(@"\test{ab, #2}"))
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(2);
                math.List[0].IsMathToken(@"\test");
                math.List[1].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List.Count.Is(2);
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[0].IsMathToken("a");
                math.List[1].AsMathSequence().List[0].AsMathSequence().List[1].IsMathToken("b");
                math.List[1].AsMathSequence().List[1].IsMathToken("#2");
                math.List[1].AsMathSequence().LeftBracket.Value.Is("{");
                math.List[1].AsMathSequence().RightBracket.Value.Is("}");
                math.List[1].AsMathSequence().Separator.Is(",");
                math.Main.TestString(@"\test{ab, #2}");
            }
        }

        [DataTestMethod]
        [DataRow(@"\test^{#1?^{#2}}")]
        [DataRow(@"\test^{#1?^{#2?}}")]
        public void 二重添え字1(string text)
        {
            var def = new MathObjectFactory(text).Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab^{cd}"))
                {
                    s.Main.TestString("ab");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
                else if (s.ToTokenString().Equals("(ab)^{cd}"))
                {
                    s.Main.TestString("(ab)");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab"))
                {
                    s.Main.TestString("ab");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("ab");
                    s.ToTokenString().TestString("ab");
                }
                else if (s.ToTokenString().Equals("(ab)"))
                {
                    s.Main.TestString("(ab)");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("(ab)");
                    s.ToTokenString().TestString("(ab)");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");
                math.Sup.IsNull();
                math.Sub.IsNull();
                math.Main.TestString(@"\test");
                math.ToTokenString().TestString(@"\test");
            }
        }


        [TestMethod]
        public void 二重添え字2()
        {
            var def = new MathObjectFactory(@"\test^{#1^{#2?}}").Create().TestSingleMath();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab^{cd}"))
                {
                    s.Main.TestString("ab");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
                else if (s.ToTokenString().Equals("(ab)^{cd}"))
                {
                    s.Main.TestString("(ab)");
                    s = (MathSequence)s.Sup;
                    s.List.Count.Is(2);
                    s.Main.TestString("cd");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory("ab").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                if (s.ToTokenString().Equals("ab"))
                {
                    s.Main.TestString("ab");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("ab");
                    s.ToTokenString().TestString("ab");
                }
                else if (s.ToTokenString().Equals("(ab)"))
                {
                    s.Main.TestString("(ab)");
                    s.Sup.IsNull();
                    s.Sub.IsNull();
                    s.Main.TestString("(ab)");
                    s.ToTokenString().TestString("(ab)");
                }
            }

            parameters = new Dictionary<string, MathObject>()
            {
                { "#2", new MathObjectFactory("cd").CreateSingle() },
            };
            list = def.ApplyParameters(parameters, true)
                .ToArray();
            list.Length.Is(1);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.List[0].IsMathToken(@"\test");

                var s = (MathSequence)math.Sup;
                s.Main.TestString("#1");
                s.ToTokenString().TestString("#1^{cd}");
                s = (MathSequence)s.Sup;
                s.List.Count.Is(2);
                s.Main.TestString("cd");

                math.ToTokenString().TestString(@"\test^{#1^{cd}}");
            }
        }

        [TestMethod]
        public void 本体が変数()
        {
            var def = new MathObjectFactory(@"#1_x").Create().TestSingle();

            var parameters = new Dictionary<string, MathObject>()
            {
                { "#1", new MathObjectFactory(@"\cat{C}").CreateSingle() },
            };
            var list = def.ApplyParameters(parameters, true).ToArray();
            list.Length.Is(2);
            foreach (var item in list)
            {
                var math = (MathSequence)item;
                math.List.Count.Is(1);
                math.Sup.IsNull();
                math.Sub.ToTokenString().TestString(@"x");

                var x = @"\cat{C}".ToTokenString();
                var y = @"(\cat{C})".ToTokenString();
                (x.Equals(math.Main) || y.Equals(math.Main)).IsTrue();
            }
        }
    }
}
