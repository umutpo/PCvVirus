using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{   
    public bool shouldLoadAutomatically = false;
    public string automaticallyLoadedSceneName = "MiniGameTemplate";

    void Start()
    {
        if (shouldLoadAutomatically)
        {
            LoadSceneDelayed(automaticallyLoadedSceneName, 2);
        }
    }

    public void LoadScene(string sceneToLoad)
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Couldn't load the scene: " + sceneToLoad);
        }
    }

    public void LoadSceneDelayed(string sceneToLoad, int delaySeconds)
    {
        StartCoroutine(DelayedLoad(sceneToLoad, delaySeconds));
    }

    private IEnumerator DelayedLoad(string sceneToLoad, int delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        LoadScene(sceneToLoad);
    }
}