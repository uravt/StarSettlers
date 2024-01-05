using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public int resolution = 30;
    public string loadFile = "";
    string[] paths;

    public Slider slider;
    public TMP_Dropdown dropdown;
    private void Awake()
    {
        if (FindObjectsOfType<SettingsManager>().Length <= 1)
            DontDestroyOnLoad(gameObject);
        else
            Destroy(gameObject);

        paths = SaveManager.getSaveFilePaths();
    }

    private void Start()
    {
        List<string> pathsList = new List<string>(paths);
        for (int i = 0; i < pathsList.Count; i++)
        {
            pathsList[i] = pathsList[i].Split("/")[pathsList[i].Split("/").Length - 1];
        }
        if(dropdown != null) dropdown.AddOptions(pathsList);
        
    }

    void Update()
    {
        slider = FindObjectOfType<Slider>();
        if(slider != null && slider.gameObject.tag.Equals("resolutionSlider"))
            resolution = (int) slider.value;
        if(dropdown != null && dropdown.value > 0)
            loadFile = paths[dropdown.value - 1];
    }

    

  

    
}
