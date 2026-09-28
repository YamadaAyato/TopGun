# リザルト実装：コードレビュー用

追加3ファイルの全文と、既存4ファイルの変更差分。コメント・メソッド順を整理し、リザルト文言をInspectorで設定できるよう更新済み。

## StageResultController.cs

[元ファイル](C:/TopGun/Assets/Scripts/System/StageResultController.cs)

```csharp
using UnityEngine;

/// <summary>
///     死亡・時間切れの通知を受け、リザルトシーンへの遷移を管理するクラス
/// </summary>
public class StageResultController : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private StageCountDownTimer _timer;
    [SerializeField] private TimeDilationController _timeDilation;
    [SerializeField] private string _resultSceneName = "Result";

    private GameRunOutcome _pendingOutcome;
    private bool _isTransitioning;

    private void OnEnable()
    {
        _playerHealth.OnDied += HandleDeath;
        _timer.OnTimeUp += HandleTimeUp;
    }

    private void Start()
    {
        GameRunResult.Clear();
        // 常駐するScoreManagerも、次のプレイでは0点から始める。
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
    }

    private void OnDisable()
    {
        if (_playerHealth != null) _playerHealth.OnDied -= HandleDeath;
        if (_timer != null) _timer.OnTimeUp -= HandleTimeUp;
    }

    private void LateUpdate()
    {
        if (_isTransitioning || _pendingOutcome == GameRunOutcome.None) return;
        _isTransitioning = true;

        // 同じフレームの時間切れと死亡は、死亡を優先する。
        var outcome = _playerHealth.IsDead ? GameRunOutcome.Failure : _pendingOutcome;
        _timer.StopCountDown();
        if (_timeDilation != null) _timeDilation.Stop();
        GameRunResult.Set(outcome);
        SceneLoader.LoadScene(_resultSceneName);
    }

    /// <summary>
    ///     死亡通知を受け、失敗の結果を保持する
    /// </summary>
    private void HandleDeath()
    {
        _pendingOutcome = GameRunOutcome.Failure;
    }

    /// <summary>
    ///     時間切れ通知を受け、結果が未設定の場合に成功の結果を保持する
    /// </summary>
    private void HandleTimeUp()
    {
        if (_pendingOutcome == GameRunOutcome.None)
            _pendingOutcome = GameRunOutcome.Success;
    }
}
```

## GameRunResult.cs

[元ファイル](C:/TopGun/Assets/Scripts/System/GameRunResult.cs)

```csharp
using UnityEngine;

/// <summary>
///     ゲームの終了結果
/// </summary>
public enum GameRunOutcome
{
    None,
    Success,
    Failure
}

/// <summary>
///     ゲームシーンからリザルトシーンへ渡す結果を保持するクラス
/// </summary>
public static class GameRunResult
{
    /// <summary> 現在のゲーム結果 </summary>
    public static GameRunOutcome Outcome { get; private set; }

    /// <summary>
    ///     ゲームの結果を未設定の状態に戻す
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        Outcome = GameRunOutcome.None;
    }

    /// <summary>
    ///     リザルトシーンに渡すゲームの結果を設定する
    /// </summary>
    /// <param name="outcome"> 終了したゲームの結果 </param>
    public static void Set(GameRunOutcome outcome)
    {
        Outcome = outcome;
    }
}
```

## ResultScreenController.cs

