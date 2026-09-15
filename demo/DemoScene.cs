using System;
using System.Numerics;
using odl3d;
using odl3d.Renderer;

namespace odl3ddemo;

class DemoScene : Scene3D
{
    public DemoScene(Window window) : base(window)
    {
        Texture texture = Texture.FromCheckerboard(64, 64, (255, 0, 255), (0, 255, 255), 8);
        Sprite3D sprite = new Sprite3D(this, texture)
        {
            Position = new Vector3(-1.25f, 0, -3),
            Scale = new Vector3(1, 1.5f, 1)
        };

        Model cube = Model.LoadOBJ(this, "assets/cube.obj");
        cube.Texture = texture;
        cube.Position = new Vector3(2f, 0, -3);
        cube.Scale = new Vector3(1.5f, 1.5f, 1.5f);
        cube.Color = Color.Red;

        Plane3D plane = new Plane3D(this, 0.5f, 0.5f, 0.1f)
        {
            Position = new Vector3(-1.5f, 1.5f, -3),
            Color = Color.Magenta
        };

        Mesh groundMesh = MeshBuilder.CreatePlane(20f, 0.2f, 20f, 50f, 50f);
        Object3D ground = new Object3D(this, groundMesh, new Texture("assets/grass.png"))
        {
            Position = new Vector3(-10, -1.5f, -10)
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

        Mesh sphereMesh = MeshBuilder.CreateSphere(0.2f, 16);
        Object3D sphere = new Object3D(this, sphereMesh, Texture.FromColor(1, 1, Color.Yellow))
        {
            Position = new Vector3(-0.2f, -1f, 0.2f)
        };
    }
}