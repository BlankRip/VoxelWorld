using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.Perlin {
    [System.Serializable]
    public struct PerlinSettings {
        public int octives;
        [Range(0.0f, 0.5f)] public float scale;
        public float hightScale;
        public float heightOffset;
        [Range(0.0f, 1.0f)] public float probability;

        public PerlinSettings(int octives, float scale, float  hightScale, float heightOffset, float probability) {
            this.heightOffset = heightOffset;
            this.octives = octives;
            this.scale = scale;
            this.hightScale = hightScale;
            this.probability = probability;
        }

        public PerlinSettings (PerlinSettings settings) {
            this.heightOffset = settings.heightOffset;
            this.octives = settings.octives;
            this.scale = settings.scale;
            this.hightScale = settings.hightScale;
            this.probability = settings.probability;
        }
    }
}