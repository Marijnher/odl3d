using System;

namespace odl3d;

/// <summary>
/// An input manager that handles keyboard input for a specific window. It listens for key press and release events, maintains the state of each key, and provides events that can be subscribed to for responding to key actions. This class is designed to be used with a Window instance and integrates with GLFW for input handling.
/// </summary>
public class WindowInputManager : AbstractInputManager
{
    /// <summary>
    /// An array of booleans representing the state of each key on the keyboard. The index corresponds to the GLFW key code, and the value is true if the key is currently pressed, or false if it is released. This allows for easy querying of key states in the Update() method.
    /// </summary>
    private bool[] keys = new bool[GLFW.GLFW_KEY_LAST + 1];

    /// <summary>
    /// Creates a new WindowInputManager for the specified window. It sets up the key callback to handle key press and release events, and initializes the keys array to track the state of each key. The WindowInputManager will listen for input events from the window and invoke the appropriate events when keys or mouse buttons are pressed or released.
    /// </summary>
    /// <param name="window">The window for which to manage input.</param>
    public WindowInputManager(Window window) : base(window)
    {
        GLFW.glfwSetKeyCallback(window.Handle, KeyCallback);
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
                InvokeKeyPressed(key);
            else if (action == GLFW.GLFW_RELEASE)
                InvokeKeyReleased(key);
        }
    }

    /// <summary>
    /// Registers a callback for a specific key, allowing the user to specify actions to be performed when the key is pressed, released, or held down. The provided callbacks are invoked when the corresponding key events occur, enabling custom input handling for that key.
    /// </summary>
    /// <param name="key">The key for which to register the callbacks.</param>
    /// <param name="onPress">The action to perform when the key is pressed.</param>
    /// <param name="onRelease">The action to perform when the key is released.</param>
    /// <param name="onDown">The action to perform when the key is held down.</param>
    public override void RegisterKey(int key, Action? onPress = null, Action? onRelease = null, Action? onDown = null)
    {
        Console.WriteLine("Registering window key: " + key);
        OnKeyPressed += (pressedKey) =>
        {
            if (pressedKey == key)
                onPress?.Invoke();
        };

        OnKeyReleased += (releasedKey) =>
        {
            if (releasedKey == key)
                onRelease?.Invoke();
        };

        OnKeyDown += (downKey) =>
        {
            if (downKey == key)
                onDown?.Invoke();
        };
    }

    /// <summary>
    /// Checks if the specified key is currently pressed. It returns true if the key is down, or false if it is up. This method can be used to query the state of keys in the Update() method or in response to input events.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently pressed, false otherwise.</returns>
    public override bool IsKeyDown(int key) => key >= 0 && key <= GLFW.GLFW_KEY_LAST && keys[key];

    /// <summary>
    /// Updates the input state by checking the keys array for any keys that are currently pressed. If a key is pressed, it invokes the OnKeyDown event for that key, allowing subscribers to respond to continuous key presses. This method should be called once per frame to ensure that input events are processed and handled appropriately.
    /// </summary>
    public override void Update()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i])
                InvokeKeyDown(i);
        }
    }

    /// <summary>
    /// Disposes of the InputManager and releases any resources it holds. This method should be called when the InputManager is no longer needed to prevent memory leaks and ensure proper cleanup of event subscriptions. After calling Dispose(), the InputManager should not be used again.
    /// </summary>
    public override void Dispose()
    {
        if (Disposed) return;
        GLFW.glfwSetKeyCallback(Window.Handle, null);
        Disposed = true;
    }
}