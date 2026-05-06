using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLMouseContextBlocker
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Shard_BlockWebGLMouseContext();

    [DllImport("__Internal")]
    private static extern void Shard_SetWebGLCanvasCursorHidden(int hidden);
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Initialize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Shard_BlockWebGLMouseContext();
        Shard_SetWebGLCanvasCursorHidden(1);
#endif
    }
}
