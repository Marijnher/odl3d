using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace odl3d;

/// <summary>
/// An input manager that handles keyboard input for a specific window. It listens for key press and release events, maintains the state of each key, and provides events that can be subscribed to for responding to key actions. This class is designed to be used with a Window instance and integrates with GLFW for input handling.
/// </summary>
public class WindowInputManager : AbstractInputManager
{
    /// <summary>
    /// The initial repeat time for mouse button presses, in seconds. This is the delay before the first repeat event is triggered when a mouse button is held down.
    /// </summary>
    public const double MOUSE_REPEAT_TIME_INITIAL = 0.4d;

    /// <summary>
    /// The subsequent repeat time for mouse button presses, in seconds. This is the interval between repeat events after the initial delay when a mouse button is held down.
    /// </summary>
    public const double MOUSE_REPEAT_TIME_SUBSEQUENT = 0.045d;

    /// <summary>
    /// An array of booleans representing the state of each key on the keyboard. The index corresponds to the GLFW key code, and the value is true if the key is currently pressed, or false if it is released. This allows for easy querying of key states in the Update() method.
    /// </summary>
    private bool[] keys = new bool[GLFW.GLFW_KEY_LAST + 1];

    /// <summary>
    /// An array of booleans representing the state of each mouse button. The index corresponds to the mouse button code, and the value is true if the button is currently pressed, or false if it is released. This allows for easy querying of mouse button states in the Update() method.
    /// </summary>
    private bool[] mouseButtons = new bool[8]; // Assuming 8 mouse buttons, adjust as needed.

    /// <summary>
    /// A dictionary that maps each mouse button to a Timer instance. This is used to track the duration for which each mouse button has been held down. The Timer is started when the button is pressed and reset when the button is released, allowing for timing-based input handling.
    /// </summary>
    private Dictionary<Mouse, Timer> mouseButtonTimers = new Dictionary<Mouse, Timer>();

    /// <summary>
    /// Creates a new WindowInputManager for the specified window. It sets up the key callback to handle key press and release events, and initializes the keys array to track the state of each key. The WindowInputManager will listen for input events from the window and invoke the appropriate events when keys or mouse buttons are pressed or released.
    /// </summary>
    /// <param name="window">The window for which to manage input.</param>
    public WindowInputManager(Window window) : base(window)
    {
        GLFW.glfwSetKeyCallback(window.Handle, KeyCallback);
        GLFW.glfwSetMouseButtonCallback(window.Handle, MouseButtonCallback);
    }

    /// <summary>
    /// The key callback function that is called by GLFW when a key event occurs. It updates the keys array to reflect the current state of the key (pressed or released) and invokes the appropriate events (OnKeyPressed, OnKeyReleased) based on the action. This allows subscribers to respond to key events in their own code.
    /// </summary>
    /// <param name="handle">The handle of the window that received the event.</param>
    /// <param name="key">The keyboard key that was pressed or released.</param>
    /// <param name="scancode">The system-specific scancode of the key.</param>
    /// <param name="action">The action (press, release, repeat) associated with the key event.</param>
    /// <param name="mods">Bit field describing which modifier keys were held down.</param>
    private void KeyCallback(IntPtr handle, int key, int scancode, int action, int mods)
    {
        if (key >= 0 && key <= GLFW.GLFW_KEY_LAST)
        {
            keys[key] = action != GLFW.GLFW_RELEASE;
            if (action == GLFW.GLFW_PRESS)
            {
                InvokeKeyPressed((Key) key);
                InvokeKeyRepeated((Key) key);
            }
            else if (action == GLFW.GLFW_RELEASE)
                InvokeKeyReleased((Key) key);
            else if (action == GLFW.GLFW_REPEAT)
                InvokeKeyRepeated((Key) key);
        }
    }

    /// <summary>
    /// The mouse button callback function that is called by GLFW when a mouse button event occurs. It invokes the appropriate events (OnMousePressed, OnMouseReleased) based on the action. This allows subscribers to respond to mouse button events in their own code.
    /// </summary>
    /// <param name="handle">The handle of the window that received the event.</param>
    /// <param name="button">The mouse button that was pressed or released.</param>
    /// <param name="action">The action (press, release) associated with the mouse button event.</param>
    /// <param name="mods">Bit field describing which modifier keys were held down.</param>
    private void MouseButtonCallback(IntPtr handle, int button, int action, int mods)
    {
        if (button >= 0 && button <= mouseButtons.Length)
        {
            mouseButtons[button] = action != GLFW.GLFW_RELEASE;
            if (action == GLFW.GLFW_PRESS)
            {
                InvokeMousePressed((Mouse) button);
                InvokeMouseRepeated((Mouse) button);
                if (!mouseButtonTimers.ContainsKey((Mouse) button))
                {
                    mouseButtonTimers[(Mouse) button] = new Timer(MOUSE_REPEAT_TIME_INITIAL);
                }
            }
            else if (action == GLFW.GLFW_RELEASE)
            {
                if (mouseButtonTimers.ContainsKey((Mouse) button))
                {
                    mouseButtonTimers.Remove((Mouse) button);
                }
                InvokeMouseReleased((Mouse) button);
            }
        }
    }

