using UnityEngine;

public class SceneChangeFromTitle : MonoBehaviour
{
    [SerializeField] private TitleInputHandler _titleInputHandler;
    [SerializeField] private string _sceneNameToLoad = "";

    private void SceneChenge()
    {
        if (_sceneNameToLoad == "")
        {
            Debug.LogError("Scene name is empty. Please set the scene name to load.");
            return;
        }
        SceneLoader.LoadScene(_sceneNameToLoad);
    }

    private void OnEnable()
    {
        _titleInputHandler.OnClicked += SceneChenge;
    }

    private void OnDisable()
    {
        _titleInputHandler.OnClicked -= SceneChenge;
    }
}
