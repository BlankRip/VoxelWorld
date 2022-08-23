using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

namespace BlockyWorld {
    public class MeshUtils
    {
        //Factorial Browniem Motion
        public static float fBM(float x, float z, int octives, float scale, float hightScale, float heightOffset) {
            float total = 0.0f;
            float frequency = 1.0f;
            for (int i = 0; i < octives; i++) {
                total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * hightScale;
                frequency *= 2;
            }
            return total + heightOffset;
        }

        public static Mesh MergeMeshes(Mesh[] meshes) {
            Mesh mesh = new Mesh();
            Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
            HashSet<VertexData> pointsHash = new HashSet<VertexData>();
            List<int> tris = new List<int>();

            int pIndex = 0;
            for (int i = 0; i < meshes.Length; i++) {
                if(meshes[i] == null)
                    continue;
                for (int j = 0; j < meshes[i].vertices.Length; j++) {
                    Vector3 vertex = meshes[i].vertices[j];
                    Vector3 normal = meshes[i].normals[j];
                    Vector2 uv = meshes[i].uv[j];
                    VertexData point = new VertexData(vertex, normal, uv);
                    if(!pointsHash.Contains(point)) {
                        pointsOrder.Add(point, pIndex);
                        pointsHash.Add(point);
                        pIndex++;
                    }
                }

                for (int t = 0; t < meshes[i].triangles.Length; t++) {
                    int triPoint = meshes[i].triangles[t];
                    Vector3 vertex = meshes[i].vertices[triPoint];
                    Vector3 normal = meshes[i].normals[triPoint];
                    Vector2 uv = meshes[i].uv[triPoint];
                    VertexData point = new VertexData(vertex, normal, uv);
                    
                    int index;
                    pointsOrder.TryGetValue(point, out index);
                    tris.Add(index);
                }
                meshes[i] = null;
            }
            
            ExtractArrays(pointsOrder, ref mesh);
            mesh.triangles = tris.ToArray();
            return mesh;
        }
        private static void ExtractArrays(Dictionary<VertexData, int> list, ref Mesh mesh) {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();

            foreach (VertexData vd in list.Keys) {
                verts.Add(vd.Item1);
                norms.Add(vd.Item2);
                uv.Add(vd.Item3);
            }
            mesh.vertices = verts.ToArray();
            mesh.normals = norms.ToArray();
            mesh.uv = uv.ToArray();
        }
    }
}