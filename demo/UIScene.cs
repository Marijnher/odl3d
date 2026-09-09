namespace odl3d;

using System;
using System.Drawing;
using System.Numerics;

class UIScene : Scene2D
{
    public UIScene(Window window) : base(window)
    {
        var topLeft = new Sprite2D(this, Texture.FromColor(32, 32, Color.Red));
        topLeft.Position = new Vector3(4, 4, 0);
        topLeft.RegisterMousePressInside(Mouse.Left, _ => topLeft.Dispose());

        var topRight = new Sprite2D(this, Texture.FromColor(32, 32, Color.Green));
        topRight.Position = new Vector3(window.Width - 36, 4, 0);
        topRight.RegisterMousePressInside(Mouse.Left, _ => topRight.Dispose());

        var bottomLeft = new Sprite2D(this, Texture.FromColor(32, 32, Color.Blue));
        bottomLeft.Position = new Vector3(4, window.Height - 36, 0);
        bottomLeft.RegisterMousePressInside(Mouse.Left, _ => bottomLeft.Dispose());

        var bottomRight = new Sprite2D(this, Texture.FromColor(32, 32, Color.Yellow));
        bottomRight.Position = new Vector3(window.Width - 36, window.Height - 36, 0);
        bottomRight.RegisterMousePressInside(Mouse.Left, _ => bottomRight.Dispose());
    }
};