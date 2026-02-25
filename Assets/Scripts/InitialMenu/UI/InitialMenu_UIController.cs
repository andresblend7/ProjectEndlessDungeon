using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialMenu_UIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    // Call this method to load the next scene by name
    public void GoToMine()
    {
        var sceneName = "Gameplay_1";
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs have been reset.");
    }

}
