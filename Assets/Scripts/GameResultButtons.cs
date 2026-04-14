using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultButtons : MonoBehaviour
{
    [Header("Konfigurasi Scene")]
    public string gameplaySceneName = "GamePlay"; 
    public string mainMenuSceneName = "MainMenu";

    // --- TAMBAHAN PENTING DI SINI ---
    private void Start()
    {
        // 1. Pastikan waktu berjalan normal (unpause)
        Time.timeScale = 1f;

        // 2. MUNCULKAN MOUSE KEMBALI
        // Ini wajib agar player bisa klik tombol
        Cursor.lockState = CursorLockMode.None; // Bebaskan cursor dari tengah layar
        Cursor.visible = true; // Buat cursor terlihat
    }
    // --------------------------------

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError($"Scene '{gameplaySceneName}' tidak ditemukan!");
        }
    }

    public void OnExitClicked()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{mainMenuSceneName}' tidak ditemukan!");
        }
    }
}