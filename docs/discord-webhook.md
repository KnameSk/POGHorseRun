# Discord Webhookの準備

1. 通知先Discordサーバーで「チャンネルの編集」を開きます。
2. 「連携サービス」→「ウェブフック」→「新しいウェブフック」を選びます。
3. 通知先チャンネルを選び、Webhook URLをコピーします。
4. PowerShellで現在のユーザー用環境変数を設定します。

```powershell
[Environment]::SetEnvironmentVariable(
  "DISCORD_WEBHOOK_URL",
  "コピーしたURL",
  "User"
)
```

新しいPowerShellを開き直してから実行してください。Webhook URLは投稿権限そのものです。画面共有、ログ、README、Issue、Gitコミットへ貼らないでください。漏えいした場合はDiscord側でWebhookを削除し、作り直します。

通知せず表示だけ確認する場合は `check --dry-run` を使います。`Discord:Enabled=false` の場合も外部送信せず、通知履歴を保存しません。
