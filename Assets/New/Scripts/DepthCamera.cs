﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthCamera : MonoBehaviour
{
    public Material floorMat;

    private Camera cam;
    private RenderTexture depthRT;

    private void Start()
    {
        cam = GetComponent<Camera>();

        depthRT = new RenderTexture(1024, 1024, 0) {wrapMode = TextureWrapMode.Clamp};

        cam.depthTextureMode = DepthTextureMode.Depth;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = Color.white;
        cam.renderingPath = RenderingPath.Forward;
        cam.SetReplacementShader(Shader.Find("HCS/S_DepthMap"), "RenderType");
        cam.targetTexture = depthRT;

        floorMat.SetTexture("_ShadowMap", depthRT);
    }

    void Update()
    {
        //物体 要传入投影到摄像机的矩阵
        floorMat.SetMatrix("_ProjMatrix", cam.projectionMatrix * cam.worldToCameraMatrix);
    }

    /*
    void Update()
    {

        //		Matrix4x4 CamView = new Matrix4x4();
        //		Matrix4x4 CamProj = new Matrix4x4();
        //
        //		Vector3 At  = new Vector3(0.0f, 0.0f, 0.0f);
        //		Vector3 Eye = new Vector3(0.0f, 5.0f, 0.0f);
        //		Vector3 Up  = new Vector3(0.0f, 1.0f, 0.0f);
        //
        //		Vector3 zaxis = Vector3.Normalize(At - Eye);
        //		Vector3 xaxis = Vector3.Normalize(Vector3.Cross(Up, zaxis));
        //		Vector3 yaxis = Vector3.Cross(zaxis, xaxis);
        //
        //		CamView.SetColumn(0, new Vector4(xaxis.x, xaxis.y, xaxis.z, -Vector3.Dot(Eye, xaxis)));
        //		CamView.SetColumn(1, new Vector4(yaxis.x, yaxis.y, yaxis.z, -Vector3.Dot(Eye, yaxis)));
        //		CamView.SetColumn(2, new Vector4(zaxis.x, zaxis.y, zaxis.z, -Vector3.Dot(Eye, zaxis)));
        //		CamView.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        //		CamView = Matrix4x4.Transpose(CamView);
        //
        //		float fovY = 107.0f;
        //		float width = 1.0f;
        //		float height = 1.0f;
        //		float zn = 0.3f;
        //		float zf = 6.0f;
        //
        //		float yScale = Mathf.Atan(fovY / 2.0f);
        //		float xScale = yScale / (width / height);
        //
        //		CamProj.SetColumn(0, new Vector4(xScale, 0.0f, 0.0f, 0.0f));
        //		CamProj.SetColumn(1, new Vector4(0.0f, yScale, 0.0f, 0.0f));
        //		CamProj.SetColumn(2, new Vector4(0.0f, 0.0f, zf / (zf - zn), -zn * zf / (zf - zn)));
        //		CamProj.SetColumn(3, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
        //		CamProj = Matrix4x4.Transpose(CamProj);
        //		Matrix4x4 camVP = CamProj * CamView;
        //
        //		Material[] materials = renderer.materials;
        //
        //        foreach( Material mat in materials ) 
        //		{
        //			mat.SetMatrix("_ProjMatrix", camVP);
        //		}


    }
    */
}