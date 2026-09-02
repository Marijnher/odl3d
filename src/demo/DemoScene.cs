using System;
using System.Numerics;

namespace odl3d;

class DemoScene : Scene3D
{
    public DemoScene(Window window) : base(window)
    {
        Texture texture = Texture.FromCheckerboard(64, 64, (255, 0, 255), (0, 255, 255), 8);
        Sprite3D sprite = new Sprite3D(this, texture)
        {
            Position = new Vector3(-1.5f, 0, -3),
            Scale = new Vector3(2, 2, 1)
        };

        string cubeFilename = System.IO.Path.Combine(AppContext.BaseDirectory, "assets", "cube.obj");
        Object cube = new Object(this, cubeFilename, texture)
        {
            Position = new Vector3(1.5f, 0, -3),
            Scale = new Vector3(1.5f, 1.5f, 1.5f),
            Color = Color.Red
        };

        Sprite3D canvas = new Sprite3D(this, Texture.FromColor(64, 64, Color.White))
        {
            Position = new Vector3(0, 1.5f, -3),
            Color = Color.Yellow, // Not used because the sprite has a texture.
            TextureColor = Color.Aqua // Applied to the texture, tinting it with an aqua color.
        };
    }
}