using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class World : MonoBehaviour
    {
        public static Vector3 worldDimensions = new Vector3(10, 10, 10);
        public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
        public GameObject chunkPrefab;

        private void Start() {
            StartCoroutine(BuildWorld());
        }

        IEnumerator BuildWorld() {
            for (int z = 0; z < worldDimensions.z; z++) {
                for (int y = 0; y < worldDimensions.y; y++) {
                    for (int x = 0; x < worldDimensions.x; x++) {
                        Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                        Vector3 position = new Vector3(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);
                        chunk.CreateChunk(chunkDimensions, position);
                        yield return null;
                    }
                }
            }
        }
    }
}