using UnityEngine;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    public GameObject panelAbout;
    public GameObject panelTutorial;
    public GameObject panelTutorial2;
    public GameObject panelTutorial3;
    public GameObject panelTutorial4;

    [Header("Video Settings")]
    public VideoPlayer tutorialVideoPlayer1; // 2. Referensi ke komponen Video Player
    public VideoPlayer tutorialVideoPlayer2; // 2. Referensi ke komponen Video Player
    public VideoPlayer tutorialVideoPlayer3; // 2. Referensi ke komponen Video Player
    public VideoPlayer tutorialVideoPlayer4; // 2. Referensi ke komponen Video Player

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
    }


    public void AboutGame()
    {
        panelAbout.SetActive(true); 
    }

    public void CloseAbout()
    {
        panelAbout.SetActive(false);
    }

    public void TutorialGame()
    {
        panelTutorial.SetActive(true);
        if (tutorialVideoPlayer1 != null)
        {
            tutorialVideoPlayer1.Play();
        }
    }
    
    public void CloseTutorial()
    {
        panelTutorial.SetActive(false);
        panelTutorial2.SetActive(false);
        if (tutorialVideoPlayer1 != null)
        {
            tutorialVideoPlayer1.Stop(); 
            // Atau gunakan .Pause() jika ingin melanjutkan nanti
        }
    }


    public void OpenTutorial2()
    {
        panelTutorial.SetActive(false);
        panelTutorial2.SetActive(true);
        // 3. Play video saat panel tutorial dibuka
        if (tutorialVideoPlayer2 != null)
        {
            tutorialVideoPlayer2.Play();
        }
    }

    public void CloseTutorial2()
    {
        panelTutorial2.SetActive(false);
        panelTutorial.SetActive(true);
        if (tutorialVideoPlayer2 != null)
        {
            tutorialVideoPlayer2.Stop(); 
            tutorialVideoPlayer1.Play(); 
            // Atau gunakan .Pause() jika ingin melanjutkan nanti
        }
    }

    public void OpenTutorial3()
    {
        panelTutorial2.SetActive(false);
        panelTutorial3.SetActive(true);
        // 3. Play video saat panel tutorial dibuka
        if (tutorialVideoPlayer3 != null)
        {
            tutorialVideoPlayer3.Play();
        }
    }

    public void CloseTutorial3()
    {
        panelTutorial3.SetActive(false);
        panelTutorial2.SetActive(true);
        if (tutorialVideoPlayer3 != null)
        {
            tutorialVideoPlayer3.Stop(); 
            tutorialVideoPlayer2.Play(); 
            // Atau gunakan .Pause() jika ingin melanjutkan nanti
        }
    }

    public void OpenTutorial4()
    {
        panelTutorial3.SetActive(false);
        panelTutorial4.SetActive(true);
        // 3. Play video saat panel tutorial dibuka
        if (tutorialVideoPlayer4 != null)
        {
            tutorialVideoPlayer4.Play();
        }
    }

    public void CloseTutorial4()
    {
        panelTutorial4.SetActive(false);
        panelTutorial3.SetActive(true);
        if (tutorialVideoPlayer4 != null)
        {
            tutorialVideoPlayer4.Stop(); 
            tutorialVideoPlayer3.Play(); 
            // Atau gunakan .Pause() jika ingin melanjutkan nanti
        }
    }

    // --- MODIFIKASI FUNGSI EXIT ---
    public void ExitGame()
    {
        Debug.Log("Exit clicked!");

        // Logika agar tombol Exit berfungsi saat testing di Unity Editor
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Logika untuk game yang sudah di-build (Windows/Android/iOS)
            Application.Quit();
        #endif
    }
}