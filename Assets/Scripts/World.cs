using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockyWorld.Perlin;

namespace BlockyWorld.WorldBuilding {
    public class World : MonoBehaviour
    {
        public static Vector3Int worldDimensions = new Vector3Int(5, 5, 5);
        public static Vector3Int extraWorldDimensions = new Vector3Int(0, 0, 0);
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

        private Camera mainCamera;

        private void Start() {
            if(chunkDimensions.x >= chunkDimensions.z)
                playerDistCheckerValue = chunkDimensions.z;
            else
                playerDistCheckerValue = chunkDimensions.x;

            loadingBar.maxValue = worldDimensions.x * worldDimensions.z;
            surfaceSettings = new PerlinSettings(surfaceGrapher.settings);
            stoneSettings = new PerlinSettings(stoneGrapher.settings);
            diamondTopSettings = new PerlinSettings(diamondTopGrapher.settings);
            diamondBottomSettings = new PerlinSettings(diamondBottomGrapher.settings);
            caveSettings = new PerlinSettings(caveGrapher.settings);
            StartCoroutine(BuildWorld());
        }

        private BlockStaticData.BlockType buildType = BlockStaticData.BlockType.Dirt;
        public void SetBuildType(int type) {
            buildType = (BlockStaticData.BlockType)type;
        }

