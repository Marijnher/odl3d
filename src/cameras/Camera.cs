using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Represents a camera in a 3D scene, providing properties for position, orientation, field of view, aspect ratio, and near/far clipping planes. This class serves as a base for more specialized camera types, such as moveable or first-person cameras.
/// </summary>
public class Camera
{
    /// <summary>
    /// The window associated with the camera. This property holds a reference to the rendering window or context that the camera is linked to. It allows the camera to access window-specific information, such as dimensions and input events, which may be necessary for calculating the aspect ratio, handling user input, or other camera-related operations.
    /// </summary>
    public Window Window { get; protected set; }

    /// <summary>
    /// The position of the camera in world space. This vector defines where the camera is located in the 3D scene. The default position is at the origin (0, 0, 0).
    /// </summary>
    public Vector3 Position { get; set; } = Vector3.Zero;

    /// <summary>
    /// The yaw angle of the camera in degrees, representing rotation around the Y-axis. A yaw of -90 degrees points the camera down the negative Z-axis, which is a common default orientation in 3D graphics. The yaw can be adjusted to rotate the camera left or right.
    /// </summary>
    public float Yaw { get; set; } = -90f; // -90 faces down -Z, matching the previous fixed camera

    /// <summary>
    /// The pitch angle of the camera in degrees, representing rotation around the X-axis. The pitch is clamped between -89 and 89 degrees to prevent gimbal lock and unnatural flipping of the camera view. A pitch of 0 degrees means the camera is level, while positive values tilt the camera upward and negative values tilt it downward.
    /// </summary>
    public float Pitch { get; set; } = 0f;

    /// <summary>
    /// The field of view (FOV) of the camera in degrees, defining the vertical angle of the camera's view frustum. A typical FOV for a perspective camera is around 60 degrees, which provides a natural perspective without excessive distortion. The FOV can be adjusted to zoom in or out on the scene.
    /// </summary>
    public float FieldOfViewDegrees { get; set; } = 60f;

    /// <summary>
    /// The aspect ratio of the camera's view, defined as the width divided by the height of the viewport. The default aspect ratio is 4:3, but it should be set to match the actual dimensions of the rendering window or viewport to avoid distortion in the rendered scene.
    /// </summary>
    public float AspectRatio { get; set; }

    /// <summary>
    /// The near clipping plane distance for the camera's view frustum. Objects closer than this distance will not be rendered. The default value is 0.1 units, which is a common choice for 3D rendering to avoid clipping artifacts with nearby objects.
    /// </summary>
    public float NearPlane { get; set; } = 0.1f;

    /// <summary>
    /// The far clipping plane distance for the camera's view frustum. Objects farther than this distance will not be rendered. The default value is 100 units, which is a typical choice for 3D rendering to limit the depth of the scene and improve performance.
    /// </summary>
    public float FarPlane { get; set; } = 100f;

    /// <summary>
    /// Calculates and returns the normalized forward direction vector of the camera based on its current yaw and pitch angles. This vector points in the direction the camera is facing in world space. The calculation uses trigonometric functions to convert the yaw and pitch angles from degrees to a 3D direction vector, which is then normalized to ensure it has a length of 1. This direction vector can be used for movement, raycasting, or other operations that require knowledge of where the camera is looking.
    /// </summary>
    public Vector3 Front
    {
        get
        {
            float yawRad = Yaw * MathF.PI / 180f;
            float pitchRad = Pitch * MathF.PI / 180f;
            Vector3 direction = new Vector3(
                MathF.Cos(yawRad) * MathF.Cos(pitchRad),
                MathF.Sin(pitchRad),
                MathF.Sin(yawRad) * MathF.Cos(pitchRad));
            return Vector3.Normalize(direction);
        }
    }

    /// <summary>
    /// Calculates and returns the normalized backward direction vector of the camera, which is the opposite of the Front vector. This vector points directly behind the camera in world space and can be used for movement or orientation purposes. It is computed by negating the Front vector, ensuring that it maintains a length of 1 and accurately represents the backward direction relative to the camera's current orientation.
    /// </summary>
    public Vector3 Back => -Front;
    
