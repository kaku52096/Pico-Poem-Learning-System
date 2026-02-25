using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaterSurfaceObject : MonoBehaviour
{
    public float relative;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = transform.position;
        float posY = SceneGenerator.Instance.GetCurrentWaterLevel() + relative;
        this.transform.position = new Vector3(pos.x, posY, pos.z);
    }
}
