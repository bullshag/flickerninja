using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiUpdate : MonoBehaviour
{
    public GameObject playerObject;
    public playerData playerData;

    public GameObject hpBarObject;
    public GameObject stBarObject;
    public Slider hpBarSlider;
    public Slider stBarSlider;
    // Start is called before the first frame update
    void Start()
    {
        playerData = playerObject.GetComponent<playerData>();
        hpBarSlider = hpBarObject.GetComponent<Slider>();
        stBarSlider = stBarObject.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        hpBarSlider.maxValue = playerData.hpMax;
        stBarSlider.maxValue = playerData.stMax;
        hpBarSlider.value = playerData.hpCurrent;
        stBarSlider.value = playerData.stCurrent;
       
    }
}
