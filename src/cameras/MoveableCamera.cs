using System;
using System.Numerics;

namespace odl3d;

public class MoveableCamera : Camera
{
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