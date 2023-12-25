# CheckTikZDiagram
TikZの図式を検証するためのツールです。
(.NET 7.0を使用したWindowsアプリケーション)

## 使い方
![ツールの画面イメージ](http://alg-d.com/CheckTikZDiagram00.png)

1. CheckTikZDiagram.exe を実行します。
2. 「ファイル選択」ボタンをクリックして.texファイルを選択します。
3. 下記仕様に従って，誤りのある部分を検出します。
4. 読み込むときのルールは「設定1」「設定2」タブで設定します。(設定した内容はconfig.xmlに保存します。)

## 仕様
### 概要
選択した.texファイルを前から順に読んでいきます。
数式環境($で囲まれた部分のみが対象)があれば、その中身を「射の定義」として保持しておきます。
tikzpicture環境があれば、その中身を読み込み、それがあらかじめ保持している「射の定義」と「一致」するかを検証します。
どのような場合に「一致」するとみなすかは以下で説明します。

### 基本
例えば次のような.texファイルがあるとします。(プリアンプルは省略)
```
\begin{document}
$f\colon a\rightarrow b$，$g\colon b\rightarrow c$，$k\colon a\rightarrow c$を圏$C$の射とする．
即ち図式で描くと次のような状況である．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\draw[->] (a) -- node[swap] {$\scriptstyle g$} (c);
\draw[->] (b) -- node {$\scriptstyle k$} (c);
\end{tikzpicture}\]
\end{document}
```
この.texファイルはgとkの位置を間違えています。
実際，これをコンパイルすると次のようなPDFが出来上がり、図式が(もしくは本文が)間違っていることが分かります。
![コンパイル結果1](http://alg-d.com/CheckTikZDiagram01.png)

この.texファイルをCheckTikZDiagram.exeで処理すると、次のように間違っていることを示すメッセージが表示されます。
![ツール結果1](http://alg-d.com/CheckTikZDiagram02.png)

- CheckTikZDiagram.exeは、tikzpicture環境の各矢印に対して処理を行います。
    - どのような形式のTikZを射とみなすかは「設定2」タブで設定します。
- 処理の結果、エラーがあった場合はメッセージを出力します。(「エラーのあった行のみ表示する」のチェックを外すと全て表示します。)
    - 一番左の「行」は、対象となる矢印が.texファイルの何行目にあるかを示しています。
    - 「結果」の上段には、対象の矢印を解釈した結果が表示されています。
    - 「結果」の下段の g: b → c (12行目) というのは、本文の12行目にこのように定義されているということを示しています。
        - 本文のどのような形式の数式を射とみなすのかは、「設定2」タブで設定します。

### 射の合成
合成できる射の場合、合成した射はその通り解釈されます。
例えば次のような.texファイルは正しいと判断されます。
(合成に何の記号(TeXコマンド)を使用するかは「設定1」タブで設定します。)
```
\begin{document}
$f\colon a\rightarrow b$，$g\colon b\rightarrow c$を圏$C$の射とする．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\draw[->] (b) -- node {$\scriptstyle g$} (c);
\draw[->] (a) -- node[swap] {$\scriptstyle g\circ f$} (c);
\end{tikzpicture}\]
\end{document}
```

### 関手
本文に書かれた射の定義において、domainもしくはcodomainが圏とみなされたものについては「関手」として扱われます。
例えば次のような場合、Fは関手とみなされます。
(何を圏とみなすかは「設定1」タブで設定します。)
```
\begin{document}
$F\colon \Set \rightarrow \Ab$を関手，$f\colon a\rightarrow b$，$g\colon b\rightarrow c$を写像とする．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$Fa$}; \node (b) at (1, 1) {$Fb$}; \node (c) at (2, 0) {$Fc$};
\draw[->] (a) -- node {$\scriptstyle Ff$} (b);
\draw[->] (b) -- node {$\scriptstyle Fg$} (c);
\draw[->] (a) -- node[swap] {$\scriptstyle F(g\circ f)$} (c);
\end{tikzpicture}\]
\end{document}
```
このとき図式中に現れる Ff は射 Fa → Fb とみなされます。
よって上記の.texファイルは正しいと判断されます。

### Homの元
Homや関手圏から元を取った場合、それも射として扱われます。
```
\begin{document}
$f\in\Hom_C(Fa, Gb)$とする．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$Fa$}; \node (b) at (1, 0) {$Gb$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\end{tikzpicture}\]
\end{document}
```

### 逆射
(いつか書く)

### 自然変換
(いつか書く)

### 2変数の関手
(いつか書く)

### 2項演算(関手)
(いつか書く)

### Kan拡張・Kanリフト
(いつか書く)

### 関手を対象に適用したときの計算
(いつか書く)

### コメント内での射の定義
本文に書かれていない射がいきなり図式に出てくるような場合、コメントを使って射の定義を書くことができます。
やり方は、TeXのコメント内に CheckTikZDiagram と書くと、その行の後ろが射の定義として認識されます。
```
\begin{document}
$f\colon a\rightarrow b$，$g\colon a\rightarrow c$を$C$の射とする．
このとき次の点線の射が取れる．
%CheckTikZDiagram $h\colon b\rightarrow c$
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1) {$b$}; \node (c) at (1, 1) {$c$};
\node (b) at (0, 0) {$a$};
\draw[->] (a) -- node[swap] {$\scriptstyle f$} (b);
\draw[->] (a) -- node {$\scriptstyle g$} (c);
\draw[->,densely dashed] (b) -- node[swap] {$\scriptstyle h$} (c);
\end{tikzpicture}\]
\end{document}
```

### tikzpicture環境内でのコメント
tikzpicture環境内で、行末にコメントでCheckTikZDiagramIgnoreと書くと、その行は無視されます。
例えば次の場合、本来はkがエラーとなりますが、このエラーは無視されます。
```
\begin{document}
$f\colon a\rightarrow b$，$g\colon b\rightarrow c$，$h\colon a\rightarrow c$を圏$C$の射とする．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\draw[->] (b) -- node {$\scriptstyle g$} (c);
\draw[->] (a) -- node[swap] {$\scriptstyle k$} (c); %CheckTikZDiagramIgnore
\end{tikzpicture}\]
\end{document}
```
次のようにtikzpicture環境の最初の行にCheckTikZDiagramIgnoreを描いた場合は、その環境全体が無視されます。
```
\begin{document}
$f\colon a\rightarrow b$，$g\colon b\rightarrow c$，$h\colon a\rightarrow c$を圏$C$の射とする．
\[\begin{tikzpicture}[auto] %CheckTikZDiagramIgnore
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\draw[->] (b) -- node {$\scriptstyle g$} (c);
\draw[->] (a) -- node[swap] {$\scriptstyle k$} (c);
\end{tikzpicture}\]
\end{document}
```
tikzpicture環境内にCheckTikZDiagramDefinitionと書くと、その環境内の射は定義として採用されます。
```
\begin{document}
次の図式を考える．
\[\begin{tikzpicture}[auto] %CheckTikZDiagramDefinition
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\draw[->] (b) -- node {$\scriptstyle g$} (c);
\end{tikzpicture}\]
このとき次のような$h\colon a\rightarrow c$が存在する．
\[\begin{tikzpicture}[auto]
\node (a) at (0, 0) {$a$}; \node (b) at (1, 1) {$b$}; \node (c) at (2, 0) {$c$};
\draw[->] (a) -- node {$\scriptstyle f$} (b);
\draw[->] (b) -- node {$\scriptstyle g$} (c);
\draw[->,densely dashed] (a) -- node[swap] {$\scriptstyle h$} (c);
\end{tikzpicture}\]
\end{document}
```

### パラメーター付きの射
射を書く際に#1, #2, …, #9を使うと、その部分はパラメーターとして扱われます。
勿論これを普通に書いてしまうと、PDF上に意味不明な記述が載ってしまうため、
これは基本的にはコメント内に上記のCheckTikZDiagramを使用して記述します。
(書き方は設定2タブ内のパラメーター付きの射も参照)
```
\begin{document}
$a\in C$に対して$f_a\colon a\rightarrow K(a)$を$D$の射とする．
%CheckTikZDiagram $f_{#1}\colon #1\rightarrow K(#1)$
\[\begin{tikzpicture}[auto]
\node (a) at (0, 1) {$a$}; \node (x) at (1, 1) {$K(a)$};
\node (b) at (0, 0) {$b$}; \node (y) at (1, 0) {$K(b)$};
\draw[->] (a) -- node {$\scriptstyle f_a$} (x);
\draw[->] (b) -- node {$\scriptstyle f_b$} (y);
\end{tikzpicture}\]
\end{document}
```
