# 初回セットアップ

1. Windows 10 / 11 に .NET 8 SDK をインストールします。
2. このフォルダで `dotnet restore HorseEntryNotifier.sln` を実行します。
3. `dotnet test HorseEntryNotifier.sln` が成功することを確認します。
4. `dotnet run --project src/HorseEntryNotifier.Console -- horse add "ジェットシェヴロン"` で馬を登録します。
5. `dotnet run --project src/HorseEntryNotifier.Console -- check --dry-run` でモック通知を確認します。
6. Discord Webhookを設定したあと、`check` を実行します。

初期状態は `JraVan:UseMock=true` です。JV-Linkを使う前に、必ず `jravan-setup.md` と `jravan-implementation-notes.md` を読んでください。

## User Secrets

開発PCでは次の方法でWebhookを保存できます。

```powershell
dotnet user-secrets --project src/HorseEntryNotifier.Console set "Discord:WebhookUrl" "ここにWebhook URL"
```

この値はプロジェクトファイルには書き込まれません。環境変数 `DISCORD_WEBHOOK_URL` が設定されている場合は、そちらを最優先します。
