using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class Manual : MonoBehaviour
{

    public static void FlagshipDescription(){
        GameObject.Find("shipDescription").GetComponent<TextMeshProUGUI>().text =
        "Big and heavy, hits hard, costs 50 ore and 25 uranium to produce";
    } 
    public static void MinerDescription(){
        GameObject.Find("shipDescription").GetComponent<TextMeshProUGUI>().text = 
        "Produces extra resources from active mining, not particularly durable or hard hitting, costs 40 ore to produce";
    }  
    public static void ExplorerDescription(){
        GameObject.Find("shipDescription").GetComponent<TextMeshProUGUI>().text = 
        "Small and quick, low movement costs, costs 30 ore to produce";
    }     
    public static void switchToPage2(){
        fadeAway(GameObject.Find("UICanvas").GetComponent<CanvasGroup>());
        fadeIn(GameObject.Find("Page 2").GetComponent<CanvasGroup>());
    }
    public static void switchToPage1(){
        fadeIn(GameObject.Find("UICanvas").GetComponent<CanvasGroup>());
        fadeAway(GameObject.Find("Page 2").GetComponent<CanvasGroup>());
    }
    private static void fadeAway(CanvasGroup g){
        g.alpha = 0;
        g.interactable = false;
        g.blocksRaycasts = false;
    }
    private static void fadeIn(CanvasGroup g){
        g.alpha = 1;
        g.interactable = true;
        g.blocksRaycasts = true;
    }
    public static void returnToGame()
    {
        SceneManager.UnloadSceneAsync(3);
        UIControl.mainGameController.SetActive(true);
        UIControl.mainRoot.visible = true;
        UIControl.mainUI.enabled = true;
    }
}
