using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BW_Quad : MonoBehaviour
{
    private void Start() {
        Mesh mesh;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        mesh = new Mesh();
        mesh.name = "Scripted Quad";
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[5];
        Vector3[] normals = new Vector3[4];
        Vector2[] uv =  new Vector2[4];
        int[] triangles = new int[6];

        Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv11 = new Vector2(1, 1);

        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f);

        vertices = new Vector3[] {p4, p5, p1, p0};
        normals = new Vector3[] {Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward};
        uv = new Vector2[] {uv11, uv01, uv00, uv10};
        triangles = new int[] {
            3, 1, 0,
            3, 2, 1
        };
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }
}
