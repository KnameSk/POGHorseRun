# Windowsタスクスケジューラ設定

最初に手動で `scripts/run-check.ps1 -DryRun` が成功することを確認してください。次に通常のPowerShellで、以下を実行します。

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-task-scheduler.ps1 -WhatIf
powershell -ExecutionPolicy Bypass -File .\scripts\install-task-scheduler.ps1
```

既定では次のトリガーを登録します。

- 木曜 16:10
- 金曜 10:10
- 土曜・日曜 10:10
- 土曜・日曜 07:00

時刻は引数で変更できます。

```powershell
.\scripts\install-task-scheduler.ps1 `
  -ThursdayTime "16:15" `
  -FridayTime "10:15" `
  -WeekendTime "10:15" `
  -RaceDayMorningTime "07:30"
```

実行結果は `logs/` に保存されます。タスクが動かない場合は、タスク履歴、最終実行結果、ユーザー環境変数 `DISCORD_WEBHOOK_URL`、JV-Linkの利用可否を確認してください。ログオンしていない状態でも動かす設定へ変更する場合、Windowsが資格情報を求めることがあります。パスワードをスクリプトへ書かないでください。
