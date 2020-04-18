using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    /// <summary>
    /// TikZにおける射を表すクラス
    /// </summary>
    public class TikZArrow
    {
        /// <summary>
        /// arrow: drawの後の [] の中身
        /// source: 最初の () の中身
        /// math: nodeの後の {$$} の中身
        /// target: 最後の () の中身
        /// </summary>
        public static Regex Construct { get; set; } = new Regex(Config.Instance.TikZArrowRegex);

        /// <summary>
        /// 射を表すMathObject
        /// </summary>
        public MathObject MathObject { get; }

        /// <summary>
        /// 元となった文字列
        /// </summary>
        public string OriginalText => MathObject.OriginalText;

        /// <summary>
        /// 射の名前
        /// </summary>
        public TokenString Name => MathObject.ToTokenString();

        /// <summary>
        /// 射のdomainのnode name
        /// </summary>
        public string SourceNodeName { get; }

        /// <summary>
        /// 射のcodomainのnode name
        /// </summary>
        public string TargetNodeName { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">射の名前を表すSequence</param>
        /// <param name="source">domainのnode name</param>
        /// <param name="target">codomainのnode name</param>
        private TikZArrow(MathObject name, string source, string target)
        {
            MathObject = name;
            SourceNodeName = source;
            TargetNodeName = target;
        }

        /// <summary>
        /// TikZのdraw文を読み込みTikZArrowを生成する。射が存在しない場合は空コレクションを返す
        /// </summary>
        /// <param name="text">draw文</param>
        /// <returns></returns>
        public static IEnumerable<TikZArrow> Create(string text)
        {
            var m = Construct.Match(text);
            if (m.Success 
                && !m.Groups["arrow"].Value.Contains("|->")
                && !m.Groups["arrow"].Value.Contains("<-|")
                && !m.Groups["math"].Value.IsNullOrWhiteSpace())
            {
                var math = m.Groups["math"].Value;
                var source = m.Groups["source"].Value;
                var target = m.Groups["target"].Value;

                foreach (var item in new MathObjectFactory(math).Create())
                {
                    if (m.Groups[1].Value.Contains("<-"))
                    {
                        yield return new TikZArrow(item, target, source);
                    }
                    else
                    {
                        yield return new TikZArrow(item, source, target);
                    }
                }
            }
        }

        public override string ToString() =>  $"{OriginalText.Trim()}: ({SourceNodeName}) → ({TargetNodeName})";
    }
}
