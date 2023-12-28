using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 設定ファイルに対応するクラス
    /// </summary>
    public class Config
    {
        private static readonly string _path = "config.xml";

        /// <summary>
        /// oppositeを表すTeXコマンド
        /// </summary>
        public string Opposite { get; set; } = "";

        /// <summary>
        /// 逆射を表すTeXコマンド
        /// </summary>
        public string Inverse { get; set; } = "";

        /// <summary>
        /// 恒等射を表すTeXコマンド
        /// </summary>
        public string IdentityMorphism { get; set; } = "";

        /// <summary>
        /// Homを表すTeXコマンド
        /// </summary>
        public string Hom { get; set; } = "\\Hom";

        /// <summary>
        /// 恒等関手を表すTeXコマンド
        /// </summary>
        public string IdentityFunctor { get; set; } = "";

        /// <summary>
        /// 対角関手を表すTeXコマンド
        /// </summary>
        public string Diagonal { get; set; } = "";

        /// <summary>
        /// 米田埋込を表すTeXコマンド
        /// </summary>
        public string Yoneda { get; set; } = "\\yoneda";

        /// <summary>
        /// 随伴を表すTeXコマンド
        /// </summary>
        public string Adjoint { get; set; } = "";

        /// <summary>
        /// 射の合成を表すTeXコマンド
        /// </summary>
        public string Composite { get; set; } = "";



        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Operators { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Compositions { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> KanExtensions { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> KanLifts { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Limits { get; set; } = new ObservableCollection<string>();

        public string[] OpenBrackets { get; set; } = new[] { "(", "{", "[", "\\{", "\\langle" };
        public string[] CloseBrackets { get; set; } = new[] { ")", "}", "]", "\\}", "\\rangle" };
        public string[] Separators { get; set; } = new[] { ",", "=", "\\cong", "\\ciso", "\\equiv", "\\cequiv" };

        /// <summary>
        /// 処理する際に無視するTeXコマンド
        /// </summary>
        public ObservableCollection<string> IgnoreCommands { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// パラメーター付き射(ユーザ設定)
        /// </summary>
        public ObservableCollection<string> Morphisms { get; set; } = new ObservableCollection<string>();

        public string MorphismsIdentity => IdentityMorphism + @"_{#1?} \colon #1 \rightarrow #1";

        /// <summary>
        /// 関手(ユーザ設定)
        /// </summary>
        public ObservableCollection<string> Functors { get; set; } = new ObservableCollection<string>();

        public string FunctorsIdentity => $"Functor " + IdentityFunctor + @"_{#1?}#2, #2";

        public string FunctorsDiagonal => $"Functor {Diagonal} #1(#2), #1";

        /// <summary>
        /// nodeのフォーマットを表す正規表現
        /// </summary>
        public string TikZNodeRegex { get; set; } = "";

        /// <summary>
        /// arrowのフォーマットを表す正規表現
        /// </summary>
        public string TikZArrowRegex { get; set; } = "";

        /// <summary>
        /// 射のフォーマットを表す正規表現
        /// </summary>
        public string MorphismRegex { get; set; } = "";

        public string OutputLogFilePath { get; set; } = "";

        public Config()
        {
        }

        private static Config? _instance;

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (File.Exists(_path))
                    {
                        try
                        {
                            _instance = Load();
                        }
                        catch (SerializationException)
                        {
                        }
                    }
                    if (_instance == null)
                    {
                        _instance = CreateDefaultValue();
                    }
                }
                return _instance; 
            }
        }


        /// <summary>
        /// クライアント設定を読み込みます
        /// </summary>
        /// <returns>読み込んだ設定</returns>
        public static Config? Load()
        {
            var serializer = new DataContractSerializer(typeof(Config));

            using var reader = XmlReader.Create(_path);
            return serializer.ReadObject(reader) as Config;
        }

        /// <summary>
        /// クライアント設定を保存します
        /// </summary>
        public void Save()
        {
            var serializer = new DataContractSerializer(typeof(Config));

            using var xw = XmlWriter.Create(_path, new XmlWriterSettings { Indent = true });
            serializer.WriteObject(xw, this);
        }

        /// <summary>
        /// デフォルト値でConfigを生成します
        /// </summary>
        /// <returns>生成されたConfig</returns>
        public static Config CreateDefaultValue()
        {
            var c = new Config
            {
                Opposite = "\\opp",
                Inverse = "-1",
                IdentityMorphism = "\\id",
                IdentityFunctor = "\\id",
                Diagonal = "\\Diagonal",
                Adjoint = "\\dashv",
                Composite = "\\compo",
                TikZNodeRegex = @"\\node\s*\((?<name>[^)]*)\).*\{\$(?<math>.*)\$\}\s*$",
                TikZArrowRegex = @"\\draw\s*\[(?<arrow>[^\]]*)[^()]*\((?<source>[^(),]*)\).*node.*\{\$(?<math>.*)\$\}.*\((?<target>[^(),]*)\)\s*$",
                MorphismRegex = @"^(?<name>.*)\\colon(?<source>.*)\\(?<arrow>[Rr]{1,2})ightarrow(?<target>.*)$",
            };

            c.Categories.AddRange(new[] { "\\cat", "\\moncat", "\\topos", "\\encat", "\\bicat", "\\tricat", "\\qcat", "\\comma", "\\cocomma",
                "\\enHo", "\\zerocat", "\\onecat", "\\twocat", "\\Set", "\\Grp", "\\Ab", "\\CMonoid", "\\Monoid", "\\Top", "\\CptHaus",
                "\\CGWH", "\\CRing", "\\Ring", "\\Dom", "\\Field", "\\CstarAlg", "\\Vect", "\\Ch", "\\Graph", "\\monoidalR", "\\Gray", "\\simplex",
                "\\asimplex", "\\Cat", "\\topcat", "\\simcat", "\\dgCat", "\\MonCat", "\\MonCatlax", "\\Mod", "\\dgMod", "\\Prof", "\\Model" });

            c.Operators.AddRange(new[] { "\\times", "\\otimes", "\\hcmp", "\\ocmp" });

            c.Compositions.AddRange(new[] { "\\circ", "\\ocmp", "\\vcmp" });

            c.KanExtensions.AddRange(new[] { "\\Lan", "\\Ran" });

            c.KanLifts.AddRange(new[] { "\\Lift", "\\Rift" });

            c.Limits.AddRange(new[] { "\\lim", "\\colim", "\\wlim", "\\wcolim", "\\bilim", "\\bicolim", "\\wbilim", "\\wbicolim",
                "\\pslim", "\\pscolim", "\\laxlim", "\\laxcolim" });

            c.IgnoreCommands.AddRange(new[] { "\\", "\\,", "\\!", "\\:", "\\;", "\\quad", "\\qquad", "\\displaystyle", "\\scriptstyle",
                "\\bigl", "\\bigr", "\\Bigl", "\\Bigr", "\\left", "\\right", "\\mathstrut" });

            c.Morphisms.AddRange(new[]
            {
                @"#1\circ - \colon \Hom_{#3?}(#2, #1s) \rightarrow \Hom_{#3?}(#2, #1t)",
                @"-\circ #1 \colon \Hom_{#3?}(#1t, #2) \rightarrow \Hom_{#3?}(#1s, #2)",
                @"#1\ocmp - \colon \Hom_{#3?}(#2, #1s) \rightarrow \Hom_{#3?}(#2, #1t)",
                @"-\ocmp #1 \colon \Hom_{#3?}(#1t, #2) \rightarrow \Hom_{#3?}(#1s, #2)",
                @"\yoneda_{#1?} \colon #1 \rightarrow \widehat{#1}",
                @"\pair{#1, #2} \colon \pair{#1s, #2s} \rightarrow \pair{#1t, #2t}",
            });

            c.Functors.AddRange(new[]
            {
                @"Functor \Hom(#1, #2), \Hom(#1, #2)",
                @"Functor \Hom_{#3}(#1, #2), \Hom_{#3}(#1, #2)",
                @"Functor \yoneda #2 #1, \Hom_{\cat{C}}(#1, #2)",
            });

            return c;
        }

        public IEnumerable<Morphism> CreateDefaultMorphisms()
        {
            foreach (var mor in Morphism.Create(MorphismsIdentity))
            {
                yield return mor;
            }

            foreach (var item in Morphisms)
            {
                foreach (var mor in Morphism.Create(item))
                {
                    yield return mor;
                }
            }
        }

        public IEnumerable<Functor> CreateDefaultFunctors()
        {
            var x = Functor.Create(FunctorsIdentity);
            if (x != null)
            {
                yield return x;
            }

            var y = Functor.Create(FunctorsDiagonal);
            if (y != null)
            {
                yield return y;
            }

            foreach (var item in Functors)
            {
                var z = Functor.Create(item);
                if (z != null)
                {
                    yield return z;
                }
            }
        }
    }
}
