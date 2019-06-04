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
        var go = new GameObject("DepthCamera");
        var camTs = go.transform;
        camTs.SetParent(lightTrans);
        camTs.localPosition = Vector3.zero;
        camTs.localRotation = Quaternion.identity;

        cam = camTs.gameObject.AddComponent<Camera>();
        cam.enabled = false;
        cam.depth = 2;
        cam.clearFlags = CameraClearFlags.SolidColor;
        //这个黑色很重要
        cam.backgroundColor = new Color(0, 0, 0, 0);

        cam.aspect = 1;
        cam.orthographic = true;
        cam.orthographicSize = 10;

        /*
         * 一个顶点，经过MVP变化之后，其xyz分量的取值范围是[-1, 1]
         * 现在我们需要使用这个变化过的顶点值来找到 shadow depth map中对应的点来比较深度，
         * 即要作为UV使用，而UV的取值范围是[0, 1]，
         * 所以需要进行一个值域的变换，这就是这个矩阵的作用。
         * 需要注意的是，要使这个矩阵成立，
         * 该vector4的 w 分量必须是 1。在shader中运算的时候必须注意。
         */
        sm.SetRow(0, new Vector4(0.5f, 0, 0, 0.5f));
        sm.SetRow(1, new Vector4(0, 0.5f, 0, 0.5f));
        sm.SetRow(2, new Vector4(0, 0, 0.5f, 0.5f));
        sm.SetRow(3, new Vector4(0, 0, 0, 1));

        //阴影的清晰度
        rt = new RenderTexture(1024, 1024, 0) {wrapMode = TextureWrapMode.Clamp};
        cam.targetTexture = rt;
        cam.SetReplacementShader(Shader.Find("HCS/S_DepthTexture"), "RenderType");
    }

    private void Update()
    {
        cam.Render();

        //相机的NDC建立
        Matrix4x4 tm = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;

        tm = sm * tm;

        Shader.SetGlobalMatrix("proMatrix",tm);
        Shader.SetGlobalTexture("depthTexture",rt);
    }
}