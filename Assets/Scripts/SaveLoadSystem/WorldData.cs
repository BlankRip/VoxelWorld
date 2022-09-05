using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockyWorld.WorldBuilding;

namespace BlockyWorld.SaveLoadSystem
{
    [System.Serializable]
    public class WorldData
    {
        public int[] chunkCheckerValues;
        public int[] chunkColumnValues;
        public int[] allChunkData;
        public bool[] chunkVisibility;

        public int playerPosX;
        public int playerPosY;
        public int playerPosZ;

        public WorldData() { }

        public WorldData(HashSet<Vector3Int> chunkChecker, HashSet<Vector2Int> chunkColumns,
            Dictionary<Vector3Int, Chunk> chunks, Vector3 playerPos)
        {
            chunkCheckerValues = new int[chunkChecker.Count * 3];
            int index = 0;
            foreach (Vector3Int v in chunkChecker) {
                chunkCheckerValues[index] = v.x;
                chunkCheckerValues[index + 1] = v.y;
                chunkCheckerValues[index + 2] = v.z;
                index += 3;
            }

            chunkColumnValues = new int[chunkColumns.Count * 2];
            index = 0;
            foreach (Vector2Int v in chunkColumns) {
                chunkColumnValues[index] = v.x;
                chunkColumnValues[index + 1] = v.y;
                index += 2;
            }

            allChunkData = new int[chunks.Count * World.chunkDimensions.x * World.chunkDimensions.y * World.chunkDimensions.z];
            chunkVisibility = new bool[chunks.Count];
            int vIndex = 0;
            index = 0;
            foreach (KeyValuePair<Vector3Int, Chunk> ch in chunks) {
                foreach (BlockStaticData.BlockType bType in ch.Value.chunkData) {
                    allChunkData[index] = (int)bType;
                    index++;
                }
                chunkVisibility[vIndex] = ch.Value.meshRenderer.enabled;
                vIndex++;
            }

            playerPosX = (int)playerPos.x;
            playerPosY = (int)playerPos.y;
            playerPosZ = (int)playerPos.z;
        }
    }
}