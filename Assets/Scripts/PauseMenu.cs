using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;       // drag panel PauseMenu di sini
    public GameObject panelTutorial;
    public GameObject panelTutorial2;   // drag panel HowToPlay di sini (kalau punya)
     public GameObject panelTutorial3;
    public GameObject panelTutorial4;
    public static bool isPaused = false;
    
    [Header("Video Settings")]
    public VideoPlayer tutorialVideoPlayer1; // 2. Referensi ke komponen Video Player
    public VideoPlayer tutorialVideoPlayer2; // 2. Referensi ke komponen Video Player
    public VideoPlayer tutorialVideoPlayer3; // 2. Referensi ke komponen Video Player
    public VideoPlayer tutorialVideoPlayer4; // 2. Referensi ke komponen Video Player


   void Update()
{

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
}

    // --- Resume Button ---
   public void ResumeGame()
{
    Debug.Log("ResumeGame jalan");

    StartCoroutine(ResumeAfterDelay());
}

IEnumerator ResumeAfterDelay()
{
    yield return new WaitForSecondsRealtime(0.05f);

    pausePanel.SetActive(false);
    if (panelTutorial != null) panelTutorial.SetActive(false);
    if (panelTutorial2 != null) panelTutorial2.SetActive(false);

    Time.timeScale = 1f;
    isPaused = false;

    // Kursor disembunyikan lagi
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
}




    // --- Pause Function ---
    void PauseGame()
    {
        pausePanel.SetActive(true);

        // Biar gameplay berhenti tapi UI tetap hidup
        Time.timeScale = 0.0001f; // bukan 0, tapi super lambat
        isPaused = true;

        // Biar kursor muncul
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    // --- How To Play Button ---
    public void TutorialGame()
    {
        panelTutorial.SetActive(true);
        if (tutorialVideoPlayer1 != null)
        {
            tutorialVideoPlayer1.Play();
        }
    }

    // public void CloseTutorial()
    // {
    //     panelTutorial.SetActive(false);
    //     panelTutorial2.SetActive(false);
    // }

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

    public void BackFromTutorial()
    {
        panelTutorial.SetActive(false);
        panelTutorial2.SetActive(false);
        pausePanel.SetActive(true);
        if (tutorialVideoPlayer1 != null)
        {
            tutorialVideoPlayer1.Stop(); 
            // Atau gunakan .Pause() jika ingin melanjutkan nanti
        }
    }

    // --- Quit Button ---
    public void QuitGame()
    {
        Time.timeScale = 1f;  // pastikan waktu jalan lagi
        SceneManager.LoadScene("MainMenu");  // ganti dengan nama scene utama kamu
        // atau kalau belum punya main menu:
        // Application.Quit();
    }
}
