using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject levelSelectUI;

    public void OnStartPressed()
    {
        mainMenuUI.SetActive(false);
        levelSelectUI.SetActive(true);
    }

    public void OnLoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
