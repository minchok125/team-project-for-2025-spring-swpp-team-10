using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicSettingsController : MonoBehaviour
{
    public Volume globalVolume;
    public Volume[] secureVolume;
    public GameObject[] reflectionProbes;

    public Toggle antiToggle;
    public Toggle postToggle;
    public Toggle reflectionToggle;


    public void CameraAntialiasing()
    {
        if (antiToggle.isOn) CameraAntialiasingOn();
        else CameraAntialiasingOff();
    }
    private void CameraAntialiasingOff()
    {
        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        cameraData.antialiasing = AntialiasingMode.None;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
    }
    private void CameraAntialiasingOn()
    {
        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
    }



    public void PostProcessingOnOff()
    {
        bool enable = postToggle.isOn;
        Debug.Log("크아아아아아앙앙ㅇ" + enable);
        if (globalVolume.profile.TryGet<Tonemapping>(out var tonemapping))
        {
            tonemapping.active = enable;
        }

        foreach (Volume volume in secureVolume)
        {
            if (volume.profile.TryGet<Vignette>(out var vignette))
                vignette.active = enable;
            if (volume.profile.TryGet<ColorAdjustments>(out var color))
                color.active = enable;
            if (volume.profile.TryGet<Tonemapping>(out var localTonemapping))
                localTonemapping.active = enable;  // 수정된 부분
        }
    }


    public void ReflectionProbeOnOff()
    {
        foreach (GameObject obj in reflectionProbes)
            obj.SetActive(reflectionToggle.isOn);
    }
}