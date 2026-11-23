// ============================================================
// TerapkanWarnaTersimpan.cs — Tugas Besar Kelompok 8 (VRScene)
//
// Dipasang di Speaker3D di VRScene (sekali saja).
// Fungsinya: saat ruang dengar dibuka, baca warna cincin
// terakhir yang dipilih user di AR (disimpan VirtualButton-
// Manager lewat PlayerPrefs kunci "WarnaCincin") lalu terapkan
// ke RingLampu speaker VR. Jadi warna pilihan di brosur
// TERBAWA masuk ke ruang VR.
//
// Cara pasang (di VRScene):
// 1. Klik Speaker3D -> Add Component -> Terapkan Warna Tersimpan.
// 2. Slot Ring Renderer  <- drag RingLampu (anak Speaker3D).
// 3. Slot Color Materials: Size = 3, isi berurutan dengan
//    material Warna1, Warna2, Warna3 dari Assets/Materials.
// ============================================================
using UnityEngine;

public class TerapkanWarnaTersimpan : MonoBehaviour
{
    public Renderer ringRenderer;      // MeshRenderer RingLampu di speaker VR
    public Material[] colorMaterials;  // Warna1, Warna2, Warna3 (urutan sama seperti di AR)

    void Start()
    {
        if (ringRenderer == null || colorMaterials == null || colorMaterials.Length == 0) return;

        int idx = PlayerPrefs.GetInt("WarnaCincin", 0);
        if (idx < 0 || idx >= colorMaterials.Length) idx = 0;
        if (colorMaterials[idx] != null)
            ringRenderer.material = colorMaterials[idx];
    }
}
