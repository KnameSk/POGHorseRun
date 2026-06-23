# 競馬出走通知 Discord Notifier

登録した競走馬がJRAの出馬表に載ったとき、Discord Webhookへ自動通知するWindows向けコンソールアプリです。C# / .NET 8、SQLite、JV-Link差し替えAdapter、モックデータで構成しています。

初期状態はモックです。本物のJRA-VAN SDKがないPCでも、馬の登録、出馬表との照合、重複防止、通知内容のプレビュー、テストまで確認できます。実JV-Link部分は仕様を推測せず、専用Adapterのスタブとして分離しています。

## できること

- 通知対象馬の登録、無効化、一覧表示
- 指定期間の出馬表チェック
- 馬名の全角・半角、空白、大小文字の揺れを吸収した照合
- Discord Embed通知（日付、競馬場、レース、条件、距離、発走、騎手、枠番・馬番、URL、通知日時）
- SQLiteによる同一馬・同一レースの重複通知防止
- 騎手変更、馬番更新を別通知キーとして扱える拡張構造
- Discordへ送らないdry-run
- Windowsタスクスケジューラ用スクリプト
- JV-Link不要のモックテストとGitHub Actions CI

## 必要なもの

- Windows 10 / 11
- .NET 8 SDK
- Discord Webhook URL
- 実データを使う場合のみ、JRA-VAN DataLab会員契約、JV-Link、JRA-VAN SDK・仕様書

## 最短セットアップ

PowerShellでこのフォルダへ移動し、次を実行します。

```powershell
dotnet restore HorseEntryNotifier.sln
dotnet test HorseEntryNotifier.sln
dotnet run --project src/HorseEntryNotifier.Console -- horse add "ジェットシェヴロン" --memo "POG指名馬"
dotnet run --project src/HorseEntryNotifier.Console -- horse list
dotnet run --project src/HorseEntryNotifier.Console -- check --dry-run
```

サンプルには2026年6月27日の「ジェットシェヴロン」が入っています。別の日付で試す場合は範囲を指定します。

```powershell
dotnet run --project src/HorseEntryNotifier.Console -- check --from 2026-06-22 --to 2026-06-29 --dry-run
```

より詳しい手順は [docs/setup.md](docs/setup.md) を参照してください。

## 馬を管理する

```powershell
dotnet run --project src/HorseEntryNotifier.Console -- horse add "アステリアドンナ"
dotnet run --project src/HorseEntryNotifier.Console -- horse add "ジェットシェヴロン" --memo "POG指名馬"
dotnet run --project src/HorseEntryNotifier.Console -- horse remove "アステリアドンナ"
dotnet run --project src/HorseEntryNotifier.Console -- horse list
```

`remove` は履歴を壊さず `Enabled=false` にします。同じ馬を再度 `add` すると有効に戻ります。データベースは既定で `horse-entry-notifier.db` です。

## Discord Webhookを設定する

Webhookの作成方法は [docs/discord-webhook.md](docs/discord-webhook.md) にあります。推奨はユーザー環境変数です。

```powershell
[Environment]::SetEnvironmentVariable("DISCORD_WEBHOOK_URL", "Webhook URL", "User")
```

設定後、新しいPowerShellで次を実行します。

```powershell
dotnet run --project src/HorseEntryNotifier.Console -- check
```

設定の優先順位は `DISCORD_WEBHOOK_URL` → .NET User Secrets → `appsettings.json` です。`appsettings.json` に本物のURLを保存しないでください。通知を止める場合は `Discord:Enabled=false`、常にプレビューする場合は `App:DryRun=true` にします。

## JRA-VAN / JV-Link

準備は [docs/jravan-setup.md](docs/jravan-setup.md) を参照してください。SDK・仕様書は `docs/jravan/` へ配置できますが、Git対象外です。

実Adapterは `IJraVanClient` の実装 `JraVanClient` に閉じ込めています。現時点ではSDKの型やレコードを推測しないスタブです。実装前に確認する仕様とマッピング方針は [docs/jravan-implementation-notes.md](docs/jravan-implementation-notes.md) に明記しています。`JraVan:UseMock=false` はAdapter実装後に切り替えてください。

JRA公式サイトの個別出馬表は2026年6月22日時点で `JRADB/accessD.html?CNAME=pw01dde.../...` 形式ですが、全要素を推測生成しません。`RaceUrl:Template` は `{raceId}`、`{raceDate}`、`{year}`、`{racecourseCode}`、`{raceNumber}`、`{horseId}` を使えます。必要情報が揃わない既定状態では、JRA公式の出馬表開催選択ページへ案内します。JRA公式サイトをスクレイピングしません。

## 定期実行

手動確認:

```powershell
.\scripts\run-check.ps1 -DryRun
```

タスク登録:

```powershell
.\scripts\install-task-scheduler.ps1 -WhatIf
.\scripts\install-task-scheduler.ps1
```

時刻変更や確認方法は [docs/windows-task-scheduler.md](docs/windows-task-scheduler.md) を参照してください。Windows VMとself-hosted runnerの注意点は [docs/cloud-windows-vm.md](docs/cloud-windows-vm.md) にあります。

## 通知が来ないとき

1. `check --dry-run` で登録馬とモック出馬表が一致するか確認します。
2. `horse list` で馬が「有効」か確認します。
3. 指定期間に対象レース日が含まれるか確認します。
4. `DISCORD_WEBHOOK_URL` を新しいPowerShellから参照できるか確認します。値そのものを画面共有やログへ出さないでください。
5. `Discord:Enabled` と `App:DryRun` を確認します。
6. 二回目以降は重複防止で送られません。`Notifications` テーブルをむやみに削除せず、まず処理結果の「重複除外」を確認します。
7. 実連携ではJV-Link、利用キー、メンテナンス時間、SDK bitness、Adapter実装を確認します。
8. タスク実行時は `logs/` とタスクスケジューラの最終実行結果を確認します。

## 秘密情報

Webhook URL、JRA-VAN利用キー、個人情報をコード、README、Issue、ログへ書かないでください。`.env`、DB、User Secrets、`docs/jravan/`、ローカル設定は `.gitignore` で保護していますが、コミット前に必ず `git diff --cached` も確認してください。

## GitHub Actions

通常のGitHub-hosted runnerではJV-Linkを安定利用できないため、CIはモックのビルドとテストだけです。実運用は次のどちらかを選びます。

1. Windows PC / Windows VM + タスクスケジューラ
2. JV-Link導入済みWindows self-hosted runner + GitHub Actions schedule

self-hosted runnerへ秘密情報を保存する場合はGitHub Secretsまたはrunner側の安全な環境変数を使い、公開PRから実データジョブを起動しないでください。

## テスト

```powershell
dotnet test HorseEntryNotifier.sln
```

馬名正規化、出馬表照合、重複通知、騎手変更候補、Discord payload、URL生成、モックJSON読み込みをテストします。実DiscordとJV-Linkには接続しません。

## 将来の拡張

- 特別登録通知
- 枠順確定通知の詳細化
- 騎手変更通知
- 出走取消通知
- 複数Discordチャンネル対応
- POGグループ別通知
- SDK環境専用のJV-Link契約テスト
