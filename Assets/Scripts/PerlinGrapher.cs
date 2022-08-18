using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3(0, 0, 0);
    [SerializeField] float hightScale = 2.0f;
    [SerializeField] float scale = 0.5f;
    [SerializeField] int octives = 1;

    private LineRenderer lr;

    private void Start() {
        Graph();
    }

    //Factorial Browniem Motion
    float fBM(float x, float z, int octives) {
        float total = 0.0f;
        float frequency = 1.0f;
        for (int i = 0; i < octives; i++) {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * hightScale;
            frequency *= 2;
        }
        return total;
    }

    private void Graph() {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 100;
        int z = 11;
        Vector3[] positions = new Vector3[lr.positionCount];
        for (int x = 0; x < lr.positionCount; x++) {
            float y = fBM(x, z, octives);
            positions[x] = new Vector3( offset.x + x, offset.y + y, offset.z + z);
        }
        lr.SetPositions(positions);
    }

    private void OnValidate() {
        Graph();
    }
}
