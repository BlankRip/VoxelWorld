using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class World : MonoBehaviour
    {
        public static Vector3 worldDimensions = new Vector3(3, 3, 3);
        public static Vector3 cunkDimensions = new Vector3(10, 10, 10);
        public GameObject chunkPrefab;
    }
}