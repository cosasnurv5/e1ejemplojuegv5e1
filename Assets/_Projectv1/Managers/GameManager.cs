using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuración.")]
        public GameObject boidPrefab;
        public int initialBoidCount = 10;
        public float respawnDelay = 3f;
        public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);

        [Header("Debug.")]
        public bool showDebugVisuals = true;

        
        private readonly List<BoidAgent> boids = new List<BoidAgent>();
        public IReadOnlyList<BoidAgent> ActiveBoids => boids;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
            
        private void Start()
        {
            SpawnInitialBoids();
            Debug.Log($"El GameManager, comenzo con {initialBoidCount} Boids.");
        }

        private void SpawnInitialBoids()
        {
            for (int i = 0; i < initialBoidCount; i++)
            {
                Vector3 randomPos = GetRandomSpawnPosition();
                GameObject obj = Instantiate(boidPrefab, randomPos, Quaternion.identity);
                if (obj.TryGetComponent<BoidAgent>(out var boid))
                {
                    boids.Add(boid);
                }
            }
        }

        public void RegisterBoid(BoidAgent boid)
        {
            if (!boids.Contains(boid))
                boids.Add(boid);
        }

        public void UnregisterBoid(BoidAgent boid)
        {
            if (boids.Contains(boid))
                boids.Remove(boid);
        }

        public void CollectAndRespawnBoid(BoidAgent boid)
        {
            Debug.Log($"El GameManager, recolectando el Boid: {boid.name}. Se resetea en {respawnDelay}s.");
            boid.gameObject.SetActive(false);
            StartCoroutine(RespawnRoutine(boid));
        }

        private IEnumerator RespawnRoutine(BoidAgent boid)
        {
            yield return new WaitForSeconds(respawnDelay);

            boid.transform.position = GetRandomSpawnPosition();
            boid.ResetAgent();
            Debug.Log($"El GameManager, el Boid {boid.name} se ha reseteado.");
        }

        public Vector3 GetRandomSpawnPosition()
        {
            return transform.position + new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                0f,
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugVisuals) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, spawnAreaSize);
        }
    }
}