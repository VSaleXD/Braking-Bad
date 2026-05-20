using UnityEngine;

public class UFOSpawner : MonoBehaviour
{
    public GameObject ufoPrefab; // Masukkan Prefab UFO kamu di sini lewat Inspector
    public float spawnRate = 2f; // Kemunculan UFO setiap sekian detik
    private float nextSpawnTime = 0f;
    public float maksUFO = 3f; // Jumlah maksimal UFO yang bisa muncul di layar
    private int currentUFOCount = 0;
    private Camera mainCamera;
    private Vector2 screenBounds;

    void Start()
    {
        mainCamera = Camera.main;
        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnUFO();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnUFO()
    {
        if (currentUFOCount >= maksUFO)
        {
            return;
        }

        Vector3 spawnPosition = Vector3.zero;

        // Pilih secara acak apakah UFO muncul dari Samping (Kiri/Kanan) atau Atas/Bawah
        bool spawnOnSides = Random.value > 0.5f;

        if (spawnOnSides)
        {
            // Muncul dari kiri luar atau kanan luar layar
            spawnPosition.x = Random.value > 0.5f ? screenBounds.x + 1f : -screenBounds.x - 1f;
            spawnPosition.y = Random.Range(-screenBounds.y, screenBounds.y);
        }
        else
        {
            // Muncul dari atas luar atau bawah luar layar
            spawnPosition.x = Random.Range(-screenBounds.x, screenBounds.x);
            spawnPosition.y = Random.value > 0.5f ? screenBounds.y + 1f : -screenBounds.y - 1f;
        }

        // Buat objek UFO baru
        Instantiate(ufoPrefab, spawnPosition, Quaternion.identity);
        currentUFOCount++;
    }
}