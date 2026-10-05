using System;
using System.Numerics;
using odl3d;

namespace odl3d.Demo;

public class ModelScene : Scene3D
{
    public ModelScene(Window window) : base(window)
    {
        Model center = Model.LoadDAE("assets/center/center.dae");
        center.Position = new Vector3(1, -1.25f, -1.75f);
        Add(center);
        
        Model gym = Model.LoadDAE("assets/gym/gym.dae");
        gym.Position = new Vector3(-3, -1.25f, -1.75f);
        Add(gym);

        Model dragonGym = Model.LoadDAE("assets/dragon_gym/c8_gym_01.dae");
        dragonGym.Position = new Vector3(3, -1.25f, -1.75f);
        Add(dragonGym);

        Model orasCenter = Model.LoadOBJ("assets/oras_center/Pokemon_center.obj");
        orasCenter.Position = new Vector3(-3.5f, -0.25f, 2f);
        orasCenter.Scale = new Vector3(0.02f, 0.02f, 0.02f);
        orasCenter.Rotation = new Vector3(0, 90, 0);
        Add(orasCenter);

        Model lacunosa = Model.LoadDAE("assets/lacunosa/Lacunosa town.dae");
        lacunosa.Position = new Vector3(14f, -1.25f, -2f);
        Add(lacunosa);
    }
}