using System.IO;
using UnityEditor;
using UnityEngine;

// Agent tooling: renders the overlay canvas of the running Game to a PNG without depending on the Game View being repainted.
// The file name is read from Temp/capture_name.txt; the result lands in Assets/Screenshots (ignored by tests, deleted after use).
public static class CanvasCapture
{
    [MenuItem("Tools/UI/Capture canvas")]
    public static void Capture()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Capture canvas: enter Play mode first."); return; }
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var cam = Camera.main;
        if (canvas == null || cam == null) return;
        var name = File.Exists("Temp/capture_name.txt") ? File.ReadAllText("Temp/capture_name.txt").Trim() : "canvas";
        var rt = new RenderTexture(1920, 1080, 24);
        var oldMode = canvas.renderMode; var oldCam = canvas.worldCamera; var oldTarget = cam.targetTexture;
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 1;
        cam.targetTexture = rt; cam.Render();
        var prev = RenderTexture.active; RenderTexture.active = rt;
        var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
        RenderTexture.active = prev;
        cam.targetTexture = oldTarget; canvas.renderMode = oldMode; canvas.worldCamera = oldCam;
        Directory.CreateDirectory("Assets/Screenshots");
        File.WriteAllBytes("Assets/Screenshots/" + name + ".png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex); rt.Release();
        Debug.Log("Canvas captured: " + name);
    }
}
