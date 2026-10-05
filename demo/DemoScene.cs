using System;
using System.Numerics;
using odl3d;
using odl3d.Renderer;

namespace odl3ddemo;

class DemoScene : Scene3D
{
    public DemoScene(Window window) : base(window)
    {
        Texture texture = TextureBuilder.CreateCheckerboard(64, 64, new Color(255, 0, 255), new Color(0, 255, 255), 8);
        Sprite3D sprite = new Sprite3D(texture)
        {
            Position = new Vector3(-1.25f, 0, -3),
            Scale = new Vector3(1, 1.5f, 1)
        };
        Add(sprite);

        Model cube = Model.LoadOBJ("assets/cube.obj");
        cube.Texture = texture;
        cube.Position = new Vector3(2f, 0, -3);
        cube.Scale = new Vector3(1.5f, 1.5f, 1.5f);
        cube.Color = Color.Red;
        Add(cube);

        Plane3D plane = new Plane3D(0.5f, 0.5f, 0.1f)
        {
            Position = new Vector3(-1.5f, 1.5f, -3),
            Color = Color.Magenta
        };
        Add(plane);

        Mesh groundMesh = MeshBuilder.CreatePlane(20f, 0.2f, 20f, 50f, 50f);
        Texture grassTexture = new Texture("assets/grass.png");
        Object3D ground = new Object3D(groundMesh, grassTexture)
        {
            Position = new Vector3(-10, -1.5f, -10)
        };
        Add(ground);
        ground.Sampler.WrapU = TextureWrap.Mirror;
        ground.Sampler.WrapV = TextureWrap.Mirror;

        Text3D helloWorld = new Text3D(Font.Get("arial", 96))
        {
            Position = new Vector3(0, 2, -2),
            Color = Color.Red,
            Content = "Hello world!",
            Depth = 0.05f
        };
        Add(helloWorld);

        TextBillboard billboard = new TextBillboard(Font.Get("arial", 48))
        {
            Position = new Vector3(-1.5f, 3f, -2),
            Content = "Billboard"
        };
        Add(billboard);

        Mesh sphereMesh = MeshBuilder.CreateSphere(0.2f, 16);
        Object3D sphere = new Object3D(sphereMesh, TextureBuilder.CreateSolid(1, 1, Color.Yellow))
        {
            Position = new Vector3(-0.2f, -1f, 0.2f)
        };
        Add(sphere);
    }
}