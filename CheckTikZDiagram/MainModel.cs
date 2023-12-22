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
        public Dictionary<TokenString, Morphism> _definedMorphismDictionary = new Dictionary<TokenString, Morphism>();
        public List<Morphism> _parameterizedMorphisms = new List<Morphism>();
        public List<Functor> _functors = new List<Functor>();

        private readonly StringBuilder _text = new StringBuilder();
        private readonly StringBuilder _texCommand = new StringBuilder();
        private readonly StringBuilder _comment = new StringBuilder(); // コメントの中身
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
            _parameterizedMorphisms = CreateDefaultMorphisms().ToList();
            _functors = CreateDefaultFunctors().ToList();

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
                _text.Append(' ');
                return;
            }

            if (_tikzFlag)
            {
                if (x == 'n' && _comment.ToString().EndsWith("CheckTikZDiagramDefinitio"))
                {
                    _tikzDefinitionFlag = true;
                }
                else if (x == 'e' && _comment.ToString().EndsWith("CheckTikZDiagramIgnor"))
                {
                    // CheckTikZDiagramIgnoreの処理はTikZDiagramでやる
                    _text.Append(_comment);
                    _text.Append(x);
                    _text.Append('\n');
                }
                else if (x == 't' && _comment.ToString().EndsWith("CheckTikZDiagramOmi"))
                {
                    _tikzOmitFlag = true;
                }
            }
            else
            {
                if (x == 'm' && _comment.ToString().EndsWith("CheckTikZDiagra"))
                {
                    _commentFlag = false;
                    return;
                }
            }

            _comment.Append(x);
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

            if (x == '$' && !_text.ToString().EndsWith(@"\", StringComparison.Ordinal))
            {
                _mathFlag = false;
                try
                {
                    ReadMathText(_text.ToString());
                }
                catch (InvalidOperationException ex)
                {
                    OutputResult(new CheckResult(_line, $"数式 {_text} が不正です。{ex.Message}", true, true));
                }
                return;
            }
            else if (x == '%')
            {
                _commentFlag = true;
                _comment.Clear();
                return;
            }

            _text.Append(x);
        }

        /// <summary>
        /// 数式($で囲まれた部分)を解釈して_definedMorphismDictionaryに格納する
        /// </summary>
        /// <param name="text">数式($は含まない)</param>
        private void ReadMathText(string text)
        {
            foreach (var mor in Morphism.Create(text, _line))
            {
                if (mor.Name.ToString().Contains("#") || mor.Source.ToString().Contains("#") || mor.Target.ToString().Contains("#"))
                {
                    _parameterizedMorphisms.Add(mor);
                }
                else
                {
                    if (IsNaturalTransformation(mor))
                    {
                        mor.SetNaturalTransformation();
                    }
                    _definedMorphismDictionary[mor.Name.ToTokenString()] = mor;
                }
            }

            var func = Functor.Create(text);
            if (func != null)
            {
                _functors.Add(func);
                OutputResult(new CheckResult(_line, func.ToString(), false, true));
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
                if (_text.ToString().EndsWith(@"\end{tikzpicture", StringComparison.Ordinal))
                {
                    try
                    {
                        new TikZDiagram(_text.ToString(), _line, _tikzDefinitionFlag, _tikzOmitFlag, ErrorOnly,
                           _definedMorphismDictionary, _parameterizedMorphisms, _functors)
                           .CheckDiagram()
                           .ForEach(x => OutputResult(x));
                    }
                    catch (InvalidOperationException ex)
                    {
                        OutputResult(new CheckResult(_line, $"tikzpicture環境 {_text} が不正です。{ex.Message}", true, true));
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
                _comment.Clear();
                return;
            }

            _text.Append(x);
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
            if (_texCommand.Length == 1)
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
                        _texCommand.Append(x);
                        return;
                }
            }
            // 制御綴 2文字目以降
            else
            {
                if (('a' <= x && x <= 'z') || ('A' <= x && x <= 'Z') || x == '{' || x == '}')
                {
                    _texCommand.Append(x);
                    return;
                }

                // 制御綴終了
                if (_texCommand.ToString() == @"\begin{tikzpicture}")
                {
                    _tikzFlag = true;
                    _text.Clear();
                    _text.Append(x);

                    _texCommandFlag = false;
                    return;
                }

                if (x == '\\')
                {
                    _texCommand.Clear();
                    _texCommand.Append('\\');
                }
                else if (x == '$')
                {
                    _mathFlag = true;
                    _text.Clear();
                    _texCommandFlag = false;
                }
                else if (x == '%')
                {
                    _commentFlag = true;
                    _comment.Clear();
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
                _text.Clear();
            }
            else if (x == '\\')
            {
                _texCommandFlag = true;
                _texCommand.Clear();
                _texCommand.Append('\\');
            }
            else if (x == '%')
            {
                _commentFlag = true;
                _comment.Clear();
            }
        }

        private void OutputResult(CheckResult result)
        {
            this.OutputEvent?.Invoke(result);
        }

        private IEnumerable<Morphism> CreateDefaultMorphisms()
        {
            foreach (var item in Config.Instance.Morphisms)
            {
                foreach (var mor in Morphism.Create(item))
                {
                    yield return mor;
                }
            }
        }

        private IEnumerable<Functor> CreateDefaultFunctors()
        {
            foreach (var item in Config.Instance.Functors)
            {
                var x = Functor.Create(item);
                if (x != null)
                {
                    yield return x;
                }
            }
        }
    }
}
