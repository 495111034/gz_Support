using System;
using System.Runtime.InteropServices;

namespace FMOD.Studio
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PARAMETER_ID
    {
        public uint data1;  /* The first half of the ID. */
        public uint data2;  /* The second half of the ID. */
    }
}

namespace FMODUnity
{

    [Serializable]
    public class ParamRef
    {
        public string Name;
        public float Value;
        public FMOD.Studio.PARAMETER_ID ID;
    }
}
