using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class RoundedImage : Image
{
    [SerializeField] public float radius = 0.05f;
    private float ratio;
    private Material _customMaterialInstance;
    private RectTransform _rectTransform;


    // 重写 GetModifiedMaterial 方法
    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        // 调用基类方法，获取经过 Mask 处理后的材质
        Material modifiedBaseMat = base.GetModifiedMaterial(baseMaterial);

        if (_customMaterialInstance == null || _customMaterialInstance.shader != modifiedBaseMat.shader)
        {
            DestroyImmediate(_customMaterialInstance);
            _customMaterialInstance = new Material(modifiedBaseMat);
        }

        // 在此处应用自定义材质属性（例如颜色、纹理等）
        ratio = GetCurrentRatio();
        _customMaterialInstance.SetFloat("_Radius", radius);
        _customMaterialInstance.SetFloat("_Ratio", ratio);

        return _customMaterialInstance;
    }


    private float GetCurrentRatio()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        Vector2 size = _rectTransform.rect.size;
        return size.y / size.x;
    }


    // 销毁时释放材质实例
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_customMaterialInstance != null)
        {
            DestroyImmediate(_customMaterialInstance);
            _customMaterialInstance = null;
        }
    }
}