    /// <summary>
    /// Calculates and returns the normalized left direction vector of the camera, which is perpendicular to both the Front vector and the global up direction (Y-axis). This vector points to the left side of the camera in world space and can be used for strafing or lateral movement. It is computed using the cross product of the global up vector (0, 1, 0) and the Front vector, ensuring that it is orthogonal to both and has a length of 1.
    /// </summary>
    public Vector3 Left => Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Front));

    /// <summary>
    /// Calculates and returns the normalized right direction vector of the camera, which is perpendicular to both the Front vector and the global up direction (Y-axis). This vector points to the right side of the camera in world space and can be used for strafing or lateral movement. It is computed using the cross product of the Front vector and the global up vector (0, 1, 0), ensuring that it is orthogonal to both and has a length of 1.
    /// </summary>
    public Vector3 Right => -Left;

    /// <summary>
    /// Calculates and returns the normalized upward direction vector of the camera, which is perpendicular to both the Right vector and the Front vector. This vector points directly above the camera in world space and can be used for vertical movement or orientation purposes. It is computed using the cross product of the Right vector and the Front vector, ensuring that it is orthogonal to both and has a length of 1.
    /// </summary>
    public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Front));

    /// <summary>
    /// Calculates and returns the normalized downward direction vector of the camera, which is the opposite of the Up vector. This vector points directly below the camera in world space and can be used for vertical movement or orientation purposes. It is computed by taking the cross product of the Front vector and the Right vector, ensuring that it is orthogonal to both and has a length of 1.
    /// </summary>
    public Vector3 Down => -Up;

    /// <summary>
    /// Initializes a new instance of the Camera class using the specified window. The aspect ratio is automatically calculated based on the window's width and height.
    /// </summary>
    /// <param name="window">The window associated with the camera, used to determine the aspect ratio and access window-specific information.</param>
    public Camera(Window window) : this(window, (float) window.Width / window.Height) { }

    /// <summary>
    /// Creates a new Camera instance with the specified width and height, calculating the aspect ratio based on these dimensions. The aspect ratio is set to the width divided by the height, which is essential for proper perspective projection in 3D rendering. This constructor allows for easy initialization of the camera with a specific viewport size, ensuring that the rendered scene maintains the correct proportions and avoids distortion.
    /// </summary>
    /// <param name="window">The window associated with the camera, used to determine the aspect ratio and access window-specific information.</param>
    /// <param name="width">The width of the viewport for the camera.</param>
    /// <param name="height">The height of the viewport for the camera.</param>
    public Camera(Window window, int width, int height) : this(window, (float) width / height) { }

    /// <summary>
    /// Initializes a new instance of the Camera class using the specified window and aspect ratio. This constructor allows for explicit control over the camera's aspect ratio, which is important for maintaining the correct perspective projection in 3D rendering. The aspect ratio is typically set to the width of the viewport divided by its height.
    /// </summary>
    /// <param name="window">The window associated with the camera, used to determine the aspect ratio and access window-specific information.</param>
    /// <param name="aspectRatio">The aspect ratio of the camera's viewport, typically set to the width divided by the height of the viewport.</param>
    public Camera(Window window, float aspectRatio)
    {
        Window = window;
        AspectRatio = aspectRatio;
    }

    /// <summary>
    /// Rotates the camera by the specified yaw and pitch deltas, updating its orientation. The yaw delta rotates the camera around the Y-axis (left/right), while the pitch delta rotates it around the X-axis (up/down). The pitch is clamped between -89 and 89 degrees to prevent gimbal lock and unnatural flipping of the camera view. This method allows for smooth camera rotation based on user input, such as mouse movement, enabling a first-person or free-look camera experience in a 3D scene.
    /// </summary>
    /// <param name="yawDelta">The change in yaw (rotation around the Y-axis) in degrees.</param>
    /// <param name="pitchDelta">The change in pitch (rotation around the X-axis) in degrees.</param>
    public void Rotate(float yawDelta, float pitchDelta)
    {
        Yaw += yawDelta;
        Pitch = Math.Clamp(Pitch + pitchDelta, -89f, 89f);
    }

    /// <summary>
    /// Calculates and returns the view matrix for the camera, which transforms world coordinates into camera (view) space. The view matrix is constructed using the camera's position and its forward direction (Front vector), along with the global up direction (Y-axis). This matrix is essential for rendering 3D scenes from the camera's perspective, as it defines how objects in the world are projected onto the screen based on the camera's location and orientation.
    /// </summary>
    /// <returns>The view matrix representing the camera's transformation from world space to view space.</returns>
    public Matrix4x4 GetViewMatrix() =>
        Matrix4x4.CreateLookAt(Position, Position + Front, Vector3.UnitY);

    /// <summary>
    /// Calculates and returns the perspective projection matrix for the camera, which defines how 3D points are projected onto a 2D screen. The projection matrix is based on the camera's field of view (FOV), aspect ratio, and near/far clipping planes. It transforms coordinates from camera space to normalized device coordinates, enabling proper perspective rendering of 3D scenes. This matrix is essential for creating a realistic sense of depth and scale in the rendered scene.
    /// </summary>
    /// <returns>The perspective projection matrix representing the camera's projection from 3D world space to 2D screen space.</returns>
    public Matrix4x4 GetProjectionMatrix() =>
        Matrix4x4.CreatePerspectiveFieldOfView(FieldOfViewDegrees * MathF.PI / 180f, AspectRatio, NearPlane, FarPlane);

    /// <summary>
    /// Updates the camera's state based on the elapsed time since the last update. This method is intended to be overridden by derived camera classes that have dynamic behavior, such as movable cameras that respond to user input. The deltaTime parameter represents the time elapsed since the last update, allowing for frame-rate-independent movement and animation.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime) { }
}
