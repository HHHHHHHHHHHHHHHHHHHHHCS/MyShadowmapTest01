using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     近远裁面的八个顶点
/// </summary>
public struct FrustumCorners
{
    public Vector3[] nearCorners;
    public Vector3[] farCorners;

    /// <summary>
    ///     近裁面 的中心点
    /// </summary>
    public Vector3 nearCenter => (nearCorners[0] + nearCorners[2]) * 0.5f;

    /// <summary>
    ///     横纵比例
    /// </summary>
    public float aspect => Vector3.Magnitude(nearCorners[0] - nearCorners[1]) /
                           Vector3.Magnitude(nearCorners[1] - nearCorners[2]);

    /// <summary>
    ///     视野大小
    /// </summary>
    public float orthographicSize => Vector3.Magnitude(nearCorners[1] - nearCorners[2]) * 0.5f;

    public static FrustumCorners New()
    {
        var fc = new FrustumCorners
        {
            nearCorners = new Vector3[4],
            farCorners = new Vector3[4]
        };
        return fc;
    }

    /// <summary>
    ///     局部坐标转换到世界坐标
    /// </summary>
    public void LocalToWorld(Transform ts)
    {
        for (var i = 0; i < 4; i++)
        {
            nearCorners[i] = ts.TransformPoint(nearCorners[i]);
            farCorners[i] = ts.TransformPoint(farCorners[i]);
        }
    }

    /// <summary>
    ///     局部坐标转换到世界坐标  返回一个全新的
    /// </summary>
    public FrustumCorners LocalToWorld_New(Transform ts)
    {
        var fc = New();

        for (var i = 0; i < 4; i++)
        {
            fc.nearCorners[i] = ts.TransformPoint(nearCorners[i]);
            fc.farCorners[i] = ts.TransformPoint(farCorners[i]);
        }

        return fc;
    }

    /// <summary>
    ///     世界坐标转换到局部坐标
    /// </summary>
    /// <returns></returns>
    public void WorldToLocal(Transform ts)
    {
        for (var i = 0; i < 4; i++)
        {
            nearCorners[i] = ts.InverseTransformPoint(nearCorners[i]);
            farCorners[i] = ts.InverseTransformPoint(farCorners[i]);
        }
    }

    /// <summary>
    ///     世界坐标转换到局部坐标 返回一个全新的
    /// </summary>
    /// <returns></returns>
    public FrustumCorners WorldToLocal_New(Transform ts)
    {
        var fc = New();

        for (var i = 0; i < 4; i++)
        {
            fc.nearCorners[i] = ts.InverseTransformPoint(nearCorners[i]);
            fc.farCorners[i] = ts.InverseTransformPoint(farCorners[i]);
        }

        return fc;
    }

    /// <summary>
    ///     重新计算 生成一个包裹裁面的最大的矩形框
    /// </summary>
    public void RecalcBox()
    {
        float[] xs =
        {
            nearCorners[0].x, nearCorners[1].x,
            nearCorners[2].x, nearCorners[3].x,
            farCorners[0].x, farCorners[1].x,
            farCorners[2].x, farCorners[3].x
        };

        float[] ys =
        {
            nearCorners[0].y, nearCorners[1].y,
            nearCorners[2].y, nearCorners[3].y,
            farCorners[0].y, farCorners[1].y,
            farCorners[2].y, farCorners[3].y
        };

        float[] zs =
        {
            nearCorners[0].z, nearCorners[1].z,
            nearCorners[2].z, nearCorners[3].z,
            farCorners[0].z, farCorners[1].z,
            farCorners[2].z, farCorners[3].z
        };

        var minX = Mathf.Min(xs);
        var maxX = Mathf.Max(xs);

        var minY = Mathf.Min(ys);
        var maxY = Mathf.Max(ys);

        var minZ = Mathf.Min(zs);
        var maxZ = Mathf.Max(zs);

        nearCorners[0] = new Vector3(minX, minY, minZ);
        nearCorners[1] = new Vector3(maxX, minY, minZ);
        nearCorners[2] = new Vector3(maxX, maxY, minZ);
        nearCorners[3] = new Vector3(minX, maxY, minZ);

        farCorners[0] = new Vector3(minX, minY, maxZ);
        farCorners[1] = new Vector3(maxX, minY, maxZ);
        farCorners[2] = new Vector3(maxX, maxY, maxZ);
        farCorners[3] = new Vector3(minX, maxY, maxZ);
    }
}

public class CSMMain : MonoBehaviour
{
    /// <summary>
    ///     阴影分几个等级
    /// </summary>
    private const int lodLevel = 4;

