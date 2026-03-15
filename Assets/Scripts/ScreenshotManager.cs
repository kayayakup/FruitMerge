// ============================================================
//  ScreenshotManager.cs  —  uses screen aspect ratio
// ============================================================
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
    private Camera captureCamera;

    public void Initialize(Camera mainCam)
    {
        captureCamera = mainCam;
    }

    /// <summary>Renders the current frame into a Texture2D and returns it.</summary>
    public Texture2D Capture()
    {
        int w = Mathf.Min(Screen.width,  720);
        int h = Mathf.RoundToInt(w * ((float)Screen.height / Screen.width));

        var rt       = new RenderTexture(w, h, 16);
        var snapshot = new Texture2D(w, h, TextureFormat.RGB24, false);

        var prev = captureCamera.targetTexture;
        captureCamera.targetTexture = rt;
        captureCamera.Render();
        captureCamera.targetTexture = prev;

        RenderTexture.active = rt;
        snapshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        snapshot.Apply();
        RenderTexture.active = null;

        rt.Release();
        Object.Destroy(rt);

        return snapshot;
    }
}