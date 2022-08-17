using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class Chunk : MonoBehaviour
    {
        [SerializeField] Vector3Int chunkSize = new Vector3Int(2, 2, 2);
        [SerializeField] Material atlas;
        [SerializeField] MeshUtils.BlockType blockType;

        private Block[,,] blocks;

        private void Start() {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = atlas;
            blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];

            for (int z = 0; z < chunkSize.z; z++) {
                for (int y = 0; y < chunkSize.y; y++) {
                    for (int x = 0; x < chunkSize.x; x++) {
                        blocks[x,y,z] = new Block(new Vector3(x, y, z), MeshUtils.BlockType.Dirt);
                    }
                }
            }
        }
    }
}