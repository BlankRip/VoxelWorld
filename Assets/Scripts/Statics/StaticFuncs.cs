using System.Collections;
using System.Collections.Generic;
using BlockyWorld.WorldBuilding;
using UnityEngine;

namespace BlockyWorld
{
    public class StaticFuncs
    {
        public static Vector3Int FromFlat(int i) {
            return new Vector3Int(i % World.chunkDimensions.x,
                (i / World.chunkDimensions.x) % World.chunkDimensions.y,
                i / (World.chunkDimensions.x * World.chunkDimensions.y));
        }

        public static int ToFlat(Vector3Int v) {
            return v.x + World.chunkDimensions.x * (v.y + World.chunkDimensions.z * v.z);
        }
    }
}