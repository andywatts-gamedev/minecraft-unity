using System;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    
    
    [SerializeField] private Light sun;
    [SerializeField, Range(0,24)] private float timeOfDay;
    [SerializeField] private float sunRotationSpeed;
    
    [Header("Lighting Presets")]
    [SerializeField] private Gradient skyColor;
    [SerializeField] private Gradient equatorColor;
    [SerializeField] private Gradient sunColor;


    private void Update()
    {
        timeOfDay += Time.deltaTime * sunRotationSpeed;
        if (timeOfDay > 24)
            timeOfDay = 0;
        UpdateSunRotation();
        UpdateLighting();
    }


    private void OnValidate()
    {
        UpdateSunRotation();
        UpdateLighting();
    }
    
    void UpdateSunRotation()
    {
        var sunRotation = Mathf.Lerp(-90, 270, timeOfDay / 24);
        gameObject.transform.rotation = Quaternion.Euler(sunRotation, transform.rotation.y, gameObject.transform.rotation.z);
    }
    
    void UpdateLighting()
    {
        float timeFaction = timeOfDay / 24f;
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeFaction);
        RenderSettings.ambientSkyColor = skyColor.Evaluate(timeFaction);
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeFaction);
        sun.color = sunColor.Evaluate(timeFaction);
    }

}