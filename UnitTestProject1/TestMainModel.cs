using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckTikZDiagram;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class TestMainModel
    {
        [TestMethod]
        public void Empty()
        {
            TestMain("", 0);
        }

        [TestMethod]
        public void 数式無し()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test

本文あああああああああ　ああああ\\
あああああああ　ああああ．

test
\end{document}", 0);

            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test

本文あああああああああ　ああああ\\
あああああああ　ああああ．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void 数式あり()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test

本文あああああああああ　ああああ\\
あああああああ$f\colon a\rightarrow b$ああああ．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void 数式ドルエスケープ()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
本文あああああ$y=f(x, \$)+4$ああああああああ
ああああ$g\colon a\rightarrow x$あああ　ああああ．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void ドル二つ()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
本文あああああ$y=f(x, \$)+4$ああああああああ
$$h\colon a\rightarrow x$$
ああああ$g\colon a\rightarrow x$あああ　ああああ．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle h$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void 数式改行()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test

ああああ\\
ああ$\test\\ abcde\\$ああああ\\ああああ．
ああああ$g\colon a\rightarrow x$あああ　ああああ．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void 数式コメント()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ\\
ああ$\test\\ abcde\\$ああああ\\ああああ．
ああああ$g\colon a\rightarrow x%bcd $
+y$ test test $f\colon a\rightarrow x$ test
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$}; \node (y) at (1.2, 1.2) {$x+y$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (y);
\end{tikzpicture}\]
test
\end{document}", 0);
        }

        [TestMethod]
        public void コメントは読まない()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ\\
ああ$\test\\ abcde\\$ああああ\\ああああ．
ああああいいいいいいいいい%$g\colon a\rightarrow x$
test test $f\colon a\rightarrow x$ test
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\end{tikzpicture}\]
%\[\begin{tikzpicture}[auto]
%\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
%\draw[->] (a) to node {$\scriptstyle h$} (x);
%\draw[->] (a) to node {$\scriptstyle i$} (x);
%\draw[->] (a) to node {$\scriptstyle j$} (x);
%\draw[->] (a) to node {$\scriptstyle k$} (x);
%\end{tikzpicture}\]
test $p\colon u\rightarrow v$
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$u$}; \node (x) at (1.2, 1.2) {$v$};
\draw[->] (a) to node {$\scriptstyle p$} (x);
\end{tikzpicture}\]
\end{document}", 1);
        }

        [TestMethod]
        public void CheckTikZDiagramコメント()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$\test  abcde $ああああ ああああ%CheckTikZDiagram $h\colon a\rightarrow x$
ああああいいいいいいいいい%test CheckTikZDiagram $g\colon a\rightarrow x$
test test $f\colon a\rightarrow x$ test
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\draw[->] (a) to node {$\scriptstyle h$} (x);
%\draw[->] (a) to node {$\scriptstyle i$} (x);
\draw[->] (a) to node {$\scriptstyle f$} (x);
\end{tikzpicture}\]
test $p\colon u\rightarrow v$
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$u$}; \node (x) at (1.2, 1.2) {$v$};
\draw[->] (a) to node {$\scriptstyle p$} (x);
\end{tikzpicture}\]
\end{document}", 0);
        }

        [TestMethod]
        public void CheckTikZDiagramDefinition()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$\test  abcde $ああああ ああああ
ああああいいいいいいいいい
test test 
\[\begin{tikzpicture}[auto] % CheckTikZDiagramDefinition
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\draw[->] (a) to node {$\scriptstyle h$} (x);
\end{tikzpicture}\]
test
\end{document}", 0);
        }

        [TestMethod]
        public void CheckTikZDiagramIgnore()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$f\colon a\rightarrow x$ああああ ああああ
