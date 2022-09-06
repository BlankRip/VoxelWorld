using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.WorldBuilding {
    public class WaterManager : MonoBehaviour
    {
        [SerializeField] GameObject player;

        private void Update() {
            gameObject.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        }
    }
}