using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

public class MeshUtils
{
    public enum BlockType {
        GrassTop, GrassSide, Dirt, Water, Stone, Sand, Air
    }

    public enum BlockSide {Nada, Bottom, Top, Left, Right, Front, Back};

    public static Vector2[,] blockUVs = {
        //Grass Top
        {
            new Vector2 (0.125f, 0.375f), new Vector2(0.1875f, 0.375f),
            new Vector2 (0.125f, 0.4375f), new Vector2(0.1875f, 0.4375f),
        },
        //Grass Side
        {
            new Vector2 (0.1875f, 0.9375f), new Vector2(0.25f, 0.9375f),
            new Vector2 (0.1875f, 1.0f), new Vector2(0.25f, 1.0f),
        },
        //Dirt
        {
            new Vector2 (0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f),
            new Vector2 (0.125f, 1.0f), new Vector2(0.1875f, 1.0f),
        },
        //Water
        {
            new Vector2 (0.875f, 0.125f), new Vector2(0.9375f, 0.125f),
            new Vector2 (0.875f, 0.1875f), new Vector2(0.9375f, 0.1875f),
        },
        //Stone
        {
            new Vector2 (0.0f, 0.875f), new Vector2(0.0625f, 0.875f),
            new Vector2 (0.0f, 0.9375f), new Vector2(0.0625f, 0.9375f),
        },
        //Sand
        {
            new Vector2 (0.125f, 0.875f), new Vector2(0.1875f, 0.875f),
            new Vector2 (0.125f, 0.9375f), new Vector2(0.1875f, 0.9375f)
        }
    };

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