        private void Update() {
            bool leftClick = Input.GetMouseButtonDown(0);
            if(leftClick || Input.GetMouseButtonDown(1)) {
                RaycastHit hitResult;
                if(mainCamera == null)
                    mainCamera = Camera.main;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out hitResult, 10.0f)) {
                    Vector3 hitBlock = Vector3.zero;
                    if(leftClick) {
                        hitBlock = hitResult.point - (hitResult.normal / 2.0f);
                    } else {
                        hitBlock = hitResult.point + (hitResult.normal / 2.0f);
                    }

                    Chunk hitChunk = hitResult.collider.gameObject.GetComponent<Chunk>();
                    Vector3Int blockPos = Vector3Int.zero;
                    blockPos.x = (int)(Mathf.Round(hitBlock.x) - hitChunk.worldPosition.x);
                    blockPos.y = (int)(Mathf.Round(hitBlock.y) - hitChunk.worldPosition.y);
                    blockPos.z = (int)(Mathf.Round(hitBlock.z) - hitChunk.worldPosition.z);
                    UpdateIntoNeighbour(ref hitChunk, ref blockPos);
                    int i = blockPos.x + chunkDimensions.z * (blockPos.y + chunkDimensions.z * blockPos.z);
                    if(leftClick)
                        hitChunk.TakeHit(i);
                    else
                        hitChunk.BuildBlockAt(buildType, i);
                    hitChunk.ReDrawChunk();
                }
            }
        }

        private void UpdateIntoNeighbour(ref Chunk hitChunk, ref Vector3Int expectedPos) {
            Vector3Int neighbour = Vector3Int.zero;
            if(expectedPos.x == chunkDimensions.x) {
                neighbour = new Vector3Int((int)(hitChunk.worldPosition.x + chunkDimensions.x),
                            (int)hitChunk.worldPosition.y,
                            (int)hitChunk.worldPosition.z);
                hitChunk = chunks[neighbour];
                expectedPos.x = 0;
            } else if(expectedPos.x == -1) {
                neighbour = new Vector3Int((int)(hitChunk.worldPosition.x - chunkDimensions.x),
                            (int)hitChunk.worldPosition.y,
                            (int)hitChunk.worldPosition.z);
                hitChunk = chunks[neighbour];
                expectedPos.x = chunkDimensions.x - 1;
            } 
            else if(expectedPos.y == chunkDimensions.y) {
                neighbour = new Vector3Int((int)hitChunk.worldPosition.x,
                            (int)(hitChunk.worldPosition.y + chunkDimensions.y),
                            (int)hitChunk.worldPosition.z);
                hitChunk = chunks[neighbour];
                expectedPos.y = 0;
            } else if(expectedPos.y == -1) {
                neighbour = new Vector3Int((int)hitChunk.worldPosition.x,
                            (int)(hitChunk.worldPosition.y - chunkDimensions.y),
                            (int)hitChunk.worldPosition.z);
                hitChunk = chunks[neighbour];
                expectedPos.y = chunkDimensions.y - 1;
            } 
            else if(expectedPos.z == chunkDimensions.z) {
                neighbour = new Vector3Int((int)hitChunk.worldPosition.x,
                            (int)hitChunk.worldPosition.y,
                            (int)(hitChunk.worldPosition.z + chunkDimensions.z));
                hitChunk = chunks[neighbour];
                expectedPos.z = 0;
            } else if(expectedPos.z == -1) {
                neighbour = new Vector3Int((int)hitChunk.worldPosition.x,
                            (int)hitChunk.worldPosition.y,
                            (int)(hitChunk.worldPosition.z - chunkDimensions.z));
                hitChunk = chunks[neighbour];
                expectedPos.z = chunkDimensions.z - 1;
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

            int xPos = (worldDimensions.x * chunkDimensions.x)/2;
            int zPos = (worldDimensions.z * chunkDimensions.z)/2;
            int yPos = (int)MeshUtils.fBM(xPos, zPos, surfaceSettings.octives, surfaceSettings.scale, 
                surfaceSettings.heightScale, surfaceSettings.heightOffset) + 6;
            lastBuildPosition = new Vector3Int(xPos, yPos, zPos);
            firstPersonController.transform.position = lastBuildPosition;

            loadingCamera.SetActive(false);
            loadingBar.gameObject.SetActive(false);
            firstPersonController.SetActive(true);

            StartCoroutine(BuildCoordinator());
            //StartCoroutine(UpdateWorld());
            StartCoroutine(BuildExtraWorld());
        }

        private IEnumerator BuildCoordinator() {
            while(true) {
                while(buildQue.Count > 0)
                    yield return StartCoroutine(buildQue.Dequeue());
                yield return null;
            }
        }

        WaitForSeconds updateGap = new WaitForSeconds(0.5f);
        private IEnumerator UpdateWorld() {
            while(true) {
                if((lastBuildPosition - firstPersonController.transform.position).magnitude > playerDistCheckerValue) {
                    lastBuildPosition = Vector3Int.CeilToInt(firstPersonController.transform.position);
                    int posX = (int)(firstPersonController.transform.position.x / chunkDimensions.x) * chunkDimensions.x;
                    int posZ = (int)(firstPersonController.transform.position.z / chunkDimensions.z) * chunkDimensions.z;
                    buildQue.Enqueue(BuildRecursiveWorld(posX, posZ, drawRadius));
                    buildQue.Enqueue(HideColumns(posX, posZ));
                }
                yield return updateGap;
            }
        }

        private IEnumerator BuildRecursiveWorld(int x, int z, int radius) {
            int nextRadius = radius - 1;
            if(radius <= 0)
                yield break;

            BuildChunkColumn(x, z + chunkDimensions.z);
            buildQue.Enqueue(BuildRecursiveWorld(x, z + chunkDimensions.z, nextRadius));
            yield return null;

            BuildChunkColumn(x, z - chunkDimensions.z);
            buildQue.Enqueue(BuildRecursiveWorld(x, z - chunkDimensions.z, nextRadius));
            yield return null;

            BuildChunkColumn(x + chunkDimensions.x, z);
            buildQue.Enqueue(BuildRecursiveWorld(x + chunkDimensions.x, z, nextRadius));
            yield return null;

            BuildChunkColumn(x - chunkDimensions.x, z);
            buildQue.Enqueue(BuildRecursiveWorld(x - chunkDimensions.x, z, nextRadius));
            yield return null;
        }

        void BuildChunkColumn(int x, int z, bool meshEnabled = true) {
            for (int y = 0; y < worldDimensions.y; y++) {
                Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
                if(!chunkChecker.Contains(position)) {
                    Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                    chunk.gameObject.name = $"Chunk_{position.x}_{position.y}_{position.z}";
                    chunk.CreateChunk(chunkDimensions, position);
                    
                    chunkChecker.Add(position);
                    chunks.Add(position, chunk);
                }
                chunks[position].meshRenderer.enabled = meshEnabled;
            }
            if(!chunkColumns.Contains(new Vector2Int(x, z)))
                chunkColumns.Add(new Vector2Int(x, z));
        }

        private IEnumerator BuildExtraWorld() {
            int zEnd = worldDimensions.z + extraWorldDimensions.z;
            int zStart = worldDimensions.z - 1;
            int xEnd = worldDimensions.x + extraWorldDimensions.x;
            int xStart = worldDimensions.x - 1;

            for (int z = zStart; z < zEnd; z++) {
                for (int x = 0; x < xEnd; x++) {
                    BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                    yield return null;
                }
            }
            for (int z = 0; z < zEnd; z++) {
                for (int x = xStart; x < xEnd; x++) {
                    BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                    yield return null;
                }
            }
        }

        private IEnumerator HideColumns(int x, int z) {
            Vector2Int fpcPos = new Vector2Int(x, z);
            foreach (Vector2Int cc in chunkColumns) {
                if((cc - fpcPos).magnitude >= drawRadius * playerDistCheckerValue) {
                    HideChunkColumn(cc.x, cc.y);
                }
            }
            yield return null;
        }

        private void HideChunkColumn(int x, int z) {
            for (int y = 0; y < worldDimensions.y; y++) {
                Vector3Int postion = new Vector3Int(x, y * chunkDimensions.y, z);
                if(chunkChecker.Contains(postion))
                    chunks[postion].meshRenderer.enabled = false;
            }
        }
    }
}