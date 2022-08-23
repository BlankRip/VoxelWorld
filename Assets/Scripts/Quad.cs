using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.WorldBuilding {
    public class Quad
    {
        public Mesh mesh;

        public Quad(BlockStaticData.BlockSide side, Vector3 offset, BlockStaticData.BlockType blockTye) {
            mesh = new Mesh();
            mesh.name = "Scripted Quad";

            Vector3[] vertices = new Vector3[4];
            Vector3[] normals = new Vector3[4];
            Vector2[] uv =  new Vector2[4];
            int[] triangles = new int[6];

            Vector2 uv00 = BlockStaticData.blockUVs[(int)blockTye, 0];
            Vector2 uv10 = BlockStaticData.blockUVs[(int)blockTye, 1];
            Vector2 uv01 = BlockStaticData.blockUVs[(int)blockTye, 2];
            Vector2 uv11 = BlockStaticData.blockUVs[(int)blockTye, 3];

            Vector3 p0 = offset + new Vector3(-0.5f, -0.5f, 0.5f);
            Vector3 p1 = offset + new Vector3(0.5f, -0.5f, 0.5f);
            Vector3 p2 = offset + new Vector3(0.5f, -0.5f, -0.5f);
            Vector3 p3 = offset + new Vector3(-0.5f, -0.5f, -0.5f);
            Vector3 p4 = offset + new Vector3(-0.5f, 0.5f, 0.5f);
            Vector3 p5 = offset + new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 p6 = offset + new Vector3(0.5f, 0.5f, -0.5f);
            Vector3 p7 = offset + new Vector3(-0.5f, 0.5f, -0.5f);

            switch(side) {
                case BlockStaticData.BlockSide.Front: {
                    vertices = new Vector3[] {p4, p5, p1, p0};
                    normals = new Vector3[] {Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward};
                    break;
                } case BlockStaticData.BlockSide.Back: {
                    vertices = new Vector3[] {p6, p7, p3, p2};
                    normals = new Vector3[] {Vector3.back, Vector3.back, Vector3.back, Vector3.back};
                    break;
                } case BlockStaticData.BlockSide.Bottom: {
                    vertices = new Vector3[] {p0, p1, p2, p3};
                    normals = new Vector3[] {Vector3.down, Vector3.down, Vector3.down, Vector3.down};
                    break;
                } case BlockStaticData.BlockSide.Top: {
                    vertices = new Vector3[] {p7, p6, p5, p4};
                    normals = new Vector3[] {Vector3.up, Vector3.up, Vector3.up, Vector3.up};
                    break;
                } case BlockStaticData.BlockSide.Left: {
                    vertices = new Vector3[] {p7, p4, p0, p3};
                    normals = new Vector3[] {Vector3.left, Vector3.left, Vector3.left, Vector3.left};
                    break;
                } case BlockStaticData.BlockSide.Right: {
                    vertices = new Vector3[] {p5, p6, p2, p1};
                    normals = new Vector3[] {Vector3.right, Vector3.right, Vector3.right, Vector3.right};
                    break;
                }
            }
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
}