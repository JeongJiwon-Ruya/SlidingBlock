using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static Vector3 GetTransformByFreezeYAndZeroZ(this Vector3 _vector3, float x, float y) {
        return new Vector3(_vector3.x + x, y, -9);
    }

    public static Vector3 ChangeOnlyX(this Vector3 _vector3, float x) {
        return new Vector3(x, _vector3.y, _vector3.z);
    }
}
