using System;
using System.Numerics;

namespace odl3d;

public class ModelScene : Scene3D
{
    public ModelScene(Window window) : base(window)
    {
        Model center = Model.LoadDAE(this, "assets/center/center.dae");
        center.Position = new Vector3(1, -1.25f, -1.75f);
        
        Model gym = Model.LoadDAE(this, "assets/gym/gym.dae");
        gym.Position = new Vector3(-3, -1.25f, -1.75f);

        Model dragonGym = Model.LoadDAE(this, "assets/dragon_gym/c8_gym_01.dae");
        dragonGym.Position = new Vector3(3, -1.25f, -1.75f);

        Model orasCenter = Model.LoadOBJ(this, "assets/oras_center/Pokemon_center.obj");
        //Mesh mesh = ObjLoader.Load("assets/oras_center/Pokemon_center.obj");
        //Object orasCenter = new Object(this, mesh, Texture.FromColor(32, 32, Color.Yellow));
        orasCenter.Position = new Vector3(-3.5f, -0.25f, 2f);
        orasCenter.Scale = new Vector3(0.02f, 0.02f, 0.02f);
        orasCenter.Rotation = new Vector3(0, 90, 0);

        RegisterKeyDown(Key.C, () => {
            Console.WriteLine("Rotation...");
            orasCenter.Rotation.Y += 0.1f;
        });
    }
}