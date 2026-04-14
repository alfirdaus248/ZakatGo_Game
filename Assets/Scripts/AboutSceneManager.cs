using UnityEngine;
using UnityEngine.SceneManagement;

public class AboutSceneManager : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
