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
        private readonly StringBuilder _comment = new StringBuilder();
        private bool _commentFlag;
        private bool _mathFlag;
        private bool _tikzFlag;
        private bool _texCommandFlag;
        private bool _definitionFlag;
        private int _line;

        public event Action<CheckResult>? OutputEvent;

        public bool ErrorOnly { get; set; } = true;

        public MainModel()
        {
        }

        /// <summary>
        /// メイン処理
        /// </summary>
        /// <param name="text">読み込むtexファイルの中身</param>
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
            _line = 1;
            
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
            // _definitionFlag == true or false
            if (!_commentFlag) throw new InvalidOperationException("_commentFlag == true でないのにCommentModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにCommentModeに来るのはおかしい");

            if (x == '\n')
            {
                _commentFlag = false;
                _text.Append(x);
                return;
            }
            else if (x == 'm' && _comment.ToString() == "CheckTikZDiagra")
            {
                if (_tikzFlag)
                {
                    _definitionFlag = true;
                }
                else
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
            // _definitionFlag == true or false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにMathModeに来るのはおかしい");
            if (!_mathFlag) throw new InvalidOperationException("_mathFlag == true でないのにMathModeに来るのはおかしい");
            if (_tikzFlag) throw new InvalidOperationException("_tikzFlag == false でないのにMathModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにMathModeに来るのはおかしい");

            if (x == '$')
            {
                _mathFlag = false;
                ReadMathText(_text.ToString());
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
        /// <param name="text"></param>
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

            if (_definedMorphismDictionary.TryGetValue(mor.Source.ToTokenString(), out var source) 
                && (source.Type == MorphismType.Functor || source.Type == MorphismType.ContravariantFunctor || source.Type == MorphismType.Bifunctor))
            {
                return true;
            }

            if (_definedMorphismDictionary.TryGetValue(mor.Target.ToTokenString(), out var target)
                && (target.Type == MorphismType.Functor || target.Type == MorphismType.ContravariantFunctor || target.Type == MorphismType.Bifunctor))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// TikZPicture環境を読むための処理
        /// </summary>
        /// <param name="x"></param>
        private void TikZMode(char x)
        {
            // commentFlag == false
            // mathFlag == false
            // tikzFlag == true
            // texCommandFlag == false
            // definitionFlag == true or false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにTikZModeに来るのはおかしい");
            if (_mathFlag) throw new InvalidOperationException("_mathFlag == false でないのにTikZModeに来るのはおかしい");
            if (!_tikzFlag) throw new InvalidOperationException("_tikzFlag == true でないのにTikZModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTikZModeに来るのはおかしい");

            if (x == '}')
            {
                if (_text.ToString().EndsWith(@"\end{tikzpicture", StringComparison.Ordinal))
                {
                    new TikZDiagram(_text.ToString(), _line, _definitionFlag, ErrorOnly,
                        _definedMorphismDictionary, _parameterizedMorphisms, _functors)
                        .CheckDiagram()
                        .ForEach(x => OutputResult(x));
                    
                    _tikzFlag = false;
                    _definitionFlag = false;
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
            // _definitionFlag == false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_mathFlag) throw new InvalidOperationException("_mathFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzFlag) throw new InvalidOperationException("_tikzFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (!_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == true でないのにTexCommandModeに来るのはおかしい");
            if (_definitionFlag) throw new InvalidOperationException("_definitionFlag == false でないのにTexCommandModeに来るのはおかしい");

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
            // commentFlag == false
            // mathFlag == false
            // tikzFlag == false
            // texCommandFlag == false
            // definitionFlag == false
            if (_commentFlag) throw new InvalidOperationException("_commentFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_mathFlag) throw new InvalidOperationException("_mathFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_tikzFlag) throw new InvalidOperationException("_tikzFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_definitionFlag) throw new InvalidOperationException("_definitionFlag == false でないのにTexCommandModeに来るのはおかしい");

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
