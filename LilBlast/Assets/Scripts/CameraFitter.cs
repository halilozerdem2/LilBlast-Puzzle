using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    // Kenar boşluk oranları (ekran yüksekliği veya genişliği bazında)
    [Header("Safe Area Percentages")]
    [SerializeField] private float topMarginPercent = 0.35f;     // Üst panelden %40
    [SerializeField] private float bottomMarginPercent = 0.20f; // Alt panelden %15
    [SerializeField] private float sideMarginPercent = 0.02f;   // Yanlardan %5

    public void FitCameraToGrid(int gridWidth, int gridHeight, Transform gridParent)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        float screenAspect = (float)Screen.width / Screen.height;

        // 🟩 1) Ekranın world ölçüsünü al (kamera ortographicSize’a göre)
        float worldScreenHeight = mainCamera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * screenAspect;

        // 🟩 2) Safe area world ölçüsünü kenar boşluklarına göre belirle
        float safeAreaWorldWidth = worldScreenWidth * (1f - 2f * sideMarginPercent);
        float safeAreaWorldHeight = worldScreenHeight * (1f - topMarginPercent - bottomMarginPercent);

        // 🟩 3) Grid’e sığdırmak için scale hesapla
        float scaleX = safeAreaWorldWidth / gridWidth;
        float scaleY = safeAreaWorldHeight / gridHeight;
        float finalScale = Mathf.Min(scaleX, scaleY);

        // 🟩 4) Grid’i world koordinatına ortala
        Vector3 gridCenter = new Vector3(
            (gridWidth / 2f) - 0.5f,
            (gridHeight / 2f) - 0.5f,
            0f);

        // 🟩 5) Safe area sınırlarının alt kenar pozisyonunu bul
        float worldBottom = -mainCamera.orthographicSize + (worldScreenHeight * bottomMarginPercent);

        // 🟩 6) Safe area sınırlarının üst kenar pozisyonunu bul (debug için)
        float worldTop = mainCamera.orthographicSize - (worldScreenHeight * topMarginPercent);

        // 🟩 7) Grid’in world pozisyonunu alt kenar offset’ine göre ayarla
        float gridWorldY = Mathf.Lerp(worldBottom, worldTop, 0.65f);

        gridParent.position = new Vector3(0f, gridWorldY, 0f) - gridCenter * finalScale;
        gridParent.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}
