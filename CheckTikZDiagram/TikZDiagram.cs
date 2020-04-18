using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        private readonly Dictionary<(TokenString, TokenString?, TokenString?), Morphism[]> _cache
            = new Dictionary<(TokenString, TokenString?, TokenString?), Morphism[]>();

        private readonly IDictionary<TokenString, Morphism> _definedMorphismDictionary;
        private readonly ReadOnlyCollection<Morphism> _parameterizedMorphisms;
        private readonly ReadOnlyCollection<Functor> _functors;
        
        private readonly string[] _tikzCommands;
        private readonly int _line;
        private readonly bool _definition;
        private readonly bool _errorOnly;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="tikz">tikzpicture環境を与える文字列</param>
        /// <param name="line">tikzpicture環境の最終行の行数</param>
        /// <param name="definition">定義として使用する場合true</param>
        /// <param name="errorOnly">エラーのみ出力する場合true</param>
        /// <param name="defDic">使用する辞書</param>
        /// <param name="paramList">使用するパラメータ付き射のリスト</param>
        /// <param name="functorList">使用する関手のリスト</param>
        public TikZDiagram(string tikz, int line, bool definition, bool errorOnly, 
            IDictionary<TokenString, Morphism> defDic, IList<Morphism> paramList, IList<Functor> functorList)
        {
            _definedMorphismDictionary = defDic;
            _parameterizedMorphisms = new ReadOnlyCollection<Morphism>(paramList);
            _functors = new ReadOnlyCollection<Functor>(functorList);
            _tikzCommands = tikz.Split('\n');
            _line = line - (_tikzCommands.Length - 1);
            _definition = definition;
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

            // 対象と射の情報を収集する
            for (int i = 0; i < _tikzCommands.Length; i++)
            {
                var ys = _tikzCommands[i].Split(';');
                foreach (var y in ys)
                {
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

                var list = new List<string>();
                foreach (var def in CreateMorphism(arrow.MathObject, source.MathObject, target.MathObject))
                {
                    if (CheckNode(def.Source, source.MathObject) && CheckNode(def.Target, target.MathObject))
                    {
                        yield return new CheckResult(num, $"[TikZ] {arrow.OriginalText.Trim()}: {source} → {target}\n[def ] {def}", false, !_errorOnly);
                        goto NEXT_ARROW;
                    }
                    list.Add($"\n    {def}");
                }

                if (list.Count == 0)
                {
                    yield return new CheckResult(num, $"{arrow.OriginalText.Trim()} の定義が存在しません", true, true);
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
        public IEnumerable<Morphism> CreateMorphism(MathObject math, MathObject? source, MathObject? target)
        {
            var key = (math.ToTokenString(), source?.ToTokenString(), target?.ToTokenString());

            // 処理本体(キャッシュに値がある場合はそれを使用する)
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = CreateMorphismMain(math, source, target).ToArray();
            }

            return _cache[key];
        }


        public IEnumerable<Morphism> CreateMorphismMain(MathObject math, MathObject? source, MathObject? target)
        {
            // 定義が_definedMorphismDictionaryに含まれる場合
            if (_definedMorphismDictionary.TryGetValue(math.ToTokenString(), out var m)) yield return m;

            // 定義が_parameterizedMorphismsに含まれる場合
            foreach (var item in Parameterized(math, source, target)) yield return item;
                
            //// 米田埋込の場合
            //var yoneda = Yoneda(math, source, target);
            //if (yoneda != null) yield return yoneda;

            // MathObjectでない場合はここで終わり
            if (!(math is MathSequence seq)) yield break;



            // 逆射の場合
            foreach (var item in Inverse(seq, source, target)) yield return item;

            // Kan拡張の場合
            foreach (var item in KanExtension(seq, source, target)) yield return item;

            // 添え字つきの場合
            foreach (var item in WithIndex(seq)) yield return item;

            // 関手適用の場合
            foreach (var item in ApplyingFunctor(seq)) yield return item;

            // 二変数関手適用の場合
            foreach (var item in ApplyingBifunctor(seq, source, target)) yield return item;

            // 射の合成の場合
            var compo = CompositeCommand(seq, source, target);
            if (compo != null) yield return compo;

            var compo2 = CompositeOperator(seq, source, target);
            if (compo2 != null) yield return compo2;

            // 二項演算の場合
            foreach (var item in BinaryOperator(seq, source, target)) yield return item;

            // 一番外側に括弧のみがついている場合
            if (seq.Sup == null && seq.Sub == null && seq.Length == 1)
            {
                foreach (var item in CreateMorphism(seq.List[0], source, target)) yield return item;
            }
        }

        private IEnumerable<Morphism> Parameterized(MathObject math, MathObject? source, MathObject? target)
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
                    // appliedが変数を持っていなければOK
                    if (!applied.Source.HasParameter() && !applied.Target.HasParameter())
                    {
                        yield return applied;
                        continue;
                    }

                    // 持っている場合、source, targetの情報を使って変数を埋める
                    var p = new Dictionary<string, MathObject>();
                    if (source != null && !applied.Source.IsSameType(source, p))
                    {
                        continue;
                    }

                    var q = new Dictionary<string, MathObject>();
                    if (target != null && !applied.Target.IsSameType(target, q))
                    {
                        continue;
                    }

                    foreach (var key in p.Keys.Where(x => !x.EndsWith('s') && !x.EndsWith('t')))
                    {
                        if (q.ContainsKey(key))
                        {
                            // pとqが矛盾している場合は無効
                            if (!CheckNode(q[key], p[key]))
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

                    // s, t
                    var array = applied.Source.GetParameters().Concat(applied.Target.GetParameters())
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

                    foreach (var morParam in AggregateParameters(paramDic, x => CreateMorphism(x, null, null)))
                    {
                        var stParam = new Dictionary<string, MathObject>(q);
                        var defList = new List<Morphism>();
                        foreach (var kvp in morParam)
                        {
                            stParam[kvp.Key + "s"] = kvp.Value.Source;
                            stParam[kvp.Key + "t"] = kvp.Value.Target;
                            defList.AddRange(kvp.Value.GetDefList());
                        }

                        foreach (var reapplied in applied.ApplyParameter(math, stParam, defList))
                        {
                            yield return reapplied;
                        }
                    }


                NEXT_APPLIED:;
                }
            }
        }

        private IEnumerable<Morphism> Inverse(MathSequence seq, MathObject? source, MathObject? target)
        {
            if (seq.Sup == null || !seq.Sup.ToTokenString().Equals(Config.Instance.Inverse))
            {
                yield break;
            }

            foreach (var mor in CreateMorphism(seq.CopyWithoutSup(), target, source))
            {
                yield return new Morphism(seq, mor.Target, mor.Source, mor.Type, mor.GetDefList());
            }
        }

        private Morphism? Yoneda(MathObject math, MathObject? source, MathObject? target)
        {
            if (!math.Main.Equals(Config.Instance.Yoneda))
            {
                return null;
            }

            var s = (math is MathSequence seq) ? (seq.Sub ?? source) : source;
            if (s != null)
            {
                return new Morphism(
                    math,
                    s,
                    new MathObjectFactory($"{Config.Instance.Cocompletion}{{{s}}}").CreateSingle(),
                    MorphismType.Functor
                );
            }

            if (target is MathSequence t
                    && t.List.Count == 2
                    && t.List[0].ToTokenString().Equals(Config.Instance.Cocompletion))
            {
                if (t.List[1] is MathToken
                    ||(t.List[1] is MathSequence x && x.Sup == null && x.Sub == null))
                {
                    return new Morphism(math, t.List[1], target, MorphismType.Functor);
                }
            }

            return null;
        }

        private IEnumerable<Morphism> KanExtension(MathSequence math, MathObject? source, MathObject? target)
        {
            if (math.Length != 3 || !Config.Instance.Kans.Any(x => math.List[0].ToTokenString().Equals(x)))
            {
                yield break;
            }

            foreach (var f in CreateMorphism(math.List[1], null, source))
            {
                foreach (var g in CreateMorphism(math.List[2], null, target))
                {
                    yield return new Morphism(math, f.Target, g.Target, f.Type, f.GetDefList().Concat(g.GetDefList()));
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
                    mor = CreateMorphism(list[i], source, null).FirstOrDefault();
                }
                else
                {
                    mor = CreateMorphism(list[i], source, target).FirstOrDefault();
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

        private IEnumerable<Morphism> WithIndex(MathSequence seq)
        {
            if (seq.Sub == null)
            {
                yield break;
            }

            var main = seq.CopyWithoutSub();

            // Main部分が自然変換(2-morphism)のものが処理対象
            foreach (var mor in CreateMorphism(main, null, null).Where(x => x.Type == MorphismType.NaturalTransformation
                                                                         || x.Type == MorphismType.TwoMorphism))
            {
                // 添え字に関手がついている場合
                foreach (var sub in CreateMorphism(seq.Sub, null, null))
                {
                    if (sub.Type == MorphismType.Functor || sub.Type == MorphismType.ContravariantFunctor || sub.Type == MorphismType.Bifunctor)
                    {
                        yield return new Morphism(
                               seq,
                               mor.Source.Add(sub.Source),
                               mor.Target.Add(sub.Target),
                               mor.Type,
                               mor.GetDefList().Concat(sub.GetDefList())
                        );
                    }
                }

                // 2変数の場合
                if (seq.Sub is MathSequence subSeq && subSeq.List.Count == 2)
                {
                    foreach (var f in CreateMorphism(mor.Source, null, null))
                    {
                        if (f.Type == MorphismType.Bifunctor)
                        {
                            yield return new Morphism(
                                seq,
                                Evaluate(mor.Source, subSeq.List[0], subSeq.List[1]),
                                Evaluate(mor.Target, subSeq.List[0], subSeq.List[1]),
                                MorphismType.OneMorphism,
                                mor.GetDefList().Concat(f.GetDefList())
                            );
                            goto NEXT_LOOP;
                        }
                    }
                }

                // それ以外の場合
                yield return new Morphism(seq, Evaluate(mor.Source, seq.Sub), Evaluate(mor.Target, seq.Sub), MorphismType.OneMorphism,
                    mor.GetDefList());

            NEXT_LOOP:;
            }
        }

        private IEnumerable<Morphism> ApplyingFunctor(MathSequence seq)
        {
            for (int i = 1; i < seq.List.Count; i++)
            {
                // 先頭の i 個が関手となっている場合処理
                foreach (var func in CreateMorphism(seq.SubSequence(0, i), null, null)
                    .Where(x => x.Type == MorphismType.Functor || x.Type == MorphismType.ContravariantFunctor))
                {
                    // 後ろの残りの部分が関手に適用する値
                    foreach (var value in CreateMorphism(seq.SubSequence(i), null, null))
                    {
                        // 自然変換(2-mophism)の場合、水平合成する
                        if (value.Type == MorphismType.NaturalTransformation || value.Type == MorphismType.TwoMorphism)
                        {
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
                                yield return new Morphism(
                                    seq,
                                    Evaluate(func.Name, value.Source),
                                    Evaluate(func.Name, value.Target),
                                    MorphismType.OneMorphism,
                                    func.GetDefList().Concat(value.GetDefList())
                                );
                            }
                            // 反変関手
                            else
                            {
                                yield return new Morphism(
                                    seq,
                                    Evaluate(func.Name, value.Target),
                                    Evaluate(func.Name, value.Source),
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
                            && x.Name.HasParameter()))
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

                foreach (var value in CreateMorphism(parameters["-"], null, null))
                {
                    var sourceParam = new Dictionary<string, MathObject>() { { "-", value.Source } };
                    var targetParam = new Dictionary<string, MathObject>() { { "-", value.Target } };

                    foreach (var source in mor.Name.ApplyParameters(sourceParam, false))
                    {
                        foreach (var target in mor.Name.ApplyParameters(targetParam, false))
                        {
                            if (mor.Type == MorphismType.Functor)
                            {
                                yield return new Morphism(seq, source, target, MorphismType.OneMorphism, mor.GetDefList().Concat(value.GetDefList()));
                            }
                            else
                            {
                                yield return new Morphism(seq, target, source, MorphismType.OneMorphism, mor.GetDefList().Concat(value.GetDefList()));
                            }
                        }
                    }
                }
            }
        }

        private MathSequence Evaluate(MathObject func, MathObject value)
        {
            if (func.ToString().Contains("-"))
            {
                var parameters = new Dictionary<string, MathObject>() { { "-", value } };
                var x = func.ApplyParameters(parameters, false).FirstOrDefault();
                if (x is MathSequence seq)
                {
                    return seq;
                }
            }

            return func.Add(value.SetBracket());
        }

        private MathSequence Evaluate(MathObject func, MathObject value1, MathObject value2)
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
                foreach (var mor in CreateMorphism(seq.List[0], null, null).Where(x => x.Type == MorphismType.Bifunctor))
                {
                    var lArray = CreateMorphism(value.List[0], sLeft, tLeft).ToArray();
                    var rArray = CreateMorphism(value.List[1], sRight, tRight).ToArray();

                    foreach (var left in lArray)
                    foreach (var right in rArray)
                    {
                        yield return new Morphism(
                            seq,
                            Evaluate(mor.Name, left.Source, right.Source),
                            Evaluate(mor.Name, left.Target, right.Target),
                            MorphismType.OneMorphism,
                            mor.GetDefList().Concat(left.GetDefList()).Concat(right.GetDefList())
                        );
                    }

                    if (sRight != null && tRight != null && sRight.Equals(tRight))
                    {
                        foreach (var left in lArray)
                        {
                            yield return new Morphism(
                                seq,
                                Evaluate(mor.Name, left.Source, sRight),
                                Evaluate(mor.Name, left.Target, sRight),
                                MorphismType.OneMorphism,
                                mor.GetDefList().Concat(left.GetDefList())
                            );
                        }
                    }

                    if (sLeft != null && tLeft != null && sLeft.Equals(tLeft))
                    {
                        foreach (var right in rArray)
                        {
                            yield return new Morphism(
                                seq,
                                Evaluate(mor.Name, sLeft, right.Source),
                                Evaluate(mor.Name, sLeft, right.Target),
                                MorphismType.OneMorphism,
                                mor.GetDefList().Concat(right.GetDefList())
                            );
                        }
                    }
                }
            }
        }

        private IEnumerable<Morphism> BinaryOperator(MathSequence math, MathObject? source, MathObject? target)
        {
            var mList = new List<(MathObject left, MathObject center, MathObject right)>();
            var sList = new List<(MathObject left, MathObject center, MathObject right)?>();
            var tList = new List<(MathObject left, MathObject center, MathObject right)?>();

            foreach (var opText in Config.Instance.Operators)
            {
                var op = new MathObjectFactory(opText).CreateSingle();
                mList.AddRange(math.Divide(op));
                if (source != null)
                {
                    foreach (var item in source.Divide(op)) sList.Add(item);
                }
                if (target != null)
                {
                    foreach (var item in target.Divide(op)) tList.Add(item);
                }
            }

            if (sList.Count == 0)
            {
                sList.Add(null);
            }
            if (tList.Count == 0)
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

                var leftArray = CreateMorphism(m.left, s?.left, t?.left).ToArray();
                var rightArray = CreateMorphism(m.right, s?.right, t?.right).ToArray();

                foreach (var left in leftArray)
                foreach (var right in rightArray)
                foreach (var item in Generate(left.Source, right.Source, left.Target, right.Target, op, left.Type, left.GetDefList().Concat(right.GetDefList())))
                {
                    yield return item;
                }
                    
                foreach (var left in leftArray)
                {
                    foreach (var item in Generate(left.Source, m.right, left.Target, m.right, op, left.Type, left.GetDefList()))
                    {
                        yield return item;
                    }
                }

                foreach (var right in rightArray)
                {
                    foreach (var item in Generate(m.left, right.Source, m.left, right.Target, op, right.Type, right.GetDefList()))
                    {
                        yield return item;
                    }
                }
            }

            IEnumerable<Morphism> Generate(MathObject sLeft, MathObject sRight, MathObject tLeft, MathObject tRight, MathObject op,
                MorphismType type, IEnumerable<Morphism> defList)
            {
                for (int i = 0; i < 16; i++)
                {
                    if ((i & 0b0001) == 0b0001 && sLeft.Length == 1) continue;
                    if ((i & 0b0010) == 0b0010 && sRight.Length == 1) continue;
                    if ((i & 0b0100) == 0b0100 && tLeft.Length == 1) continue;
                    if ((i & 0b1000) == 0b1000 && tRight.Length == 1) continue;

                    var newSource = new MathSequence(new[] { WithBracket(sLeft, (i & 0b0001) == 0b0001), op, WithBracket(sRight, (i & 0b0010) == 0b0010) });
                    if (math.ExistsBracket)
                    {
                        newSource = newSource.SetBracket(math.LeftBracket, math.RightBracket);
                    }

                    var newTarget = new MathSequence(new[] { WithBracket(tLeft, (i & 0b0100) == 0b0100), op, WithBracket(tRight, (i & 0b1000) == 0b1000) });
                    if (math.ExistsBracket)
                    {
                        newTarget = newTarget.SetBracket(math.LeftBracket, math.RightBracket);
                    }

                    yield return new Morphism(math, newSource, newTarget, type, defList);
                }
            }

            static MathObject WithBracket(MathObject m, bool flag)
            {
                return flag ? m.SetBracket() : m;
            }
        }

        public bool CheckNode(MathObject left, MathObject right)
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

        public IEnumerable<MathObject> EvaluateFunctor(MathObject math)
        {
            yield return math;

            if (math is MathSequence seq && seq.List.Count > 1)
            {
                foreach (var mor in CreateMorphism(seq.List[0], null, null)
                    .Where(x => x.Type == MorphismType.Functor || x.Type == MorphismType.ContravariantFunctor))
                {
                    foreach (var value in EvaluateFunctor(seq.SubSequence(1)))
                    {
                        yield return seq.List[0].Add(value);
                        yield return seq.List[0].Add(value.SetBracket());
                    }
                }
            }

            var list = _definedMorphismDictionary.Values
                .Where(x => (x.Type == MorphismType.Functor || x.Type == MorphismType.ContravariantFunctor)
                            && x.Name.HasParameter())
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
    }
}
