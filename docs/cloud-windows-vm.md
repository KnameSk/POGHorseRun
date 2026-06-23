# Windows VM / GitHub Actionsで動かす場合

## 推奨: Windows PCまたはWindows VM

JV-Linkを公式手順でインストールし、JRA-VANの利用設定を行ったWindows環境で、タスクスケジューラから `scripts/run-check.ps1` を実行します。VMは時刻を `Asia/Tokyo` 相当に合わせ、スリープを避け、Windows Update後の再起動も考慮します。

## Windows self-hosted runner

JV-Linkを導入した専用WindowsマシンにGitHub Actions self-hosted runnerを置き、scheduleで実行できます。ただし、次に注意してください。

- runnerはリポジトリのコードを実行できる強い権限を持ちます。公開リポジトリのPRで実JV-Linkジョブを動かさないでください。
- Webhook URLとJRA-VANキーはGitHub Secretsまたはrunner側の安全な環境変数に保存します。
- scheduleはUTC基準です。日本時間へ換算します。
- 同時実行を防ぎ、JV-LinkのCOM呼び出しを直列化します。

通常のGitHub-hosted runnerにはJV-Linkや利用キーがありません。そのため `.github/workflows/test.yml` はモックでビルド・単体テストだけを行い、実データ取得やDiscord送信は行いません。
