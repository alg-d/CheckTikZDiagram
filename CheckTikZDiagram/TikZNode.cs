using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    /// <summary>
    /// TikZにおける対象を表すクラス
    /// </summary>
    public class TikZNode
    {
        /// <summary>
        /// name: 最初の () の中身
        /// math: 最後の {$$} の中身
        /// </summary>
        public static Regex Construct { get; set; } = new Regex(Config.Instance.TikZNodeRegex);

        /// <summary>
        /// 元となった文字列
        /// </summary>
        public string OriginalText => MathObject.OriginalText;

        /// <summary>
        /// nodeの名前
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// nodeの中身を表すMathObject
        /// </summary>
        public MathObject MathObject { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="nodeName">対象のnode name</param>
        /// <param name="mathText">対象を表す数式</param>
        private TikZNode(string nodeName, MathObject math)
        {
            NodeName = nodeName;
            MathObject = math;
        }

        /// <summary>
        /// TikZのnode文を読み込みTikZNodeを生成する。対象が存在しない場合はnullを返す
        /// </summary>
        /// <param name="text">node文</param>
        /// <returns></returns>
        public static TikZNode? Create(string text)
        {
            var m = Construct.Match(text);
            if (m.Success)
            {
                var name = m.Groups["name"].Value;
                var math = m.Groups["math"].Value;
                return new TikZNode(name, new MathObjectFactory(math).CreateSingle());
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"({NodeName}) {OriginalText}";
        }
    }
}
