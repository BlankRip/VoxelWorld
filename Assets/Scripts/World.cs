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

        private HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
        private HashSet<Vector2Int> chunkColumns = new HashSet<Vector2Int>();
        private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
        private Vector3Int lastBuildPosition;
        private float playerDistCheckerValue;
        private int drawRadius = 3;
        private Queue<IEnumerator> buildQue = new Queue<IEnumerator>();

        private void Start() {
            if(chunkDimensions.x >= chunkDimensions.z)
                playerDistCheckerValue = chunkDimensions.z * chunkDimensions.z;
            else
                playerDistCheckerValue = chunkDimensions.x * chunkDimensions.x;

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
                Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
                if(!chunkChecker.Contains(position)) {
                    Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                    chunk.gameObject.name = $"Chunk_{position.x}_{position.y}_{position.z}";
                    chunk.CreateChunk(chunkDimensions, position);
                    
                    chunkChecker.Add(position);
                    chunks.Add(position, chunk);
                } else {
                    chunks[position].SetMeshVisibility(true);
                }
            }
        }

        private IEnumerator BuildWorld() {
            for (int z = 0; z < worldDimensions.z; z++) {
                for (int x = 0; x < worldDimensions.x; x++) {
                    BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z);
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
            lastBuildPosition = new Vector3Int(xPos, yPos, zPos);
            firstPersonController.transform.position = lastBuildPosition;
            firstPersonController.SetActive(true);
            loadingBar.gameObject.SetActive(false);
            StartCoroutine(BuildCoordinator());
            StartCoroutine(UpdateWorld());
        }

        private IEnumerator BuildCoordinator() {
            while(true) {
                while(buildQue.Count > 0) {
                    yield return StartCoroutine(buildQue.Dequeue());
                }
                yield return null;
            }
        }

        private IEnumerator UpdateWorld() {
            WaitForSeconds updateGap = new WaitForSeconds(0.5f);
            while(true) {
                float dist = (lastBuildPosition - firstPersonController.transform.position).sqrMagnitude;
                if(dist > playerDistCheckerValue) {
                    lastBuildPosition = Vector3Int.CeilToInt(firstPersonController.transform.position);
                    int posX = (int)(firstPersonController.transform.position.x / chunkDimensions.x) * chunkDimensions.x;
                    int posZ = (int)(firstPersonController.transform.position.z / chunkDimensions.z) * chunkDimensions.z;
                    buildQue.Enqueue(BuildRecursiveWorld(posX, posZ, drawRadius));
                }
                yield return updateGap;
            }
        }

        private IEnumerator BuildRecursiveWorld(int x, int z, int radius) {
            int nextRadius = radius - 1;
            if(radius <= 0)
                yield break;

            int varialbleValue = z + chunkDimensions.z;
            BuildChunkColumn(x, varialbleValue);
            buildQue.Enqueue(BuildRecursiveWorld(x, varialbleValue, nextRadius));
            yield return null;

            varialbleValue = z - chunkDimensions.z;
            BuildChunkColumn(x, varialbleValue);
            buildQue.Enqueue(BuildRecursiveWorld(x, varialbleValue, nextRadius));
            yield return null;

            varialbleValue = x + chunkDimensions.x;
            BuildChunkColumn(varialbleValue, z);
            buildQue.Enqueue(BuildRecursiveWorld(varialbleValue, z, nextRadius));
            yield return null;

            varialbleValue = x - chunkDimensions.x;
            BuildChunkColumn(varialbleValue, z);
            buildQue.Enqueue(BuildRecursiveWorld(varialbleValue, z, nextRadius));
            yield return null;
        }
    }
}