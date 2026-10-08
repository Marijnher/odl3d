using System;
using System.Numerics;
using odl3d;

namespace odl3d.Demo;

class UIScene : Scene2D
{
    private Text2D infoText;
    private float fpsElapsed;
    private int fpsFrames;

    public UIScene(Window window) : base(window)
    {
        var topLeft = new Sprite2D(TextureBuilder.CreateSolid(32, 32, Color.Red))
        {
            Position = new Vector3(4, 4, 0)
        };
        Add(topLeft);

        var topRight = new Sprite2D(TextureBuilder.CreateSolid(32, 32, Color.Green))
        {
            Position = new Vector3(window.Width - 36, 4, 0)
        };
        Add(topRight);

        infoText = new Text2D("arial", 16);
        infoText.Position = new Vector3(6, window.Height - infoText.MeasureString().Y - 18, 1);
        Add(infoText);
        UpdateInfoText(0, 0);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        fpsElapsed += deltaTime;
        fpsFrames++;

        if (fpsElapsed >= 0.25f)
        {
            UpdateInfoText(fpsFrames / fpsElapsed, GetVertexCount());
            fpsElapsed = 0;
            fpsFrames = 0;
        }
    }

    private int GetVertexCount()
    {
        int vertexCount = 0;
        foreach (Scene3D scene in Window.Scenes3D)
        {
            if (!scene.Visible) continue;
            foreach (Object3D sceneObject in scene.Objects)
            {
                if (sceneObject.Visible) vertexCount += sceneObject.VertexCount;
            }
        }
        foreach (Scene2D scene2D in Window.Scenes2D)
        {
            if (!scene2D.Visible) continue;
            foreach (Object3D sceneObject in scene2D.Objects)
            {
                if (sceneObject.Visible) vertexCount += sceneObject.VertexCount;
            }
        }
        return vertexCount;
    }

    private void UpdateInfoText(float fps, int vertexCount)
    {
        long displayedFps = Math.Max(0L, (long)Math.Round(fps));
        infoText.Content = $"FPS: {displayedFps}\nVTX: {vertexCount}";
    }
};