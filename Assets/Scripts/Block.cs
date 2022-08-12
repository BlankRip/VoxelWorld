using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld {
    public class Block : MonoBehaviour
    {
        public enum BlockSide {Nada, Bottom, Top, Left, Right, Front, Back};

        [SerializeField] BlockSide testSide = BlockSide.Front;

        private void Start() {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Quad q = new Quad();
            meshFilter.mesh = q.Build(testSide, new Vector3(1, 1, 1));
        }
    }       
}