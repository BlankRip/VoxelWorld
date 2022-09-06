using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;


namespace BlockyWorld.WorldBuilding {
    public class Chunk : MonoBehaviour
    {
        [Header("Chunk Data")]
        public Vector3Int chunkSize = new Vector3Int(2, 2, 2);
        [SerializeField] Material atlas;
        [SerializeField] Material fluid;

        private Block[,,] blocks;
        //Flaten 3d array [x + width(3d.x) * (y + depth(3d.z) * z)] = [x, y, z] in 3d array
        //Flat to 3d x = i % width (3d.x);  y = i/width(3d.x) % height (3d.y);   z = i / (width (3d.x) * height (3d.y))
        [HideInInspector] public BlockStaticData.BlockType[] chunkData;
        [HideInInspector] private BlockStaticData.BlockType[] healthData;
        [HideInInspector] public Vector3 worldPosition;
        [HideInInspector] public MeshRenderer meshRendererSolid;
        [HideInInspector] public MeshRenderer meshRendererFluid;
        private GameObject solidMesh;
        private GameObject fluidMesh;

        CalculateBlockTypes calculateBlockTypes;
        JobHandle jobHandle;
        public NativeArray<Unity.Mathematics.Random> randomArray {get; private set;}

        public bool TakeHit(int blockIndex) {
            int blockHealth = BlockStaticData.blockTypeHealth[(int)chunkData[blockIndex]];
            if(blockHealth != -1) {
                if(healthData[blockIndex] == BlockStaticData.BlockType.NoCrack)
                    StartCoroutine(HealBlock(blockIndex));
                healthData[blockIndex]++;
                if(healthData[blockIndex] == BlockStaticData.BlockType.NoCrack + blockHealth) {
                    chunkData[blockIndex] = BlockStaticData.BlockType.Air;
                    return true;
                }
            }
            return false;
        }

        public void ResetBlockHealth(int blockIndex) {
            healthData[blockIndex] = BlockStaticData.BlockType.NoCrack;
        }

        public void LoadHealthData(int blockCount) {
            healthData = new BlockStaticData.BlockType[blockCount];
            for (int i = 0; i < blockCount; i++)
                healthData[i] = BlockStaticData.BlockType.NoCrack;
        }

        public void ReDrawChunk() {
            DestroyImmediate(GetComponent<MeshFilter>());
            DestroyImmediate(GetComponent<MeshRenderer>());
            DestroyImmediate(GetComponent<Collider>());
            CreateChunk(chunkSize, worldPosition, true);
        }

        private WaitForSeconds healTime = new WaitForSeconds(3);
        public System.Collections.IEnumerator HealBlock(int blockIndex) {
            yield return healTime;
            if(chunkData[blockIndex] != BlockStaticData.BlockType.Air) {
                healthData[blockIndex] = BlockStaticData.BlockType.NoCrack;
                ReDrawChunk();
            }
        }

        public void BuildBlockAt(BlockStaticData.BlockType buildType, int blockIndex) {
            chunkData[blockIndex] = buildType;
            healthData[blockIndex] = BlockStaticData.BlockType.NoCrack;
        }

        private void BuildChunkData() {
            int blockCount = chunkSize.x * chunkSize.y * chunkSize.z;
            chunkData = new BlockStaticData.BlockType[blockCount];
            healthData = new BlockStaticData.BlockType[blockCount];
            NativeArray<BlockStaticData.BlockType> blockTypes = new NativeArray<BlockStaticData.BlockType>(chunkData, Allocator.Persistent);
            NativeArray<BlockStaticData.BlockType> healthTypes = new NativeArray<BlockStaticData.BlockType>(healthData, Allocator.Persistent);

            Unity.Mathematics.Random[] tempRandomArray = new Unity.Mathematics.Random[blockCount];
            System.Random seed = new System.Random();
            for (int i = 0; i < blockCount; i++)
                tempRandomArray[i] = new Unity.Mathematics.Random((uint)seed.Next());
            randomArray = new NativeArray<Unity.Mathematics.Random>(tempRandomArray, Allocator.Persistent);

            calculateBlockTypes = new CalculateBlockTypes() {
                cData = blockTypes,
                hData = healthTypes,
                chunkSize = new Vector2Int(this.chunkSize.x, this.chunkSize.y),
                location = worldPosition,
                randoms = randomArray
            };

            jobHandle = calculateBlockTypes.Schedule(chunkData.Length, 64);
            jobHandle.Complete();
            calculateBlockTypes.cData.CopyTo(chunkData);
            calculateBlockTypes.hData.CopyTo(healthData);

            blockTypes.Dispose();
            healthTypes.Dispose();
            randomArray.Dispose();

            BuildTrees();
        }

