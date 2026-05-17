using UnityEngine;

public class MouseLock : MonoBehaviour
{
    void Start()
    {
        // lock the mouse
        Cursor.lockState = CursorLockMode.Locked;
    
        // make the mouse invisible
        Cursor.visible = false;
    }
}