[元ファイル](C:/TopGun/Assets/Scripts/UI/ResultScreenController.cs)

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     リザルトの表示とタイトルシーンへの遷移を管理するクラス
/// </summary>
public class ResultScreenController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private TMP_Text _outcomeText;
    [SerializeField] private UnityEngine.UI.Button _titleButton;

    [Header("シーン設定")]
    [SerializeField] private string _titleSceneName = "Title";

    [Header("表示設定")]
    [SerializeField, Tooltip("成功時に表示する文字")] private string _successText = "成功";
    [SerializeField, Tooltip("失敗時に表示する文字")] private string _failureText = "失敗";
    [SerializeField, Tooltip("結果が未設定の場合に表示する文字")] private string _defaultText = "リザルト";
    [SerializeField] private Color _successColor = new Color(0.35f, 0.9f, 0.75f);
    [SerializeField] private Color _failureColor = new Color(1f, 0.4f, 0.4f);

    private bool _isLeaving;

    /// <summary>
    ///     ボタンの連打を防止し、タイトルシーンへ遷移する
    /// </summary>
    public void ReturnToTitle()
    {
        if (_isLeaving) return;
        _isLeaving = true;
        _titleButton.interactable = false;
        SceneLoader.LoadScene(_titleSceneName);
    }

    private void OnEnable()
    {
        _titleButton.onClick.AddListener(ReturnToTitle);
    }

    private void Start()
    {
        UpdateOutcomeText();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_titleButton.gameObject);
    }

    private void OnDisable()
    {
        _titleButton.onClick.RemoveListener(ReturnToTitle);
    }

    /// <summary>
    ///     ゲームの結果に合わせて表示文字と色を更新する
    /// </summary>
    private void UpdateOutcomeText()
    {
        switch (GameRunResult.Outcome)
        {
            case GameRunOutcome.Success:
                _outcomeText.text = _successText;
                _outcomeText.color = _successColor;
                break;
            case GameRunOutcome.Failure:
                _outcomeText.text = _failureText;
                _outcomeText.color = _failureColor;
                break;
            default:
                _outcomeText.text = _defaultText;
                break;
        }
    }
}
```

## PlayerHealth.cs の変更

[ファイル全文](C:/TopGun/Assets/Scripts/Player/PlayerHealth.cs)

```diff
diff --git a/Assets/Scripts/Player/PlayerHealth.cs b/Assets/Scripts/Player/PlayerHealth.cs
index f070583..0248e01 100644
--- a/Assets/Scripts/Player/PlayerHealth.cs
+++ b/Assets/Scripts/Player/PlayerHealth.cs
@@ -8,12 +8,15 @@ public class PlayerHealth : MonoBehaviour, IDamageable
 {
     /// <summary> HP変化時のイベント </summary>
     public event Action<int, int> OnHealthChanged;
+    /// <summary> 死亡時に一度だけ発火するイベント </summary>
+    public event Action OnDied;
 
     /// <summary>
-    /// ダメージを与えられるか返す
-    /// 無敵がtrueの時Hitできるため逆を返す
+    ///     無敵でも死亡状態でもない場合、ダメージを受けられることを返す
     /// </summary>
-    public bool CanBeHit => !_isInvincible;
+    public bool CanBeHit => !_isInvincible && !_isDead;
+    /// <summary> 死亡しているか </summary>
+    public bool IsDead => _isDead;
     /// <summary> 現在のHP </summary>
     public int CurrentHealth => _currentHealth;
     /// <summary> 最大HP </summary>
@@ -22,36 +25,52 @@ public class PlayerHealth : MonoBehaviour, IDamageable
     [SerializeField, ReadOnly, Tooltip("現在無敵がどうか")] private bool _isInvincible;
     [SerializeField, ReadOnly] private int _currentHealth;
     [SerializeField] private int _maxHealth;
+    private bool _isDead;
 
-    /// <summary> 無敵判定を切り替える </summary>
-    /// <param name="value"></param>
+    /// <summary>
+    ///     無敵判定を切り替える
+    /// </summary>
+    /// <param name="value"> 無敵状態にする場合はtrue </param>
     public void SetInvincible(bool value)
     {
         _isInvincible = value;
     }
 
+    /// <summary>
+    ///     ダメージを適用し、HPの変化や死亡を通知する
+    /// </summary>
+    /// <param name="damage"> 受けるダメージ量 </param>
     public void TakeDamage(int damage)
     {
-        if (_isInvincible) return;
+        if (!CanBeHit || damage <= 0) return;
         _currentHealth -= damage;
         _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
         float intensity = Mathf.Clamp01((float)damage / 30f);
         GameEvents.RaisePlayerHit(intensity);
 
-        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
         Debug.Log($"プレイヤーに{damage}ダメージ、現在HP{_currentHealth}");
 
         if (_currentHealth <= 0)
         {
             Die();
         }
+        else
+        {
+            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
+        }
     }
 
+    /// <summary>
+    ///     HPを0にし、死亡イベントを一度だけ発火する
+    /// </summary>
     public void Die()
     {
+        if (_isDead) return;
+        _isDead = true;
         _currentHealth = 0;
         OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
         Debug.Log("プレイヤー死亡");
+        OnDied?.Invoke();
     }
 
     private void Awake()
@@ -64,6 +83,7 @@ public class PlayerHealth : MonoBehaviour, IDamageable
 
     private void OnDisable()
     {
-        PlayerLocator.Instance.Unregister();
+        if (PlayerLocator.Instance != null && PlayerLocator.Instance.PlayerHealth == this)
+            PlayerLocator.Instance.Unregister();
     }
 }
