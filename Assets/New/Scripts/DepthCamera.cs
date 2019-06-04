using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthCamera : MonoBehaviour
{
    private void Start()
    {
        Shader shader = Shader.Find("");

        Camera cam = GetComponent<Camera>();

        cam.depthTextureMode = DepthTextureMode.Depth;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = Color.white;
        cam.renderingPath = RenderingPath.Forward;
        cam.SetReplacementShader(Shader.Find("HCS/S_DepthMap"), "RenderType");
    }
}