        private void BuildTrees() {
            for (int i = 0; i < chunkData.Length; i++) {
                if(chunkData[i] == BlockStaticData.BlockType.WoodBase) {
                    foreach ((Vector3Int, BlockStaticData.BlockType) data in BlockStaticData.treeDesign) {
                        Vector3Int blockpos = StaticFuncs.FromFlat(i) + data.Item1;
                        int bIndex = StaticFuncs.ToFlat(blockpos);
                        if(bIndex >= 0 && bIndex < chunkData.Length) {
                            chunkData[bIndex] = data.Item2;
                            healthData[bIndex] = BlockStaticData.BlockType.NoCrack;
                        }
                    }
                }
            }
        }

        struct CalculateBlockTypes : IJobParallelFor
        {
            public NativeArray<BlockStaticData.BlockType> cData;
            public NativeArray<BlockStaticData.BlockType> hData;
            public Vector2Int chunkSize;
            public Vector3 location;
            public NativeArray<Unity.Mathematics.Random> randoms;

            public void Execute(int i) {
                int x = (i % chunkSize.x) + (int)location.x;
                int y = ((i / chunkSize.x) % chunkSize.y) + (int)location.y;
                int z = (i / (chunkSize.x * chunkSize.y)) + (int)location.z;

                hData[i] = BlockStaticData.BlockType.NoCrack;
                if(y <= 1) {
                    cData[i] = BlockStaticData.BlockType.BedRock;
                    return;
                }

                Unity.Mathematics.Random random = randoms[i];
                
                int surfaceHeight = (int)MeshUtils.fBM(x, z, World.surfaceSettings.octives, World.surfaceSettings.scale,
                    World.surfaceSettings.heightScale, World.surfaceSettings.heightOffset);
                int stoneHeight = (int)MeshUtils.fBM(x, z, World.stoneSettings.octives, World.stoneSettings.scale,
                    World.stoneSettings.heightScale, World.stoneSettings.heightOffset);
                int diamondTopHeight = (int)MeshUtils.fBM(x, z, World.diamondTopSettings.octives, World.diamondTopSettings.scale,
                    World.diamondTopSettings.heightScale, World.diamondTopSettings.heightOffset);
                int diamondBottomHeight = (int)MeshUtils.fBM(x, z, World.diamondBottomSettings.octives, World.diamondBottomSettings.scale,
                    World.diamondBottomSettings.heightScale, World.diamondBottomSettings.heightOffset);
                int digCave = (int)MeshUtils.fBM3D(x, y, z, World.caveSettings.octives, World.caveSettings.scale,
                    World.caveSettings.heightScale, World.caveSettings.heightOffset);
                int plantTree = (int)MeshUtils.fBM3D(x, y, z, World.treeSettings.octives, World.treeSettings.scale,
                    World.treeSettings.heightScale, World.treeSettings.heightOffset);

                if(digCave < World.caveSettings.drawCutoff) {
                    cData[i] = BlockStaticData.BlockType.Air;
                } else if(surfaceHeight == y) {
                    if(plantTree < World.treeSettings.drawCutoff && random.NextFloat(1) <= 0.1f) {
                        cData[i] = BlockStaticData.BlockType.WoodBase;
                    } else
                        cData[i] = BlockStaticData.BlockType.GrassSide;
                } else if ((y < diamondTopHeight) && (y > diamondBottomHeight) &&  (random.NextFloat(1) < World.diamondTopSettings.probability)) {
                    cData[i] = BlockStaticData.BlockType.Diamond;
                } else if(y < stoneHeight && (random.NextFloat(1) < World.stoneSettings.probability)) {
                    cData[i] = BlockStaticData.BlockType.Stone;
                } else if(surfaceHeight > y) {
                    cData[i] = BlockStaticData.BlockType.Dirt;
                } else if (y < 20) {
                    cData[i] = BlockStaticData.BlockType.Water;
                } else {
                    cData[i] = BlockStaticData.BlockType.Air;
                }
            }
        }

