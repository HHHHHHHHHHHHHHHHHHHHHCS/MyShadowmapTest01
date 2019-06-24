using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FrustumCorners
{
    public Vector3[] nearCorners;
    public Vector3[] farCorners;

    public static FrustumCorners New()
    {
        FrustumCorners fc = new FrustumCorners()
        {
            nearCorners = new Vector3[4],
            farCorners = new Vector3[4],
        };
        return fc;
    }
}