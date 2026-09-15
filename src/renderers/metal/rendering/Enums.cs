using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public enum LoadAction : ulong
    {
        DontCare = 0,
        Load = 1,
        Clear = 2
    }

    public enum StoreAction : ulong
    {
        DontCare = 0,
        Store = 1
    }

    public enum CompareFunction : ulong
    {
        Never = 0,
        Less = 1,
        Equal = 2,
        LessEqual = 3,
        Greater = 4,
        NotEqual = 5,
        GreaterEqual = 6,
        Always = 7
    }
}