```

## EnemyBase.cs の変更

[ファイル全文](C:/TopGun/Assets/Scripts/Enemy/EnemyBase.cs)

```diff
diff --git a/Assets/Scripts/Enemy/EnemyBase.cs b/Assets/Scripts/Enemy/EnemyBase.cs
index 930e180..1cecd3e 100644
--- a/Assets/Scripts/Enemy/EnemyBase.cs
+++ b/Assets/Scripts/Enemy/EnemyBase.cs
@@ -10,18 +10,33 @@ public abstract class EnemyBase : MonoBehaviour, IDamageable
     [SerializeField, ReadOnly] private int _currentHp;
     [SerializeField] private int _maxHp;
 
-     private Action<EnemyBase> _onRelease;
+    private Action<EnemyBase> _onRelease;
+    private bool _isDead;
 
-    /// <summary> スポーン時の初期化処理をする </summary>
+    /// <summary> 現在のHP </summary>
+    public int CurrentHealth => _currentHp;
+    /// <summary> 最大HP </summary>
+    public int MaxHealth => _maxHp;
+
+    /// <summary>
+    ///     スポーン時にHPと死亡状態を初期化する
+    /// </summary>
     /// <param name="onRelease"> どう戻すかの関数 </param>
     public void Spawn(Action<EnemyBase> onRelease)
     {
         _onRelease = onRelease;
         _currentHp = _maxHp;
+        _isDead = false;
+        OnSpawned();
     }
 
+    /// <summary>
+    ///     ダメージを適用し、HPがなくなった場合は死亡処理をする
+    /// </summary>
+    /// <param name="damage"> 受けるダメージ量 </param>
     public void TakeDamage(int damage)
     {
+        if (_isDead || damage <= 0) return;
         _currentHp -= damage;
         Debug.Log($"{this.name} took {damage} damage. Current HP: {_currentHp}/{_maxHp}");
         if (_currentHp <= 0)
@@ -30,8 +45,13 @@ public abstract class EnemyBase : MonoBehaviour, IDamageable
         }
     }
 
+    /// <summary>
+    ///     爆発とスコア加算を行い、自身を破棄する
+    /// </summary>
     public virtual void Die()
     {
+        if (_isDead) return;
+        _isDead = true;
         _currentHp = 0;
         //Release();
         ProjectileService.Instance.SpawnExplosion(ExplosionType.Big, this.transform);
@@ -39,6 +59,11 @@ public abstract class EnemyBase : MonoBehaviour, IDamageable
         Destroy(gameObject);
     }
 
+    protected virtual void Awake()
+    {
+        _currentHp = _maxHp;
+    }
+
     /// <summary>
     ///     自身が役目を終えたことを通知し、
     ///     生成時に渡された解放コールバックを呼び出す。
@@ -48,6 +73,8 @@ public abstract class EnemyBase : MonoBehaviour, IDamageable
         _onRelease?.Invoke(this);
     }
 
-    /// <summary> スポーン時のイベント </summary>
+    /// <summary>
+    ///     スポーン時に派生クラス固有の初期化処理をする
+    /// </summary>
     protected virtual void OnSpawned() { }
 }
```

## TurretEnemyBase.cs の変更

[ファイル全文](C:/TopGun/Assets/Scripts/Enemy/TurretEnemyBase.cs)

```diff
diff --git a/Assets/Scripts/Enemy/TurretEnemyBase.cs b/Assets/Scripts/Enemy/TurretEnemyBase.cs
index b7d9f8f..d036205 100644
--- a/Assets/Scripts/Enemy/TurretEnemyBase.cs
+++ b/Assets/Scripts/Enemy/TurretEnemyBase.cs
@@ -16,6 +16,30 @@ public abstract class TurretEnemyBase : EnemyBase
     protected float _timer;
     protected EnemyShooter _shooter;
 
