// ============================================================
// PhoneCameraLook.cs — Tugas Besar Kelompok 8 (VRScene versi HP)
//
// Dipasang di kamera VRScene. Cara melihat sekeliling:
// - HP yang punya gyroscope: gerakkan HP-nya, pandangan mengikuti.
// - Tanpa gyroscope / di Editor: seret 1 jari (atau mouse).
// Sentuhan yang sedang dipakai mengangkat objek (lihat
// TouchGrabber) TIDAK ikut memutar kamera.
// ============================================================
using UnityEngine;

public class PhoneCameraLook : MonoBehaviour
{
    public bool useGyro = true;           // matikan di Inspector kalau arah gyro di HP terasa terbalik
    public float dragSensitivity = 0.15f; // derajat putar per piksel seretan

    private bool gyroActive = false;
    private float yaw;
    private float pitch;
    private Vector3 prevMouse;

    void Start()
    {
        Vector3 e = transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = e.x;
        if (pitch > 180f) pitch -= 360f;
        prevMouse = Input.mousePosition;

        if (useGyro && SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            gyroActive = true;
        }
    }

    void Update()
    {
        if (gyroActive)
        {
            Quaternion q = Input.gyro.attitude;
            Quaternion koreksi = new Quaternion(q.x, q.y, -q.z, -q.w);
            transform.rotation = Quaternion.Euler(90f, 90f, 0f) * koreksi;
            return;
        }

        // --- Mode seret (jari / mouse) ---
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (TouchGrabber.IsClaimed(t.fingerId)) return; // jari ini sedang mengangkat objek
            if (t.phase == TouchPhase.Moved)
            {
                yaw += t.deltaPosition.x * dragSensitivity;
                pitch -= t.deltaPosition.y * dragSensitivity;
                pitch = Mathf.Clamp(pitch, -80f, 80f);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 m = Input.mousePosition;
            Vector2 delta = (Vector2)(m - prevMouse);
            prevMouse = m;
            if (TouchGrabber.IsClaimed(-1)) return; // mouse sedang mengangkat objek
            yaw += delta.x * dragSensitivity;
            pitch -= delta.y * dragSensitivity;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            prevMouse = Input.mousePosition;
        }
    }
}
