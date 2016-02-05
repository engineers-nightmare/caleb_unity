using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public class EnemySpawner : NetworkBehaviour
    {
        public GameObject EnemyPrefab;
        public int NumEnemies;
        public int SpawnRadius;

        public override void OnStartServer()
        {
            for (var i = 0; i < NumEnemies; ++i)
            {
                var pos = new Vector3(
                    Random.Range(-SpawnRadius, SpawnRadius),
                    Random.Range(-SpawnRadius, SpawnRadius),
                    Random.Range(-SpawnRadius, SpawnRadius));

                var rotation = Quaternion.Euler(
                    Random.Range(0, 180),
                    Random.Range(0, 180),
                    Random.Range(0, 180));

                var enemy = (GameObject) Instantiate(EnemyPrefab, pos, rotation);
                NetworkServer.Spawn(enemy);
            }
        }
    }
}
