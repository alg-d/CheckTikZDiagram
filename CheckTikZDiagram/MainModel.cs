using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//Microsoft.CodeAnalysis.FxCopAnalyzers

namespace CheckTikZDiagram
{
    public class MainModel
    {
        public Dictionary<TokenString, Morphism> _definedMorphismDictionary = new();
        public List<Morphism> _parameterizedMorphisms = new();
        public List<Functor> _functors = new();

        private readonly StringBuilder _textTemp = new(); // 現在読み込んでいる数式、TikZ
        private readonly StringBuilder _texCommandTemp = new(); // 現在読み込んでいるコマンド
        private readonly StringBuilder _commentTemp = new(); // 現在読み込んでいるコメント
        private bool _commentFlag; // コメントのときtrue
        private bool _mathFlag; // 数式環境のときtrue
        private bool _tikzFlag; // tikzpicture環境のときtrue
        private bool _texCommandFlag;
        private bool _tikzDefinitionFlag; // CheckTikZDiagramDefinitionコメントのあるtikzpicture環境のときtrue (tikzpictureを定義として使用する)
        private bool _tikzOmitFlag; // CheckTikZDiagramOmitコメントのあるtikzpicture環境のときtrue (演算子の省略を認める)
        private int _line;

        public event Action<CheckResult>? OutputEvent;

        public bool ErrorOnly { get; set; } = true;

        public MainModel()
        {
        }

        /// <summary>
        /// メイン処理
        /// texファイルを前から読んでいき数式環境、tikzpicture環境ごとに処理する
        /// </summary>
        /// <param name="text">読み込むtexファイルの中身全体の文字列</param>
        /// <param name="progressNumber">何行ごとにProgressBarを進めるか</param>
        public void MainLoop(string text, int progressNumber)
        {
            TikZNode.Construct = new Regex(Config.Instance.TikZNodeRegex);
            TikZArrow.Construct = new Regex(Config.Instance.TikZArrowRegex);
            Morphism.Construct = new Regex(Config.Instance.MorphismRegex);

            _definedMorphismDictionary = new Dictionary<TokenString, Morphism>();
            _parameterizedMorphisms = Config.Instance.CreateDefaultMorphisms().ToList();
            _functors = Config.Instance.CreateDefaultFunctors().ToList();

            _commentFlag = false;
            _mathFlag = false;
            _tikzFlag = false;
            _texCommandFlag = false;
            _tikzOmitFlag = false;
            _line = 1;

            // 「\\」は削除
            text = text.Replace(@"\\", " ");

            foreach (var x in text)
            {
                if (x == '\n')
                {
                    if (_line % progressNumber == 0)
                    {
                        OutputResult(new CheckResult(_line));
                    }
                    _line++;
                }

                if (_commentFlag)
                {
                    CommentMode(x);
                }
                else if (_mathFlag)
                {
                    MathMode(x);
                }
                else if (_tikzFlag)
                {
                    TikZMode(x);
                }
                else if (_texCommandFlag)
                {
                    TexCommandMode(x);
                }
                else
                {
                    TextMode(x);
                }
            }
        }

        private void CommentMode(char x)
        {
            // _commentFlag == true
            // _mathFlag == true or false
            // _tikzFlag == true or false
            // _texCommandFlag == false
            // _tikzDefinitionFlag == true or false
            // _tikzOmitFlag == true or false
            if (!_commentFlag) throw new InvalidOperationException("_commentFlag == true でないのにCommentModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにCommentModeに来るのはおかしい");

            if (x == '\n')
            {
                _commentFlag = false;
                _textTemp.Append(x);
                return;
            }

            if (_tikzFlag)
            {
                if (x == 'n' && _commentTemp.ToString().EndsWith("CheckTikZDiagramDefinitio"))
                {
                    _tikzDefinitionFlag = true;
                }
                else if (x == 'e' && _commentTemp.ToString().EndsWith("CheckTikZDiagramIgnor"))
                {
                    // CheckTikZDiagramIgnoreの処理はTikZDiagramでやる
                    _textTemp.Append(_commentTemp);
                    _textTemp.Append(x);
                    _textTemp.Append('\n');
                }
                else if (x == 't' && _commentTemp.ToString().EndsWith("CheckTikZDiagramOmi"))
                {
                    _tikzOmitFlag = true;
                }
            }
            else
            {
                if (x == 'm' && _commentTemp.ToString().EndsWith("CheckTikZDiagra"))
                {
                    _commentFlag = false;
                    return;
                }
            }

            _commentTemp.Append(x);
        }

