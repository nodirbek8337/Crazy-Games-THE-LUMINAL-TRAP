using UnityEngine;

public class EmissionFlicker : MonoBehaviour
{
    [Header("Targets")]
    public Renderer targetRenderer;
    public Material[] targetMaterials;

    [Header("Flicker")]
    public Color emissionColor = Color.white;
    public float flickerSpeed = 2f;
    public float minEmission = 0.1f;
    public float maxEmission = 2f; 

    private float emissionIntensity;
    private Material[] runtimeMaterials;

    void Awake()
    {
        CacheTargets();
        PrepareEmissionMaterials();
    }

    void OnEnable()
    {
        CacheTargets();
        PrepareEmissionMaterials();
    }

    private void CacheTargets()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if ((targetMaterials == null || targetMaterials.Length == 0) && targetRenderer != null)
        {
            runtimeMaterials = targetRenderer.materials;
        }
        else if (targetMaterials != null && targetMaterials.Length > 0)
        {
            runtimeMaterials = targetMaterials;
        }
    }

    private void PrepareEmissionMaterials()
    {
        if (runtimeMaterials == null || runtimeMaterials.Length == 0)
        {
            return;
        }

        for (int i = 0; i < runtimeMaterials.Length; i++)
        {
            Material material = runtimeMaterials[i];
            if (material == null)
            {
                continue;
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                Debug.LogWarning($"EmissionFlicker on '{name}' found a material without _EmissionColor: {material.name}", this);
            }
        }
    }

    void Update()
    {
        if (runtimeMaterials == null || runtimeMaterials.Length == 0)
        {
            return;
        }

        emissionIntensity = Mathf.Lerp(minEmission, maxEmission, (Mathf.Sin(Time.time * flickerSpeed) + 1f) / 2f);

        for (int i = 0; i < runtimeMaterials.Length; i++)
        {
            Material material = runtimeMaterials[i];
            if (material == null || !material.HasProperty("_EmissionColor"))
            {
                continue;
            }

            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }

        if (targetRenderer != null)
        {
            DynamicGI.SetEmissive(targetRenderer, emissionColor * emissionIntensity);
        }
    }
}
