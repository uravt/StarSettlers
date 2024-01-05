using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class switchscenes : MonoBehaviour
{
    public float progress = 0;
    public Canvas canvas;
    public Slider ProgressBar;

    public void StartGame(){
        canvas.gameObject.SetActive(true);
        gameObject.SetActive(false);
        StartCoroutine(LoadYourSceneAsync());
    }


    IEnumerator LoadYourSceneAsync(){
        ProgressBar = FindObjectOfType<Slider>();
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        while(!operation.isDone){
            float progress = Mathf.Clamp01(operation.progress / .9f);
            Debug.Log(progress);
            ProgressBar.value = progress;
        yield return null;
        }
    }

}
