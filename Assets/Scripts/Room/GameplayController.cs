using UnityEngine;

public class GameplayController : MonoBehaviour
{
    private bool isPaused = false;

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        if (isPaused) return;

        Debug.Log("Juego pausado");
        Time.timeScale = 0f;
        isPaused = true;

        // Aquí puedes activar tu UI de pausa
        // pauseMenu.SetActive(true);
    }

    void ResumeGame()
    {
        if (!isPaused) return;

        Debug.Log("Juego reanudado");
        Time.timeScale = 1f;
        isPaused = false;

        // Aquí puedes ocultar tu UI de pausa
        // pauseMenu.SetActive(false);
    }
}
