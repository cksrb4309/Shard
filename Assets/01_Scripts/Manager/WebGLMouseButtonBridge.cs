using System.Runtime.InteropServices;

public static class WebGLMouseButtonBridge
{
    const int RightButton = 2;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int Shard_ConsumeWebGLMouseButtonDown(int button);

    [DllImport("__Internal")]
    private static extern void Shard_ClearWebGLMouseButtonDown(int button);

    [DllImport("__Internal")]
    private static extern int Shard_IsWebGLMouseButtonPressed(int button);
#endif

    public static bool ConsumeRightButtonDown()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return Shard_ConsumeWebGLMouseButtonDown(RightButton) != 0;
#else
        return false;
#endif
    }

    public static void ClearRightButtonDown()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Shard_ClearWebGLMouseButtonDown(RightButton);
#endif
    }

    public static bool IsRightButtonPressed()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return Shard_IsWebGLMouseButtonPressed(RightButton) != 0;
#else
        return false;
#endif
    }
}
