using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTextureCamera : MonoBehaviour
{
    public Transform lightTrans;

    private Camera cam;
    private RenderTexture rt;

    Matrix4x4 sm = new Matrix4x4();

    private void Awake()
    {
        var camTs = new GameObject("DepthCamera").transform;
        camTs.SetParent(lightTrans);
        camTs.localPosition = Vector3.zero;
        camTs.localRotation = Quaternion.identity;

        cam = camTs.gameObject.AddComponent<Camera>();
        cam.depth = 2;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1, 1, 1, 0);

        cam.aspect = 1;
        cam.orthographic = true;
        cam.orthographicSize = 10;

        sm.m00 = sm.m11 = sm.m22 = sm.m03 = sm.m13 = sm.m23 = 0.5f;
        sm.m33 = 1;

        rt = new RenderTexture(1920, 1080, 0) {wrapMode = TextureWrapMode.Clamp};
        cam.targetTexture = rt;
        cam.SetReplacementShader(Shader.Find("HCS/DepthTexture"), "RenderType");
    }

    private void Update()
    {
        cam.Render();
        //相机的矩阵计算
        Matrix4x4 tm = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;

        tm = sm * tm;

        Shader.SetGlobalMatrix("proMatrix",tm);
        Shader.SetGlobalTexture("depthTexture",rt);
    }
}