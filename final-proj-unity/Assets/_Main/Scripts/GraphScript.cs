using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphScript : MonoBehaviour {
    public void graph(float value) {
        if (GraphManager.Graph != null) {
            GraphManager.Graph.Plot("CameraCanvas_WorldSpace", value, Color.green, new GraphManager.Matrix4x4Wrapper(transform.position, transform.rotation, transform.localScale));
        }
    }
}
