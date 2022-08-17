using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class Block
    {
        public Mesh mesh;

        public Block(Vector3 offset, MeshUtils.BlockType type) {
            Quad[] quads = new Quad[6];
            quads[0] = new Quad(MeshUtils.BlockSide.Bottom, offset, type);
            quads[1] = new Quad(MeshUtils.BlockSide.Top, offset, type);
            quads[2] = new Quad(MeshUtils.BlockSide.Left, offset, type);
            quads[3] = new Quad(MeshUtils.BlockSide.Right, offset, type);
            quads[4] = new Quad(MeshUtils.BlockSide.Front, offset, type);
            quads[5] = new Quad(MeshUtils.BlockSide.Back, offset, type);

            Mesh[] sideMeshes = new Mesh[6];
            for (int i = 0; i < quads.Length; i++)
                sideMeshes[i] = quads[i].mesh;
            
            mesh = MeshUtils.MergeMeshes(sideMeshes);
            mesh.name = "Cube_0_0_0";
        }
    }       
}