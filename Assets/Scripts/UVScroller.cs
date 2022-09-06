using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.WorldBuilding
{
    public class UVScroller : MonoBehaviour
    {
        private Vector2 scrollSpeed = new Vector2(0.0f, 0.01f);
        private Vector2 uvOffset = Vector2.zero;
        private Renderer renderer;

        private void Start() {
            renderer = GetComponent<Renderer>();
        }

        private void LateUpdate() {
            uvOffset += scrollSpeed * Time.deltaTime;
            if(uvOffset.x > 0.0625f)
                uvOffset = new Vector2(0, uvOffset.y);
            if(uvOffset.y > 0.0625f)
                uvOffset = new Vector2(uvOffset.x, 0);
            
            renderer.materials[0].SetTextureOffset("_MainTex", uvOffset);
        }

    }
}