using UnityEngine;

namespace ToyBox.Infrastructure.UI;
public static partial class UI {
    private static GUIStyle? m_CachedProgressBarStyle = null;
    public static void ProgressBar(double progress, string label) {
        Label(label + $" {progress * 100:F2}%");
        Rect progressRect = GUILayoutUtility.GetRect(200, 20);
        GUI.Box(progressRect, "");

        float fillWidth = (float)(progress * progressRect.width);
        Rect fillRect = new Rect(progressRect.x, progressRect.y, fillWidth, progressRect.height);

        if (m_CachedProgressBarStyle == null) {
            m_CachedProgressBarStyle = new GUIStyle(GUI.skin.box);
            Texture2D greenTexture = new Texture2D(1, 1);
            greenTexture.SetPixel(0, 0, Color.green);
            greenTexture.Apply();
            m_CachedProgressBarStyle.normal.background = greenTexture;
        }
        GUI.Box(fillRect, GUIContent.none, m_CachedProgressBarStyle);
    }
}
