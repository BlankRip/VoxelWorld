using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class BlockStaticData
    {
        public enum BlockSide {Nada, Bottom, Top, Left, Right, Front, Back};

        public enum BlockType {
            GrassTop, GrassSide, Dirt, Water, Stone, Leaves, Wood, WoodBase, Sand, Gold, BedRock, RedStone, Diamond,
            NoCrack, Crack1, Crack2, Crack3, Crack4, Air
        };

        public static int[] blockTypeHealth = {
            /*GrassTop*/ 2, /*GrassSide*/ 2, /*Dirt*/ 1, /*Water*/ 1, 
            /*Stone*/ 4, /*Leaves*/ 2, /*Wood*/ 4, /*WoodBase*/ 4,
            /*Sand*/ 3, /*Gold*/ 4, /*BedRock*/ -1, /*RedStone*/ 3,
            /*Diamond*/ 4, /*NoCrack*/ -1, /*Crack1*/ -1, /*Crack2*/ -1,
            /*Crack3*/ -1, /*Crack4*/ -1, /*Air*/ -1
        };

        public static HashSet<BlockType> canDrop = new HashSet<BlockType> { BlockType.Sand, BlockType.Water };
        public static HashSet<BlockType> canFlow = new HashSet<BlockType> { BlockType.Water };

        public static (Vector3Int, BlockType)[] treeDesign = new (Vector3Int, BlockType)[] {
                (new Vector3Int(0,1,-1), BlockType.Leaves),
                (new Vector3Int(1,1,-1), BlockType.Leaves),
                (new Vector3Int(-1,2,-1), BlockType.Leaves),
                (new Vector3Int(0,2,-1), BlockType.Leaves),
                (new Vector3Int(0,3,-1), BlockType.Leaves),
                (new Vector3Int(-1,4,-1), BlockType.Leaves),
                (new Vector3Int(0,4,-1), BlockType.Leaves),
                (new Vector3Int(-1,5,-1), BlockType.Leaves),
                (new Vector3Int(1,5,-1), BlockType.Leaves),
                (new Vector3Int(0,0,0), BlockType.Wood),
                (new Vector3Int(0,1,0), BlockType.Wood),
                (new Vector3Int(0,2,0), BlockType.Wood),
                (new Vector3Int(1,2,0), BlockType.Leaves),
                (new Vector3Int(0,3,0), BlockType.Leaves),
                (new Vector3Int(1,3,0), BlockType.Leaves),
                (new Vector3Int(0,4,0), BlockType.Leaves),
                (new Vector3Int(0,5,0), BlockType.Leaves),
                (new Vector3Int(1,5,0), BlockType.Leaves),
                (new Vector3Int(0,1,1), BlockType.Leaves),
                (new Vector3Int(-1,2,1), BlockType.Leaves),
                (new Vector3Int(0,2,1), BlockType.Leaves),
                (new Vector3Int(-1,3,1), BlockType.Leaves),
                (new Vector3Int(0,3,1), BlockType.Leaves),
                (new Vector3Int(0,4,1), BlockType.Leaves),
                (new Vector3Int(-1,5,1), BlockType.Leaves),
                (new Vector3Int(0,5,1), BlockType.Leaves),
                (new Vector3Int(1,5,1), BlockType.Leaves)
        };

        public static Vector2[,] blockUVs = {
            /*GrassTop*/
            {
                new Vector2(0.125f, 0.375f), new Vector2(0.1875f,0.375f),
                new Vector2(0.125f, 0.4375f), new Vector2(0.1875f,0.4375f)
            },
            /*GrassSide*/
            { 
                new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f),
                new Vector2( 0.1875f, 1.0f ), new Vector2( 0.25f, 1.0f )
            },
            /*Dirt*/
            {
                new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f),
                new Vector2( 0.125f, 1.0f ), new Vector2( 0.1875f, 1.0f )
            },
            /*Water*/
            {
                new Vector2(0.875f,0.125f),  new Vector2(0.9375f,0.125f),
                new Vector2(0.875f,0.1875f), new Vector2(0.9375f,0.1875f)
            },
            /*Stone*/
            {
                new Vector2( 0, 0.875f ), new Vector2( 0.0625f, 0.875f),
                new Vector2( 0, 0.9375f ),new Vector2( 0.0625f, 0.9375f )
            },
            /*Leaves*/
            {
                new Vector2(0.0625f,0.375f),  new Vector2(0.125f,0.375f),
                new Vector2(0.0625f,0.4375f), new Vector2(0.125f,0.4375f)
            },
 		    /*Wood*/
            {
                new Vector2(0.375f,0.625f),  new Vector2(0.4375f,0.625f),
                new Vector2(0.375f,0.6875f), new Vector2(0.4375f,0.6875f)
            },
 		    /*WoodBase*/
            {
                new Vector2(0.375f,0.625f),  new Vector2(0.4375f,0.625f),
                new Vector2(0.375f,0.6875f), new Vector2(0.4375f,0.6875f)
            },
            /*Sand*/
            {
                new Vector2(0.125f,0.875f),  new Vector2(0.1875f,0.875f),
                new Vector2(0.125f,0.9375f), new Vector2(0.1875f,0.9375f)
            },
            /*Gold*/
            {
                new Vector2(0f,0.8125f),  new Vector2(0.0625f,0.8125f),
                new Vector2(0f,0.875f), new Vector2(0.0625f,0.875f)
            },
            /*BedRock*/
            {
                new Vector2( 0.3125f, 0.8125f ), new Vector2( 0.375f, 0.8125f),
                new Vector2( 0.3125f, 0.875f ), new Vector2( 0.375f, 0.875f )
            },
            /*RedStone*/
            {
                new Vector2( 0.1875f, 0.75f ), new Vector2( 0.25f, 0.75f),
                new Vector2( 0.1875f, 0.8125f ), new Vector2( 0.25f, 0.8125f )
            },
            /*Diamond*/
            {
                new Vector2( 0.125f, 0.75f ), new Vector2( 0.1875f, 0.75f),
                new Vector2( 0.125f, 0.8125f ), new Vector2( 0.1875f, 0.8125f )
            },
            /*NoCrack*/
            {
                new Vector2(0f,0.9375f), new Vector2(0.0625f,0.9375f),
                new Vector2(0f,1.0f), new Vector2(0.0625f,1.0f)
            },
            /*Crack1*/
            {
                new Vector2(0f,0f), new Vector2(0.0625f,0f),
                new Vector2(0f,0.0625f), new Vector2(0.0625f,0.0625f)
            },
            /*Crack2*/
            {
                new Vector2(0.0625f,0f), new Vector2(0.125f,0f),
                new Vector2(0.0625f,0.0625f), new Vector2(0.125f,0.0625f)
            },
            /*Crack3*/
            {
                new Vector2(0.125f,0f), new Vector2(0.1875f,0f),
                new Vector2(0.125f,0.0625f), new Vector2(0.1875f,0.0625f)
            },
            /*Crack4*/
            {
                new Vector2(0.1875f,0f), new Vector2(0.25f,0f),
                new Vector2(0.1875f,0.0625f), new Vector2(0.25f,0.0625f)
            }
        };
    }
}