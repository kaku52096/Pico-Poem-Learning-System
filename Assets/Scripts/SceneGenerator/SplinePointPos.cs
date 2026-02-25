using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePointPos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CrestRivers.Instance.UpdateNewSplinePoint(this.transform.position);
    }
}
