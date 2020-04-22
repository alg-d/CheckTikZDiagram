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
    public class Config
    {
        private static readonly string path = "config.xml";
        public string Yoneda { get; set; } = "";
        public string Opposite { get; set; } = "";
        public string Inverse { get; set; } = "";
        public string Adjoint { get; set; } = "";
        public string Composite { get; set; } = "";
        public string Diagonal { get; set; } = "";
        public string Cocompletion { get; set; } = "";
        public string Product { get; set; } = "";



        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Operators { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Compositions { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> KanExtensions { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> KanLifts { get; set; } = new ObservableCollection<string>();

        public string[] OpenBrackets { get; set; } = new[] { "(", "{", "[", "\\{", "\\langle" };
        public string[] CloseBrackets { get; set; } = new[] { ")", "}", "]", "\\}", "\\rangle" };
        public string[] Separators { get; set; } = new[] { ",", "=", "\\cong" };

        /// <summary>
        /// 処理する際に無視するTeXコマンド
        /// </summary>
        public string[] IgnoreCommands { get; set; } = new[] { "\\scriptstyle", "\\bigl", "\\bigr", "\\Bigl", "\\Bigr", "\\left", "\\right", "^{\\mathstrut}" };

        public ObservableCollection<string> Morphisms { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Functors { get; set; } = new ObservableCollection<string>();


        public string TikZNodeRegex { get; set; } = "";

        public string TikZArrowRegex { get; set; } = "";

        public string MorphismRegex { get; set; } = "";


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
                    if (File.Exists(path))
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
        /// <param name="path">読み込む設定ファイルのパス</param>
        /// <returns>読み込んだ設定</returns>
        public static Config? Load()
        {
            var serializer = new DataContractSerializer(typeof(Config));

            using var reader = XmlReader.Create(path);
            return serializer.ReadObject(reader) as Config;
        }

        /// <summary>
        /// クライアント設定を保存します
        /// </summary>
        /// <param name="path">保存先のパス</param>
        public void Save()
        {
            var serializer = new DataContractSerializer(typeof(Config));

            using var xw = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
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
                Yoneda = "\\yoneda",
                Opposite = "\\opp",
                Inverse = "-1",
                Adjoint = "\\dashv",
                Composite = "\\compo",
                Diagonal = "\\Delta",
                Cocompletion = "\\widehat",
                Product = "\\otimes",
                TikZNodeRegex = @"\\node\s*\((?<name>[^)]*)\).*\{\$(?<math>.*)\$\}\s*$",
                TikZArrowRegex = @"\\draw\s*\[(?<arrow>[^\]]*)[^()]*\((?<source>[^()]*)\).*node.*\{\$(?<math>.*)\$\}.*\((?<target>[^()]*)\)\s*$",
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

            c.Morphisms.AddRange(new[]
            {
                @"\id_{#1?} \colon #1 \rightarrow #1",
                @"#1\circ - \colon \Hom_{#3?}(#2, #1s) \rightarrow \Hom_{#3?}(#2, #1t)",
                @"-\circ #1 \colon \Hom_{#3?}(#1t, #2) \rightarrow \Hom_{#3?}(#1s, #2)",
                @"#1\ocmp - \colon \Hom_{#3?}(#2, #1s) \rightarrow \Hom_{#3?}(#2, #1t)",
                @"-\ocmp #1 \colon \Hom_{#3?}(#1t, #2) \rightarrow \Hom_{#3?}(#1s, #2)",
                @"\yoneda_{#1?} \colon #1 \rightarrow \widehat{#1}",
            });

            c.Functors.AddRange(new[]
            {
                @"Functor \id_{#1?}#2, #2",
                @"Functor \Delta #1(#2), #1",
                @"Functor \Hom(#1, #2), \Hom(#1, #2)",
                @"Functor \Hom_{#3}(#1, #2), \Hom_{#3}(#1, #2)",
                @"Functor \yoneda #2 #1, \Hom_{\cat{C}}(#1, #2)",
            });

            return c;
        }
    }
}
