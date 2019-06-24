using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSMMain : MonoBehaviour
{
    private FrustumCorners mainCamera_fcs, lightCamera_fcs;


    private void Start()
    {
        InitFrustumCorners();
        CalcMainCameraFrustumCorners();
    }

    private void InitFrustumCorners()
    {
        mainCamera_fcs = FrustumCorners.New();
        lightCamera_fcs = FrustumCorners.New();
    }

    private void CalcMainCameraFrustumCorners()
    {
        var mainCamera = Camera.main;

        //这里得到的是局部坐标要转换
        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), mainCamera.nearClipPlane,
            Camera.MonoOrStereoscopicEye.Mono, mainCamera_fcs.nearCorners);

        for (int i = 0; i < 4; i++)
        {
            mainCamera_fcs.nearCorners[i] = mainCamera.transform.TransformPoint(mainCamera_fcs.nearCorners[i]);
        }

        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), mainCamera.farClipPlane,
            Camera.MonoOrStereoscopicEye.Mono, mainCamera_fcs.farCorners);

        for (int i = 0; i < 4; i++)
        {
            mainCamera_fcs.farCorners[i] = mainCamera.transform.TransformPoint(mainCamera_fcs.farCorners[i]);
        }
    }
}