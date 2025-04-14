using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeniusSDKBootstrap : MonoBehaviour
{
    [SerializeField] private string nextScene = "GeniusStoreDemo";

    private IEnumerator Start()
    {
        // Wait until GeniusSDK is ready
        while (!GeniusSDKWrapper.Instance.IsReady)
        {
            yield return null;
        }
        Debug.Log("GeniusSDK is ready, loading next scene...");
        SceneManager.LoadScene(nextScene);
    }
}
