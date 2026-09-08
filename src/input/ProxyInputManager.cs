using System;
using System.Collections.Generic;

namespace odl3d;

/// <summary>
/// A specialized InputManager for handling input events within a Scene or Object. It acts as a proxy to the Window's InputManager, allowing the owner to register and respond to key events while maintaining its own state and event subscriptions. This class is responsible for managing key press, release, and down events specific to the owner, and ensures proper disposal of event handlers when the owner is no longer active.
/// </summary>
public class ProxyInputManager : AbstractInputManager
{
    private List<Action<Key>> keyPressedEvents = new List<Action<Key>>();
    private List<Action<Key>> keyDownEvents = new List<Action<Key>>();
    private List<Action<Key>> keyRepeatedEvents = new List<Action<Key>>();
    private List<Action<Key>> keyReleasedEvents = new List<Action<Key>>();
    private List<Action<Mouse>> mousePressedEvents = new List<Action<Mouse>>();
    private List<Action<Mouse>> mouseDownEvents = new List<Action<Mouse>>();
    private List<Action<Mouse>> mouseRepeatedEvents = new List<Action<Mouse>>();
    private List<Action<Mouse>> mouseReleasedEvents = new List<Action<Mouse>>();

    /// <summary>
    /// Initializes a new instance of the ProxyInputManager class for the specified window. This constructor sets up the input manager to handle key events for the owner, allowing the owner to respond to user input. It also initializes lists to keep track of registered key events for proper disposal later.
    /// </summary>
    /// <param name="window">The window associated with the owner.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public ProxyInputManager(Window window) : base(window)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Window.InputManager.OnKeyPressed += InvokeKeyPressed;
        Window.InputManager.OnKeyDown += InvokeKeyDown;
        Window.InputManager.OnKeyRepeated += InvokeKeyRepeated;
        Window.InputManager.OnKeyReleased += InvokeKeyReleased;
        Window.InputManager.OnMousePressed += InvokeMousePressed;
        Window.InputManager.OnMouseDown += InvokeMouseDown;
        Window.InputManager.OnMouseRepeated += InvokeMouseRepeated;
        Window.InputManager.OnMouseReleased += InvokeMouseReleased;
    }

    /// <summary>
    /// Checks if a specific key is currently being held down. This method queries the state of the key from the associated window's InputManager, allowing the owner to respond to continuous key presses during its update cycle.
    /// </summary>
    /// <param name="key">The key code of the key to check.</param>
    /// <returns>True if the key is currently being held down; otherwise, false.</returns>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override bool IsKeyDown(Key key)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        return Window.InputManager.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a specific mouse button is currently being held down. This method queries the state of the mouse button from the associated window's InputManager, allowing the owner to respond to continuous mouse button presses during its update cycle.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button is currently being held down; otherwise, false.</returns>
    public override bool IsMouseDown(Mouse button)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        return Window.InputManager.IsMouseDown(button);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified key is pressed. This allows the owner to respond to key press events for the given key.
    /// </summary>
    /// <param name="key">The key code of the key to register.</param>
    /// <param name="onPress">The action to perform when the key is pressed.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterKeyPress(Key key, Action onPress)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Key> keyPressedEvent = (pressedKey) =>
        {
            if (pressedKey == key)
                onPress?.Invoke();
        };
        Window.InputManager.OnKeyPressed += keyPressedEvent;
        keyPressedEvents.Add(keyPressedEvent);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified key is held down. This allows the owner to respond to continuous key press events for the given key.
    /// </summary>
    /// <param name="key">The key code of the key to register.</param>
    /// <param name="onDown">The action to perform when the key is held down.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterKeyDown(Key key, Action onDown)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Key> keyDownEvent = (downKey) =>
        {
            if (downKey == key)
                onDown?.Invoke();
        };
        Window.InputManager.OnKeyDown += keyDownEvent;
        keyDownEvents.Add(keyDownEvent);        
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified key is repeated. This allows the owner to respond to repeated key press events for the given key.
    /// </summary>
    /// <param name="key">The key code of the key to register.</param>
    /// <param name="onRepeat">The action to perform when the key is repeated.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterKeyRepeated(Key key, Action onRepeat)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Key> keyRepeatedEvent = (repeatedKey) =>
        {
            if (repeatedKey == key)
                onRepeat?.Invoke();
        };
        Window.InputManager.OnKeyRepeated += keyRepeatedEvent;
        keyRepeatedEvents.Add(keyRepeatedEvent);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified key is released. This allows the owner to respond to key release events for the given key.
    /// </summary>
    /// <param name="key">The key code of the key to register.</param>
    /// <param name="onRelease">The action to perform when the key is released.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterKeyReleased(Key key, Action onRelease)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Key> keyReleasedEvent = (releasedKey) =>
        {
            if (releasedKey == key)
                onRelease?.Invoke();
        };
        Window.InputManager.OnKeyReleased += keyReleasedEvent;
        keyReleasedEvents.Add(keyReleasedEvent);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified mouse button is pressed. This allows the owner to respond to mouse press events for the given button.
    /// </summary>
    /// <param name="button">The mouse button to register.</param>
    /// <param name="onPress">The action to perform when the mouse button is pressed.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterMousePress(Mouse button, Action onPress)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Mouse> mousePressedEvent = (pressedButton) =>
        {
            if (pressedButton == button)
                onPress?.Invoke();
        };
        Window.InputManager.OnMousePressed += mousePressedEvent;
        mousePressedEvents.Add(mousePressedEvent);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified mouse button is held down. This allows the owner to respond to mouse down events for the given button.
    /// </summary>
    /// <param name="button">The mouse button to register.</param>
    /// <param name="onDown">The action to perform when the mouse button is held down.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterMouseDown(Mouse button, Action onDown)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Mouse> mouseDownEvent = (downButton) =>
        {
            if (downButton == button)
                onDown?.Invoke();
        };
        Window.InputManager.OnMouseDown += mouseDownEvent;
        mouseDownEvents.Add(mouseDownEvent);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified mouse button is repeatedly pressed. This allows the owner to respond to mouse repeat events for the given button.
    /// </summary>
    /// <param name="button">The mouse button to register.</param>
    /// <param name="onRepeat">The action to perform when the mouse button is repeatedly pressed.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterMouseRepeated(Mouse button, Action onRepeat)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Mouse> mouseRepeatedEvent = (repeatedButton) =>
        {
            if (repeatedButton == button)
                onRepeat?.Invoke();
        };
        Window.InputManager.OnMouseRepeated += mouseRepeatedEvent;
        mouseRepeatedEvents.Add(mouseRepeatedEvent);
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified mouse button is released. This allows the owner to respond to mouse release events for the given button.
    /// </summary>
    /// <param name="button">The mouse button to register.</param>
    /// <param name="onRelease">The action to perform when the mouse button is released.</param>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void RegisterMouseRelease(Mouse button, Action onRelease)
    {
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        Action<Mouse> mouseReleasedEvent = (releasedButton) =>
        {
            if (releasedButton == button)
                onRelease?.Invoke();
        };
        Window.InputManager.OnMouseReleased += mouseReleasedEvent;
        mouseReleasedEvents.Add(mouseReleasedEvent);
    }

    /// <summary>
    /// Updates the input state for the owner. This method is called during the owner's update cycle to process input events and invoke the appropriate callbacks for registered keys. It allows the owner to respond to user input in real-time.
    /// </summary>
    public override void Update() { }

    /// <summary>
    /// Disposes of the ProxyInputManager and releases its resources. It unregisters all key event callbacks from the associated window's InputManager, clears the lists of registered events, and marks the ProxyInputManager as disposed. After calling this method, the ProxyInputManager should not be used again.
    /// </summary>
    /// <exception cref="InputException">Thrown if the parent window's InputManager is null.</exception>
    public override void Dispose()
    {
        if (Disposed) return;
        if (Window.InputManager == null) throw new InputException("Parent window's InputManager is null.");
        keyPressedEvents.ForEach(e => Window.InputManager.OnKeyPressed -= e);
        keyPressedEvents.Clear();
        keyDownEvents.ForEach(e => Window.InputManager.OnKeyDown -= e);
        keyDownEvents.Clear();
        keyRepeatedEvents.ForEach(e => Window.InputManager.OnKeyRepeated -= e);
        keyRepeatedEvents.Clear();
        keyReleasedEvents.ForEach(e => Window.InputManager.OnKeyReleased -= e);
        keyReleasedEvents.Clear();
        mousePressedEvents.ForEach(e => Window.InputManager.OnMousePressed -= e);
        mousePressedEvents.Clear();
        mouseDownEvents.ForEach(e => Window.InputManager.OnMouseDown -= e);
        mouseDownEvents.Clear();
        mouseRepeatedEvents.ForEach(e => Window.InputManager.OnMouseRepeated -= e);
        mouseRepeatedEvents.Clear();
        mouseReleasedEvents.ForEach(e => Window.InputManager.OnMouseReleased -= e);
        mouseReleasedEvents.Clear();
        Window.InputManager.OnKeyPressed -= InvokeKeyPressed;
        Window.InputManager.OnKeyDown -= InvokeKeyDown;
        Window.InputManager.OnKeyRepeated -= InvokeKeyRepeated;
        Window.InputManager.OnKeyReleased -= InvokeKeyReleased;
        Window.InputManager.OnMousePressed -= InvokeMousePressed;
        Window.InputManager.OnMouseDown -= InvokeMouseDown;
        Window.InputManager.OnMouseRepeated -= InvokeMouseRepeated;
        Window.InputManager.OnMouseReleased -= InvokeMouseReleased;
        Disposed = true;
    }
}