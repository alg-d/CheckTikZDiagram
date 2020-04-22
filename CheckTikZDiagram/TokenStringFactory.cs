using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTikZDiagram
{
    public class TokenStringFactory
    {
        private readonly string _sourceText;

        private readonly List<Token> _tokens = new List<Token>();
        private readonly StringBuilder _temp = new StringBuilder();
        private readonly StringBuilder _origin = new StringBuilder();
        private bool _texCommandFlag = false;
        private bool _texParameterFlag = false;
        private bool _supOrSubFlag = false;

        public TokenStringFactory(string text)
        {
            _sourceText = text.Trim();
        }

        public TokenString Create()
        {
            foreach (var x in _sourceText)
            {
                if (_texCommandFlag)
                {
                    TexCommandMode(x);
                }
                else if (_texParameterFlag)
                {
                    TexParameterMode(x);
                }
                else
                {
                    TextMode(x);
                }
            }

            if (_texCommandFlag || _texParameterFlag)
            {
                AddToken(_temp.ToString(), _supOrSubFlag);
            }

            return _tokens.ToTokenString();
        }

        private void TexCommandMode(char x)
        {
            // _texCommandFlag == true
            // _texParamterFlag == false
            if (!_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == true でないのにTexCommandModeに来るのはおかしい");
            if (_texParameterFlag) throw new InvalidOperationException("_texParameterFlag == false でないのにTexCommandModeに来るのはおかしい");

            // 制御綴 1文字目
            if (_temp.Length == 1)
            {
                _origin.Append(x);

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
                    case '#':
                        _texCommandFlag = false;
                        return;

                    default:
                        _temp.Append(x);
                        return;
                }
            }
            else
            {
                if (('a' <= x && x <= 'z') || ('A' <= x && x <= 'Z'))
                {
                    _temp.Append(x);
                    _origin.Append(x);
                }
                else
                {
                    // 一つのTeXコマンドからなるMathObjectを追加
                    AddToken(_temp.ToString(), _supOrSubFlag);
                    _texCommandFlag = false;
                    TextMode(x);
                }
            }
        }

        private void TexParameterMode(char x)
        {
            // _texCommandFlag == false
            // _texParamterFlag == true
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTexParameterModeに来るのはおかしい");
            if (!_texParameterFlag) throw new InvalidOperationException("_texParameterFlag == true でないのにTexParameterModeに来るのはおかしい");

            if (_temp.Length == 1)
            {
                _temp.Append(x);
                _origin.Append(x);
            }
            else if (x == '?' || x == 's' || x == 't')
            {
                _temp.Append(x);
                _origin.Append(x);
                AddToken(_temp.ToString(), _supOrSubFlag);
                _texParameterFlag = false;
            }
            else
            {
                AddToken(_temp.ToString(), _supOrSubFlag);
                _texParameterFlag = false;
                TextMode(x);
            }
        }

        private void TextMode(char x)
        {
            // _texCommandFlag == false
            // _texParamterFlag == false
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTextModeに来るのはおかしい");
            if (_texParameterFlag) throw new InvalidOperationException("_texParameterFlag == false でないのにTextModeに来るのはおかしい");

            if (x == ' ')
            {
                _origin.Append(x);
            }
            else if (x == '\\')
            {
                _texCommandFlag = true;
                _temp.Clear();
                _temp.Append(x);
                _origin.Append(x);
            }
            else if (x == '#')
            {
                _texParameterFlag = true;
                _temp.Clear();
                _temp.Append(x);
                _origin.Append(x);
            }
            else if (x == '\r' || x == '\n')
            {
                _origin.Append(' ');
            }
            else if (x == '\'')
            {
                // ' は ^ { \prime } とみなす
                _tokens.Add(new Token("^", ""));
                _origin.Append(x);
                AddToken(@"\prime", true);
            }
            else
            {
                if (x == '^' || x == '_')
                {
                    _supOrSubFlag = true;
                    _origin.Append(x);
                    AddToken(x, false);
                }
                else if (x == '{')
                {
                    _supOrSubFlag = false;
                    _origin.Append(x);
                    AddToken(x, false);
                }
                else
                {
                    // x 一文字からなるTokenを追加
                    _origin.Append(x);
                    AddToken(x, _supOrSubFlag);
                }
            }
        }

        private void AddToken(string value, bool bracket)
        {
            if (bracket)
            {
                _tokens.Add(new Token("{", ""));
            }
            _tokens.Add(new Token(value, _origin.ToString()));
            if (bracket)
            {
                _tokens.Add(new Token("}", ""));
            }
            _origin.Clear();
            if (bracket)
            {
                _supOrSubFlag = false;
            }
        }

        private void AddToken(char value, bool bracket)
        {
            AddToken(value.ToString(), bracket);
        }
    }
}
