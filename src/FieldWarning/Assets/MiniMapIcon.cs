using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    private Quaternion _rotation;

    public void Start()
    {
        _rotation = Quaternion.AngleAxis(90, Vector3.right);
    }

    public void LateUpdate()
    {
        transform.rotation = _rotation;
    }
}
