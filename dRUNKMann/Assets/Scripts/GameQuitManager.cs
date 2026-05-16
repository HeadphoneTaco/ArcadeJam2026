using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class GameQuitManager : MonoBehaviour
{
    private const float IdleQuitSeconds = 180f;
    private static GameQuitManager instance;

    private float lastInputTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(GameQuitManager));
        instance = managerObject.AddComponent<GameQuitManager>();
        DontDestroyOnLoad(managerObject);
    }

    private void OnEnable()
    {
        lastInputTime = Time.realtimeSinceStartup;
        InputSystem.onEvent += HandleInputEvent;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= HandleInputEvent;
    }

    private void Update()
    {
        if (WasMouseThreePressed() || Time.realtimeSinceStartup - lastInputTime >= IdleQuitSeconds)
        {
            QuitGame();
        }
    }

    private bool WasMouseThreePressed()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return false;
        }

        return mouse.middleButton.wasPressedThisFrame || WasMouseButtonPressed(mouse, "button3");
    }

    private bool WasMouseButtonPressed(Mouse mouse, string buttonPath)
    {
        foreach (InputControl control in mouse.allControls)
        {
            ButtonControl button = control as ButtonControl;

            if (button != null && button.name == buttonPath && button.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device == null || (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()))
        {
            return;
        }

        foreach (InputControl control in eventPtr.EnumerateChangedControls(device))
        {
            if (IsPlayerInput(control))
            {
                lastInputTime = Time.realtimeSinceStartup;
                return;
            }
        }
    }

    private bool IsPlayerInput(InputControl control)
    {
        return control != null
            && !control.noisy
            && !control.synthetic
            && IsPlayerInputDevice(control.device);
    }

    private bool IsPlayerInputDevice(InputDevice device)
    {
        return device is Keyboard
            || device is Mouse
            || device is Gamepad
            || device is Joystick
            || device is Touchscreen;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
