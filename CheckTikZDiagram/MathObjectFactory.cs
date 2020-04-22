using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 文字列からMathObjectを生成するためのクラス
    /// </summary>
    public class MathObjectFactory
    {
        private readonly TokenString _source;

        private List<MathObject> _sequence = new List<MathObject>();
        private readonly List<Token> _inBracket = new List<Token>();
        private readonly StringBuilder _original = new StringBuilder();
        private Token _scriptToken = Token.Empty;
        private int _bracketCount;

        public MathObjectFactory(string text)
        {
            foreach (var item in Config.Instance.IgnoreCommands)
            {
                text = text.Replace(item, "", StringComparison.Ordinal);
            }
            _source = text.ToTokenString();
        }

        public MathObjectFactory(TokenString text)
        {
            _source = text;
        }

        private void Clear()
        {
            _sequence = new List<MathObject>();
            _original.Clear();
            _bracketCount = 0;
        }

        /// <summary>
        /// Factoryに指定した文字列からSequenceを生成する(separatorで分解せず、ただ一つのSequenceを生成)
        /// </summary>
        /// <returns></returns>
        public MathObject CreateSingle()
        {
            var xs = CreateMain().ToArray();
            if (xs.Length == 0)
            {
                return new MathSequence(Array.Empty<MathObject>(), "", _source.ToOriginalString());
            }
            else if (xs.Length == 1)
            {
                return xs[0];
            }
            else
            {
                return new MathSequence(xs, ",", _source.ToOriginalString());
            }
        }

        /// <summary>
        /// Factoryに指定した文字列からSequenceを生成する(separatorで分解する)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MathObject> Create()
        {
            return CreateMain();
        }

        /// <summary>
        /// Factoryに指定した文字列からSequenceを生成する(処理本体)
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MathObject> CreateMain()
        {
            Clear();

            for (int i = 0; i < _source.Tokens.Count; i++)
            {
                var token = _source.Tokens[i];

                if (_bracketCount > 0)
                {
                    BracketMode(token);
                    continue; // 括弧の中ではSeparatorの判定はしない
                }
                else
                {
                    NormalMode(token);
                }

                // Separatorの判定
                if (token.IsSeparator())
                {
                    if (_sequence.Count >= 1)
                    {
                        yield return ReturnMathObject();
                    }
                    Clear();
                }
            }

            if (_sequence.Count >= 1)
            {
                yield return ReturnMathObject();
            }
        }

        private MathObject ReturnMathObject()
        {
            if (_sequence.Count == 1)
            {
                return _sequence[0];
            }
            else
            {
                var math = new MathSequence(_sequence, "", _original.ToString());
                return math;
            }
        }

        private void BracketMode(Token token)
        {
            // _bracketCount > 0
            if (_bracketCount == 0) throw new InvalidOperationException("_bracketCount > 0 でないのにBracketModeに来るのはおかしい");

            _original.Append(token.Origin);

            if (token.IsOpenBracket())
            {
                _bracketCount++;
            }
            else if (token.IsCloseBracket())
            {
                _bracketCount--;
            }

            if (_bracketCount == 0) // 全ての括弧が閉じられた場合
            {
                // 括弧の中身でMathObjectを生成
                var left = _inBracket.First();
                var main = _inBracket.Skip(1).ToArray().ToTokenString();
                var mathObject = new MathObjectFactory(main).CreateSingle();

                // 添え字でない場合は括弧をMathObjectに含める
                if (_scriptToken.IsEmpty)
                {
                    if (mathObject is MathSequence m)
                    {
                        if (!m.IsSimple)
                        {
                            mathObject = new MathSequence(new MathObject[] { mathObject }, "", main.ToOriginalString());
                        }
                    }
                    mathObject = mathObject.SetBracket(left, token);
                }

                AddSequence(mathObject, left, token);
            }
            else
            {
                _inBracket.Add(token);
            }
        }

        private void NormalMode(Token token)
        {
            // _bracketCount == 0
            if (_bracketCount > 0) throw new InvalidOperationException("_bracketCount == 0 でないのにNormalModeに来るのはおかしい");

            if (token.IsSeparator())
            {
                return;
            }

            _original.Append(token.Origin);

            if (token.IsOpenBracket())
            {
                _bracketCount++;
                _inBracket.Clear();
                _inBracket.Add(token);
            }
            else if (token.Value == "^" || token.Value == "_")
            {
                if (_sequence.Count == 0)
                {
                    AddSequence(new MathObjectFactory("").CreateSingle(), Token.Empty, Token.Empty);
                }
                _scriptToken = token;
            }
            else
            {
                AddSequence(token);
            }
        }

        private void AddSequence(Token token)
        {
            AddSequence(new MathToken(token), Token.Empty, Token.Empty);
        }

        private void AddSequence(MathObject mathObject, Token left, Token right)
        {
            var last = _sequence.Count - 1;
            if (_scriptToken.IsEmpty)
            {
                _sequence.Add(mathObject);
            }
            else
            {
                _sequence[last] = _sequence[last].SetScript(_scriptToken, left, mathObject, right);
                _scriptToken = Token.Empty;
            }
        }
    }
}
