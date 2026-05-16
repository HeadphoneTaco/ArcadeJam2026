using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Allan";
    
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
    
    public void PlayGame()
    {
        playButton.Select();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        quitButton.Select();
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}