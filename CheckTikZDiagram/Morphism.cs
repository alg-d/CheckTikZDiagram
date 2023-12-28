using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 数式内における一つの射を表すクラス
    /// </summary>
    public class Morphism
    {
        static private readonly Regex _doubleColon = new Regex(@"^(.*?\\colon)(.*?)(\\colon.*)$");
        static private readonly Regex _inSet = new Regex(@"^(.*)\\in\s*(.*)$");

        /// <summary>
        /// name: 射の数式
        /// source: domainの数式
        /// arrow: 射の種類を決める部分
        /// target: codomainの数式
        /// </summary>
        public static Regex Construct { get; set; } = new Regex(Config.Instance.MorphismRegex);

        /// <summary>
        /// 定義に使用したMorphismのリスト
        /// </summary>
        private readonly ReadOnlyCollection<Morphism> _defMorphismList;

        /// <summary>
        /// 定義された行数
        /// </summary>
        private readonly int _defLine;

        /// <summary>
        /// 射の名前
        /// </summary>
        public MathObject Name { get; }

        /// <summary>
        /// 射のdomain
        /// </summary>
        public MathObject Source { get; }

        /// <summary>
        /// 射のcodomain
        /// </summary>
        public MathObject Target { get; }

        /// <summary>
        /// 射の種類
        /// </summary>
        public MorphismType Type { get; private set; }

        public bool IsFunctor => Type == MorphismType.Functor 
                              || Type == MorphismType.ContravariantFunctor
                              || Type == MorphismType.Bifunctor;

        public Morphism(MathObject math, MathObject source, MathObject target, MorphismType type, IEnumerable<Morphism> defMorphismList)
        {
            Name = math;
            Source = source;
            Target = target;

            if (type == MorphismType.OneMorphism
                && (source.IsCategory() || target.IsCategory()))
            {
                if (source.Contains(@"\times"))
                {
                    Type = MorphismType.Bifunctor;
                }
                else if (IsContravariant(source, target))
                {
                    Type = MorphismType.ContravariantFunctor;
                }
                else
                {
                    Type = MorphismType.Functor;
                }
            }
            else
            {
                Type = type;
            }

            _defMorphismList = new ReadOnlyCollection<Morphism>(defMorphismList.ToList());
            _defLine = -1;
        }

        public Morphism(MathObject math, MathObject source, MathObject target, MorphismType type, int defLine)
            : this(math, source, target, type, Array.Empty<Morphism>())
        {
            _defLine = defLine;
        }

        public Morphism(MathObject math, MathObject source, MathObject target, MorphismType type)
            : this(math, source, target, type, Array.Empty<Morphism>())
        {
        }

        public Morphism(MathObject math, string source, string target, MorphismType type, IEnumerable<Morphism> defMorphismList)
            : this(math, new MathObjectFactory(source).CreateSingle(), new MathObjectFactory(target).CreateSingle(), type, defMorphismList)
        {
        }

        public Morphism(string name, string source, string target, MorphismType type, int defLine)
            : this(new MathObjectFactory(name).CreateSingle(),
                   new MathObjectFactory(source).CreateSingle(),
                   new MathObjectFactory(target).CreateSingle(),
                   type, Array.Empty<Morphism>())
        {
            _defLine = defLine;
        }

        public Morphism(MathObject math, string source, string target, MorphismType type)
            : this(math, source, target, type, Array.Empty<Morphism>())
        {
        }

        public void SetNaturalTransformation()
        {
            Type = MorphismType.NaturalTransformation;
        }

        private static MorphismType GetMorphismType(string text)
        {
            return text switch
            {
                "r" => MorphismType.OneMorphism,
                "R" => MorphismType.TwoMorphism,
                "Rr" => MorphismType.ThreeMorphism,
                "RR" => MorphismType.FourMorphism,
                _ => MorphismType.Unknown,
            };
        }

        /// <summary>
        /// 文字列(射の定義)からMorphismを生成する
        /// </summary>
        /// <param name="text">射の定義を表す文字列</param>
        /// <param name="defLine">定義の位置</param>
        /// <returns></returns>
        public static IEnumerable<Morphism> Create(string text, int defLine = -1)
        {
            foreach (var (math, source, target, arrow) in CreateMainNormal(text).Concat(CreateMainInSet(text)))
            {
                // 射の名前ではs変数, t変数は使用不可
                if (!math.GetVariables().Any(x => x.EndsWith('s') || x.EndsWith('t')))
                {
                    yield return new Morphism(math, source, target, arrow, defLine);
                }
            }
        }

        private static IEnumerable<(MathObject, MathObject, MathObject, MorphismType)> CreateMainNormal(string text)
        {
            var d = _doubleColon.Match(text);
            if (d.Success)
            {
                // \colon が複数ある場合の処理
                text = d.Groups[1].Value + d.Groups[2].Value;
            }

            var m = Construct.Match(text);
            if (m.Success)
            {
                var name = m.Groups["name"].Value.Trim();
                var source = m.Groups["source"].Value.Trim();
                var arrow = m.Groups["arrow"].Value.Trim();
                var target = m.Groups["target"].Value.Trim();

                if (d.Success)
                {
                    // \colon が複数ある場合の処理
                    foreach (var item in Morphism.CreateMainNormal(source + d.Groups[3].Value)) yield return item;
                    foreach (var item in Morphism.CreateMainNormal(target + d.Groups[3].Value)) yield return item;
                }

                var sourceMath = new MathObjectFactory(source).CreateSingle();
                var targetMath = new MathObjectFactory(target).CreateSingle();

                var xs = name.Split(Config.Instance.Adjoint, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < xs.Length; i++)
                {
                    foreach (var item in new MathObjectFactory(xs[i]).Create())
                    {
                        if (i % 2 == 0)
                        {
                            yield return (item, sourceMath, targetMath, GetMorphismType(arrow));
                        }
                        else
                        {
                            yield return (item, targetMath, sourceMath, GetMorphismType(arrow));
                        }
                    }
                }
            }
        }


        private static IEnumerable<(MathObject, MathObject, MathObject, MorphismType)> CreateMainInSet(string text)
        {
            var h = _inSet.Match(text);
            if (!h.Success)
            {
                yield break;
            }

            foreach (var set in new MathObjectFactory(h.Groups[2].Value).Create())
            {
                if (set.TryGetSourceAndTargetAsHom(out var source, out var target))
                {
                    foreach (var item in new MathObjectFactory(h.Groups[1].Value).Create())
                    {
                        yield return (item, source, target, MorphismType.OneMorphism);
                    }
                }
            }
        }

        private bool IsContravariant(MathObject source, MathObject target)
        {
            var opp = new Token(Config.Instance.Opposite, "");
            return ( source.Main.Tokens[0].Equals(opp) && !target.Main.Tokens[0].Equals(opp))
                || (!source.Main.Tokens[0].Equals(opp) &&  target.Main.Tokens[0].Equals(opp));
        }

        public IEnumerable<Morphism> ApplyParameter(MathObject math, IReadOnlyDictionary<string, MathObject> parameters, bool setNull)
        {
            var sources = this.Source.ApplyParameters(parameters, setNull);
            var targets = this.Target.ApplyParameters(parameters, setNull);

            foreach (var s in sources)
            {
                foreach (var t in targets)
                {
                    yield return new Morphism(math, s, t, this.Type, this.GetDefList());
                }
            }
        }

        public IEnumerable<Morphism> ApplyParameter(MathObject math, IReadOnlyDictionary<string, MathObject> parameters, bool setNull, IEnumerable<Morphism> defList)
        {
            var sources = this.Source.ApplyParameters(parameters, setNull);
            var targets = this.Target.ApplyParameters(parameters, setNull);

            foreach (var s in sources)
            {
                foreach (var t in targets)
                {
                    yield return new Morphism(math, s, t, this.Type, this.GetDefList().Concat(defList));
                }
            }
        }

        public IEnumerable<Morphism> GetDefList()
        {
            if (this._defMorphismList.Count == 0)
            {
                yield return this;
            }
            else
            {
                foreach (var item in this._defMorphismList)
                {
                    yield return item;
                }
            }
        }

        public override string ToString() => ToString(0, 0);

        public string ToString(int m, int n)
        {
            var s = this.Source.OriginalText.IsNullOrEmpty() ? this.Source.ToString() : this.Source.OriginalText.Trim();
            var t = this.Target.OriginalText.IsNullOrEmpty() ? this.Target.ToString() : this.Target.OriginalText.Trim();

            if (this._defMorphismList.Count == 0)
            {
                if (this._defLine > 0)
                {
                    return $"{this.Name.OriginalText.Trim()}: {new string(' ', m)}{s} → {new string(' ', n)}{t} ({this._defLine}行目)";
                }
                else
                {
                    return $"{this.Name.OriginalText.Trim()}: {new string(' ', m)}{s} → {new string(' ', n)}{t}";
                }
            }
            else
            {
                var list = this._defMorphismList.Distinct()
                    .Select(x => x.ToString());
                var text = string.Join(" | ", list);
                return $"{this.Name.OriginalText.Trim()}: {new string(' ', m)}{s} → {new string(' ', n)}{t} [ {text} ]";
            }
        }
    }
}
