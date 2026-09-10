namespace odl3d;

/// <summary>
/// A 3D sprite is a simple quad mesh with a texture applied, used for billboarding or other 3D effects. It is a subclass of Object that uses the built-in quad mesh and a specified texture.
/// </summary>
public class Sprite3D : Object
{
    /// <summary>
    /// Creates a new Sprite3D with the given texture. The sprite uses a built-in quad mesh and can be positioned, scaled, and rotated in 3D space like any other Object.
    /// </summary>
    /// <param name="scene">The scene to which this sprite belongs.</param>
    /// <param name="texture">The texture to use for the sprite.</param>
    public Sprite3D(Scene<Object> scene, Texture? texture = null) : base(scene, Mesh.Quad, texture) { }
}
