using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowMapFloor : MonoBehaviour
{
    private Camera lightCam = null;
    private Material mat;

    // Use this for initialization
    void Start()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.name == "Camera")
                lightCam = cam;
        }

        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        return;
        mat.SetMatrix("_ProjMatrix", lightCam.projectionMatrix * lightCam.worldToCameraMatrix);
    }
}
