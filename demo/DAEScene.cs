using System;
using System.Numerics;

namespace odl3d;

public class DAEScene : Scene3D
{
    public DAEScene(Window window) : base(window)
    {
        Model center = new Model(this, "assets/center/center.dae");
        center.Position = new Vector3(1, -1.25f, -1.75f);
        
        Model gym = new Model(this, "assets/gym/gym.dae");
        gym.Position = new Vector3(-3, -1.25f, -1.75f);
    }
}