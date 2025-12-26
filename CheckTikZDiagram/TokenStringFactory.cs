using System;
using System.Collections.Generic;
using System.Text;

namespace CheckTikZDiagram
{
    /// <summary>
    /// 文字列からTokenStringを生成するためのクラス
    /// </summary>
    public class TokenStringFactory
    {
        private readonly string _sourceText;

        private readonly List<Token> _tokens = new();
        private readonly StringBuilder _temp = new();
        private readonly StringBuilder _origin = new();
        private bool _texCommandFlag = false;
        private bool _texArgumentFlag = false;
        private bool _supOrSubFlag = false; // 直前が ^ または _ の場合true
        private bool _primeFlag = false; // 直前が ' の場合true
        private bool _rightCurlyBracket = false; // } の追加が必要な場合true

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
                else if (_texArgumentFlag)
                {
                    TexArgumentMode(x);
                }
                else if (_primeFlag)
                {
                    PrimeMode(x);
                }
                else
                {
                    TextMode(x);
                }
            }

            if (_texCommandFlag || _texArgumentFlag)
            {
                if (_temp.ToString() == "#")
                {
                    throw new InvalidOperationException("# で終わることはできません。");
                }
                AddToken(_temp.ToString(), _supOrSubFlag);
            }
            else if (_primeFlag)
            {
                _tokens.Add(new Token("}", ""));
            }

            return _tokens.ToTokenString();
        }

        private void TexCommandMode(char x)
        {
            // _texCommandFlag == true
            // _texArgumentFlag == false
            // _supOrSubFlag == true or false
            // _primeFlag == false
            if (!_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == true でないのにTexCommandModeに来るのはおかしい");
            if (_texArgumentFlag) throw new InvalidOperationException("_texArgumentFlag == false でないのにTexCommandModeに来るのはおかしい");
            if (_primeFlag) throw new InvalidOperationException("_primeFlag == false でないのにTexCommandModeに来るのはおかしい");

            // 制御綴 1文字目
            if (_temp.Length == 1)
            {
                _origin.Append(x);

                switch (x)
                {
                    case '[':
                    case ']':
                    case '\\':
                    case ' ':
                    case '$':
                    case '%':
                    case '"':
                    case '&':
                    case '_':
                    case '?':
                    case '#':
                        _texCommandFlag = false;
                        return;

                    case ',':
                    case '!':
                    case ':':
                    case ';':
                        _texCommandFlag = false;
                        _temp.Append(x);
                        AddToken(_temp.ToString(), _supOrSubFlag);
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
                    // 直前の文字までを一つのTeXコマンドとして、Tokenにする
                    AddToken(_temp.ToString(), _supOrSubFlag);
                    _texCommandFlag = false;
                    TextMode(x);
                }
            }
        }

        private void TexArgumentMode(char x)
        {
            // _texCommandFlag == false
            // _texArgumentFlag == true
            // _supOrSubFlag == true or false
            // _primeFlag == false
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTexArgumentModeに来るのはおかしい");
            if (!_texArgumentFlag) throw new InvalidOperationException("_texArgumentFlag == true でないのにTexArgumentModeに来るのはおかしい");
            if (_primeFlag) throw new InvalidOperationException("_primeFlag == false でないのにTexArgumentModeに来るのはおかしい");

            if (_temp.Length == 1)
            {
                if (x >= '0' && x <= '9')
                {
                    _temp.Append(x);
                    _origin.Append(x);
                }
                else
                {
                    throw new InvalidOperationException("# の直後は数字でなければなりません。");
                }
            }
            else if (x.AllowedCharacter())
            {
                _texArgumentFlag = false;
                _temp.Append(x);
                _origin.Append(x);
                AddToken(_temp.ToString(), _supOrSubFlag);
            }
            else
            {
                _texArgumentFlag = false;
                AddToken(_temp.ToString(), _supOrSubFlag);
                TextMode(x);
            }
        }

        private void PrimeMode(char x)
        {
            // _texCommandFlag == false
            // _texArgumentFlag == false
            // _supOrSubFlag == true or false
            // _primeFlag == true
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにPrimeModeに来るのはおかしい");
            if (_texArgumentFlag) throw new InvalidOperationException("_texArgumentFlag == false でないのにPrimeModeに来るのはおかしい");
            if (!_primeFlag) throw new InvalidOperationException("_primeFlag == true でないのにPrimeModeに来るのはおかしい");

            if (x == ' ')
            {
                _origin.Append(x);
            }
            else if (_supOrSubFlag)
            {
                _supOrSubFlag = false;
                _primeFlag = false;

                if (x == '{')
                {
                    _origin.Append(x);
                }
                else
                {
                    _rightCurlyBracket = true;
                    TextMode(x);
                }
            }
            else
            {
                if (x == '\'')
                {
                    _origin.Append(x);
                    AddToken(@"\prime", false);
                }
                else if (x == '^')
                {
                    _supOrSubFlag = true;
                    _origin.Append(x);
                }
                else
                {
                    _primeFlag = false;
                    _tokens.Add(new Token("}", ""));
                    TextMode(x);
                }
            }
        }

        private void TextMode(char x)
        {
            // _texCommandFlag == false
            // _texArgumentFlag == false
            // _supOrSubFlag == true or false
            // _primeFlag == false
            if (_texCommandFlag) throw new InvalidOperationException("_texCommandFlag == false でないのにTextModeに来るのはおかしい");
            if (_texArgumentFlag) throw new InvalidOperationException("_texArgumentFlag == false でないのにTextModeに来るのはおかしい");
            if (_primeFlag) throw new InvalidOperationException("_primeFlag == false でないのにTextModeに来るのはおかしい");

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
                _texArgumentFlag = true;
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
                _supOrSubFlag = false;
                _primeFlag = true;
                _origin.Append(x);
                _tokens.Add(new Token("^", ""));
                _tokens.Add(new Token("{", ""));
                AddToken(@"\prime", false);
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

            if (_rightCurlyBracket)
            {
                _rightCurlyBracket = false;
                _tokens.Add(new Token("}", ""));
            }
        }

        private void AddToken(char value, bool bracket)
        {
            AddToken(value.ToString(), bracket);
        }
    }
}
