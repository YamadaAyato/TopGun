# 既存クラスの名前・メンバー順序修正

## クラス名とファイル名

| 旧名 | 新名 |
| --- | --- |
| ScoreViwer | ScoreViewer |
| ScorePopupSpawer | ScorePopupSpawner |
| EvationGauge | EvasionGauge |
| EnemySpawer | EnemySpawner |
| CameraSwicher | CameraSwitcher |
| BulletDetectingFlareColider | BulletDetectingFlareCollider |
| PlayerAirCraftController | PlayerAircraftController |

同じmetaを新しいファイル名へ移動し、GUIDを維持。各クラスにMovedFrom属性を追加。
参照する機体移動・回避・演出・UIコードも更新。
WindowsのGitでは、大文字小文字だけの改名はファイル移動として表示されない場合があります。

## その他の綴り

- RecorverCharge → RecoverCharge（呼び出し元も更新）
- BallelRolling → BarrelRolling
- TryStartBallelRollEvade → TryStartBarrelRollEvade
- _currntEvadeType → _currentEvadeType
- SceneChenge → HandleClicked（クリックイベントのハンドラ）

## メンバー順序

26クラスでイベント・プロパティ・フィールド・publicメソッド・ライフサイクル・ハンドラ・privateメソッド・privateネスト型を規約順に整理。
メソッド本文は変更せず、コメントと属性も宣言と一緒に移動。
PlayerInputHandlerのregionは移動後に範囲が分断されるのを避けて削除。
EnemySpawnerの省略されていたprivateを明示し、クラスSummaryを追加。

## 確認

- Assets/Scripts全72ファイルを作業開始時のスナップショットと構文比較。
- 意図した識別子の改名を除き、全メソッドのトークン、全フィールド宣言、フィールド初期化順が一致。
- 全metaファイルの内容が一致（GUID維持）。
- 構文によるメンバー順序の再検査で残候補なし。
- Assembly-CSharpビルド成功：警告0・エラー0。
- git diff --check成功。
- 旧メソッド名等をAssets内で検索し、残る呼び出し・UnityEventの文字列参照なし。

## 未確認・対象外

Unity Editorは未起動のため、再インポート後のMissing ScriptやPlay動作は未確認。
Scene・Prefabは直接編集していません。
_ballelRollSpline、_timeDlicon、_counterColdown、_exsampleTransform等、既存のInspector保存名はPrefabの上書き設定への影響を避けて維持。
GameObject名、ログ表示文字列、今回依頼外の処理変更は対象外です。
未コミットの状態でレビューを待ちます。

変更内容のレビューをお願いします