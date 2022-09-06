using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockyWorld.Perlin;
using BlockyWorld.SaveLoadSystem;

namespace BlockyWorld.WorldBuilding {
    public class World : MonoBehaviour
    {
        public static Vector3Int worldDimensions = new Vector3Int(5, 5, 5);
        public static Vector3Int extraWorldDimensions = new Vector3Int(2, 5, 2);
        public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);

        public static PerlinSettings surfaceSettings;
        public static PerlinSettings stoneSettings;
        public static PerlinSettings diamondTopSettings;
        public static PerlinSettings diamondBottomSettings;
        public static PerlinSettings caveSettings;

        [SerializeField] bool loadFromFile = false;

        [SerializeField] GameObject chunkPrefab;
        [SerializeField] GameObject loadingCamera;
        public GameObject firstPersonController;
        [SerializeField] Slider loadingBar; 

        [Header("Graphers")]
        [SerializeField] PerlinGrapher surfaceGrapher;
        [SerializeField] PerlinGrapher stoneGrapher;
        [SerializeField] PerlinGrapher diamondTopGrapher;
        [SerializeField] PerlinGrapher diamondBottomGrapher;
        [SerializeField] Perlin3DGrapher caveGrapher;

        public HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
        public HashSet<Vector2Int> chunkColumns = new HashSet<Vector2Int>();
        public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
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
            if(loadFromFile)
                StartCoroutine(LoadWorldFromFile());
            else
                StartCoroutine(BuildWorld());
        } 

        IEnumerator LoadWorldFromFile() {
            WorldData wd = FileSaver.Load();
            if(wd == null) {
                StartCoroutine(BuildWorld());
                yield break;
            }

            chunkChecker.Clear();
            for (int i = 0; i < wd.chunkCheckerValues.Length; i += 3)
                chunkChecker.Add(new Vector3Int(wd.chunkCheckerValues[i], wd.chunkCheckerValues[i+1], wd.chunkCheckerValues[i+2]));
            
            chunkColumns.Clear();
            for (int i = 0; i < wd.chunkColumnValues.Length; i += 2)
                chunkColumns.Add(new Vector2Int(wd.chunkColumnValues[i], wd.chunkColumnValues[i+1]));

            int index = 0;
            int vIndex = 0;
            loadingBar.maxValue = chunkChecker.Count;
            int blockCount = chunkDimensions.x * chunkDimensions.y * chunkDimensions.z;
            foreach (Vector3Int chunkPos in chunkChecker) {
                Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                chunk.gameObject.name = $"Chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}";
                chunk.chunkData = new BlockStaticData.BlockType[blockCount];
                chunk.LoadHealthData(blockCount);

                for (int i = 0; i < blockCount; i++) {
                    chunk.chunkData[i] = (BlockStaticData.BlockType)wd.allChunkData[index];
                    index++;
                }
                chunk.CreateChunk(chunkDimensions, chunkPos, true);
                chunk.ReDrawChunk();
                chunk.meshRenderer.enabled = wd.chunkVisibility[vIndex];
                vIndex++;
                chunks.Add(chunkPos, chunk);

                loadingBar.value++;
                yield return null;
            }

            firstPersonController.transform.position = new Vector3(wd.playerPosX, wd.playerPosY, wd.playerPosZ);
            loadingCamera.SetActive(false);
            loadingBar.gameObject.SetActive(false);
            firstPersonController.SetActive(true);
            lastBuildPosition = Vector3Int.CeilToInt(firstPersonController.transform.position);

            StartCoroutine(BuildCoordinator());
            //StartCoroutine(UpdateWorld());
        }

        public void SaveWorld() {
            FileSaver.Save(this);
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
                    (Vector3Int, Vector3Int) blockNeighbour = GetWorldNeighbour(blockPos, Vector3Int.CeilToInt(hitChunk.worldPosition));
                    hitChunk = chunks[blockNeighbour.Item2];
                    int i = ToFlat(blockNeighbour.Item1);
                    if(leftClick) {
                        bool blockDestroyed = hitChunk.TakeHit(i);
                        if(blockDestroyed) {
                            Vector3Int nBlock = FromFlat(i);
                            (Vector3Int, Vector3Int) neighbourBlock = GetWorldNeighbour(new Vector3Int(nBlock.x, nBlock.y + 1, nBlock.z),
                                                                                            Vector3Int.CeilToInt(hitChunk.worldPosition));
                            Vector3Int block = neighbourBlock.Item1;
                            int neighbourBlockIndex = ToFlat(block);
                            Chunk neighbourChunk = chunks[neighbourBlock.Item2];
                            StartCoroutine(Drop(neighbourChunk, neighbourBlockIndex));
                        }

                    }
                    else {
                        hitChunk.BuildBlockAt(buildType, i);
                        StartCoroutine(Drop(hitChunk, i));
                    }
                    hitChunk.ReDrawChunk();
                }
            }
        }

        WaitForSeconds dropDelay = new WaitForSeconds(0.1f);
        public IEnumerator Drop(Chunk chunk, int blockIndex, int strength = 3) {
            if (!BlockStaticData.canDrop.Contains(chunk.chunkData[blockIndex]))
                yield break;
            yield return dropDelay;
            while(true) {
                Vector3Int thisBlock = FromFlat(blockIndex);
                (Vector3Int, Vector3Int) neighbourBlock = GetWorldNeighbour(new Vector3Int(thisBlock.x, thisBlock.y - 1, thisBlock.z),
                                                                                Vector3Int.CeilToInt(chunk.worldPosition));
                Vector3Int block = neighbourBlock.Item1;
                int neighbourBlockIndex = ToFlat(block);
                Chunk neighbourChunk = chunks[neighbourBlock.Item2];
                if(neighbourChunk != null && neighbourChunk.chunkData[neighbourBlockIndex] == BlockStaticData.BlockType.Air) {
                    neighbourChunk.chunkData[neighbourBlockIndex] = chunk.chunkData[blockIndex];
                    neighbourChunk.ResetBlockHealth(neighbourBlockIndex);
                    chunk.chunkData[blockIndex] = BlockStaticData.BlockType.Air;
                    chunk.ResetBlockHealth(blockIndex);

                    (Vector3Int, Vector3Int) nBlockAbove = GetWorldNeighbour(new Vector3Int(thisBlock.x, thisBlock.y + 1, thisBlock.z),
                                                                                Vector3Int.CeilToInt(chunk.worldPosition));
                    Vector3Int blockAbove = nBlockAbove.Item1;
                    int nBlockAboveIndex = ToFlat(blockAbove);
                    Chunk nChunkAbove = chunks[nBlockAbove.Item2];
                    StartCoroutine(Drop(nChunkAbove, nBlockAboveIndex));

                    yield return dropDelay;
                    chunk.ReDrawChunk();
                    if(neighbourChunk != chunk)
                        neighbourChunk.ReDrawChunk();
                    
                    chunk = neighbourChunk;
                    blockIndex = neighbourBlockIndex;
                } else if(BlockStaticData.canFlow.Contains(chunk.chunkData[blockIndex])) {
                    FlowIntoNeighbour(thisBlock, Vector3Int.CeilToInt(chunk.worldPosition), Vector3Int.right, strength - 1);
                    FlowIntoNeighbour(thisBlock, Vector3Int.CeilToInt(chunk.worldPosition), Vector3Int.left, strength - 1);
                    FlowIntoNeighbour(thisBlock, Vector3Int.CeilToInt(chunk.worldPosition), Vector3Int.forward, strength - 1);
                    FlowIntoNeighbour(thisBlock, Vector3Int.CeilToInt(chunk.worldPosition), Vector3Int.back, strength - 1);
                    yield break;
                } else
                    yield break;
            }
        }

        private void FlowIntoNeighbour(Vector3Int blockPos, Vector3Int chunkPos, Vector3Int neighbourDir, int strength) {
            strength--;
            if(strength <= 0)
                return;
            Vector3Int neighbourPos = blockPos + neighbourDir;
            (Vector3Int, Vector3Int) neighbourBlock = GetWorldNeighbour(neighbourPos, chunkPos);
            Vector3Int block = neighbourBlock.Item1;
            int neighboutBlockIndex = ToFlat(block);
            Chunk neighbourChunk = chunks[neighbourBlock.Item2];
            if(neighbourChunk == null)
                return;
            if(neighbourChunk.chunkData[neighboutBlockIndex] == BlockStaticData.BlockType.Air) {
                neighbourChunk.chunkData[neighboutBlockIndex] = chunks[chunkPos].chunkData[ToFlat(blockPos)];
                neighbourChunk.ResetBlockHealth(neighboutBlockIndex);
                neighbourChunk.ReDrawChunk();
                StartCoroutine(Drop(neighbourChunk, neighboutBlockIndex, strength--));
            }
        }

        private Vector3Int FromFlat(int i) {
            return new Vector3Int(i % chunkDimensions.x,
                (i / chunkDimensions.x) % chunkDimensions.y,
                i / (chunkDimensions.x * chunkDimensions.y));
        }

        private int ToFlat(Vector3Int v) {
            return v.x + chunkDimensions.x * (v.y + chunkDimensions.z * v.z);
        }

        public (Vector3Int, Vector3Int) GetWorldNeighbour(Vector3Int blockIndex, Vector3Int chunkIndex) {
            Chunk chunk = chunks[chunkIndex];
            Vector3Int neighbour = chunkIndex;
            if(blockIndex.x == chunkDimensions.x) {
                neighbour = new Vector3Int((int)(chunk.worldPosition.x + chunkDimensions.x),
                            (int)chunk.worldPosition.y,
                            (int)chunk.worldPosition.z);
                blockIndex.x = 0;
            } else if(blockIndex.x == -1) {
                neighbour = new Vector3Int((int)(chunk.worldPosition.x - chunkDimensions.x),
                            (int)chunk.worldPosition.y,
                            (int)chunk.worldPosition.z);
                blockIndex.x = chunkDimensions.x - 1;
            } 
            else if(blockIndex.y == chunkDimensions.y) {
                neighbour = new Vector3Int((int)chunk.worldPosition.x,
                            (int)(chunk.worldPosition.y + chunkDimensions.y),
                            (int)chunk.worldPosition.z);
                blockIndex.y = 0;
            } else if(blockIndex.y == -1) {
                neighbour = new Vector3Int((int)chunk.worldPosition.x,
                            (int)(chunk.worldPosition.y - chunkDimensions.y),
                            (int)chunk.worldPosition.z);
                blockIndex.y = chunkDimensions.y - 1;
            } 
            else if(blockIndex.z == chunkDimensions.z) {
                neighbour = new Vector3Int((int)chunk.worldPosition.x,
                            (int)chunk.worldPosition.y,
                            (int)(chunk.worldPosition.z + chunkDimensions.z));
                blockIndex.z = 0;
            } else if(blockIndex.z == -1) {
                neighbour = new Vector3Int((int)chunk.worldPosition.x,
                            (int)chunk.worldPosition.y,
                            (int)(chunk.worldPosition.z - chunkDimensions.z));
                blockIndex.z = chunkDimensions.z - 1;
            }

            return (blockIndex, neighbour);
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
            int zStart = worldDimensions.z;
            int xEnd = worldDimensions.x + extraWorldDimensions.x;
            int xStart = worldDimensions.x;

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