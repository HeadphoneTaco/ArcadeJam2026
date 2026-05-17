using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
    [SerializeField] private AudioSource menuAudioSource; 
    [SerializeField] private AudioClip gameMusicClip;
    
    // 1. Tied to Up Arrow: Moves visual highlight to Play
    public void HighlightPlay()
    {
        playButton.Select();
    }

    // 2. Tied to Down Arrow: Moves visual highlight to Quit
    public void HighlightQuit()
    {
        quitButton.Select();
    }

    // 3. Tied to Key 1: Executes the highlighted button's action
    public void ConfirmSelection()
    {
        // Ask Unity which button is currently highlighted/selected
        GameObject currentSelection = EventSystem.current.currentSelectedGameObject;

        // If Play is highlighted when Key 1 is pressed
        if (currentSelection == playButton.gameObject)
        {
            if (menuAudioSource != null)
            {
                menuAudioSource.Stop();
                DontDestroyOnLoad(menuAudioSource.gameObject);
                menuAudioSource.clip = gameMusicClip;
                menuAudioSource.Play();
            }
            SceneManager.LoadScene(gameSceneName);
        }
        // If Quit is highlighted when Key 1 is pressed
        else if (currentSelection == quitButton.gameObject)
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}