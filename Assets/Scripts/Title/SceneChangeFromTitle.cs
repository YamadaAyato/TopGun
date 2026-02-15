using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneChangeFromTitle : MonoBehaviour
{
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeDuration;
    [SerializeField] private TitleInputHandler _titleInputHandler;
    [SerializeField] private string _sceneNameToLoad = "";

    private void SceneChenge()
    {
        if (_sceneNameToLoad == "")
        {
            Debug.LogError("Scene name is empty. Please set the scene name to load.");
            return;
        }

        _fadeImage.gameObject.SetActive(true);
        _fadeImage.DOFade(1f, _fadeDuration)
            .OnComplete(() => SceneLoader.LoadScene(_sceneNameToLoad));
    }

    private void OnEnable()
    {
        _titleInputHandler.OnClicked += SceneChenge;
        _fadeImage.gameObject.SetActive(false);
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0f);
    }

    private void OnDisable()
    {
        _titleInputHandler.OnClicked -= SceneChenge;
    }
}
