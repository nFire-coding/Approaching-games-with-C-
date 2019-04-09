using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{

    public static bool gameIsPaused = false;
    public GameObject pauseMenu;

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
	}

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void SaveGame()
    {
        Time.timeScale = 1f;
        int currentScene = -1;
        try
        {
            currentScene = GameObject.Find("Game").GetComponent<MyGameManager>().CurrentScene;
        }
        catch
        {
            Debug.Log("you can't save while saving a single level");
            pauseMenu.SetActive(false);
            gameIsPaused = false;
            return;
        }

        Debug.Log("Scena: " + currentScene);
        PersistenceManager.Save(currentScene);
        Debug.Log("Game Saved");
        pauseMenu.SetActive(false);
        gameIsPaused = false;
        return;

    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
