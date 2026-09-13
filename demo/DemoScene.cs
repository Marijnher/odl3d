using System;
using System.Numerics;

namespace odl3d.Demo;

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

        Model cube = Model.LoadOBJ(this, "assets/cube.obj");
        cube.Texture = texture;
        cube.Position = new Vector3(1.5f, 0, -3);
        cube.Scale = new Vector3(1.5f, 1.5f, 1.5f);
        cube.Color = Color.Red;

        Sprite3D canvas = new Sprite3D(this, Texture.FromColor(64, 64, Color.White))
        {
            Position = new Vector3(0, 1.5f, -3),
            Color = Color.Yellow, // Not used because the sprite has a texture.
            TextureColor = Color.Aqua // Applied to the texture, tinting it with an aqua color.
        };

        Plane plane = new Plane(this, 0.5f, 0.5f, 0.1f)
        {
            Position = new Vector3(-1.5f, 1.5f, -3),
            Color = Color.Magenta
        };

        Mesh groundMesh = Mesh.CreatePlane(20f, 0.2f, 20f, 50f, 50f);
        Object ground = new Object(this, groundMesh, new Texture("assets/grass.png"))
        {
            Position = new Vector3(0f, -1.5f, -5f)
        };
        ground.Texture!.WrapModeH = TextureWrap.Mirror;
        ground.Texture!.WrapModeV = TextureWrap.Mirror;

        Text3D helloWorld = new Text3D(this, Font.Get("arial", 96))
        {
            Position = new Vector3(0, 2, -2),
            Color = Color.Red,
            Content = "Hello world!",
            Depth = 0.05f
        };

        TextBillboard billboard = new TextBillboard(this, Font.Get("arial", 48))
        {
            Position = new Vector3(-1.5f, 3f, -2),
            Content = "Billboard"
        };
    }
}