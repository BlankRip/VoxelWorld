using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;


namespace BlockyWorld {
    public class Chunk : MonoBehaviour
    {
        [Header("Chunk Data")]
        public Vector3Int chunkSize = new Vector3Int(2, 2, 2);
        [SerializeField] Material atlas;
        [SerializeField] MeshUtils.BlockType blockType;

        [Header("Perlin height graph")]
        [SerializeField] Vector3Int offset = new Vector3Int(0, -33, 0);
        [SerializeField] int octives = 8;
        [SerializeField] float scale = 0.001f;
        [SerializeField] float hightScale = 10.0f;

        private Block[,,] blocks;
        //Flaten 3d array [x + width(3d.x) * (y + depth(3d.z) * z)] = [x, y, z] in 3d array
        //Flat to 3d x = i % width (3d.x);  y = i/width(3d.x) % height (3d.y);   z = i / (width (3d.x) * height (3d.y))
        [HideInInspector] public MeshUtils.BlockType[] chunkData;

        void BuildChunkData() {
            int blockCount = chunkSize.x * chunkSize.y * chunkSize.z;
            chunkData = new MeshUtils.BlockType[blockCount];
            for (int i = 0; i < blockCount; i++) {
                int x = i % chunkSize.x + offset.x;
                int y = (i / chunkSize.x) % chunkSize.y;
                int z = i / (chunkSize.x * chunkSize.z) + offset.z;
                if(MeshUtils.fBM(x, z, octives, scale, hightScale, offset.y) > y)
                    chunkData[i] = blockType;
                else
                    chunkData[i] = MeshUtils.BlockType.Air;
            }
        }

        private void Start() {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = atlas;
            blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];
            BuildChunkData();

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
                        blocks[x,y,z] = new Block(new Vector3(x, y, z), chunkData[(x + (chunkSize.x * (y + (chunkSize.z * z))))], this);
                        if(blocks[x, y, z].mesh != null) {
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
                                                new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0), 
                                                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                                                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2));
            JobHandle handle = jobs.Schedule(inputMeshes.Count, 4);
            Mesh newMesh = new Mesh();
            newMesh.name = "Chunk";
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
            meshFilter.mesh = newMesh;
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

                NativeArray<Vector3> outputVerts = outputMesh.GetVertexData<Vector3>();
                NativeArray<Vector3> outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
                NativeArray<Vector3> outputUV = outputMesh.GetVertexData<Vector3>(stream: 2);

                for (int i = 0; i < vCount; i++) {
                    outputVerts[i + vStart] = verts[i];
                    outputNormals[i + vStart] = normals[i];
                    outputUV[i + vStart] = uv[i];
                }
                verts.Dispose();
                normals.Dispose();
                uv.Dispose();

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