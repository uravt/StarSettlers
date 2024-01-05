using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    public PlanetGenInfo[] info;
    SettingsManager sm;
    private void Awake()
    {
        sm = FindObjectOfType<SettingsManager>();
    }
    public GameObject GenerateNewPlanet()
    {
        GameObject planet = new GameObject("planet", typeof(Planet));

        System.Random rand = new System.Random();
        int randArrayLoc = rand.Next(1, info.Length);
        Planet planetComponent = planet.GetComponent<Planet>();
        planetComponent.resolution = sm.resolution;
        planetComponent.shapeSettings = info[randArrayLoc].shapeSettings;
        planetComponent.colorSettings = info[randArrayLoc].colorSettings;
        planetComponent.GeneratePlanet();
        return planet;
    }

    public GameObject GenerateNewPlanet(int type, Vector3Int location)
    { 
        Mathf.Clamp(type, 1, 4);
        GameObject planet = new GameObject("planet", typeof(Planet));
        Planet planetComponent = planet.GetComponent<Planet>();
        //opposite of desired behavior
        float scale = 1.0f - Mathf.Lerp(0.5f , 0.0f, Mathf.InverseLerp(0, 8, Vector3.Distance((Vector3)location, new Vector3(6, 6))));

        switch (type){
            case 1:
                planetComponent.resolution = sm.resolution;
                planetComponent.shapeSettings = info[1].shapeSettings;
                planetComponent.colorSettings = info[1].colorSettings;
                planetComponent.GeneratePlanet();
                planetComponent.transform.localScale = new Vector3(scale, scale, scale);
                break;
            case 2:
                planetComponent.resolution = sm.resolution;
                planetComponent.shapeSettings = info[2].shapeSettings;
                planetComponent.colorSettings = info[2].colorSettings;
                planetComponent.GeneratePlanet();
                planetComponent.transform.localScale = new Vector3(scale, scale, scale);
                break;
            case 3:
                planetComponent.resolution = sm.resolution;
                planetComponent.shapeSettings = info[3].shapeSettings;
                planetComponent.colorSettings = info[3].colorSettings;
                planetComponent.GeneratePlanet();
                planetComponent.transform.localScale = new Vector3(scale, scale, scale);
                break;
            case 4:
                planetComponent.resolution = sm.resolution;
                planetComponent.shapeSettings = info[4].shapeSettings;
                planetComponent.colorSettings = info[4].colorSettings;
                planetComponent.GeneratePlanet();
                planetComponent.transform.localScale = new Vector3(scale, scale, scale);
                break;
        }
        return planet;
    }
    public GameObject GenerateSun()
    {
        GameObject sun = new GameObject("Sun", typeof(Planet));
        Planet planetComponent = sun.GetComponent<Planet>();
        planetComponent.resolution = sm.resolution;
        planetComponent.shapeSettings = info[0].shapeSettings;
        planetComponent.colorSettings = info[0].colorSettings;
        planetComponent.GeneratePlanet();
        return sun;
    }
}


