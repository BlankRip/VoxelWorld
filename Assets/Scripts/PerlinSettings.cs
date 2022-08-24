using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.Perlin {
    [System.Serializable]
    public struct PerlinSettings {
        public int octives;
        [Range(0.0f, 0.5f)] public float scale;
        public float heightScale;
        public float heightOffset;
        [Range(0.0f, 1.0f)] public float probability;
        
        [Header("Only applicable for 3D")]
        [Range(0.0f, 10.0f)] public float drawCutoff;

        public PerlinSettings(int octives, float scale, float  hightScale, float heightOffset, float probability = 1.0f, float drawCutoff = 0.0f) {
            this.heightOffset = heightOffset;
            this.octives = octives;
            this.scale = scale;
            this.heightScale = hightScale;
            this.probability = probability;
            this.drawCutoff = drawCutoff;
        }

        public PerlinSettings (PerlinSettings settings) {
            this.heightOffset = settings.heightOffset;
            this.octives = settings.octives;
            this.scale = settings.scale;
            this.heightScale = settings.heightScale;
            this.probability = settings.probability;
            this.drawCutoff = settings.drawCutoff;
        }
    }
}