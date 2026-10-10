# 共通規約リファクタリング：人間レビュー待ち

規約: https://www.notion.so/Unity-C-3f2ed4d1f2c881beb3c0e0b4e86d7aa6
2026-10-10取得。作業開始時のGit差分なし。コミット・ステージングは行っていません。
作者はGitの作者名だけでは断定せず、会話上で作成履歴を把握している機能を中心に選定しました。

## 変更したファイル

すべて Assets/Scripts 以下。

| ファイル | 整理内容 |
| --- | --- |
| Player/AircraftCollisionGuard.cs | 移動制約を、めり込み解消と移動経路の掃引に分割。判定値を定数化し、密集時の再取得理由とInspector説明を追加 |
| Effects/FlightWindParticles.cs | 粒子上限・速度・生成範囲・消失距離をInspector設定に変更（従来値を初期値として維持） |
| Effects/WingtipCondensation.cs | 軌跡消去の重複を統合。旋回判定の平滑化係数と解除係数をInspector設定に変更 |
| Effects/JustEvadeMonochrome.cs | 描画設定のTooltip、白黒演出の定数名、空行を整理 |
| System/AirDefenseZone.cs | 監視対象のTooltipを追加 |
| System/StageResultController.cs | 参照のHeader、遷移先のTooltip、メソッド間の空行を整理 |
| UI/ResultScreenController.cs | 遷移先と文字色のTooltipを追加 |
| UI/EnemyProjectileSignal.cs | 公開HashSetフィールドを読み取り専用のコレクションPropertyへ変更。Summary・Tooltipを追加 |
| Player/PlayerAirCraftController.cs | 既存コードのFowardMovementをForwardMovementへ修正 |
| System/StageCountDownTimer.cs | 既存コードの_isfinishedを_isFinishedへ修正。privateメソッドをライフサイクル後へ移動 |

## 確認済み

- Assembly-CSharp.csprojのC#ビルド成功：警告0、エラー0。
- git diff --check成功。
- 既存SerializeFieldの名前・型・初期値は維持。レビュー対応で風・飛行機雲の調整用SerializeFieldを追加。
- Scene、Prefab、metaの変更なし。既存GUIDは維持。
- EnemyProjectileSignal.Activeの外部参照はProjectileDirectionViewの列挙のみ。新しい型でコンパイル成功。
- private名の修正は対象クラス内の参照を更新。Assets内のコード・Scene・Prefabに旧名の他参照なし。
- 衝突検査の順序、最大3回の滑り処理、閾値、死亡イベントへの経路を差分で確認。

## レビュー時の確認項目

- [ ] 衝突判定の分割と命名が読みやすいか。
- [ ] Activeが読み取り専用の公開APIとなる変更を受け入れるか。
- [ ] 通常飛行・回避とも、正面衝突は失敗し、浅い接触は滑るか。
- [ ] 翼端の雲とジャスト回避の白黒演出が従来どおりか。
- [ ] ゴール成功／死亡・時間切れ・防空圏失敗とリザルト表示が従来どおりか。

Unity Editorは起動しておらず、Editorでの再インポート、Play確認、実際のシーン参照検証は未実施です。
全スクリプトの完全な規約準拠を保証する作業ではありません。作者不明のクラス名・SerializeField名の広範囲な変更、ゲーム仕様変更は含みません。

変更内容のレビューをお願いします

比較元コミット: 4aef2a4c384e49ca58703f03049b150f10df949b

## 追加レビュー対応（2026-10-10）

- 最新Notion規約の明示型・for/foreachの波括弧を適用。
- 10ファイル内で処理のまとまりを区切るコメント前・ブロック後の空行を整理。
- 存在確認が必要な機体操作・回避・爆発色コンポーネントにTryGetComponentを使用。
- RequireComponentで要求する必須参照と、親階層を検索するGetComponentInParentは維持。
- 風の固定フェードなど、ParticleSystemへの初期化代入自体は引き続き残る。Inspector側設定へ移すには見た目を引き継ぐシーン設定が別途必要。
- C#ビルド警告0・エラー0。Unity Play確認と追加Inspector値の実シーン検証は未実施。

変更内容のレビューをお願いします

## コメント空行・定数の再確認

- レビュー対象10ファイルを点検。連続コメント内とブロック先頭コメント前の不要な空行をPlayerAirCraftController、StageCountDownTimerで削除。コードからコメントのまとまりへ移る空行は維持。
- Assets/Scripts全体のconst/static readonlyを検索し、全宣言を確認。
- JustEvadeMonochromeのVolume優先度1000は他のVolumeとの調整値なので、_volumePriorityとしてInspectorへ公開。既定値は維持。
- MONOCHROME_SATURATION=-100：完全な白黒を表す固定値として維持。
- MIN_FADE_DURATION=0.01：除算の分母を正に保ち、極端に短いフェードを制限する内部の下限として維持。
- QUERY_CAPACITY=32：共通の検索バッファ容量。満杯なら全件取得に切り替わるため、ゲーム上の衝突数制限ではない。内部定数として維持。
- MAX_SLIDE_STEPS=3：1回の移動で行う滑り補正の処理上限として維持。
- MIN_MOVEMENT_SQUARED=0.000001：極小移動を打ち切る数値的な閾値として維持。
- EnemyProjectileSignalの_activeは定数ではなく可変な集合。readonlyは集合の参照の再代入だけを防ぐため、そのまま維持。
- C#ビルド警告0・エラー0、git diff --check成功。Unity Play確認は未実施。

変更内容のレビューをお願いします