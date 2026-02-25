using UnityEngine;

public class LayerDistCulling : MonoBehaviour
{
    public Camera mainCamera;
    public float treeDistance = 512;
    public int treeLayer = 9;
    public float buildingDistance = 512;
    public int buildingLayer = 10;

    float[] distance = new float[32];

    void Start()
    {
        // Layer层显示的距离在Distance内 超过这个距离就不会显示
        distance[treeLayer] = treeDistance;
        distance[buildingLayer] = buildingDistance;

        // 将数组赋给摄像机的LayerCullDistance
        mainCamera.layerCullDistances = distance;
    }
}