using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour {

    public GameObject cameraRig;

	// Use this for initialization
	void Start () {
        // UnityEngine.XR.InputTracking.disablePositionalTracking = true;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 oldPos = cameraRig.transform.position;
        cameraRig.transform.position = new Vector3(oldPos.x + 0.010f, oldPos.y, oldPos.z);
	}
}
