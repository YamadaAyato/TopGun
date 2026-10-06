# 現方針に合わせた整理・見直し候補

更新: 2026-10-05

## 現方針
谷を抜けてゴールへ到達すると成功。死亡・時間切れは失敗。回避・ジャスト回避・フレア迎撃・カウンターでスコアを稼ぐ。

## 削除せず残した見直し候補

| 対象 | 調査結果 | 判断候補 |
| --- | --- | --- |
| `Assets/Scripts/Player/State/PlayerStateMachine.cs` | ChangeStateの外部呼び出しが見当たらず、シーン・PrefabのスクリプトGUID参照も見当たらない。 | 旧設計の未接続の土台。現在の操作をStateへ移行しないなら将来削除候補。 |
| `Assets/Scripts/Interface/IPlayerState.cs` | 参照は上記StateMachineのみ。具象State実装が見当たらない。 | StateMachineとセットで判断。 |
| `Assets/Scripts/Enemy/EnemySpawer.cs` | 通常砲台専用のプール。SpawnTurretの外部呼び出し・シーン/PrefabからのGUID参照が見当たらない。 | 現状の配置済み敵には不要。今後の区間別生成を作る際に統合か廃止を判断。 |

`GameMode`、`GameManager`、`CombatState`、`ValleyState`、`ChaseState` に相当する実装は、現在の `Assets/Scripts` には見当たらない。古い設計資料の想定と実装を区別すること。

## 現方針に合っているため残すもの

- StageGoal / StageResultController: ゴール成功、死亡・時間切れ失敗をイベントで処理。
- StageCountDownTimer: 制限時間による失敗判定に必要。
- GameRunResult / GameRunOutcome: 成功・失敗の結果受け渡し。ゲームモードではない。
- ProjectileService / ObjectPool: 弾・爆発の共通生成に使用。
- ProjectileDirectionView / EnemyProjectileSignal: レーダーを取り除いた後も機体周囲の矢印表示に使用。

## 素材の移動先

- Materials/Environment: 水素材
- Materials/Environment/Snow: 木・岩の積雪素材
- Materials/TerrainLayers: 地形の塗り分けレイヤー
- Shaders/Environment: 川・積雪シェーダー
- Textures/Environment: 雪のテクスチャ
- Terrain: 現行谷のTerrainData
- Meshes/Environment: 川のメッシュ
- Prefabs/Enemy/EnemyBullet.prefab: 現行通常弾設定を統合。確認時の速度150。
- Prefabs/Enemy/EnemyHomingBullet.prefab: 現行ミサイル設定を統合。確認時の速度180。

## 整理完了・確認結果

- 未使用の旧谷資産33ファイルと旧生成シーン2ファイル（CanyonPreview / CanyonTerrainPreview）、空になった旧フォルダを削除。
- 現行シーン・TestGameScene・Title・Result・ビルド対象シーンの依存に削除対象がないことを確認してから削除。
- レーダーを削除し、機体周囲の矢印はProjectileDirectionViewとして保持。
- 現行シーンのMissing Scriptは0。列挙された依存ファイルの欠落なし。
- ProjectileServiceは元のEnemyBullet / EnemyHomingBulletを参照。速度は150 / 180を確認。
- 現行地形はAssets/Terrain/AlpineTerrain.assetを参照。
- TestGameSceneは参照用として保持。現行シーン名AlpineValleyPreviewは変更していない。
- 整理後の実プレイ確認は未実施。上記はUnity Editor上での参照・設定確認。