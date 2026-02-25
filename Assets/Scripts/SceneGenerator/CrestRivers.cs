using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrestRivers : SingletonMono<CrestRivers>
{
    public Transform crestRivers;
    public GameObject riverPrefab;
    public GameObject riverSplinePointPrefab;

    public class River
    {
        private GameObject river;
        private List<Vector3> splinePoints = new();
        private float splineMinLenth = 50f;
        private float riverHeight = 5f;

        // check if the spline point should be contained
        public bool IsExtend(Vector3 pos)
        {
            float len;
            foreach (var spline in splinePoints)
            {
                len = Vector3.Distance(spline, pos);
                if (len < splineMinLenth)
                    return true;
            }

            return false;
        }

        // add new spline point to list and set spline point prefab
        public void AddNewSplinePoint(Vector3 pos)
        {
            splinePoints.Add(pos);
            Vector3 position = pos;
            position.y += riverHeight;
            _ = Instantiate(CrestRivers.Instance.riverSplinePointPrefab, position, Quaternion.identity, river.transform);
        }

        // init river at point pos
        public River(Vector3 pos)
        {
            Vector3 position = pos;
            position.y += riverHeight;
            river = Instantiate(CrestRivers.Instance.riverPrefab, position, Quaternion.identity, CrestRivers.Instance.crestRivers);
            AddNewSplinePoint(pos);
        }

        // destory spline points
        public void Clear()
        {
            Destroy(river);
        }
    }


    public List<River> rivers = new();


    public void UpdateNewSplinePoint(Vector3 pos)
    {
        // init
        if (rivers.Count == 0)
        {
            rivers.Add(new River(pos));
        }

        bool newRiver = true;
        foreach (var river in rivers)
        {
            if (river.IsExtend(pos))
            {
                newRiver = false;
                river.AddNewSplinePoint(pos);
            }       
        }

        // create new river
        if (newRiver)
        {
            rivers.Add(new River(pos));
        }
    }

    // clear rivers and spline points 
    public void ClearRiver()
    {
        foreach (var river in rivers)
        {
            river.Clear();
        }

        rivers.Clear();
    }
}
