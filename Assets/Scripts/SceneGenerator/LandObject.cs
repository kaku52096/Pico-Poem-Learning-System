using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LandObject : MonoBehaviour
{
    public float relativeHeight;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = transform.position;
        this.transform.position = new Vector3(pos.x, pos.y + relativeHeight, pos.z);
    }
}
