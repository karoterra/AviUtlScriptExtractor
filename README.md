# AviUtlScriptExtractor

[AviUtl](http://spring-fragrance.mints.ne.jp/aviutl/)
プロジェクトファイルから拡張編集で使用したスクリプトの一覧を作成するツールです。

## インストール

[Releases](https://github.com/karoterra/AviUtlScriptExtractor/releases)
から最新版の ZIP ファイルをダウンロードし、好きな場所に展開してください。

- `AviUtlScriptExtractor-<バージョン>-win-x64-fd.zip`
  - .NET 6 ランタイムをインストール済みの場合
- `AviUtlScriptExtractor-<バージョン>-win-x64-sc.zip`
  - .NET 6 ランタイムをインストールせずに使う場合
  - よく分からないがとにかく使いたい場合

アンインストール時には展開したフォルダを削除してください。

## 対応バージョン

本ツールは以下のバージョンで作成したプロジェクトファイルにて動作確認しています。

- AviUtl version 1.10,
- 拡張編集プラグイン version 0.92

## 使い方

### コンソールを開かない場合
AviUtl プロジェクトファイル（*.aup）を`AviUtlScriptExtractor.exe`にドラッグ&ドロップしてください。
プロジェクトファイルと同じ場所に`<元のファイル名>_script.csv`が生成されます。

### コンソールから使う場合
```
> AviUtlScriptExtractor --help
AviUtlScriptExtractor 0.2.0
Copyright © 2021 karoterra
USAGE:
通常:
  AviUtlScriptExtractor C:\path\to\project.aup
出力先を指定:
  AviUtlScriptExtractor --out C:\path\to\scripts.csv C:\path\to\project.aup
ヘッダーを出力しない:
  AviUtlScriptExtractor --header Off C:\path\to\project.aup
スクリプト名と使用回数のみ出力:
  AviUtlScriptExtractor --col Script,Count C:\path\to\project.aup
使用回数(降順)、スクリプト名(昇順)でソート:
  AviUtlScriptExtractor --sort COUNT,script C:\path\to\project.aup

  -o, --out            出力するcsvファイルのパス。

  --header             ヘッダーの出力指定(on|off|multi)。on: ヘッダーを出力する。off: ヘッダーを出力しない。multi: 複数列出力する場合のみヘッダーを出力する。

  --col                出力する列名をカンマ区切りで指定(script|filename|type|author|nicoid|url|comment|count)。

  --sort               ソートする列をカンマ区切りで指定。大文字にすると降順。

  --help               Display this help screen.

  --version            Display version information.

  filename (pos. 0)    Required. aupファイルのパス。
```

## 設定

実行ファイルと同じ場所にある `setting.json` を書き換えることで csv の出力の仕方を変更することができます。
コマンドライン引数を指定した場合はその値が優先されます。
書式は以下の通りです。

```json
{
    "header": "on",
    "columns": [
        "script",
        "filename",
        "type",
        "author",
        "nicoid",
        "url",
        "comment",
        "count"
    ],
    "sort": [
        "COUNT",
        "script"
    ],
    "authors": [
        {
            "name": "スクリプトの作者1",
            "scripts": [
                {
                    "name": "スクリプトファイル名.anm",
                    "nicoid": "ニコニコの作品ID(例:sm38814110、省略可)",
                    "url": "配布URLなど(省略可)",
                    "comment": "何か備考があればここに(省略可)",
                    "dependencies": [
                        "rikky_module等スクリプトの動作に必要なものがあれば",
                        "省略可"
                    ]
                }
            ]
        }
    ]
}
```

- `"header"` : ヘッダーを出力するかどうか指定する
  - `"on"` : ヘッダーを出力する
  - `"off"` : ヘッダーを出力しない
  - `"multi"` : 複数列出力する場合にはヘッダーを出力する
- `"columns"` : 出力する列を指定する
  - `"script"` : スクリプト名
  - `"filename"` : スクリプトファイル名
  - `"type"` : スクリプトの種類
  - `"author"` : `authors[].name` で指定したスクリプトの作者
  - `"nicoid"` : `authors[].scripts[].nicoid` で指定したニコニコ作品ID
  - `"url"` : `authors[].scripts[].url` で指定したURL
  - `"comment"` : `authors[].scripts[].comment` で指定したコメント
  - `"count"` : そのスクリプトが aup ファイル内で使用された回数
- `"sort"` : ソートする列の名前を指定する。小文字で書くと昇順、大文字で書くと降順でソートする。
- `"authors"` : csv に出力するためのスクリプトに関する情報を書く場所。

## 更新履歴

更新履歴は [CHANGELOG](CHANGELOG.md) を参照してください。

## ライセンス

このソフトウェアは、MITライセンスのもとで公開されます。
詳細は [LICENSE](LICENSE) を参照してください。

使用したライブラリ等については[CREDITS](CREDITS.md) を参照してください。
