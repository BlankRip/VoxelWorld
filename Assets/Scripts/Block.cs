 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.WorldBuilding {
    public class Block
    {
        public Mesh mesh;
        private Chunk parentChunk;

        public Block(Vector3 offset, BlockStaticData.BlockType type, Chunk chunk) {
            parentChunk = chunk;
            if(type == BlockStaticData.BlockType.Air)
                return;
            
            Vector3 localPosInFloat = (offset - chunk.worldPosition);
            Vector3Int blockLocalPos = new Vector3Int((int)localPosInFloat.x, (int)localPosInFloat.y, (int)localPosInFloat.z);
            List<Quad> quads = new List<Quad>();
            if(!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y - 1, blockLocalPos.z)) {
                if(type == BlockStaticData.BlockType.GrassSide)
                    quads.Add(new Quad(BlockStaticData.BlockSide.Bottom, offset, BlockStaticData.BlockType.Dirt));
                else
                    quads.Add(new Quad(BlockStaticData.BlockSide.Bottom, offset, type));
            }
            if(!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y + 1, blockLocalPos.z)) {
                if(type == BlockStaticData.BlockType.GrassSide)
                    quads.Add(new Quad(BlockStaticData.BlockSide.Top, offset, BlockStaticData.BlockType.GrassTop));
                else
                    quads.Add(new Quad(BlockStaticData.BlockSide.Top, offset, type));
            }
            if(!HasSolidNeighbour(blockLocalPos.x - 1, blockLocalPos.y, blockLocalPos.z))
                quads.Add(new Quad(BlockStaticData.BlockSide.Left, offset, type));
            if(!HasSolidNeighbour(blockLocalPos.x + 1, blockLocalPos.y, blockLocalPos.z))
                quads.Add(new Quad(BlockStaticData.BlockSide.Right, offset, type));
            if(!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y, blockLocalPos.z + 1))
                quads.Add(new Quad(BlockStaticData.BlockSide.Front, offset, type));
            if(!HasSolidNeighbour(blockLocalPos.x, blockLocalPos.y, blockLocalPos.z - 1))
                quads.Add(new Quad(BlockStaticData.BlockSide.Back, offset, type));

            if(quads.Count == 0)
                return;

            Mesh[] sideMeshes = new Mesh[quads.Count];
            for (int i = 0; i < quads.Count; i++)
                sideMeshes[i] = quads[i].mesh;
            
            mesh = MeshUtils.MergeMeshes(sideMeshes);
            mesh.name = $"Cube_{offset.x}_{offset.y}_{offset.z}";
        }

        private bool HasSolidNeighbour(int x, int y, int z) {
            if(x < 0 || x >= parentChunk.chunkSize.x || y < 0 || y >= parentChunk.chunkSize.y ||
            z < 0 || z >= parentChunk.chunkSize.z) {
                return false;
            }

            int neighbourIndex = x + (parentChunk.chunkSize.x * (y + (parentChunk.chunkSize.z * z)));
            if(parentChunk.chunkData[neighbourIndex] == BlockStaticData.BlockType.Air ||
            parentChunk.chunkData[neighbourIndex] == BlockStaticData.BlockType.Water) {
                return false;
            }
            return true;
        }
    }       
}