    /// <summary>
    /// Registers a callback for a specific key press event.
    /// </summary>
    /// <param name="key">The key for which to register the callbacks.</param>
    /// <param name="onPress">The action to perform when the key is pressed.</param>
    public override void RegisterKeyPress(Key key, Action onPress)
    {
        OnKeyPressed += (pressedKey) =>
        {
            if (pressedKey == key)
                onPress?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific key down event.
    /// </summary>
    /// <param name="key">The key for which to register the callback.</param>
    /// <param name="onDown">The action to perform when the key is held down.</param>
    public override void RegisterKeyDown(Key key, Action onDown)
    {
        OnKeyDown += (downKey) =>
        {
            if (downKey == key)
                onDown?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific key repeated event.
    /// </summary>
    /// <param name="key">The key for which to register the callback.</param>
    /// <param name="onRepeat">The action to perform when the key is repeated.</param>
    public override void RegisterKeyRepeated(Key key, Action onRepeat)
    {
        OnKeyRepeated += (repeatedKey) =>
        {
            if (repeatedKey == key)
                onRepeat?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific key release event.
    /// </summary>
    /// <param name="key">The key for which to register the callback.</param>
    /// <param name="onRelease">The action to perform when the key is released.</param>
    public override void RegisterKeyReleased(Key key, Action onRelease)
    {
        OnKeyReleased += (releasedKey) =>
        {
            if (releasedKey == key)
                onRelease?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific mouse button press event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callbacks.</param>
    /// <param name="onPress">The action to perform when the mouse button is pressed.</param>
    public override void RegisterMousePress(Mouse button, Action onPress)
    {
        OnMousePressed += (pressedButton) =>
        {
            if (pressedButton == button)
                onPress?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific mouse button down event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callback.</param>
    /// <param name="onDown">The action to perform when the mouse button is held down.</param>
    public override void RegisterMouseDown(Mouse button, Action onDown)
    {
        OnMouseDown += (downButton) =>
        {
            if (downButton == button)
                onDown?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific mouse button repeated event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callback.</param>
    /// <param name="onRepeat">The action to perform when the mouse button is repeated.</param>
    public override void RegisterMouseRepeated(Mouse button, Action onRepeat)
    {
        OnMouseRepeated += (repeatedButton) =>
        {
            if (repeatedButton == button)
                onRepeat?.Invoke();
        };
    }

    /// <summary>
    /// Registers a callback for a specific mouse button release event.
    /// </summary>
    /// <param name="button">The mouse button for which to register the callback.</param>
    /// <param name="onRelease">The action to perform when the mouse button is released.</param>
    public override void RegisterMouseRelease(Mouse button, Action onRelease)
    {
        OnMouseReleased += (releasedButton) =>
        {
            if (releasedButton == button)
                onRelease?.Invoke();
        };
    }

    /// <summary>
    /// Checks if the specified key is currently pressed. It returns true if the key is down, or false if it is up. This method can be used to query the state of keys in the Update() method or in response to input events.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently pressed, false otherwise.</returns>
    public override bool IsKeyDown(Key key) => key >= 0 && (int) key < keys.Length && keys[(int) key];

    /// <summary>
    /// Checks if the specified mouse button is currently pressed. It returns true if the button is down, or false if it is up. This method can be used to query the state of mouse buttons in the Update() method or in response to input events.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button is currently pressed, false otherwise.</returns>
    public override bool IsMouseDown(Mouse button) => button >= 0 && (int) button < mouseButtons.Length && mouseButtons[(int) button];

    /// <summary>
    /// Updates the input state by checking the keys array for any keys that are currently pressed. If a key is pressed, it invokes the OnKeyDown event for that key, allowing subscribers to respond to continuous key presses. This method should be called once per frame to ensure that input events are processed and handled appropriately.
    /// </summary>
    public override void Update()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i])
                InvokeKeyDown((Key) i);
        }
        for (int i = 0; i < mouseButtons.Length; i++)
        {
            if (mouseButtons[i])
            {
                Mouse button = (Mouse) i;
                InvokeMouseDown(button);
                if (mouseButtonTimers.ContainsKey(button))
                {
                    Timer timer = mouseButtonTimers[button];
                    if (timer.IsFinished)
                    {
                        InvokeMouseRepeated(button);
                        timer.Reset(MOUSE_REPEAT_TIME_SUBSEQUENT);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Disposes of the InputManager and releases any resources it holds. This method should be called when the InputManager is no longer needed to prevent memory leaks and ensure proper cleanup of event subscriptions. After calling Dispose(), the InputManager should not be used again.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        GLFW.glfwSetKeyCallback(Window.Handle, null);
        GLFW.glfwSetMouseButtonCallback(Window.Handle, null);
        Disposed = true;
    }
}