+    protected override void Awake()
+    {
+        base.Awake();
+        _shooter = GetComponent<EnemyShooter>();
+    }
+
+    protected virtual void Update()
+    {
+        _timer += Time.deltaTime;
+
+        // 一定時間ごとにプレイヤーを探して撃つ
+        if (_timer > _fireInterval)
+        {
+            if (TryGetPlayer(out Transform player) &&
+                IsPlayerInRange(player) &&
+                IsPlayerInFov(player) &&
+                HasLineOfSight(player))
+            {
+                FireAtPlayer(player);
+                _timer = 0f;
+            }
+        }
+    }
+
     protected override void OnSpawned()
     {
         base.OnSpawned();
@@ -83,27 +107,4 @@ public abstract class TurretEnemyBase : EnemyBase
         }
         return true;
     }
-
-    protected virtual void Awake()
-    {
-        _shooter = GetComponent<EnemyShooter>();
-    }
-
-    protected virtual void Update()
-    {
-        _timer += Time.deltaTime;
-
-        // 一定時間ごとにプレイヤーを探して撃つ
-        if (_timer > _fireInterval)
-        {
-            if (TryGetPlayer(out Transform player) &&
-                IsPlayerInRange(player) &&
-                IsPlayerInFov(player) &&
-                HasLineOfSight(player))
-            {
-                FireAtPlayer(player);
-                _timer = 0f;
-            }
-        }
-    }
 }
```

## TimeDilationController.cs の変更

[ファイル全文](C:/TopGun/Assets/Scripts/Player/Counter/TimeDilationController.cs)

```diff
diff --git a/Assets/Scripts/Player/Counter/TimeDilationController.cs b/Assets/Scripts/Player/Counter/TimeDilationController.cs
index 37f67f7..171d7d8 100644
--- a/Assets/Scripts/Player/Counter/TimeDilationController.cs
+++ b/Assets/Scripts/Player/Counter/TimeDilationController.cs
@@ -5,6 +5,7 @@ using UnityEngine;
 /// </summary>
 public class TimeDilationController : MonoBehaviour
 {
+    /// <summary> スロー演出を再生しているか </summary>
     public bool IsPlaying => _playing;
 
     [Tooltip("元の FixedUpdate の間隔を保存する変数")] private float _baseFixedDeltaTime;
@@ -30,15 +31,13 @@ public class TimeDilationController : MonoBehaviour
     }
 
     /// <summary>
-    ///     スロー演出の適用をする
+    ///     スロー演出を停止し、時間倍率と物理更新間隔を元に戻す
     /// </summary>
-    /// <param name="scale"> 適用させるタイムスケールの値 </param>
-    private void ApplyScale(float scale)
+    public void Stop()
     {
-        Time.timeScale = scale;
-        Time.fixedDeltaTime = _baseFixedDeltaTime * scale;
-
-        Debug.Log($"TimeScale : {Time.timeScale}, FixedDeltaTime : {Time.fixedDeltaTime}");
+        if (!_playing) return;
+        _playing = false;
+        ApplyScale(1f);
     }
 
     private void Awake()
@@ -46,6 +45,11 @@ public class TimeDilationController : MonoBehaviour
         _baseFixedDeltaTime = Time.fixedDeltaTime;
     }
 
+    private void OnDisable()
+    {
+        Stop();
+    }
+
     private void Update()
     {
         if (!_playing) return;
@@ -55,8 +59,19 @@ public class TimeDilationController : MonoBehaviour
         // 適用させる時間を超えたら元に戻す
         if (_timerUnscaled >= _durationUnscaled)
         {
-            _playing = false;
-            ApplyScale(1f);
+            Stop();
         }
     }
+
+    /// <summary>
+    ///     時間倍率と物理更新間隔にスロー演出を適用する
+    /// </summary>
+    /// <param name="scale"> 適用させるタイムスケールの値 </param>
+    private void ApplyScale(float scale)
+    {
+        Time.timeScale = scale;
+        Time.fixedDeltaTime = _baseFixedDeltaTime * scale;
+
+        Debug.Log($"TimeScale : {Time.timeScale}, FixedDeltaTime : {Time.fixedDeltaTime}");
+    }
 }
```