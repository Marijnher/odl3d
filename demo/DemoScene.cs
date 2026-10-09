using System;
using System.Numerics;
using odl3d;
using odl3d.Renderer;

namespace odl3d.Demo;

class DemoScene : Scene3D
{
    public DemoScene(Window window, Light sun) : base(window)
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
            Position = new Vector3(-10, -1.5f, -10),
        };
        ground.Sampler.WrapU = TextureWrap.Mirror;
        ground.Sampler.WrapV = TextureWrap.Mirror;
        Add(ground);

        Text3D helloWorld = new Text3D("arial", 96)
        {
            Position = new Vector3(0, 2, -2),
            Color = Color.Red,
            Content = "Hello world!",
            Depth = 0.05f
        };
        Add(helloWorld);

        TextBillboard billboard = new TextBillboard("arial", 48)
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

        // Lighting: an orbiting point light with a glowing marker, plus a rectangular area light with an emissive panel.
        AmbientColor = new Color(60, 60, 60);
        AddLight(sun);
        _orbitLight = new PointLight(new Vector3(0, 0.5f, -1.5f), new Color(255, 200, 150), intensity: 6f, range: 8f);
        AddLight(_orbitLight);
        _orbitMarker = new Object3D(MeshBuilder.CreateSphere(0.08f, 12))
        {
            Color = Color.Black,
            Emissive = new Color(255, 200, 150),
            Lit = true
        };
        Add(_orbitMarker);

        const float panelWidth = 2f, panelHeight = 1f;
        Vector3 panelCenter = new(0f, 0.5f, -5f);
        AddLight(new AreaLight(panelCenter, panelWidth, panelHeight, new Color(150, 200, 255), intensity: 4f) { Range = 10f });
        Add(new Object3D(MeshBuilder.CreateQuad())
        {
            Position = panelCenter - new Vector3(panelWidth / 2f, panelHeight / 2f, 0f),
            Scale = new Vector3(panelWidth, panelHeight, 1f),
            Color = Color.Black,
            Emissive = new Color(150, 200, 255)
        });
    }

    private readonly PointLight _orbitLight;
    private readonly Object3D _orbitMarker;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        float angle = (float) Window.GetTime();
        _orbitLight.Position = new Vector3(MathF.Cos(angle) * 2.5f, 0.5f + MathF.Sin(angle * 0.7f), -2f + MathF.Sin(angle) * 1.5f);
        _orbitMarker.Position = _orbitLight.Position;
    }
}