        /// <summary>
        /// 本文の数式を読むための処理
        /// </summary>
        /// <param name="x"></param>
        private void MathMode(char x)
        {
            // _commentFlag == false
            // _mathFlag == true
            // _tikzFlag == false
            // _texCommandFlag == false
            // _tikzDefinitionFlag == true or false
            // _tikzOmitFlag == false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにMathModeに来るのはおかしい");
            if (!_mathFlag) throw new InvalidOperationException("_mathFlag == true でないのにMathModeに来るのはおかしい");
            if (_tikzFlag) throw new InvalidOperationException("_tikzFlag == false でないのにMathModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにMathModeに来るのはおかしい");
            if (_tikzOmitFlag) throw new InvalidOperationException("_omitFlag == false でないのにMathModeに来るのはおかしい");

            if (x == '$' && !_textTemp.ToString().EndsWith(@"\", StringComparison.Ordinal))
            {
                _mathFlag = false;
                try
                {
                    ReadMathText(_textTemp.ToString());
                }
                catch (InvalidOperationException ex)
                {
                    OutputResult(new CheckResult(_line, $"数式 {_textTemp} が不正です。{ex.Message}", true, true));
                }
                return;
            }
            else if (x == '%')
            {
                _commentFlag = true;
                _commentTemp.Clear();
                return;
            }

            _textTemp.Append(x);
        }

        /// <summary>
        /// 数式($で囲まれた部分)を解釈して_definedMorphismDictionaryに格納する
        /// </summary>
        /// <param name="text">数式($は含まない)</param>
        private void ReadMathText(string text)
        {
            // 先に関手の定義かどうか判断する
            var func = Functor.Create(text);
            if (func != null)
            {
                _functors.Add(func);
                OutputResult(new CheckResult(_line, func.ToString(), false, true));
            }
            else
            {
                foreach (var mor in Morphism.Create(text, _line))
                {
                    if (mor.Name.ToString().Contains("#") || mor.Source.ToString().Contains("#") || mor.Target.ToString().Contains("#"))
                    {
                        // #が含まれる場合はパラメーター付き射として扱う
                        _parameterizedMorphisms.Add(mor);
                    }
                    else
                    {
                        if (IsNaturalTransformation(mor))
                        {
                            // 自然変換の場合はここで自然変換扱いに変更しておく
                            mor.SetNaturalTransformation();
                        }
                        _definedMorphismDictionary[mor.Name.ToTokenString()] = mor;
                    }
                }
            }
        }

        private bool IsNaturalTransformation(Morphism mor)
        {
            if (mor.Type != MorphismType.TwoMorphism)
            {
                return false;
            }

            if (_definedMorphismDictionary.TryGetValue(mor.Source.ToTokenString(), out var source) && source.IsFunctor)
            {
                return true;
            }

            if (_definedMorphismDictionary.TryGetValue(mor.Target.ToTokenString(), out var target) && target.IsFunctor)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// tikzpicture環境を読むための処理
        /// </summary>
        /// <param name="x"></param>
        private void TikZMode(char x)
        {
            // _commentFlag == false
            // _mathFlag == false
            // _tikzFlag == true
            // _texCommandFlag == false
            // _tikzDefinitionFlag == true or false
            // _tikzIgnoreFlag == true or false
            // _tikzOmitFlag == true or false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにTikZModeに来るのはおかしい");
            if (_mathFlag) throw new InvalidOperationException("_mathFlag == false でないのにTikZModeに来るのはおかしい");
            if (!_tikzFlag) throw new InvalidOperationException("_tikzFlag == true でないのにTikZModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTikZModeに来るのはおかしい");

            if (x == '}')
            {
                if (_textTemp.ToString().EndsWith(@"\end{tikzpicture", StringComparison.Ordinal))
                {
                    try
                    {
                        new TikZDiagram(_textTemp.ToString(), _line, _tikzDefinitionFlag, _tikzOmitFlag, ErrorOnly,
                           _definedMorphismDictionary, _parameterizedMorphisms, _functors)
                           .CheckDiagram()
                           .ForEach(x => OutputResult(x));
                    }
                    catch (InvalidOperationException ex)
                    {
                        OutputResult(new CheckResult(_line, $"tikzpicture環境 {_textTemp} が不正です。{ex.Message}", true, true));
                    }

                    _tikzFlag = false;
                    _tikzDefinitionFlag = false;
                    _tikzOmitFlag = false;
                    return;
                }
            }
            else if (x == '%')
            {
                _commentFlag = true;
                _commentTemp.Clear();
                return;
            }

            _textTemp.Append(x);
        }

        private void TexCommandMode(char x)
        {
            // _commentFlag == false
            // _mathFlag == false
            // _tikzFlag == false
            // _texCommandFlag == true
            // _tikzDefinitionFlag == false
            // _tikzOmitFlag == false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_mathFlag) throw new InvalidOperationException("_mathFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzFlag) throw new InvalidOperationException("_tikzFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (!_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == true でないのにTexCommandModeに来るのはおかしい");
            if (_tikzDefinitionFlag) throw new InvalidOperationException("_definitionFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzOmitFlag) throw new InvalidOperationException("_omitFlag == false でないのにTexCommandModeに来るのはおかしい");

            // 制御綴 1文字目
            if (_texCommandTemp.Length == 1)
            {
                switch (x)
                {
                    case '[':
                    case ']':
                    case '\\':
                    case ',':
                    case ' ':
                    case '$':
                    case '%':
                    case '!':
                    case '"':
                    case '&':
                    case '_':
                    case '?':
                        _texCommandFlag = false;
                        return;

                    default:
                        _texCommandTemp.Append(x);
                        return;
                }
            }
            // 制御綴 2文字目以降
            else
            {
                if (('a' <= x && x <= 'z') || ('A' <= x && x <= 'Z') || x == '{' || x == '}')
                {
                    _texCommandTemp.Append(x);
                    return;
                }

                // 制御綴終了
                if (_texCommandTemp.ToString() == @"\begin{tikzpicture}")
                {
                    _tikzFlag = true;
                    _textTemp.Clear();
                    _textTemp.Append(x);

                    _texCommandFlag = false;
                    return;
                }

                if (x == '\\')
                {
                    _texCommandTemp.Clear();
                    _texCommandTemp.Append('\\');
                }
                else if (x == '$')
                {
                    _mathFlag = true;
                    _textTemp.Clear();
                    _texCommandFlag = false;
                }
                else if (x == '%')
                {
                    _commentFlag = true;
                    _commentTemp.Clear();
                    _texCommandFlag = false;
                }
                else
                {
                    _texCommandFlag = false;
                }
            }
        }

        private void TextMode(char x)
        {
            // _commentFlag == false
            // _mathFlag == false
            // _tikzFlag == false
            // _texCommandFlag == false
            // _tikzDefinitionFlag == false
            // _tikzOmitFlag == false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_mathFlag) throw new InvalidOperationException("_mathFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzFlag) throw new InvalidOperationException("_tikzFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzDefinitionFlag) throw new InvalidOperationException("_definitionFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzOmitFlag) throw new InvalidOperationException("_omitFlag == false でないのにTexCommandModeに来るのはおかしい");

            if (x == '$')
            {
                _mathFlag = true;
                _textTemp.Clear();
            }
            else if (x == '\\')
            {
                _texCommandFlag = true;
                _texCommandTemp.Clear();
                _texCommandTemp.Append('\\');
            }
            else if (x == '%')
            {
                _commentFlag = true;
                _commentTemp.Clear();
            }
        }

        private void OutputResult(CheckResult result)
        {
            this.OutputEvent?.Invoke(result);
        }
    }
}
