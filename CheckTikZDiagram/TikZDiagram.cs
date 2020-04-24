using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace CheckTikZDiagram
{
    /// <summary>
    /// tikzpicture環境を表すクラス
    /// </summary>
    public class TikZDiagram
    {
        /// <summary>
        /// CreateMorphismのキャッシュ
        /// </summary>
        private readonly Dictionary<(TokenString, TokenString?, TokenString?), IEnumerable<(Morphism, bool?)>> _cacheCreateMorphism
            = new Dictionary<(TokenString, TokenString?, TokenString?), IEnumerable<(Morphism, bool?)>>();

        /// <summary>
        /// CheckNodeのキャッシュ
        /// </summary>
        private readonly Dictionary<(TokenString, TokenString), bool> _cacheCheckNode
            = new Dictionary<(TokenString, TokenString), bool>();

        private readonly IDictionary<TokenString, Morphism> _definedMorphismDictionary;
        private readonly ReadOnlyCollection<Morphism> _parameterizedMorphisms;
        private readonly ReadOnlyCollection<Functor> _functors;
        
        private readonly string[] _tikzCommands;
        private readonly int _line;
        private readonly bool _definition;
        private readonly bool _omitOperator;
        private readonly bool _errorOnly;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="tikz">tikzpicture環境を与える文字列</param>
        /// <param name="line">tikzpicture環境の最終行の行数</param>
        /// <param name="definition">定義として使用する場合true</param>
        /// <param name="omitOperator">演算子の省略を認める場合true</param>
        /// <param name="errorOnly">エラーのみ出力する場合true</param>
        /// <param name="defDic">使用する辞書</param>
        /// <param name="paramList">使用するパラメータ付き射のリスト</param>
        /// <param name="functorList">使用する関手のリスト</param>
        public TikZDiagram(string tikz, int line, bool definition, bool omitOperator, bool errorOnly, 
            IDictionary<TokenString, Morphism> defDic, IList<Morphism> paramList, IList<Functor> functorList)
        {
            _definedMorphismDictionary = defDic;
            _parameterizedMorphisms = new ReadOnlyCollection<Morphism>(paramList);
            _functors = new ReadOnlyCollection<Functor>(functorList);
            _tikzCommands = tikz.Split('\n');
            _line = line - (_tikzCommands.Length - 1);
            _definition = definition;
            _omitOperator = omitOperator;
            _errorOnly = errorOnly;
        }

        /// <summary>
        /// tikzpicture環境の正しさをチェックする(definitionFlag = true の場合はチェックせず定義として採用する)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CheckResult> CheckDiagram()
        {
            var nodeDic = new Dictionary<string, TikZNode>();
            var arrowList = new List<(TikZArrow, int)>();
            var temp = "";

            // 対象と射の情報を収集する
            for (int i = 0; i < _tikzCommands.Length; i++)
            {
                if (_tikzCommands[i].EndsWith("CheckTikZDiagramIgnore"))
                {
                    continue;
                }

                var ys = _tikzCommands[i].Split(';');
                for (int j = 0; j < ys.Length; j++)
                {
                    var y = ys[j];

                    if (j == 0)
                    {
                        y = temp + y;
                    }

                    if (j == ys.Length - 1)
                    {
                        temp = y + " ";
                        continue;
                    }

                    var node = TikZNode.Create(y);
                    if (node != null)
                    {
                        if (nodeDic.ContainsKey(node.NodeName))
                        {
                            yield return new CheckResult(_line + i, $"node ({node.NodeName}) は既に存在します", true, true);
                        }
                        else
                        {
                            nodeDic.Add(node.NodeName, node);
                        }
                        continue;
                    }

                    foreach (var arrow in TikZArrow.Create(y))
                    {
                        arrowList.Add((arrow, _line + i));
                    }
                }
            }

            // 射のdom, codが正しいか確認する
            foreach (var (arrow, num) in arrowList)
            {
                if (!nodeDic.TryGetValue(arrow.SourceNodeName, out var source))
                {
                    yield return new CheckResult(num, $"{arrow}: domainのnodeが存在しません", true, true);
                    continue;
                }

                if (!nodeDic.TryGetValue(arrow.TargetNodeName, out var target))
                {
                    yield return new CheckResult(num, $"{arrow}: codomainのnodeが存在しません", true, true);
                    continue;
                }

                if (_definition)
                {
                    _definedMorphismDictionary[arrow.Name] = new Morphism(arrow.MathObject, source.MathObject, target.MathObject, MorphismType.OneMorphism, num);
                    continue;
                }

                WriteLog("[{0}] {1}", num, arrow);
                var list = new List<string>();
                foreach (var (mor, result) in CreateMorphism(arrow.MathObject, source.MathObject, target.MathObject))
                {
                    WriteLog("[{0}, {1}] {2}", num, result, mor);
                    if (result == true
                        ||(result == null && CheckSourceAndTarget(mor, source.MathObject, target.MathObject)))
                    {
                        yield return new CheckResult(num, $"[TikZ] {arrow.OriginalText.Trim()}: {source} → {target}\n[def ] {mor}", false, !_errorOnly);
                        goto NEXT_ARROW;
                    }
                    list.Add($"\n    {mor}");
                }

                if (list.Count == 0)
                {
                    yield return new CheckResult(num, $"{arrow.OriginalText.Trim()}: {source} → {target} の定義が存在しません", true, true);
                }
                else
                {
                    var result = list.Aggregate($"{arrow.OriginalText.Trim()}: {source} → {target} のdomainもしくはcodomainが定義と異なります", (x, y) => x + y);
                    yield return new CheckResult(num, result, true, true);
                }
            NEXT_ARROW:;
            }
        }

        /// <summary>
        /// MathObjectをMorphismに変換する
        /// </summary>
        /// <param name="math">変換対象のMathObject</param>
        /// <param name="source">domainを表す文字列</param>
        /// <param name="target">codomainを表す文字列</param>
        /// <returns>変換後のMorphismの候補</returns>
        public IEnumerable<(Morphism, bool?)> CreateMorphism(MathObject math, MathObject? source, MathObject? target, [CallerMemberName] string memberName = "")
        {
            _logCount++;
            var key = (math.ToTokenString(), source?.ToTokenString(), target?.ToTokenString());
            WriteLog("[{0} ⇒ CreateMorphism] {1}, {2}, {3}", memberName, math, source, target);

            // 処理本体(キャッシュに値がある場合はそれを使用する)
            if (!_cacheCreateMorphism.ContainsKey(key))
            {
                _cacheCreateMorphism[key] = Array.Empty<(Morphism, bool?)>(); // 無限ループを防ぐため、一旦空配列を入れる
                _cacheCreateMorphism[key] = CreateCache(CreateMorphismMain(math, source, target));
            }
            else
            {
                WriteLog("キャッシュ有り");
            }

            _logCount--;
            return _cacheCreateMorphism[key];


            static IEnumerable<(Morphism, bool?)> CreateCache(IEnumerable<(Morphism, bool?)> enumerable)
            {
                var temp = new List<(Morphism, bool?)>();
                foreach (var (mor, result) in enumerable)
                {
                    if (result == true)
                    {
                        // trueのものがあればそれ一つのみを使用する
                        return new[] { (mor, result) };
                    }

                    temp.Add((mor, result));
                }

                return temp;
            }
        }

        /// <summary>
        /// メイン処理
        /// </summary>
        /// <param name="math"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>Morphism: 候補の射  bool: 与えたsource, targetと一致する射であることが確認済ならtrue</returns>
        public IEnumerable<(Morphism, bool?)> CreateMorphismMain(MathObject math, MathObject? source, MathObject? target)
        {
            // 定義が_definedMorphismDictionaryに含まれる場合
            if (_definedMorphismDictionary.TryGetValue(math.ToTokenString(), out var m))
            {
                WriteLog("DefinedMorphism");

                if (source != null && target != null)
                {
                    yield return (m, CheckSourceAndTarget(m, source, target));
                }
                else
                {
                    yield return (m, null);
                }
            }

            // 定義が_parameterizedMorphismsに含まれる場合
            foreach (var item in Parameterized(math, source, target)) yield return item;

            // MathObjectでない場合はここで終わり
            if (!(math is MathSequence seq)) yield break;


            // 射の合成(コマンド)の場合
            var compo = CompositeCommand(seq, source, target);
            if (compo != null) yield return (compo, null);

            // SupもSubもない場合
            if (seq.Sup == null && seq.Sub == null)
            {
                // 一番外側に括弧のみがついている場合
                if (seq.ExistsBracket)
                {
                    source = RemoveBracket(source);
                    target = RemoveBracket(target);
                    if (seq.List.Count == 1)
                    {
                        foreach (var item in CreateMorphism(seq.List[0], source, target)) yield return item;
                    }
                    else
                    {
                        foreach (var item in CreateMorphism(new MathSequence(seq.List, seq.Separator), source, target)) yield return item;
                    }

                    yield break;
                }
            }


            // 逆射の場合
            foreach (var item in Inverse(seq, source, target)) yield return item;

            // Kan拡張の場合
            foreach (var item in KanExtension(seq, source, target)) yield return item;

            // Kanリフトの場合
            foreach (var item in KanLift(seq, source, target)) yield return item;

            // 添え字つきの場合
            foreach (var item in WithIndex(seq, source, target)) yield return item;

            // 射の合成(二項演算)の場合
            var compo2 = CompositeOperator(seq, source, target);
            if (compo2 != null) yield return (compo2, null);

            // 二項演算の場合
            var bo = BinaryOperator(seq, source, target).ToArray();
            foreach (var item in bo) yield return item;
            if (bo.Length > 0) yield break;

            // 関手適用の場合
            foreach (var item in ApplyingFunctor(seq)) yield return (item, null);

            // 二変数関手適用の場合
            foreach (var item in ApplyingBifunctor(seq, source, target)) yield return (item, null);
        }

        private MathObject? RemoveBracket(MathObject? mathObject)
        {
            if (mathObject == null)
            {
                return null;
            }

            if (mathObject is MathSequence seq && seq.Sup == null && seq.Sub == null && seq.ExistsBracket)
            {
                if (seq.List.Count == 1)
                {
                    return seq.List[0];
                }
                else
                {
                    return new MathSequence(seq.List, seq.Separator);
                }
            }
            else
            {
                return mathObject;
            }
        }

        private IEnumerable<(Morphism, bool?)> Parameterized(MathObject math, MathObject? source, MathObject? target)
        {
            foreach (var def in _parameterizedMorphisms)
            {
                var parameters = new Dictionary<string, MathObject>();
                if (!def.Name.IsSameType(math, parameters))
                {
                    continue;
                }

                foreach (var applied in def.ApplyParameter(math, parameters, false))
                {
                    // appliedが変数を持っていなければOK (この時点でapplied.Nameは変数無し)
                    if (!applied.Source.HasVariables() && !applied.Target.HasVariables())
                    {
                        WriteLog("Parameterized0 {0}", def);
                        if (source == null || target == null)
                        {
                            yield return (applied, null);
                        }
                        else
                        {
                            yield return (applied, CheckSourceAndTarget(applied, source, target));
                        }

                        continue;
                    }

                    // 持っている場合、source, targetの情報を使って変数を埋める
                    var p = new Dictionary<string, MathObject>();
                    if (source != null && !applied.Source.IsSameType(source, p)) continue;
                    
                    var q = new Dictionary<string, MathObject>();
                    if (target != null && !applied.Target.IsSameType(target, q)) continue;

                    // p, qに矛盾がないか確認
                    if (!CheckParameter(p)) continue;

                    if (!CheckParameter(q)) continue;

                    foreach (var key in p.Keys.Where(x => !x.EndsWith('s') && !x.EndsWith('t')))
                    {
                        if (q.ContainsKey(key))
                        {
                            // pとqが矛盾している場合は無効
                            if (!EqualsAsMathObject(q[key], p[key]))
                            {
                                goto NEXT_APPLIED;
                            }
                        }
                        else
                        {
                            // pのみにある情報をqに集約する
                            q[key] = p[key];
                        }
                    }

                    // s変数、t変数
                    var array = applied.Source.GetVariables().Concat(applied.Target.GetVariables())
                        .Where(x => x.EndsWith('s') || x.EndsWith('t'))
                        .Select(x => x.Substring(0, 2))
                        .Distinct()
                        .ToArray();
                    var paramDic = new Dictionary<string, MathObject>();
                    foreach (var paramName in array)
                    {
                        if (!parameters.TryGetValue(paramName, out var value))
                        {
                            goto NEXT_APPLIED;
                        }
                        paramDic[paramName] = value;
                    }

                    foreach (var morParam in AggregateParameters(paramDic, x => CreateMorphism(x, null, null).Select(y => y.Item1)))
                    {
                        // s変数、t変数がない場合はmorParamは空の辞書
                        var stParam = new Dictionary<string, MathObject>(q);
                        var defList = new List<Morphism>();
                        foreach (var kvp in morParam)
                        {
                            stParam[kvp.Key + "s"] = kvp.Value.Source;
                            stParam[kvp.Key + "t"] = kvp.Value.Target;
                            defList.AddRange(kvp.Value.GetDefList());
                        }

                        foreach (var item in applied.ApplyParameter(math, stParam, defList))
                        {
                            if (!item.Source.GetVariables().Any() && !item.Target.GetVariables().Any())
                            {
                                WriteLog("Parameterized1 {0}", def);
                                if (source != null && target != null)
                                {
                                    yield return (item, array.Length == 0 || CheckSourceAndTarget(item, source, target));
                                }
                                else
                                {
                                    yield return (item, null);
                                }
                            }
                        }
                    }


                NEXT_APPLIED:;
                }
            }
        }

        private bool CheckParameter(IReadOnlyDictionary<string, MathObject> parameters)
        {
            for (int i = 0; i < 10; i++)
            {
                if (parameters.TryGetValue($"#{i}a", out var a) && parameters.TryGetValue($"#{i}b", out var b))
                {
                    if (!EqualsAsMathObject(a, b))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerable<(Morphism, bool?)> Inverse(MathSequence seq, MathObject? source, MathObject? target)
        {
            if (seq.Sup == null || !seq.Sup.ToTokenString().Equals(Config.Instance.Inverse))
            {
                yield break;
            }

            foreach (var (mor, result) in CreateMorphism(seq.CopyWithoutSup(), target, source))
            {
                WriteLog("Inverse {0} {1}", mor, result);
                yield return (new Morphism(seq, mor.Target, mor.Source, mor.Type, mor.GetDefList()), result);
            }
        }

        private IEnumerable<(Morphism, bool?)> KanExtension(MathSequence math, MathObject? source, MathObject? target)
        {
            if (math.Length != 3 || !Config.Instance.KanExtensions.Any(x => math.List[0].ToTokenString().Equals(x)))
            {
                yield break;
            }

            foreach (var (f, _) in CreateMorphism(math.List[1], null, source))
            {
                foreach (var (g, _) in CreateMorphism(math.List[2], null, target))
                {
                    WriteLog("KanExtension {0} {1}", f, g);
                    yield return (new Morphism(math, f.Target, g.Target, f.Type, f.GetDefList().Concat(g.GetDefList())), null);
                }
            }
        }

        private IEnumerable<(Morphism, bool?)> KanLift(MathSequence math, MathObject? source, MathObject? target)
        {
            if (math.Length != 3 || !Config.Instance.KanLifts.Any(x => math.List[0].ToTokenString().Equals(x)))
            {
                yield break;
            }

            foreach (var (f, _) in CreateMorphism(math.List[1], target, null))
            {
                foreach (var (g, _) in CreateMorphism(math.List[2], source, null))
                {
                    WriteLog("KanLift {0} {1}", f, g);
                    yield return (new Morphism(math, g.Source, f.Source, f.Type, f.GetDefList().Concat(g.GetDefList())), null);
                }
            }
        }

        private Morphism? CompositeCommand(MathSequence math, MathObject? source, MathObject? target)
        {
            if (math.Length != 2 
                || !math.List[0].ToTokenString().Equals(Config.Instance.Composite)
                || !(math.List[1] is MathSequence seq))
            {
                return null;
            }

            WriteLog("CompositeCommand {0}", seq);
            return Composition(math, source, target, seq.List);
        }

        private Morphism? CompositeOperator(MathSequence math, MathObject? source, MathObject? target)
        {
            foreach (var compo in Config.Instance.Compositions)
            {
                var xs = math.Split(compo).Reverse().ToArray();
                if (xs.Length == 1)
                {
                    continue;
                }

                WriteLog("CompositeOperator {0}", compo);
                return Composition(math, source, target, xs);
            }
            return null;
        }

        private Morphism? Composition(MathSequence math, MathObject? source, MathObject? target, IReadOnlyList<MathObject> list)
        {
            var temp = new List<Morphism>();
            for (int i = 0; i < list.Count; i++)
            {
                Morphism? mor;
                if (i < list.Count - 1)
                {
                    (mor, _) = CreateMorphism(list[i], source, null).FirstOrDefault();
                }
                else
                {
                    (mor, _) = CreateMorphism(list[i], source, target).FirstOrDefault();
                }

                if (mor != null)
                {
                    source = mor.Target;
                    temp.Add(mor);
                }
                else
                {
                    return null;
                }
            }

            return new Morphism(math, temp.First().Source, temp.Last().Target, MorphismType.OneMorphism, temp.SelectMany(mor => mor.GetDefList()));
        }

        private IEnumerable<(Morphism, bool?)> WithIndex(MathSequence seq, MathObject? source, MathObject? target)
        {
            if (seq.Sub == null) yield break;
            
            var main = seq.CopyWithoutSub();
            var (sFunc, sValue) = GetFunctor(source, seq.Sub.ToTokenString());
            var (tFunc, tValue) = GetFunctor(target, seq.Sub.ToTokenString());

            // main部分が自然変換(もしくは2-morphism)のものが処理対象
            foreach (var (mor, result) in CreateMorphism(main, sFunc, tFunc).Where(x => x.Item1.Type == MorphismType.NaturalTransformation
                                                                         || x.Item1.Type == MorphismType.TwoMorphism))
            {
                // 添え字に関手がついている場合
                foreach (var (sub, _) in CreateMorphism(seq.Sub, sValue, tValue).Where(x => x.Item1.IsFunctor))
                {
                    WriteLog("CWithIndex 関手 {0}, {1}", mor, sub);
                    yield return (new Morphism(
                            seq,
                            mor.Source.Add(sub.Source),
                            mor.Target.Add(sub.Target),
                            mor.Type,
                            mor.GetDefList().Concat(sub.GetDefList())
                    ), null);
                }

                // 2変数の場合
                if (seq.Sub is MathSequence subSeq && subSeq.List.Count == 2)
                {
                    foreach (var (f, _) in CreateMorphism(mor.Source, null, null).Where(x => x.Item1.Type == MorphismType.Bifunctor))
                    {
                        WriteLog("CWithIndex 2変数関手 {0}, {1}", mor, f);
                        yield return (new Morphism(
                            seq,
                            EvaluateBifunctor(mor.Source, subSeq.List[0], subSeq.List[1]),
                            EvaluateBifunctor(mor.Target, subSeq.List[0], subSeq.List[1]),
                            MorphismType.OneMorphism,
                            mor.GetDefList().Concat(f.GetDefList())
                        ), null);
                    }
                }

                // それ以外の場合
                WriteLog("CWithIndex それ以外 {0}", mor);
                if (sValue != null && tValue != null)
                {
                    yield return (new Morphism(seq, Evaluate(mor.Source, sValue, seq.Sub), Evaluate(mor.Target, tValue, seq.Sub), MorphismType.OneMorphism,
                        mor.GetDefList()), result);
                }
                else
                {
                    yield return (new Morphism(seq, Evaluate(mor.Source, sValue, seq.Sub), Evaluate(mor.Target, tValue, seq.Sub), MorphismType.OneMorphism,
                        mor.GetDefList()), null);
                }
            }

            static (MathObject?, MathObject?) GetFunctor(MathObject? math, TokenString script)
            {
                if (!(math is MathSequence seq) || seq.List.Count <= 1) return (null, null);

                var last = seq.List.Last();
                if (last.ToTokenString().Equals(script))
                {
                    return (seq.SubSequence(0, seq.List.Count - 1), last);
                }
                else if (last is MathSequence s && s.Sup == null && s.Sub == null
                    && s.LeftBracket.Equals(Token.LeftParenthesis) && s.RightBracket.Equals(Token.RightParenthesis)
                    && s.Main.Equals(script))
                {
                    return (seq.SubSequence(0, seq.List.Count - 1), last);
                }
                else
                {
                    return (null, null);
                }
            }
        }

        private IEnumerable<Morphism> ApplyingFunctor(MathSequence seq)
        {
            for (int i = 1; i < seq.List.Count; i++)
            {
                // 先頭の i 個が関手となっている場合処理
                foreach (var (func, _) in CreateMorphism(seq.SubSequence(0, i), null, null)
                    .Where(x => x.Item1.Type == MorphismType.Functor || x.Item1.Type == MorphismType.ContravariantFunctor))
                {
                    // 後ろの残りの部分が関手に適用する値
                    foreach (var (value, _) in CreateMorphism(seq.SubSequence(i), null, null))
                    {
                        // 自然変換(2-mophism)の場合、水平合成する
                        if (value.Type == MorphismType.NaturalTransformation || value.Type == MorphismType.TwoMorphism)
                        {
                            WriteLog("ApplyingFunctor 水平合成 {0}, {1}", func, value);
                            yield return new Morphism(
                                seq,
                                new MathSequence(new[] { func.Name, value.Source }),
                                new MathSequence(new[] { func.Name, value.Target }),
                                value.Type,
                                func.GetDefList().Concat(value.GetDefList())
                            );
                        }
                        // 1-morphismの場合は普通に適用
                        else if (value.Type == MorphismType.OneMorphism)
                        {
                            // 共変関手
                            if (func.Type == MorphismType.Functor)
                            {
                                WriteLog("ApplyingFunctor 共変関手 {0}, {1}", func, value);
                                yield return new Morphism(
                                    seq,
                                    Evaluate(func.Name, null, value.Source),
                                    Evaluate(func.Name, null, value.Target),
                                    MorphismType.OneMorphism,
                                    func.GetDefList().Concat(value.GetDefList())
                                );
                            }
                            // 反変関手
                            else
                            {
                                WriteLog("ApplyingFunctor 反変関手 {0}, {1}", func, value);
                                yield return new Morphism(
                                    seq,
                                    Evaluate(func.Name, null, value.Target),
                                    Evaluate(func.Name, null, value.Source),
                                    MorphismType.OneMorphism,
                                    func.GetDefList().Concat(value.GetDefList())
                                );
                            }
                        }
                    }
                }
            }

            foreach (var mor in _definedMorphismDictionary.Values
                .Where(x => (x.Type == MorphismType.Functor || x.Type == MorphismType.ContravariantFunctor)
                            && x.Name.HasVariables()))
            {
                var parameters = new Dictionary<string, MathObject>();
                if (!mor.Name.IsSameType(seq, parameters))
                {
                    continue;
                }

                if (parameters.Count != 1 || !parameters.ContainsKey("-"))
                {
                    continue;
                }

                foreach (var (value, _) in CreateMorphism(parameters["-"], null, null))
                {
                    var sourceParam = new Dictionary<string, MathObject>() { { "-", value.Source } };
                    var targetParam = new Dictionary<string, MathObject>() { { "-", value.Target } };

                    foreach (var source in mor.Name.ApplyParameters(sourceParam, false))
                    {
                        foreach (var target in mor.Name.ApplyParameters(targetParam, false))
                        {
                            if (mor.Type == MorphismType.Functor)
                            {
                                WriteLog("ApplyingFunctor defined 共変関手 {0}, {1}", mor, value);
                                yield return new Morphism(seq, source, target, MorphismType.OneMorphism, mor.GetDefList().Concat(value.GetDefList()));
                            }
                            else
                            {
                                WriteLog("ApplyingFunctor defined 反変関手 {0}, {1}", mor, value);
                                yield return new Morphism(seq, target, source, MorphismType.OneMorphism, mor.GetDefList().Concat(value.GetDefList()));
                            }
                        }
                    }
                }
            }
        }

        private MathSequence Evaluate(MathObject func, MathObject? value0, MathObject value1)
        {
            if (func.ToString().Contains("-"))
            {
                var parameters = new Dictionary<string, MathObject>() { { "-", value1 } };
                var x = func.ApplyParameters(parameters, false).FirstOrDefault();
                if (x is MathSequence seq)
                {
                    return seq;
                }
            }

            if (value0 != null)
            {
                return func.Add(value0);
            }
            else
            {
                return func.Add(value1.SetBracket());
            }
        }

        private MathSequence EvaluateBifunctor(MathObject func, MathObject value1, MathObject value2)
        {
            var value = new MathSequence(new[] { value1, value2 }, ",");
            return func.Add(value.SetBracket());
        }

        private IEnumerable<Morphism> ApplyingBifunctor(MathSequence seq, MathObject? source, MathObject? target)
        {
            if (seq.List.Count != 2)
            {
                yield break;
            }

            MathObject? sLeft = null;
            MathObject? sRight = null;
            if (source is MathSequence x && x.List.Count == 2 && x.List[1] is MathSequence sSeq && sSeq.List.Count == 2)
            {
                sLeft = sSeq.List[0];
                sRight = sSeq.List[1];
            }
            MathObject? tLeft = null;
            MathObject? tRight = null;
            if (target is MathSequence y && y.List.Count == 2 && y.List[1] is MathSequence tSeq && tSeq.List.Count == 2)
            {
                tLeft = tSeq.List[0];
                tRight = tSeq.List[1];
            }

            if (seq.List[1] is MathSequence value && value.List.Count == 2)
            {
                foreach (var (mor, _) in CreateMorphism(seq.List[0], null, null).Where(x => x.Item1.Type == MorphismType.Bifunctor))
                {
                    var lArray = CreateMorphism(value.List[0], sLeft, tLeft).Select(x => x.Item1).ToArray();
                    var rArray = CreateMorphism(value.List[1], sRight, tRight).Select(x => x.Item1).ToArray();

                    foreach (var left in lArray)
                    foreach (var right in rArray)
                        {
                            WriteLog("ApplyingBifunctor");
                            yield return new Morphism(
                            seq,
                            EvaluateBifunctor(mor.Name, left.Source, right.Source),
                            EvaluateBifunctor(mor.Name, left.Target, right.Target),
                            MorphismType.OneMorphism,
                            mor.GetDefList().Concat(left.GetDefList()).Concat(right.GetDefList())
                        );
                    }

                    if (sRight != null && tRight != null && sRight.Equals(tRight))
                    {
                        foreach (var left in lArray)
                        {
                            WriteLog("ApplyingBifunctor");
                            yield return new Morphism(
                                seq,
                                EvaluateBifunctor(mor.Name, left.Source, sRight),
                                EvaluateBifunctor(mor.Name, left.Target, sRight),
                                MorphismType.OneMorphism,
                                mor.GetDefList().Concat(left.GetDefList())
                            );
                        }
                    }

                    if (sLeft != null && tLeft != null && sLeft.Equals(tLeft))
                    {
                        foreach (var right in rArray)
                        {
                            WriteLog("ApplyingBifunctor");
                            yield return new Morphism(
                                seq,
                                EvaluateBifunctor(mor.Name, sLeft, right.Source),
                                EvaluateBifunctor(mor.Name, sLeft, right.Target),
                                MorphismType.OneMorphism,
                                mor.GetDefList().Concat(right.GetDefList())
                            );
                        }
                    }
                }
            }
        }

        private IEnumerable<(Morphism, bool?)> BinaryOperator(MathSequence seq, MathObject? source, MathObject? target)
        {
            var mList = new List<(MathObject left, MathObject center, MathObject right)>();
            var sList = new List<(MathObject left, MathObject center, MathObject right)?>();
            var tList = new List<(MathObject left, MathObject center, MathObject right)?>();

            foreach (var opText in Config.Instance.Operators)
            {
                var op = new MathObjectFactory(opText).CreateSingle();
                mList.AddRange(seq.Divide(op));
                if (source != null)
                {
                    foreach (var item in source.Divide(op)) sList.Add(item);
                }
                if (target != null)
                {
                    foreach (var item in target.Divide(op)) tList.Add(item);
                }
            }

            var flag = false;
            if (_omitOperator)
            {
                if (source is MathSequence sSeq && DivideWithoutOp(sSeq, out var sLeft, out var sRight)
                    && target is MathSequence tSeq && DivideWithoutOp(tSeq, out var tLeft, out var tRight))
                {
                    foreach (var m in mList)
                    {
                        foreach (var item in BinaryOperatorHelper(seq, source, target, m.left, m.right, sLeft, sRight, tLeft, tRight, null))
                        {
                            WriteLog("BinaryOperator {0} ⇒ {1}", m, item);
                            yield return item;
                        }
                    }

                    flag = true;
                }
            }

            if (source == null && !flag)
            {
                sList.Add(null);
            }
            if (target == null && !flag)
            {
                tList.Add(null);
            }

            foreach (var m in mList)
            foreach (var s in sList)
            foreach (var t in tList)
            {
                MathObject op;
                if (s.HasValue && t.HasValue)
                {
                    if (s.Value.center.Equals(t.Value.center))
                    {
                        op = s.Value.center;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (s.HasValue)
                {
                    op = s.Value.center;
                }
                else if (t.HasValue)
                {
                    op = t.Value.center;
                }
                else
                {
                    op = m.center;
                }

                foreach (var item in BinaryOperatorHelper(seq, source, target, m.left, m.right, s?.left, s?.right, t?.left, t?.right, op))
                {
                    WriteLog("BinaryOperator {0} | {1} | {2} ⇒ {3}", m, s, t, item);
                    yield return item;
                }
            }

            static bool DivideWithoutOp(MathSequence seq, out MathObject left, out MathObject right)
            {
                left = seq;
                right = seq;

                if (!seq.IsSimple || seq.List.Count < 2)
                {
                    return false;
                }

                if (seq.List.Count == 2)
                {
                    left = seq.List[0];
                    right = seq.List[1];
                    return true;
                }

                if (seq.List[0].ToString().StartsWith(@"\"))
                {
                    for (int i = 1; i < seq.List.Count; i++)
                    {
                        if (seq.List[i].Main.ToString().StartsWith(@"\"))
                        {
                            left = seq.SubSequence(0, i);
                            right = seq.SubSequence(i);
                            return true;
                        }
                    }
                }

                left = seq.List[0];
                right = seq.SubSequence(1);
                return true;
            }
        }

        private IEnumerable<(Morphism, bool?)> BinaryOperatorHelper(MathSequence seq, MathObject? source, MathObject? target
            , MathObject mLeft, MathObject mRight, MathObject? sLeft, MathObject? sRight, MathObject? tLeft, MathObject? tRight, MathObject? op)
        {
            var leftArray = CreateMorphism(mLeft, sLeft, tLeft).ToArray();
            var rightArray = CreateMorphism(mRight, sRight, tRight).ToArray();
            var slBracket = HasParenthesisOnly(sLeft, source?.ToString().First());
            var srBracket = HasParenthesisOnly(sRight, source?.ToString().Last());
            var tlBracket = HasParenthesisOnly(tLeft, target?.ToString().First());
            var trBracket = HasParenthesisOnly(tRight, target?.ToString().Last());
            var bracket = slBracket != null && srBracket != null && tlBracket != null && trBracket != null;

            foreach (var (left, lResult) in leftArray)
            foreach (var (right, rResult) in rightArray)
            foreach (var item in Generate(seq, left.Source, right.Source, left.Target, right.Target,
                op, left.Type, left.GetDefList().Concat(right.GetDefList()),
                slBracket, srBracket, tlBracket, trBracket))
            {
                if (lResult != null && rResult != null && bracket)
                {
                    yield return (item, lResult.Value && rResult.Value);
                }
                else
                {
                    yield return (item, null);
                }
            }

            if ((sRight == null || EqualsAsMathObject(sRight, mRight))
                && (tRight == null || EqualsAsMathObject(tRight, mRight)))
            {
                foreach (var (left, lResult) in leftArray)
                {
                    foreach (var item in Generate(seq, left.Source, mRight, left.Target, mRight,
                        op, left.Type, left.GetDefList(),
                        slBracket, srBracket, tlBracket, trBracket))
                    {
                        if (lResult != null && bracket)
                        {
                            yield return (item, lResult.Value);
                        }
                        else
                        {
                            yield return (item, null);
                        }
                    }
                }
            }

            if ((sLeft == null || EqualsAsMathObject(sLeft, mLeft))
                && (tLeft == null || EqualsAsMathObject(tLeft, mLeft)))
            {
                foreach (var (right, rResult) in rightArray)
                {
                    foreach (var item in Generate(seq, mLeft, right.Source, mLeft, right.Target,
                        op, right.Type, right.GetDefList(),
                        slBracket, srBracket, tlBracket, trBracket))
                    {
                        if (rResult != null && bracket)
                        {
                            yield return (item, rResult.Value);
                        }
                        else
                        {
                            yield return (item, null);
                        }
                    }
                }
            }
        }

        bool? HasParenthesisOnly(MathObject? math, char? x)
        {
            if (math == null)
            {
                if (x.HasValue && x.Value != '(' && x != ')')
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }

            return math is MathSequence seq && seq.Sup == null && seq.Sub == null
                && seq.LeftBracket.Equals(Token.LeftParenthesis)
                && seq.RightBracket.Equals(Token.RightParenthesis);
        }

        // slBracket: trueの場合、sLeftに ( ) を付ける。falseの場合、付けない。nullの場合、両方のパターンを返す
        IEnumerable<Morphism> Generate(MathSequence seq, MathObject sLeft, MathObject sRight, MathObject tLeft, MathObject tRight,
            MathObject? op, MorphismType type, IEnumerable<Morphism> defList,
            bool? slBracket, bool? srBracket, bool? tlBracket, bool? trBracket)
        {
            for (int i = 0; i < 16; i++)
            {
                if (slBracket.HasValue && slBracket.Value != ((i & 0b0001) == 0b0001)) continue;
                if (srBracket.HasValue && srBracket.Value != ((i & 0b0010) == 0b0010)) continue;
                if (tlBracket.HasValue && tlBracket.Value != ((i & 0b0100) == 0b0100)) continue;
                if (trBracket.HasValue && trBracket.Value != ((i & 0b1000) == 0b1000)) continue;

                var newSource = GenerateHelper(WithBracket(sLeft, (i & 0b0001) == 0b0001), op, WithBracket(sRight, (i & 0b0010) == 0b0010));
                if (seq.ExistsBracket)
                {
                    newSource = newSource.SetBracket(seq.LeftBracket, seq.RightBracket);
                }

                var newTarget = GenerateHelper(WithBracket(tLeft, (i & 0b0100) == 0b0100), op, WithBracket(tRight, (i & 0b1000) == 0b1000));
                if (seq.ExistsBracket)
                {
                    newTarget = newTarget.SetBracket(seq.LeftBracket, seq.RightBracket);
                }

                yield return new Morphism(seq, newSource, newTarget, type, defList);
            }

            static MathSequence GenerateHelper(MathObject a, MathObject? b, MathObject c)
            {
                if (b != null)
                {
                    return a.Add(b).Add(c);
                }
                else
                {
                    return a.Add(c);
                }
            }
        }

        MathObject WithBracket(MathObject m, bool flag)
        {
            return flag ? m.SetBracket() : m;
        }

        private bool CheckSourceAndTarget(Morphism morphism, MathObject source, MathObject target, [CallerMemberName] string memberName = "")
        {
            return EqualsAsMathObject(morphism.Source, source, memberName) && EqualsAsMathObject(morphism.Target, target, memberName);
        }


        public bool EqualsAsMathObject(MathObject left, MathObject right, [CallerMemberName] string memberName = "")
        {
            _logCount++;
            var key = (left.ToTokenString(), right.ToTokenString());
            WriteLog("[{0} ⇒ EqualsAsMathObject] {1}, {2}", memberName, left, right);

            // 処理本体(キャッシュに値がある場合はそれを使用する)
            if (!_cacheCheckNode.ContainsKey(key))
            {
                _cacheCheckNode[key] = EqualsAsMathObjectMain(left, right);
            }

            _logCount--;
            return _cacheCheckNode[key];


            bool EqualsAsMathObjectMain(MathObject left, MathObject right)
            {
                foreach (var leftValue in EvaluateFunctor(left))
                {
                    foreach (var rightValue in EvaluateFunctor(right))
                    {
                        if (leftValue.Equals(rightValue))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }


        public IEnumerable<MathObject> EvaluateFunctor(MathObject math)
        {
            yield return math;

            if (math is MathSequence seq)
            {
                if (seq.Sup == null && seq.Sub == null && seq.ExistsBracket)
                {
                    yield return new MathSequence(seq.List, seq.Separator, seq.Main.ToOriginalString());
                }

                if (seq.List.Count > 1)
                {
                    foreach (var (mor, _) in CreateMorphism(seq.List[0], null, null)
                        .Where(x => x.Item1.Type == MorphismType.Functor || x.Item1.Type == MorphismType.ContravariantFunctor))
                    {
                        foreach (var value in EvaluateFunctor(seq.SubSequence(1)))
                        {
                            yield return seq.List[0].Add(value);
                            yield return seq.List[0].Add(value.SetBracket());
                        }
                    }
                }
            }


            var list = _definedMorphismDictionary.Values
                .Where(x => (x.Type == MorphismType.Functor || x.Type == MorphismType.ContravariantFunctor)
                            && x.Name.HasVariables())
                .SelectMany(x =>
                {
                    var token = new MathToken(new Token("#1", "#1"));
                    var parameters = new Dictionary<string, MathObject>() { { "-", token } };
                    return x.Name.ApplyParameters(parameters, false)
                        .Select(y => new Functor(x.Name.Add(token.SetBracket()), y));
                });

            foreach (var functor in _functors.Concat(list))
            {
                var parameters = new Dictionary<string, MathObject>();
                if (functor.Name.IsSameType(math, parameters))
                {
                    foreach (var px in AggregateParameters(parameters, x => EvaluateFunctor(x)))
                    {
                        foreach (var item in functor.Evaluate(px))
                        {
                            yield return item;
                        }

                        var dic = new Dictionary<string, MathObject>();
                        foreach (var item in px)
                        {
                            if (!(item.Value is MathSequence m) ||
                                (m.LeftBracket.IsEmpty && m.Sup == null && m.Sub == null))
                            {
                                dic[item.Key] = item.Value.SetBracket();
                            }
                            else
                            {
                                dic[item.Key] = item.Value;
                            }
                        }

                        foreach (var item in functor.Evaluate(dic))
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// パラメーターの組(parameters)の各MathObjectにEvaluateFunctorを適用して得られるすべてのDictionaryを返す
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IEnumerable<Dictionary<string, T>> AggregateParameters<T>(Dictionary<string, MathObject> parameters, Func<MathObject, IEnumerable<T>> func)
        {
            var result = new List<Dictionary<string, T>>
            {
                new Dictionary<string, T>()
            };

            foreach (var kvp in parameters)
            {
                var old = result;
                result = new List<Dictionary<string, T>>();

                foreach (var x in func(kvp.Value))
                {
                    foreach (var dic in old)
                    {
                        var newDic = new Dictionary<string, T>(dic);
                        newDic[kvp.Key] = x;
                        result.Add(newDic);
                    }
                }
            }

            return result;
        }

        private int _logCount = 0;


        private void WriteLog(string format, int line, TikZArrow arrow)
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, line.ToString(), arrow.ToString()));
            }
#endif
        }

        private void WriteLog(string format, string memberName, MathObject math, MathObject? source, MathObject? target)
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, memberName, math.ToString(), source?.ToString() ?? "null", target?.ToString() ?? "null"));
            }
#endif
        }

        private void WriteLog<T1, T2, T3, T4>(string format, T1 obj1, T2? obj2, T3? obj3, T4 obj4) where T1 : struct where T2 : struct where T3 : struct where T4 : struct
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, obj1.ToString(), obj2?.ToString() ?? "null", obj3?.ToString() ?? "null", obj4.ToString()));
            }
#endif
        }

        private void WriteLog<T>(string format, T obj) where T : notnull
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, obj.ToString()));
            }
#endif
        }

        private void WriteLog<T1, T2>(string format, T1 mor, T2 result) where T1 : notnull where T2 : notnull
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, mor.ToString(), result.ToString()));
            }
