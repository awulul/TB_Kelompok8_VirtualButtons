// ============================================================
// TouchGrabber.cs — Tugas Besar Kelompok 8 (VRScene versi HP)
// Versi 2: objek yang dipegang TIDAK BISA menembus lantai /
// dinding — posisinya dibatasi (clamp) supaya tidak hilang
// masuk ke dalam lantai saat diseret ke bawah.
//
// Dipasang di kamera VRScene (satu kali saja, di kamera).
// Cara pakai di game:
// - Sentuh & tahan objek apa pun yang punya Rigidbody
//   (speaker, bola, kubus) -> objek terangkat mengikuti jari.
// - Gerakkan jari / gerakkan HP (mode gyro) -> objek ikut berpindah.
// - Lepaskan -> objek TERLEMPAR, kecepatannya dihitung dari
//   gerakan terakhir (fisika Rigidbody yang meneruskan).
// Di Editor: klik-tahan pakai mouse, gerakkan, lepaskan.
// Pengganti versi HP untuk XR Grab Interactable + controller.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class TouchGrabber : MonoBehaviour
{
    public float maxGrabDistance = 8f;  // jangkauan maksimum tangan (meter)
    public float maxThrowSpeed = 12f;   // batas kecepatan lemparan (m/s)
    public float floorY = 0f;           // tinggi permukaan lantai ruangan
    public float roomHalfExtent = 2.8f; // setengah lebar ruangan (dinding di +/-3)

    private Camera cam;
    private Rigidbody grabbed;
    private float grabDistance;
    private float bottomOffset = 0.1f;  // jarak titik asal objek ke sisi bawahnya
    private int activeFinger = -999;    // fingerId sentuhan aktif; -1 = mouse

    private readonly List<Vector3> posHistory = new List<Vector3>();
    private readonly List<float> timeHistory = new List<float>();

    // Sentuhan/mouse yang sedang dipakai mengangkat objek.
    // PhoneCameraLook membaca ini supaya kamera tidak ikut berputar.
    private static readonly HashSet<int> claimed = new HashSet<int>();
    public static bool IsClaimed(int fingerId) { return claimed.Contains(fingerId); }

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began)
                    TryGrab(t.fingerId, t.position);
                else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                         && grabbed != null && t.fingerId == activeFinger)
                    Release();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) TryGrab(-1, Input.mousePosition);
            if (Input.GetMouseButtonUp(0) && grabbed != null && activeFinger == -1) Release();
        }
    }

    void FixedUpdate()
    {
        if (grabbed == null) return;

        Vector3 target = PointerRay().GetPoint(grabDistance);

        // --- Pembatas: objek yang dipegang tidak boleh masuk lantai / keluar dinding ---
        target.y = Mathf.Max(target.y, floorY + bottomOffset + 0.01f);
        target.x = Mathf.Clamp(target.x, -roomHalfExtent, roomHalfExtent);
        target.z = Mathf.Clamp(target.z, -roomHalfExtent, roomHalfExtent);

        grabbed.MovePosition(Vector3.Lerp(grabbed.position, target, 0.6f));

        posHistory.Add(grabbed.position);
        timeHistory.Add(Time.time);
        while (timeHistory.Count > 2 && Time.time - timeHistory[0] > 0.15f)
        {
            posHistory.RemoveAt(0);
            timeHistory.RemoveAt(0);
        }
    }

    private Ray PointerRay()
    {
        if (activeFinger >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId == activeFinger)
                    return cam.ScreenPointToRay(t.position);
            }
        }
        if (activeFinger == -1 && Input.GetMouseButton(0))
            return cam.ScreenPointToRay(Input.mousePosition);

        // Mode gyro tanpa jari di layar: objek di depan tengah kamera.
        return new Ray(cam.transform.position, cam.transform.forward);
    }

    private void TryGrab(int fingerId, Vector2 screenPos)
    {
        if (grabbed != null) return; // satu objek saja dalam satu waktu

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxGrabDistance);
        Rigidbody best = null;
        float bestDist = float.MaxValue;
        foreach (RaycastHit h in hits)
        {
            if (h.rigidbody != null && h.distance < bestDist)
            {
                best = h.rigidbody;
                bestDist = h.distance;
            }
        }
        if (best == null) return;

        grabbed = best;
        grabDistance = bestDist;
        activeFinger = fingerId;
        claimed.Add(fingerId);

        // Ukur sisi bawah objek (dari semua collider anak) untuk pembatas lantai.
        Collider[] cols = grabbed.GetComponentsInChildren<Collider>();
        if (cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
            bottomOffset = grabbed.position.y - b.min.y;
            if (bottomOffset < 0.02f) bottomOffset = 0.02f;
        }

        grabbed.isKinematic = true; // selama dipegang, fisika diambil alih tangan
        posHistory.Clear();
        timeHistory.Clear();

        // Kalau yang diangkat punya AudioSource dan belum bunyi, nyalakan.
        AudioSource a = grabbed.GetComponentInChildren<AudioSource>();
        if (a != null && !a.isPlaying) a.Play();
    }

    private void Release()
    {
        claimed.Remove(activeFinger);

        Vector3 v = Vector3.zero;
        if (posHistory.Count >= 2)
        {
            float dt = timeHistory[timeHistory.Count - 1] - timeHistory[0];
            if (dt > 0.01f)
                v = (posHistory[posHistory.Count - 1] - posHistory[0]) / dt;
            if (v.magnitude > maxThrowSpeed) v = v.normalized * maxThrowSpeed;
        }

        grabbed.isKinematic = false; // fisika aktif lagi
        grabbed.velocity = v;         // <- ini lemparannya
        grabbed.angularVelocity = Vector3.zero;

        grabbed = null;
        activeFinger = -999;
    }
}
