namespace odl3d;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

class UIScene : Scene2D
{
    private static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
    {
        ['0'] = new[] { "111", "101", "101", "101", "111" },
        ['1'] = new[] { "010", "110", "010", "010", "111" },
        ['2'] = new[] { "110", "001", "010", "100", "111" },
        ['3'] = new[] { "110", "001", "010", "001", "110" },
        ['4'] = new[] { "101", "101", "111", "001", "001" },
        ['5'] = new[] { "111", "100", "110", "001", "110" },
        ['6'] = new[] { "011", "100", "111", "101", "111" },
        ['7'] = new[] { "111", "001", "010", "010", "010" },
        ['8'] = new[] { "111", "101", "111", "101", "111" },
        ['9'] = new[] { "111", "101", "111", "001", "110" },
        [':'] = new[] { "000", "010", "000", "010", "000" },
        ['F'] = new[] { "111", "100", "110", "100", "100" },
        ['P'] = new[] { "110", "101", "110", "100", "100" },
        ['S'] = new[] { "011", "100", "010", "001", "110" },
        ['V'] = new[] { "101", "101", "101", "101", "010" },
        ['T'] = new[] { "111", "010", "010", "010", "010" },
        ['X'] = new[] { "101", "101", "010", "101", "101" },
    };

    private Texture fpsTexture;
    private readonly Sprite2D fpsSprite;
    private float fpsElapsed;
    private int fpsFrames;

    public UIScene(Window window) : base(window)
    {
        var topLeft = new Sprite2D(this, Texture.FromColor(32, 32, Color.Red));
        topLeft.Position = new Vector3(4, 4, 0);
        topLeft.RegisterMousePressInside(Mouse.Left, _ => topLeft.Dispose());

        var topRight = new Sprite2D(this, Texture.FromColor(32, 32, Color.Green));
        topRight.Position = new Vector3(window.Width - 36, 4, 0);
        topRight.RegisterMousePressInside(Mouse.Left, _ => topRight.Dispose());

        fpsTexture = new Texture(32, 14);
        fpsSprite = new Sprite2D(this, fpsTexture);
        fpsSprite.Scale = new Vector3(2, 2, 1);
        fpsSprite.Position = new Vector3(6, window.Height - fpsTexture.Height * fpsSprite.Scale.Y - 6, 1);
        UpdateFpsTexture(0, 0);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        fpsElapsed += deltaTime;
        fpsFrames++;

        if (fpsElapsed >= 0.25f)
        {
            UpdateFpsTexture(fpsFrames / fpsElapsed, GetVertexCount());
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
            foreach (Object sceneObject in scene.Objects)
            {
                if (sceneObject.Visible) vertexCount += sceneObject.VertexCount;
            }
        }
        return vertexCount;
    }

    private void UpdateFpsTexture(float fps, int vertexCount)
    {
        long displayedFps = Math.Max(0L, (long)Math.Round(fps));
        string[] lines = { $"FPS: {displayedFps}", $"VTX: {vertexCount}" };
        int requiredWidth = Math.Max(lines[0].Length, lines[1].Length) * 4;
        if (requiredWidth > fpsTexture.Width)
        {
            Texture oldTexture = fpsTexture;
            fpsTexture = new Texture(requiredWidth, 14);
            fpsSprite.Texture = fpsTexture;
            oldTexture.Dispose();
            fpsSprite.Position = new Vector3(6, Window.Height - fpsTexture.Height * fpsSprite.Scale.Y - 6, 1);
        }

        for (int i = 0; i < fpsTexture.Pixels.Length; i += 4)
        {
            fpsTexture.SetPixel((i / 4) % fpsTexture.Width, (i / 4) / fpsTexture.Width, 0, 0, 0, 0);
        }

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string text = lines[lineIndex];
            for (int characterIndex = 0; characterIndex < text.Length; characterIndex++)
            {
                if (!Font.TryGetValue(text[characterIndex], out string[]? glyph)) continue;
                int xOffset = characterIndex * 4;
                for (int y = 0; y < glyph.Length; y++)
                {
                    for (int x = 0; x < glyph[y].Length; x++)
                    {
                        if (glyph[y][x] == '1')
                            fpsTexture.SetPixel(xOffset + x, fpsTexture.Height - 2 - lineIndex * 7 - y, 255, 255, 255, 255);
                    }
                }
            }
        }
    }
};