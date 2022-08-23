using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class BlockStaticData
    {
        public enum BlockSide {Nada, Bottom, Top, Left, Right, Front, Back};

        public enum BlockType {
            GrassTop, GrassSide, Dirt, Water, Stone, Sand, Gold, BedRock, RedStone, Diamond,
            NoCrack, Crack1, Crack2, Crack3, Crack4, Air
        };

        public static Vector2[,] blockUVs = {
            /*GrassTop*/
            {
                new Vector2(0.125f, 0.375f), new Vector2(0.1875f,0.375f),
                new Vector2(0.125f, 0.4375f), new Vector2(0.1875f,0.4375f)
            },
            /*GrassSide*/
            { new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f),
                new Vector2( 0.1875f, 1.0f ),new Vector2( 0.25f, 1.0f )
            },
            /*Dirt*/
            {
                new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f),
                new Vector2( 0.125f, 1.0f ),new Vector2( 0.1875f, 1.0f )
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
                new Vector2( 0.3125f, 0.875f ),new Vector2( 0.375f, 0.875f )
            },
            /*RedStone*/
            {
                new Vector2( 0.1875f, 0.75f ), new Vector2( 0.25f, 0.75f),
                new Vector2( 0.1875f, 0.8125f ),new Vector2( 0.25f, 0.8125f )
            },
            /*Diamond*/
            {
                new Vector2( 0.125f, 0.75f ), new Vector2( 0.1875f, 0.75f),
                new Vector2( 0.125f, 0.8125f ),new Vector2( 0.1875f, 0.8125f )
            },
            /*NoCrack*/
            {
                new Vector2( 0.6875f, 0f ), new Vector2( 0.75f, 0f),
                new Vector2( 0.6875f, 0.0625f ),new Vector2( 0.75f, 0.0625f )
            },
            /*Crack1*/
            {
                new Vector2(0f,0f),  new Vector2(0.0625f,0f),
                new Vector2(0f,0.0625f), new Vector2(0.0625f,0.0625f)
            },
            /*Crack2*/
            {
                new Vector2(0.0625f,0f),  new Vector2(0.125f,0f),
                new Vector2(0.0625f,0.0625f), new Vector2(0.125f,0.0625f)
            },
            /*Crack3*/
            {
                new Vector2(0.125f,0f),  new Vector2(0.1875f,0f),
                new Vector2(0.125f,0.0625f), new Vector2(0.1875f,0.0625f)
            },
            /*Crack4*/
            {
                new Vector2(0.1875f,0f),  new Vector2(0.25f,0f),
                new Vector2(0.1875f,0.0625f), new Vector2(0.25f,0.0625f)
            }
        };
    }
}