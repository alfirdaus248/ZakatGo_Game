using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public Slider timerSlider;
    public Text timerText;
    public float gameTime = 60f;

    private bool stopTimer = false;
    private float remainingTime;

    void Start()
    {
        remainingTime = gameTime;
        if(timerSlider != null) 
        {
            timerSlider.maxValue = gameTime;
            timerSlider.value = gameTime;
        }
    }

    void Update()
    {
        if (stopTimer) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            stopTimer = true;
            TimerEnded();
        }

        // Update UI (hanya jika komponen ada)
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
        
        if (timerSlider != null)
            timerSlider.value = remainingTime;
    }

    void TimerEnded()
    {
        Debug.Log("Waktu habis! Kalah.");

        // --- SIMPAN STATUS KALAH (0) ---
        PlayerPrefs.SetInt("GameStatus", 0); 

        Time.timeScale = 1f; 
        SceneManager.LoadScene("GameResult");
    }
}