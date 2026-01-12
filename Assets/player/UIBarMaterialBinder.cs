using UnityEngine;
using UnityEngine.UI;

public class UIBarMaterialBinder : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;
    public Material sourceMaterial;

    Material mat;
    int fillID;

    void Awake()
    {
        UnityEngine.Debug.Log("[UIBar] Awake CALLED ");

        if (fillImage == null || sourceMaterial == null)
        {
            Debug.LogError("[UIBar] fillImage or sourceMaterial is null");
            return;
        }

        fillImage.material = Instantiate(sourceMaterial);
        mat = fillImage.material;

        fillID = Shader.PropertyToID("_Fill");

        bool hasFill = mat != null && mat.HasProperty(fillID);

        Debug.Log($"[UIBar] material = {mat}");
        Debug.Log($"[UIBar] shader = {(mat != null ? mat.shader.name : "null")}");
        Debug.Log($"[UIBar] has _Fill = {hasFill}");
    }

    void Update()
    {
        if (slider == null || mat == null) return;

        float t = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        mat.SetFloat(fillID, t);

        if (Time.frameCount % 60 == 0)
            Debug.Log($"[UIBar] slider={slider.value}  min={slider.minValue} max={slider.maxValue}  t={t:F3}  _Fill={mat.GetFloat(fillID):F3}");
    }
}
