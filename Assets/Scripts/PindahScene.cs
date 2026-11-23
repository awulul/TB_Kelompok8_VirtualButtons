// ============================================================
// PindahScene.cs — Tugas Besar Kelompok 8
//
// Fungsi pindah scene untuk tombol UI biasa (bukan Virtual Button).
// Pasang script ini di satu GameObject kosong, lalu hubungkan
// tombol lewat Inspector: On Click -> objek ini -> pilih
// PindahScene.KeAR atau PindahScene.KeVR.
// Syarat: kedua scene harus terdaftar di File -> Build Settings.
// ============================================================
using UnityEngine;
using UnityEngine.SceneManagement;

public class PindahScene : MonoBehaviour
{
    public void KeAR()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void KeVR()
    {
        SceneManager.LoadScene("VRScene");
    }
}
