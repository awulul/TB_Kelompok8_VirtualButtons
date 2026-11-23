// ============================================================
// VirtualButtonManager.cs — Tugas Besar Kelompok 8 (bagian AR)
// VERSI Vuforia 9.6.3 (polling)
//
// Kenapa 9.6.3? Class VirtualButtonBehaviour SUDAH DIHAPUS dari
// Vuforia 11 (compile error CS0246 terbukti di project ini), dan
// Vuforia 10 tidak lagi bisa didownload dari portal. Vuforia
// 9.6.3 diambil sebagai file .tgz dari registry resmi PTC dan
// dipasang lewat Package Manager -> Add package from tarball.
//
// Di 9.6.3, interface IVirtualButtonEventHandler dan
// ITrackableEventHandler TIDAK tersedia (CS0246 terbukti), jadi
// script ini memakai POLLING: tiap frame membaca properti
// VirtualButtonBehaviour.Pressed dan ImageTargetBehaviour
// .CurrentStatus. Hasilnya sama: tombol terdeteksi saat area
// tombol di brosur ditutupi jari.
//
// Tombol didefinisikan PERMANEN di editor sebagai
// VirtualButtonBehaviour anak ImageTarget (BtnSuara, BtnWarna,
// BtnVR), areanya diatur di Scene view menutupi kotak tombol
// pada gambar brosur.
//
// Revisi 21 Nov 2026: tombol TIDAK lagi dibuat saat runtime.
// CreateVirtualButton terbukti selalu gagal di kombinasi
// Vuforia 9.6.3 + dataset DB_Brosur ini (diuji 6 konvensi
// koordinat/posisi, dengan template, dengan coba-ulang —
// semuanya null / "Could not create Virtual Button").
// Solusinya: ketiga tombol didefinisikan permanen di editor
// sebagai VirtualButtonBehaviour anak ImageTarget bernama
// BtnSuara, BtnWarna, BtnVR, dan script ini tinggal mencari
// + membaca status Pressed-nya.
//
// Cara pakai:
// 1. Buat dulu 3 Virtual Button di editor (lihat panduan
//    Tahap 4 Bagian 5 revisi).
// 2. Pasang script ini di GameObject kosong "ARManager".
// 3. Hubungkan semua slot di Inspector (lihat panduan Tahap 4).
// 4. Tombol "ditekan" dengan cara MENUTUPI area tombol di brosur
//    dengan jari selama +-0,5 detik (bukan menyentuh layar HP).
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class VirtualButtonManager : MonoBehaviour
{
    [Header("Target & Model")]
    public ImageTargetBehaviour imageTarget;   // objek ImageTarget di Hierarchy
    public GameObject speakerModel;            // Speaker3D (anak ImageTarget)
    public AudioSource speakerAudio;           // AudioSource di Speaker3D
    public Renderer speakerRenderer;           // MeshRenderer RingLampu
    public Material[] colorMaterials;          // Warna1, Warna2, Warna3

    [Header("UI")]
    public GameObject vrInfoPanel;             // PanelVR (awalnya nonaktif)
    public Text statusText;                    // StatusText di Canvas

    private VirtualButtonBehaviour vbSuara;
    private VirtualButtonBehaviour vbWarna;
    private VirtualButtonBehaviour vbVr;
    private bool prevSuara, prevWarna, prevVr;
    private TrackableBehaviour.Status prevStatus;
    private bool statusReady = false;

    private int colorIndex = 0;
    private Coroutine pulseRoutine;

    void Start()
    {
        if (imageTarget == null)
        {
            Debug.LogError("Slot Image Target belum diisi di Inspector!");
            return;
        }

        // Tombol didefinisikan di editor sebagai anak
        // ImageTarget — cari berdasarkan nama GameObject-nya.
        vbSuara = FindButton("BtnSuara");
        vbWarna = FindButton("BtnWarna");
        vbVr    = FindButton("BtnVR");
        if (vbSuara == null || vbWarna == null || vbVr == null)
        {
            Debug.LogError("Ada Virtual Button yang tidak ditemukan " +
                "di anak ImageTarget! Pastikan ada GameObject " +
                "BtnSuara, BtnWarna, dan BtnVR dengan komponen " +
                "Virtual Button Behaviour.");
            return;
        }

        prevStatus = imageTarget.CurrentStatus;
        statusReady = true;
        SetStatus("Arahkan kamera ke brosur...");
    }

    private VirtualButtonBehaviour FindButton(string buttonName)
    {
        VirtualButtonBehaviour[] all =
            imageTarget.GetComponentsInChildren<VirtualButtonBehaviour>(true);
        foreach (VirtualButtonBehaviour vb in all)
            if (vb.gameObject.name == buttonName) return vb;
        return null;
    }

    void Update()
    {
        if (imageTarget == null || !statusReady) return;

        // --- Pantau status target: ketemu / hilang ---
        TrackableBehaviour.Status s = imageTarget.CurrentStatus;
        if (s != prevStatus)
        {
            prevStatus = s;
            OnStatusChanged(s);
        }

        // --- Tombol cuma diproses selagi brosur terlacak ---
        if (!IsTracked(s)) return;

        CheckButton(vbSuara, ref prevSuara, OnBtnSuara);
        CheckButton(vbWarna, ref prevWarna, OnBtnWarna);
        CheckButton(vbVr,    ref prevVr,    OnBtnVr);
    }

    private void CheckButton(VirtualButtonBehaviour vb, ref bool prevPressed,
        System.Action action)
    {
        if (vb == null) return;
        bool now = vb.Pressed;
        if (now && !prevPressed) action();   // cuma sekali pas awal ditekan
        prevPressed = now;
    }

    private bool IsTracked(TrackableBehaviour.Status s)
    {
        return s == TrackableBehaviour.Status.DETECTED ||
               s == TrackableBehaviour.Status.TRACKED ||
               s == TrackableBehaviour.Status.EXTENDED_TRACKED;
    }

    private void OnStatusChanged(TrackableBehaviour.Status s)
    {
        if (IsTracked(s))
        {
            SetStatus("Brosur terdeteksi! Tutupi kotak tombol di brosur untuk menekannya.");
        }
        else
        {
            SetStatus("Arahkan kamera ke brosur...");
            if (speakerAudio != null && speakerAudio.isPlaying) speakerAudio.Stop();
            StopPulse();
        }
    }

    // ---------- Aksi tombol-tombol ----------
    private void OnBtnSuara()
    {
        if (speakerAudio == null) return;
        if (speakerAudio.isPlaying)
        {
            speakerAudio.Stop();
            StopPulse();
            SetStatus("Tombol SUARA: musik berhenti.");
        }
        else
        {
            speakerAudio.Play();
            StartPulse();
            SetStatus("Tombol SUARA: musik diputar!");
        }
    }

    private void OnBtnWarna()
    {
        if (speakerRenderer == null || colorMaterials == null || colorMaterials.Length == 0) return;
        colorIndex = (colorIndex + 1) % colorMaterials.Length;
        speakerRenderer.material = colorMaterials[colorIndex];
        SetStatus("Tombol WARNA: warna speaker diganti (" + (colorIndex + 1) + "/" + colorMaterials.Length + ").");
    }

    private void OnBtnVr()
    {
        if (vrInfoPanel != null) vrInfoPanel.SetActive(true);
        SetStatus("Tombol VR: panel Ruang Dengar Virtual dibuka.");
    }

    // Dipanggil dari Button "Tutup" di PanelVR (On Click di Inspector)
    public void HideVrPanel()
    {
        if (vrInfoPanel != null) vrInfoPanel.SetActive(false);
    }

    // ---------- Animasi denyut speaker saat musik menyala ----------
    private void StartPulse()
    {
        StopPulse();
        if (speakerModel != null) pulseRoutine = StartCoroutine(Pulse());
    }

    private void StopPulse()
    {
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = null;
        if (speakerModel != null) speakerModel.transform.localScale = Vector3.one;
    }

    private IEnumerator Pulse()
    {
        Vector3 baseScale = Vector3.one;
        while (true)
        {
            float s = 1f + 0.08f * Mathf.Sin(Time.time * 6f);
            speakerModel.transform.localScale = baseScale * s;
            yield return null;
        }
    }

    private void SetStatus(string msg) { if (statusText != null) statusText.text = msg; }
}
