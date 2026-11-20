// ============================================================
// GrabbableSpeaker.cs — Tugas Besar Kelompok 8 (bagian VR)
//
// Dipasang di speaker di VRScene. Fungsinya:
// - Saat speaker DIGRAB controller, pastikan musiknya menyala.
// - Tidak mengatur grab/physics secara manual: itu dikerjakan
//   komponen XR Grab Interactable + Rigidbody (lihat panduan
//   Tahap 6). Spatial audio dikerjakan AudioSource dengan
//   Spatial Blend = 1 — script ini hanya pemicu kecilnya.
//
// Untuk XR Interaction Toolkit versi 2.x (Unity 2022.3).
// ============================================================
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabbableSpeaker : MonoBehaviour
{
    public AudioSource audioSource;   // AudioSource speaker (Spatial Blend = 1)

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Musik menyala saat speaker dipegang (kalau belum menyala)
        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Musik SENGAJA dibiarkan menyala setelah dilempar:
        // justru di sini spatial audio-nya terasa — suara speaker
        // menjauh & berpindah arah mengikuti posisi jatuhnya.
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabbed);
            grab.selectExited.RemoveListener(OnReleased);
        }
    }
}
