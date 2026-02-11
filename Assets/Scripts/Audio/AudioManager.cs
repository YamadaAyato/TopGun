using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ゲーム全体の音声管理を行うシングルトンクラス
/// </summary>
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    /// <summary>
    ///     サウンドデータクラス
    /// </summary>
    [Serializable]
    public class SoundData
    {
        public string Name => _name;
        public AudioClip Clip => _clip;
        public float Volume => _volume;

        [SerializeField] private string _name;
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
    }

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;

    [Header("SE設定")]
    [SerializeField] private int _sePoolSize = 16;

    [Header("3DSE デフォルト設定")]
    [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;
    [SerializeField] private float _minDistance = 3f;
    [SerializeField] private float _maxDistance = 30f;

    [Header("SEリスト")]
    [SerializeField] private List<SoundData> _seList = new();

    [Header("BGMリスト")]
    [SerializeField] private List<SoundData> _bgmList = new();

    // Name => SoundDataの高速参照用マップ
    private readonly Dictionary<string, SoundData> _seMap = new();
    private readonly Dictionary<string, SoundData> _bgmMap = new();

    // プール管理
    private readonly Queue<AudioSource> _se2DPool = new();
    private readonly Queue<AudioSource> _se3DPool = new();

    // BGMのフェードTween管理（Pause/Resume時に競合を防ぐ）
    private Tween _bgmFadeTween;

    /// <summary> 
    ///     2DでSE再生をする(画面固定音源)
    /// </summary>
    public void PlaySE2D(string name, float volumeMul = 1f)
    {
        if (!TryGetSE(name, out var se)) return;

        // プールから取得
        var src = GetFromPool(_se2DPool, is3D: false);
        src.transform.position = Vector3.zero;

        // SoundDataの基準音量 × 呼び出し側の倍率
        src.volume = Mathf.Clamp01(se.Volume * volumeMul);
        src.PlayOneShot(se.Clip);

        // 再生時間後にプールへ戻す
        ReturnLater(src, _se2DPool, se.Clip.length);
    }

    /// <summary>
    ///     3DでSE再生をする(ワールド位置指定)
    /// </summary>
    public void PlaySE3D(string name, Vector3 worldPos, float volumeMul = 1f)
    {
        if (!TryGetSE(name, out var se)) return;

        // プールから取得
        var src = GetFromPool(_se3DPool, is3D: true);
        src.transform.position = worldPos;

        // SoundDataの基準音量 × 呼び出し側の倍率
        src.volume = Mathf.Clamp01(se.Volume * volumeMul);
        src.PlayOneShot(se.Clip);

        // 再生時間後にプールへ戻す
        ReturnLater(src, _se3DPool, se.Clip.length);
    }

    /// <summary> 
    ///     3DでSE再生をする(Transform追従) 
    /// </summary>
    public void PlaySE3D(string name, Transform follow, float volumeMul = 1f)
    {
        if (follow == null) return;
        PlaySE3D(name, follow.position, volumeMul);
    }

    /// <summary>
    ///     BGM再生をする(ループ再生)
    /// </summary>
    public void PlayBGM(string name)
    {
        if (!TryGetBGM(name, out var bgm)) return;

        _bgmSource.loop = true;
        _bgmSource.volume = bgm.Volume;
        _bgmSource.clip = bgm.Clip;
        _bgmSource.Play();
    }
    /// <summary> 
    ///     BGMを強制停止する
    /// </summary>
    public void StopBGM()
    {
        if (_bgmSource == null) return;
        _bgmSource.Stop();
    }

    /// <summary>
    ///     BGMを一時停止する（再生位置保持）
    /// </summary>
    public void PauseBGM()
    {
        if (_bgmSource == null) return;

        // フェード中なら止める（音量TweenとPauseがぶつからないように）
        if (_bgmFadeTween != null && _bgmFadeTween.IsActive())
            _bgmFadeTween.Kill();

        // 再生中だけPause
        if (_bgmSource.isPlaying)
            _bgmSource.Pause();
    }

    /// <summary>
    ///     一時停止したBGMを再開する（同じ再生位置から）
    /// </summary>
    public void ResumeBGM()
    {
        if (_bgmSource == null) return;

        if (_bgmFadeTween != null && _bgmFadeTween.IsActive())
            _bgmFadeTween.Kill();

        _bgmSource.UnPause();
    }


    /// <summary>
    ///     BGMをフェードアウトする
    /// </summary>
    /// <param name="fadeTime"></param>
    public void FadeOutBGM(float fadeTime)
    {
        if (_bgmSource == null) return;

        if (_bgmFadeTween != null && _bgmFadeTween.IsActive())
            _bgmFadeTween.Kill();

        _bgmFadeTween = _bgmSource
            .DOFade(0f, fadeTime)
            .SetUpdate(true);
    }

    /// <summary>
    ///     BGMをフェードインする
    /// </summary>
    /// <param name="fadeTime"></param>
    /// <param name="targetVolume"></param>
    public void FadeInBGM(float fadeTime, float targetVolume = 1f)
    {
        if (_bgmSource == null) return;

        if (_bgmFadeTween != null && _bgmFadeTween.IsActive())
            _bgmFadeTween.Kill();

        _bgmSource.volume = 0f;

        _bgmFadeTween = _bgmSource
            .DOFade(Mathf.Clamp01(targetVolume), fadeTime)
            .SetUpdate(true);
    }


    /// <summary>
    ///     Inspectorで設定されたリストから辞書化する
    /// </summary>
    private void BuildMaps()
    {
        // 初期化してから名前で登録
        _seMap.Clear();
        foreach (var s in _seList)
        {
            if (s == null || string.IsNullOrEmpty(s.Name)) continue;
            _seMap[s.Name] = s;
        }

        _bgmMap.Clear();
        foreach (var b in _bgmList)
        {
            if (b == null || string.IsNullOrEmpty(b.Name)) continue;
            _bgmMap[b.Name] = b;
        }
    }

    /// <summary>
    ///     プールを事前生成しておく
    /// </summary>
    private void WarmupPools()
    {
        for (int i = 0; i < _sePoolSize; i++)
        {
            _se2DPool.Enqueue(CreateSESource(is3D: false));
            _se3DPool.Enqueue(CreateSESource(is3D: true));
        }
    }

    /// <summary>
    ///     SE用のAudioSourceを作成する
    ///     2d/3Dの設定もここで行う
    ///     2d : spatialBlend = 0f
    ///     3d : spatialBlend = 1f,距離減衰設定
    /// </summary>
    /// <param name="is3D"></param>
    /// <returns></returns>
    private AudioSource CreateSESource(bool is3D)
    {
        // 新規GameObjectを作成してAudioSourceを追加
        var go = new GameObject(is3D ? "SE3D_Source" : "SE2D_Source");
        go.transform.SetParent(transform);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;

        if (is3D)
        {
            // 3D音源設定
            src.spatialBlend = 1f;
            src.rolloffMode = _rolloffMode;
            src.minDistance = _minDistance;
            src.maxDistance = _maxDistance;
        }
        else
        {
            // 2D音源設定
            src.spatialBlend = 0f;
        }

        return src;
    }

    /// <summary>
    ///     SE名からSoundDataを取得する
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sound"></param>
    /// <returns></returns>
    private bool TryGetSE(string name, out SoundData sound) => _seMap.TryGetValue(name, out sound);
    /// <summary>
    ///     BGM名からSoundDataを取得する
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sound"></param>
    /// <returns></returns>
    private bool TryGetBGM(string name, out SoundData sound) => _bgmMap.TryGetValue(name, out sound);

    /// <summary>
    ///     プールからAudioSourceを取得する
    ///     足りない場合は新規作成する
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="is3D"></param>
    /// <returns></returns>
    private AudioSource GetFromPool(Queue<AudioSource> pool, bool is3D)
    {
        if (pool.Count > 0) return pool.Dequeue();
        return CreateSESource(is3D);
    }
    /// <summary>
    ///     AudioSourceを指定時間後にプールへ戻す
    /// </summary>
    /// <param name="src"></param>
    /// <param name="pool"></param>
    /// <param name="delay"></param>

    private void ReturnLater(AudioSource src, Queue<AudioSource> pool, float delay)
    {
        // clip長で戻す（PlayOneShotなので clipは保持しない）
        DOVirtual.DelayedCall(delay, () =>
        {
            if (src == null) return;
            src.Stop();
            pool.Enqueue(src);
        });
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildMaps();

        // BGM source が未設定なら自動作成
        if (_bgmSource == null)
        {
            var bgmGo = new GameObject("BGM_Source");
            bgmGo.transform.SetParent(transform);
            _bgmSource = bgmGo.AddComponent<AudioSource>();

            // BGMは2D固定
            _bgmSource.spatialBlend = 0f;
            _bgmSource.loop = true;
        }

        WarmupPools();
    }
}