        public void CreateChunk(Vector3Int dimensions, Vector3 postion, bool rebuilding = false) {
            worldPosition = postion;
            chunkSize = dimensions;

            MeshFilter meshFilterSolid;
            MeshFilter meshFilterFluid;
            if(solidMesh == null) {
                solidMesh = new GameObject("Solid");
                solidMesh.transform.parent = this.gameObject.transform;
                meshFilterSolid = solidMesh.AddComponent<MeshFilter>();
                meshRendererSolid = solidMesh.AddComponent<MeshRenderer>();
                meshRendererSolid.material = atlas;
            } else {
                meshFilterSolid = solidMesh.GetComponent<MeshFilter>();
                DestroyImmediate(solidMesh.GetComponent<Collider>());
            }
            if(fluidMesh == null) {
                fluidMesh = new GameObject("Fluid");
                fluidMesh.transform.parent = this.gameObject.transform;
                meshFilterFluid = fluidMesh.AddComponent<MeshFilter>();
                meshRendererFluid = fluidMesh.AddComponent<MeshRenderer>();
                meshRendererFluid.material = fluid;
                fluidMesh.AddComponent<UVScroller>();
            } else {
                meshFilterFluid = fluidMesh.GetComponent<MeshFilter>();
                DestroyImmediate(fluidMesh.GetComponent<Collider>());
            }

            blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];
            if(!rebuilding)
                BuildChunkData();

            for (int pass = 0; pass < 2; pass++)
            {
                List<Mesh> inputMeshes = new List<Mesh>();
                int vertexStart = 0;
                int triStart = 0;
                int meshCount = chunkSize.x * chunkSize.y * chunkSize.z;
                int m = 0;
                var jobs = new ProcessMeshDataJob();
                jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                for (int z = 0; z < chunkSize.z; z++) {
                    for (int y = 0; y < chunkSize.y; y++) {
                        for (int x = 0; x < chunkSize.x; x++) {
                            blocks[x,y,z] = new Block(new Vector3(x, y, z) + worldPosition, chunkData[(x + chunkSize.x * (y + chunkSize.z * z))],
                                this, healthData[(x + chunkSize.x * (y + chunkSize.z * z))]);
                            if(blocks[x, y, z].mesh != null && 
                                (((pass == 0) && !BlockStaticData.canFlow.Contains(chunkData[(x + chunkSize.x * (y + chunkSize.z * z))])) ||
                                ((pass == 1) && BlockStaticData.canFlow.Contains(chunkData[(x + chunkSize.x * (y + chunkSize.z * z))]))))
                            {
                                inputMeshes.Add(blocks[x, y, z].mesh);
                                int vertexCount = blocks[x, y, z].mesh.vertexCount;
                                int indexCount = (int)blocks[x, y, z].mesh.GetIndexCount(0);
                                jobs.vertexStart[m] = vertexStart;
                                jobs.triStart[m] = triStart;
                                vertexStart += vertexCount;
                                triStart += indexCount;
                                m++;
                            }
                        }
                    }
                }

                jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
                Mesh.MeshDataArray outputMeshData = Mesh.AllocateWritableMeshData(1);
                jobs.outputMesh = outputMeshData[0];
                jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
                jobs.outputMesh.SetVertexBufferParams(vertexStart,
                                                    new VertexAttributeDescriptor(VertexAttribute.Position), 
                                                    new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                                                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2),
                                                    new VertexAttributeDescriptor(VertexAttribute.TexCoord1, stream: 3));
                JobHandle handle = jobs.Schedule(inputMeshes.Count, 4);
                Mesh newMesh = new Mesh();
                newMesh.name = $"Chunk_{worldPosition.x}_{worldPosition.y}_{worldPosition.z}";
                SubMeshDescriptor subMesh = new SubMeshDescriptor(0, triStart, MeshTopology.Triangles);
                subMesh.firstVertex = 0;
                subMesh.vertexCount = vertexStart;

