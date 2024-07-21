using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyController : MonoBehaviour
{
    public Volume skyVolume;
    public Light mainLight;
    public Camera mainCamera;

    public float minHeight = 0f;
    public float maxHeight = 1000f;

    [Header("Cloud Settings")]
    public float cloudDisappearStartHeight = 300f;
    public float cloudDisappearEndHeight = 500f;

    private PhysicallyBasedSky sky;
    private VolumetricClouds volumetricClouds;

    void Start()
    {
        if (skyVolume.profile.TryGet(out PhysicallyBasedSky pbSky))
        {
            sky = pbSky;
        }
        
        if (skyVolume.profile.TryGet(out VolumetricClouds clouds))
        {
            volumetricClouds = clouds;
        }
        else
        {
            Debug.LogWarning("Volumetric Clouds component not found in the volume. Adding it.");
            volumetricClouds = skyVolume.profile.Add<VolumetricClouds>(false);
        }
    }

    void Update()
    {
        if (sky == null || volumetricClouds == null) return;

        float currentHeight = mainCamera.transform.position.y;
        float normalizedHeight = Mathf.Clamp01((currentHeight - minHeight) / (maxHeight - minHeight));

        AdjustSky(normalizedHeight);
        AdjustLight(normalizedHeight);
        AdjustClouds(currentHeight);
    }

    void AdjustSky(float normalizedHeight)
    {
        // HDRP 12 에서의 PhysicallyBasedSky 조정
        sky.planetaryRadius.value = Mathf.Lerp(6378100f, 6378100f * 0.1f, normalizedHeight);
        sky.groundTint.value = Color.Lerp(Color.grey, Color.black, normalizedHeight);
        sky.exposure.value = Mathf.Lerp(0f, -2f, normalizedHeight);
        // sky.atmosphereThickness.value = Mathf.Lerp(1f, 0.1f, normalizedHeight);

        // 대기 밀도 조정
        float airDensity = Mathf.Lerp(1f, 0.0001f, normalizedHeight);
        sky.airDensityR.value = airDensity;
        sky.airDensityG.value = airDensity;
        sky.airDensityB.value = airDensity;
    }

    void AdjustLight(float normalizedHeight)
    {
        // 빛 조정
        mainLight.color = Color.Lerp(Color.white, new Color(0.8f, 0.8f, 1f), normalizedHeight);
        mainLight.intensity = Mathf.Lerp(130000f, 65000f, normalizedHeight);
    }

    void AdjustClouds(float currentHeight)
    {
        float cloudFade = Mathf.Clamp01((currentHeight - cloudDisappearStartHeight) / (cloudDisappearEndHeight - cloudDisappearStartHeight));

        // HDRP 12에서의 VolumetricClouds 조정
        volumetricClouds.enable.value = cloudFade < 1f;
        
        if (volumetricClouds.enable.value)
        {
            volumetricClouds.densityMultiplier.value = Mathf.Lerp(1f, 0f, cloudFade);
            
            // 구름 고도 조정 (선택적)
            if (volumetricClouds.bottomAltitude.overrideState)
            {
                volumetricClouds.bottomAltitude.value = Mathf.Lerp(3000f, 5000f, cloudFade);
            }
            if (volumetricClouds.altitudeRange.overrideState)
            {
                volumetricClouds.altitudeRange.value = Mathf.Lerp(3000f, 5000f, cloudFade);
            }
        }
    }
}