#endif
        }

        private void WriteLog<T1, T2>(string format, T1 mor, T2? result) where T1 : notnull where T2 : struct
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, mor.ToString(), result?.ToString() ?? "null"));
            }
#endif
        }

        private void WriteLog<T1, T2, T3>(string format, T1 obj1, T2 obj2, T3 obj3) where T1 : notnull where T2 : notnull where T3 : notnull
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, obj1.ToString(), obj2.ToString(), obj3.ToString()));
            }
#endif
        }

        private void WriteLog<T1, T2, T3>(string format, T1 obj1, T2? obj2, T3 obj3) where T1 : notnull where T2 : struct where T3 : notnull
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, obj1.ToString(), obj2?.ToString() ?? "null", obj3.ToString()));
            }
#endif
        }

        private void WriteLog<T1, T2, T3, T4>(string format, T1 obj1, T2 obj2, T3 obj3, T4 obj4) where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                WriteLog(string.Format(format, obj1.ToString(), obj2.ToString(), obj3.ToString(), obj4.ToString()));
            }
#endif
        }

        private void WriteLog(string text)
        {
#if DEBUG
            if (!Config.Instance.OutputLogFilePath.IsNullOrEmpty())
            {
                System.IO.File.AppendAllText(Config.Instance.OutputLogFilePath, new string('\t', _logCount) + text + "\r\n");
            }
#endif
        }
    }
}
