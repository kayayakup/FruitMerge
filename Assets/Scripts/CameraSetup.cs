// ============================================================
//  CameraSetup.cs
// ============================================================
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    public Camera MainCamera       { get; private set; }
    public Camera ScreenshotCamera { get; private set; }

    public void Initialize()
    {
        // ── Main camera ───────────────────────────────────────
        var mainGO = new GameObject("MainCamera");
        mainGO.tag = "MainCamera";
        MainCamera = mainGO.AddComponent<Camera>();
        MainCamera.orthographic     = true;
        MainCamera.orthographicSize = GameConfig.CamOrthoSize;
        MainCamera.transform.position = new Vector3(0f, GameConfig.CamY, -10f);
        MainCamera.backgroundColor  = new Color(0.97f, 0.93f, 0.97f);
        MainCamera.clearFlags       = CameraClearFlags.SolidColor;
        MainCamera.depth            = 0;
        mainGO.AddComponent<AudioListener>();

        // ── Screenshot / capture camera ───────────────────────
        var ssGO = new GameObject("ScreenshotCamera");
        ScreenshotCamera = ssGO.AddComponent<Camera>();
        ScreenshotCamera.orthographic     = true;
        ScreenshotCamera.orthographicSize = GameConfig.CamOrthoSize;
        ScreenshotCamera.transform.position = new Vector3(0f, GameConfig.CamY, -10f);
        ScreenshotCamera.backgroundColor  = new Color(0.97f, 0.93f, 0.97f);
        ScreenshotCamera.clearFlags       = CameraClearFlags.SolidColor;
        ScreenshotCamera.depth            = -1;
        ScreenshotCamera.enabled          = false;  // only used for capture
    }
}