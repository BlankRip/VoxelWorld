using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeWorld {
    public class WorldController : MonoBehaviour {
        [SerializeField] GameObject block;
        [SerializeField] Vector3Int worldSize = new Vector3Int(2, 2, 2);
        [SerializeField] int numTopLayersToUseRandom = 3;

        private void Start() {
            StartCoroutine(BuildWorld());
        }

        public IEnumerator BuildWorld() {
            GameObject cube;
            WaitForSeconds gap = new WaitForSeconds(0.001f);
            bool shouldRanomize = false;
            for (int y = 0; y < worldSize.y; y++) {
                if(y >= worldSize.y - numTopLayersToUseRandom)
                    shouldRanomize = true;
                for (int z = 0; z < worldSize.z; z++) {
                    for (int x = 0; x < worldSize.x; x++) {
                        if(shouldRanomize && Random.Range(0, 101) < 35) 
                            continue;
                        cube = GameObject.Instantiate(block, new Vector3(x, y, z), Quaternion.identity);
                        cube.name = $"{x}_{y}_{z}";
                        yield return gap;
                    }
                }
            }
        }    
    }
}