                handle.Complete();

                jobs.outputMesh.subMeshCount = 1;
                jobs.outputMesh.SetSubMesh(0, subMesh);

                Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] {newMesh});
                jobs.meshData.Dispose();
                jobs.vertexStart.Dispose();
                jobs.triStart.Dispose();

                newMesh.RecalculateBounds();
                if(pass == 0) {
                    meshFilterSolid.mesh = newMesh;
                    MeshCollider collider = solidMesh.AddComponent<MeshCollider>();
                    collider.sharedMesh = meshFilterSolid.mesh;
                } else {
                    meshFilterFluid.mesh = newMesh;
                    MeshCollider collider = fluidMesh.AddComponent<MeshCollider>();
                    collider.sharedMesh = meshFilterFluid.mesh;
                }
            }
        }

        [BurstCompile]
        struct ProcessMeshDataJob: IJobParallelFor
        {
            [ReadOnly] public Mesh.MeshDataArray meshData;
            public Mesh.MeshData outputMesh;
            public NativeArray<int> vertexStart;
            public NativeArray<int> triStart;

            public void Execute(int index) {
                Mesh.MeshData data = meshData[index];
                int vCount = data.vertexCount;
                int vStart = vertexStart[index];

                NativeArray<float3> verts = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                data.GetVertices(verts.Reinterpret<Vector3>());

                NativeArray<float3> normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                data.GetNormals(normals.Reinterpret<Vector3>());

                NativeArray<float3> uv = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                data.GetUVs(0, uv.Reinterpret<Vector3>());

                NativeArray<float3> uv2 = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                data.GetUVs(1, uv2.Reinterpret<Vector3>());

                NativeArray<Vector3> outputVerts = outputMesh.GetVertexData<Vector3>();
                NativeArray<Vector3> outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
                NativeArray<Vector3> outputUV = outputMesh.GetVertexData<Vector3>(stream: 2);
                NativeArray<Vector3> outputUV2 = outputMesh.GetVertexData<Vector3>(stream: 3);

                for (int i = 0; i < vCount; i++) {
                    outputVerts[i + vStart] = verts[i];
                    outputNormals[i + vStart] = normals[i];
                    outputUV[i + vStart] = uv[i];
                    outputUV2[i + vStart] = uv2[i];
                }
                verts.Dispose();
                normals.Dispose();
                uv.Dispose();
                uv2.Dispose();

                int tStart = triStart[index];
                int tCount = data.GetSubMesh(0).indexCount;
                NativeArray<int> outputTris = outputMesh.GetIndexData<int>();

                if(data.indexFormat == IndexFormat.UInt16) {
                    NativeArray<ushort> tris = data.GetIndexData<ushort>();
                    for (int i = 0; i < tCount; i++) {
                        int idx = tris[i];
                        outputTris [i + tStart] = vStart + idx;
                    }
                } else {
                    NativeArray<int> tris = data.GetIndexData<int>();
                    for (int i = 0; i < tCount; i++) {
                        int idx = tris[i];
                        outputTris [i + tStart] = vStart + idx;
                    }
                }
            }
        }
    }
}