using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockyWorld.Perlin;

namespace BlockyWorld.WorldBuilding {
    public class World : MonoBehaviour
    {
        public static Vector3 worldDimensions = new Vector3(3, 3, 3);
        public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
        [SerializeField] GameObject chunkPrefab;
        [SerializeField] GameObject loadingCamera;
        [SerializeField] GameObject firstPersonController;
        [SerializeField] Slider loadingBar; 

        public static PerlinSettings surfaceSettings;
        [SerializeField] PerlinGrapher surfaceGrapher;
        public static PerlinSettings stoneSettings;
        [SerializeField] PerlinGrapher stoneGrapher;


        private void Start() {
            loadingBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
            surfaceSettings = new PerlinSettings(surfaceGrapher.settings);
            stoneSettings = new PerlinSettings(stoneGrapher.settings);
            StartCoroutine(BuildWorld());
        }

        IEnumerator BuildWorld() {
            for (int z = 0; z < worldDimensions.z; z++) {
                for (int y = 0; y < worldDimensions.y; y++) {
                    for (int x = 0; x < worldDimensions.x; x++) {
                        Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                        Vector3 position = new Vector3(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);
                        chunk.CreateChunk(chunkDimensions, position);
                        loadingBar.value++;
                        yield return null;
                    }
                }
            }

            loadingCamera.SetActive(false);
            float xPos = (worldDimensions.x * chunkDimensions.x)/2;
            float zPos = (worldDimensions.z * chunkDimensions.z)/2;
            Chunk c = chunkPrefab.GetComponent<Chunk>();
            float yPos = MeshUtils.fBM(xPos, zPos, surfaceSettings.octives, surfaceSettings.scale, 
                surfaceSettings.hightScale, surfaceSettings.heightOffset) + 6;
            firstPersonController.transform.position = new Vector3(xPos, yPos, zPos);
            firstPersonController.SetActive(true);
            loadingBar.gameObject.SetActive(false);
        }
    }
}