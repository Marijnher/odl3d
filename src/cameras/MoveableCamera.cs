using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A camera that can be moved using keyboard and mouse input. This class extends the base Camera class and provides functionality for first-person style movement and orientation control.
/// </summary>
public class MoveableCamera : Camera
{
    /// <summary>
    /// Initializes a new instance of the MoveableCamera class with the specified window.
    /// </summary>
    /// <param name="window">The window associated with the camera, used to capture input and manage the camera's view.</param>
    public MoveableCamera(Window window) : base(window) { }

    /// <summary>
    /// The movement speed of the camera. This value determines how fast the camera moves in response to user input. The default speed is 3 units per second, but it can be adjusted to make the camera move faster or slower.
    /// </summary>
    public float Speed = 3f;

    /// <summary>
    /// The sensitivity of the camera to mouse movement. This value determines how much the camera's orientation changes in response to mouse input. A higher sensitivity results in faster rotation, while a lower sensitivity provides finer control. The default value is 0.1, but it can be adjusted to suit user preference.
    /// </summary>
    public float Sensitivity = 0.1f;

    /// <summary>
    /// The last recorded position of the mouse cursor. This is used to calculate the change in mouse position (delta) between frames, which in turn is used to update the camera's orientation based on mouse movement.
    /// </summary>
    private Vector2? lastPosition;

    /// <summary>
    /// Updates the camera's position and orientation based on user input. This method should be called once per frame.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, used to ensure consistent movement speed regardless of frame rate.</param>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        Vector3 movement = Vector3.Zero;
        if (Window.IsKeyDown(Key.W)) movement += Front;
        if (Window.IsKeyDown(Key.S)) movement += Back;
        if (Window.IsKeyDown(Key.A)) movement += Left;
        if (Window.IsKeyDown(Key.D)) movement += Right;
        if (Window.IsKeyDown(Key.Space)) movement += Up;
        if (Window.IsKeyDown(Key.LeftShift)) movement += Down;
        if (movement != Vector3.Zero)
            Position += Vector3.Normalize(movement) * Speed * deltaTime;

        Vector2 mousePos = Window.GetCursorPosition();
        Vector2 delta = mousePos - (lastPosition ?? mousePos);
        lastPosition = mousePos;
        
        // screen Y grows downward, so an upward mouse move should increase pitch
        Rotate(delta.X * Sensitivity, -delta.Y * Sensitivity);
    }
}