    /// <summary>
    ///     Unity的QualitySettings里提供了对相机视锥的分割设置
    /// </summary>
    private readonly float[] lodSplits = {0, 0.067f, 0.133f, 0.267f, 0.533f};

    private readonly RenderTexture[] depthTextures = new RenderTexture[4];

    /// <summary>
    ///     灯光
    /// </summary>
    public Light dirLight;

    /// <summary>
    /// 摄像机深度Shader
    /// </summary>
    public Shader shadowCaster;

    private Camera dirLightCamera;

    /// <summary>
    ///     灯光的分割物们,放在近裁面中心点
    /// </summary>
    private readonly Transform[] dirLightSplitTss = new Transform[4];

    /// <summary>
    ///     主摄像机和灯光摄像机 的两个裁面
    /// </summary>
    private FrustumCorners[] mainCamera_fcs, lightCamera_fcs;


    /// <summary>
    ///     计算shadowmap阴影用的矩阵
    /// </summary>
    private Matrix4x4 shadowMapMatrix = Matrix4x4.identity;

    /// <summary>
    ///     Lod存的世界空间到阴影的矩阵
    /// </summary>
    private readonly List<Matrix4x4> world2ShadowMats = new List<Matrix4x4>(4);

    private void Awake()
    {
        shadowMapMatrix.SetRow(0, new Vector4(0.5f, 0, 0, 0.5f));
        shadowMapMatrix.SetRow(1, new Vector4(0, 0.5f, 0, 0.5f));
        shadowMapMatrix.SetRow(2, new Vector4(0, 0, 0.5f, 0.5f));
        shadowMapMatrix.SetRow(3, new Vector4(0, 0, 0, 1f));

        InitFrustumCorners();

        InitMainCameraFCS();


        dirLightCamera = CreateDirLightCamera();

        CreateRenderTexture();
    }

    private void Update()
    {
        UpdateCameraSplitFCS();

        if (dirLight)
        {
            Shader.SetGlobalFloat("_gShadowBias", 0.005f);
            Shader.SetGlobalFloat("_gShadowStrength", 0.5f);

            world2ShadowMats.Clear();
            for (var i = 0; i < 4; i++)
            {
                SetLightCameraLod(i);

                //渲染阴影RT
                dirLightCamera.targetTexture = depthTextures[i];
                dirLightCamera.RenderWithShader(shadowCaster, "");

                //投影矩阵转换到相距下面
                var projectionMatrix = GL.GetGPUProjectionMatrix(dirLightCamera.projectionMatrix, false);
                world2ShadowMats.Add(projectionMatrix * dirLightCamera.worldToCameraMatrix);
            }

            Shader.SetGlobalMatrixArray("_gWorld2Shadow", world2ShadowMats);
        }
    }

    private void OnDestroy()
    {
        dirLightCamera = null;

        for (var i = 0; i < 4; i++)
            if (depthTextures[i])
                DestroyImmediate(depthTextures[i]);
    }


    /// <summary>
    ///     创建两个摄像机的裁面顶点数组
    /// </summary>
    private void InitFrustumCorners()
    {
        mainCamera_fcs = new FrustumCorners[lodLevel];
        lightCamera_fcs = new FrustumCorners[lodLevel];
        for (var i = 0; i < lodLevel; i++)
        {
            mainCamera_fcs[i] = FrustumCorners.New();
            lightCamera_fcs[i] = FrustumCorners.New();
        }
    }

    /// <summary>
    ///     创建RT
    /// </summary>
    private void CreateRenderTexture()
    {
        var rtFormat = RenderTextureFormat.Default;

        for (var i = 0; i < lodLevel; i++)
        {
            depthTextures[i] = new RenderTexture(1024, 1024, 24, rtFormat);
            Shader.SetGlobalTexture("_gShadowMapTexture" + i, depthTextures[i]);
        }
    }

    /// <summary>
    ///     创建灯光Camera
    /// </summary>
    public Camera CreateDirLightCamera()
    {
        var goLightCamera = new GameObject("Directional Light Camera");
        var LightCamera = goLightCamera.AddComponent<Camera>();

        LightCamera.cullingMask = 1 << LayerMask.NameToLayer("Caster");
        LightCamera.backgroundColor = Color.white;
        LightCamera.clearFlags = CameraClearFlags.SolidColor;
        LightCamera.orthographic = true;
        LightCamera.enabled = false;

        for (var i = 0; i < lodLevel; i++) dirLightSplitTss[i] = new GameObject("SplitGo_" + i).transform;

        return LightCamera;
    }