あ$g\colon a\rightarrow x$ああ
ああああいいいいいい
いいい
test $l\colon a\rightarrow x$ああああ ああああ
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x); %CheckTikZDiagramIgnore
\draw[->] (a) to node {$\scriptstyle h$} (x); %CheckTikZDiagramIgnore
\draw[->] (a) to node {$\scriptstyle i$} (x);
\draw[->] (a) to node {$\scriptstyle j$} (x); \draw[->] (a) to node {$\scriptstyle k$} (x); %CheckTikZDiagramIgnore
\draw[->] (a) to node {$\scriptstyle l$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void 空矢印()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$f\colon a\rightarrow x$ああああ ああああ
ああああいいいいいいいいい
test test 
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (x) at (1, 0) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle $} (x);
\draw[->] (a) to node {$$} (x);
\end{tikzpicture}\]
test
\end{document}", 0);
        }

        [TestMethod]
        public void 不正な変数文字()
        {
            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
本文あああああ
あああ$f\colon a\rightarrow b$ああああ．
ああああ　ああああ
%CheckTikZDiagram $X(#1)\colon F(#) \rightarrow G(#1)$
あああああああ$g\colon a\rightarrow b$ああああ．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1.2) {$a$}; \node (x) at (1.2, 1.2) {$b$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\draw[->] (a) to node {$\scriptstyle g$} (x);
\end{tikzpicture}\]
test
\end{document}", 1);
        }

        [TestMethod]
        public void Config_TikZNode()
        {
            Config.Instance.TikZNodeRegex = @"\\path node\s*\((?<name>[^)]*)\).*\{\$(?<math>.*)\$\}\s*$";

            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$f\colon a\rightarrow x$ああああ ああああ
ああああいいいいいいいいい
test test 
\[\begin{tikzpicture}[auto]
\path node (a) at (0, 0) {$a$}; \path node (x) at (1, 0) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\end{tikzpicture}\]
test
\end{document}", 0);

            Config.Instance.TikZNodeRegex = @"\\node\s*\((?<name>[^)]*)\).*\{\$(?<math>.*)\$\}\s*$";
        }

        [TestMethod]
        public void Config_TikZArrow()
        {
            Config.Instance.TikZArrowRegex = @"\\path\s*\[(?<arrow>[^\]]*)[^()]*\((?<source>[^()]*)\).*node.*\{\$(?<math>.*)\$\}.*\((?<target>[^()]*)\)\s*$";

            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$f\colon a\rightarrow x$ああああ ああああ
ああああいいいいいいいいい
test test 
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (x) at (1, 0) {$x$};
\path[draw, ->] (a) to node {$\scriptstyle f$} (x);
\end{tikzpicture}\]
test
\end{document}", 0);

            Config.Instance.TikZArrowRegex = @"\\draw\s*\[(?<arrow>[^\]]*)[^()]*\((?<source>[^()]*)\).*node.*\{\$(?<math>.*)\$\}.*\((?<target>[^()]*)\)\s*$";
        }

        [TestMethod]
        public void Config_Morphism()
        {
            Config.Instance.MorphismRegex = @"^\\arrow\{(?<arrow>[Rr]{1,2})\}\{(?<name>.*)\}\{(?<source>.*)\}\{(?<target>.*)\}$";

            TestMain(@"\documentclass[uplatex,a4j,12pt,dvipdfmx]{jsarticle}
\author{algd}
\title{ああああああああああああああああああ}
\begin{document}
test
ああああ
ああ$\arrow{r}{f}{a}{x}$ああああ ああああ
ああああいいいいいいいいい
test test 
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (x) at (1, 0) {$x$};
\draw[->] (a) to node {$\scriptstyle f$} (x);
\end{tikzpicture}\]
test
\end{document}", 0);

            Config.Instance.MorphismRegex = @"^(?<name>.*)\\colon(?<source>.*)\\(?<arrow>[Rr]{1,2})ightarrow(?<target>.*)$";
        }



        private void TestMain(string text, int errorCount)
        {
            var model = new MainModel();
            var count = 0;

            model.OutputEvent += (result) =>
            {
                if (!result.Message.IsNullOrEmpty())
                {
                    Console.WriteLine(result.Message);
                }
                if (result.IsError)
                {
                    count++;
                }
            };

            model.MainLoop(text, int.MaxValue);
            count.Is(errorCount);
        }
    }
}
