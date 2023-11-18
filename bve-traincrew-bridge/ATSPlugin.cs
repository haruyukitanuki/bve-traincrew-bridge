using System.Runtime.InteropServices;

internal static class AtsPlugin
{

    const string dllname = "TASC.dll";

    [StructLayout(LayoutKind.Sequential)]
    public struct ATS_VEHICLESPEC
    {
        public int BrakeNotches;
        public int PowerNotches;
        public int AtsNotch;
        public int B67Notch;
        public int Cars;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ATS_VEHICLESTATE
    {
        public double Location;
        public float Speed;
        public int Time;
        public float BcPressure;
        public float MrPressure;
        public float ErPressure;
        public float BpPressure;
        public float SapPressure;
        public float Current;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ATS_BEACONDATA
    {
        public int Type;
        public int Signal;
        public float Distance;
        public int Optional;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ATS_HANDLES
    {
        public int Brake;
        public int Power;
        public int Reverser;
        public int ConstantSpeed;
    }

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void Load();

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void Dispose();

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern int GetPluginVersion();

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void SetVehicleSpec(ATS_VEHICLESPEC vehicleSpec);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void Initialize(int initPos);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern ATS_HANDLES Elapse(ATS_VEHICLESTATE vehicleState, int[] panel, int[] sound);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void SetPower(int power);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void SetBrake(int brake);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void SetReverser(int reverser);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void KeyDown(int key);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void KeyUp(int key);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void HornBlow(int hornType);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void DoorOpen();

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void DoorClose();

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void SetSignal(int signalType);

    [DllImport(dllname, CallingConvention = CallingConvention.StdCall)]
    public static extern void SetBeaconData(ATS_BEACONDATA beaconData);
}