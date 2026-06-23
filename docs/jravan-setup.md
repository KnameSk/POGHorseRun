# JRA-VAN DataLab / JV-Linkの準備

## 必要なもの

- JRA-VAN DataLabの利用契約
- Windows版JV-Link
- JRA-VAN SDK、開発ガイド、データ仕様書

JV-Linkのインストールとサービスキー設定は、JRA-VAN公式の案内に従ってください。本リポジトリはSDKや仕様書を再配布しません。確認用資料は `docs/jravan/` に置けますが、このフォルダの中身は `.gitignore` で除外されています。

## 実連携を有効にする前に

現在の `JraVanClient` は、仕様を推測しないための明示的なスタブです。`JraVan:UseMock=false` にすると、Adapterが未実装であることを示す例外を返します。

実装時は次をJRA-VAN SDKで確認します。

- JV-Link COMの初期化、サービスキー、データラボ接続の手順
- 出馬表に対応するデータ種別と取得タイミング
- レコード読み取りループの戻り値、EOF、エラーコード
- 競走識別キー（年月日、場、回、日、レース番号）の正式な組み立て
- 馬、騎手、枠番、馬番、トラック、距離、条件の正式なフィールド
- コード表から表示名への変換
- JV-LinkのbitnessとCOM参照の配布条件

詳しい実装境界は `jravan-implementation-notes.md` を参照してください。
