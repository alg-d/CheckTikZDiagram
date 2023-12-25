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

        /// <summary>
        /// 生成時に使用する一時リスト (数式本体用)
        /// </summary>
        private List<MathObject> _tempMath = new();

        /// <summary>
        /// 生成時に使用する一時StringBuilder (元文字列用)
        /// 先頭と末尾の、無視するTeXコマンドは含まない
        /// </summary>
        private readonly StringBuilder _tempOriginal = new();

        private readonly List<Token> _inBracket = new();
        private Token _scriptToken = Token.Empty;
        private int _bracketCount;

        public MathObjectFactory(string text)
        {
            _source = text.ToTokenString();
        }

        public MathObjectFactory(TokenString text)
        {
            _source = text;
        }

        private void Clear()
        {
            _tempMath = new List<MathObject>();
            _tempOriginal.Clear();
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

            // 末尾の無視するTeXコマンドは含まない
            var last = -1;
            for (int i = 0; i < _source.Tokens.Count; i++)
            {
                if (!_source.Tokens[i].IsIgnored())
                {
                    last = i;
                }
            }

            for (int i = 0; i < last + 1; i++)
            {
                var token = _source.Tokens[i];

                // 無視するTeXコマンド
                if (token.IsIgnored())
                {
                    // 先頭の無視するTeXコマンドは_tempOriginalにも含まない
                    if (_tempMath.Count >= 1)
                    {
                        // なのでそうでない場合はAppend
                        _tempOriginal.Append(token.Origin);
                    }
                    continue;
                }

                // 括弧内の処理
                if (_bracketCount > 0)
                {
                    BracketMode(token);
                    continue; // 括弧の中ではSeparatorの判定はしない
                }

                // Separatorの判定
                if (token.IsSeparator())
                {
                    if (_tempMath.Count >= 1)
                    {
                        yield return ReturnMathObject();
                    }
                    Clear();
                    continue;
                }
                // :=の対応
                else if (token.Value == ":" && i < last && _source.Tokens[i + 1].Value == "=")
                {
                    if (_tempMath.Count >= 1)
                    {
                        yield return ReturnMathObject();
                    }
                    i++;
                    Clear();
                    continue;
                }

                // 通常の場合
                NormalMode(token);
            }

            if (_tempMath.Count >= 1)
            {
                yield return ReturnMathObject();
            }
        }

        private MathObject ReturnMathObject()
        {
            if (_tempMath.Count == 1)
            {
                return _tempMath[0];
            }
            else
            {
                var math = new MathSequence(_tempMath, "", _tempOriginal.ToString());
                return math;
            }
        }

        private void BracketMode(Token token)
        {
            // _bracketCount > 0
            if (_bracketCount == 0) throw new InvalidOperationException("_bracketCount > 0 でないのにBracketModeに来るのはおかしい");

            _tempOriginal.Append(token.Origin);

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

            _tempOriginal.Append(token.Origin);

            if (token.IsOpenBracket())
            {
                _bracketCount++;
                _inBracket.Clear();
                _inBracket.Add(token);
            }
            else if (token.Value == "^" || token.Value == "_")
            {
                if (_tempMath.Count == 0)
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
            var last = _tempMath.Count - 1;
            if (_scriptToken.IsEmpty)
            {
                _tempMath.Add(mathObject);
            }
            else
            {
                if (mathObject.Length > 0)
                {
                    _tempMath[last] = _tempMath[last].SetScript(_scriptToken, left, mathObject, right);
                }
                _scriptToken = Token.Empty;
            }
        }
    }
}
