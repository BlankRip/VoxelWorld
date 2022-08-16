using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class Block : MonoBehaviour
    {
        public enum BlockSide {Nada, Bottom, Top, Left, Right, Front, Back};

        private void Start() {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Quad[] quads = new Quad[6];
            quads[0] = new Quad(BlockSide.Bottom, new Vector3(0, 0, 0));
            quads[1] = new Quad(BlockSide.Top, new Vector3(0, 0, 0));
            quads[2] = new Quad(BlockSide.Left, new Vector3(0, 0, 0));
            quads[3] = new Quad(BlockSide.Right, new Vector3(0, 0, 0));
            quads[4] = new Quad(BlockSide.Front, new Vector3(0, 0, 0));
            quads[5] = new Quad(BlockSide.Back, new Vector3(0, 0, 0));

            Mesh[] sideMeshes = new Mesh[6];
            for (int i = 0; i < quads.Length; i++)
                sideMeshes[i] = quads[i].mesh;
            
            meshFilter.mesh = MeshUtils.MergeMeshes(sideMeshes);
            meshFilter.mesh.name = "Cube_0_0_0";
        }
    }       
}