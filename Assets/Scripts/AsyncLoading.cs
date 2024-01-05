using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AsyncLoading : MonoBehaviour
{

    public Slider yourSlider;
    public switchscenes ss;

    public void Start()
    {
        ss = FindObjectOfType<switchscenes>();
    }

    public void Update (){
        yourSlider.value = ss.progress;

    }
    public void MoveSlider(int num){
        yourSlider.value = num;
    }
}
