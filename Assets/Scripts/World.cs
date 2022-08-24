using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockyWorld.Perlin;

namespace BlockyWorld.WorldBuilding {
    public class World : MonoBehaviour
    {
        public static Vector3Int worldDimensions = new Vector3Int(4, 4, 4);
        public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);

        public static PerlinSettings surfaceSettings;
        public static PerlinSettings stoneSettings;
        public static PerlinSettings diamondTopSettings;
        public static PerlinSettings diamondBottomSettings;
        public static PerlinSettings caveSettings;

        [SerializeField] GameObject chunkPrefab;
        [SerializeField] GameObject loadingCamera;
        [SerializeField] GameObject firstPersonController;
        [SerializeField] Slider loadingBar; 

        [Header("Graphers")]
        [SerializeField] PerlinGrapher surfaceGrapher;
        [SerializeField] PerlinGrapher stoneGrapher;
        [SerializeField] PerlinGrapher diamondTopGrapher;
        [SerializeField] PerlinGrapher diamondBottomGrapher;
        [SerializeField] Perlin3DGrapher caveGrapher;


        private void Start() {
            loadingBar.maxValue = worldDimensions.x * worldDimensions.z;
            surfaceSettings = new PerlinSettings(surfaceGrapher.settings);
            stoneSettings = new PerlinSettings(stoneGrapher.settings);
            diamondTopSettings = new PerlinSettings(diamondTopGrapher.settings);
            diamondBottomSettings = new PerlinSettings(diamondBottomGrapher.settings);
            caveSettings = new PerlinSettings(caveGrapher.settings);
            StartCoroutine(BuildWorld());
        }

        void BuildChunkColumn(int x, int z) {
            for (int y = 0; y < worldDimensions.y; y++) {
                Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                Vector3Int position = new Vector3Int(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);
                chunk.CreateChunk(chunkDimensions, position);
            }
        }

        IEnumerator BuildWorld() {
            for (int z = 0; z < worldDimensions.z; z++) {
                for (int x = 0; x < worldDimensions.x; x++) {
                    BuildChunkColumn(x, z);
                    loadingBar.value++;
                    yield return null;
                }
            }

            loadingCamera.SetActive(false);
            int xPos = (worldDimensions.x * chunkDimensions.x)/2;
            int zPos = (worldDimensions.z * chunkDimensions.z)/2;
            Chunk c = chunkPrefab.GetComponent<Chunk>();
            int yPos = (int)MeshUtils.fBM(xPos, zPos, surfaceSettings.octives, surfaceSettings.scale, 
                surfaceSettings.heightScale, surfaceSettings.heightOffset) + 6;
            firstPersonController.transform.position = new Vector3Int(xPos, yPos, zPos);
            firstPersonController.SetActive(true);
            loadingBar.gameObject.SetActive(false);
        }
    }
}