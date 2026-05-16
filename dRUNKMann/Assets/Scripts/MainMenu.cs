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
        if (EventSystem.current.currentSelectedGameObject == playButton.gameObject)
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void QuitGame()
    {
        if (EventSystem.current.currentSelectedGameObject == quitButton.gameObject)
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
        
        else
        {
            quitButton.Select();
        }
    }
}