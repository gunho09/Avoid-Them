using UnityEngine;
using System.Collections;

public class HitFlashController : MonoBehaviour
{
    public Color flashColor = Color.gray;
    public float flashDuration = 0.1f;
    public SpriteRenderer[] targets;

    private Color[] originColors;
    private Coroutine co;

    void Awake()
    {
        CacheRenderers();
        CacheOriginColors();
        Debug.Log($"[HitFlash] Awake on {name}, targets={targets?.Length ?? 0}", this);
    }

    void CacheRenderers()
    {
        if (targets == null || targets.Length == 0)
            targets = GetComponentsInChildren<SpriteRenderer>(true);
    }

    void CacheOriginColors()
    {
        originColors = new Color[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            originColors[i] = targets[i] != null ? targets[i].color : Color.white;
    }

    public void Flash()
    {
        CacheRenderers();
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"[HitFlash] No SpriteRenderer found on {name}", this);
            return;
        }

        if (originColors == null || originColors.Length != targets.Length)
            CacheOriginColors();

        Debug.Log($"[HitFlash] Flash called on {name}", this);

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < targets.Length; i++)
            if (targets[i] != null) targets[i].color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        Restore();
        co = null;
    }

    public void Restore()
    {
        if (targets == null || originColors == null) return;

        for (int i = 0; i < targets.Length; i++)
            if (targets[i] != null) targets[i].color = originColors[i];
    }
}
