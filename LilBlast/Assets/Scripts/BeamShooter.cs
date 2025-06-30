using UnityEngine;
using UnityEngine.VFX;

public class BeamShooter : MonoBehaviour
{
    [Header("Bağlantılar")]
    public VisualEffect beamVFX;

    [Header("Bağlantı Noktaları")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Işın Süresi")]
    public float destroyDelay = 0.5f; // Beam süresi bitince yok olma

    private void Awake()
    {
        // Eğer Inspector'da atanmadıysa otomatik bul
        if (beamVFX == null)
            beamVFX = GetComponent<VisualEffect>();
    }

    private void OnEnable()
    {

        beamVFX.SendEvent("OnPlay");

    }

    private void Update()
    {
        // Eğer beam uçarken nokta hareket ediyorsa pozisyonu güncel tutmak istersen:
        beamVFX.SetVector3("StartPosition", startPoint.position);
        beamVFX.SetVector3("EndPosition", endPoint.position);
    }
    
}