    /// <summary>
    ///     初始化主摄像机的裁面顶点
    /// </summary>
    private void InitMainCameraFCS()
    {
        var mainCamera = Camera.main;

        var splitNears = new float[lodLevel];
        var splitsFars = new float[lodLevel];

        var near = mainCamera.nearClipPlane;
        var far = mainCamera.farClipPlane;

        float amount = 0;
        for (var i = 0; i < lodLevel; i++)
        {
            amount += lodSplits[i];
            splitNears[i] = Mathf.Lerp(near, far, amount);
            splitsFars[i] = Mathf.Lerp(near, far, amount + lodSplits[i + 1]);
        }

        Shader.SetGlobalVector("_gLightSplitsNear",
            new Vector4(splitNears[0], splitNears[1], splitNears[2], splitNears[3]));
        Shader.SetGlobalVector("_gLightSplitsFar",
            new Vector4(splitsFars[0], splitsFars[1], splitsFars[2], splitsFars[3]));

        for (var i = 0; i < lodLevel; i++)
        {
            mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), splitNears[i],
                Camera.MonoOrStereoscopicEye.Mono, mainCamera_fcs[i].nearCorners);
            mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), splitsFars[i],
                Camera.MonoOrStereoscopicEye.Mono, mainCamera_fcs[i].farCorners);
            //CalculateFrustumCorners 计算的是局部坐标需要转到世界坐标
            mainCamera_fcs[i].LocalToWorld(mainCamera.transform);
        }
    }

    /// <summary>
    ///     初始化阴影包围盒
    /// </summary>
    private void UpdateCameraSplitFCS()
    {
        if (dirLightCamera == null)
            return;

        for (var i = 0; i < 4; i++)
        {
            var splitTs = dirLightSplitTss[i];

            lightCamera_fcs[i] = mainCamera_fcs[i].WorldToLocal_New(splitTs);
            lightCamera_fcs[i].RecalcBox();

            //设置分割体的位置和旋转 lightCamera_fcs 这时候是局部坐标  要转换到世界坐标
            splitTs.transform.position = splitTs.transform.TransformPoint(lightCamera_fcs[i].nearCenter);
            splitTs.transform.rotation = dirLight.transform.rotation;
        }
    }

    /// <summary>
    ///     设置灯光阴影相机用哪个Lod
    /// </summary>
    /// <param name="index"></param>
    private void SetLightCameraLod(int index)
    {
        dirLightCamera.transform.position = dirLightSplitTss[index].position;
        dirLightCamera.transform.rotation = dirLightSplitTss[index].rotation;

        dirLightCamera.nearClipPlane = lightCamera_fcs[index].nearCorners[0].z;
        dirLightCamera.farClipPlane = lightCamera_fcs[index].farCorners[0].z;

        dirLightCamera.aspect = lightCamera_fcs[index].aspect;

        dirLightCamera.orthographicSize = lightCamera_fcs[index].orthographicSize;
    }


    private void OnDrawGizmos()
    {
        if (dirLightCamera == null)
            return;

        for (var i = 0; i < lodLevel; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(mainCamera_fcs[i].nearCorners[1], mainCamera_fcs[i].nearCorners[2]);

            var fcs = lightCamera_fcs[i].LocalToWorld_New(dirLightSplitTss[i]);


            Gizmos.color = Color.red;
            Gizmos.DrawLine(fcs.nearCorners[0], fcs.nearCorners[1]);
            Gizmos.DrawLine(fcs.nearCorners[1], fcs.nearCorners[2]);
            Gizmos.DrawLine(fcs.nearCorners[2], fcs.nearCorners[3]);
            Gizmos.DrawLine(fcs.nearCorners[3], fcs.nearCorners[0]);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(fcs.farCorners[0], fcs.farCorners[1]);
            Gizmos.DrawLine(fcs.farCorners[1], fcs.farCorners[2]);
            Gizmos.DrawLine(fcs.farCorners[2], fcs.farCorners[3]);
            Gizmos.DrawLine(fcs.farCorners[3], fcs.farCorners[0]);

            Gizmos.DrawLine(fcs.nearCorners[0], fcs.farCorners[0]);
            Gizmos.DrawLine(fcs.nearCorners[1], fcs.farCorners[1]);
            Gizmos.DrawLine(fcs.nearCorners[2], fcs.farCorners[2]);
            Gizmos.DrawLine(fcs.nearCorners[3], fcs.farCorners[3]);
        }
    }
}