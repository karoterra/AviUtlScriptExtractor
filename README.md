# AviUtlScriptExtractor

AviUtlプロジェクトファイルから拡張編集で使用したスクリプトの一覧を作成するツールです。

## 動作環境

本ツールの動作には .NET Framework 4.7.2 が必要です。

## 対応バージョン

本ツールは以下のバージョンで作成したプロジェクトファイルにて動作確認しています。

- AviUtl version 1.10,
- 拡張編集プラグイン version 0.92

## 使い方

AviUtlプロジェクトファイル（*.aup）を`AviUtlScriptExtractor.exe`にドラッグ&ドロップしてください。
プロジェクトファイルと同じ場所に`<元のファイル名>_script.csv`が生成されます。

### setting.json

実行ファイルと同じ場所にある `setting.json` にスクリプトに関する情報を記載しておくことで、その情報も csv に出力します。
`setting.json` は以下のような形式で記述してください。
サンプルとして付属している `setting.json` も合わせて参考にしてください。

```json
{
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

## 更新履歴

更新履歴は [CHANGELOG](CHANGELOG.md) を参照してください。

## ライセンス

このソフトウェアは、MITライセンスのもとで公開されます。
詳細は [LICENSE](LICENSE) を参照してください。

使用したライブラリ等については[CREDITS](CREDITS.md) を参照してください。
