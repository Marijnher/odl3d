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
        orasCenter.Position = new Vector3(-3.5f, -0.25f, 2f);
        orasCenter.Scale = new Vector3(0.02f, 0.02f, 0.02f);
        orasCenter.Rotation = new Vector3(0, 90, 0);

        Model lacunosa = Model.LoadDAE(this, "assets/lacunosa/Lacunosa town.dae");
        lacunosa.Position = new Vector3(14f, -1.25f, -2f);

        RegisterKeyDown(Key.C, () => {
            Console.WriteLine("Rotation...");
            orasCenter.Rotation.Y += 1f;
        });

    }
}