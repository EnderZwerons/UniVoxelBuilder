using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//this'll probably get removed at some point because there will be a new ui system
public class CommonSlider : MonoBehaviour
{
    public bool prefs;

    public float maxValue;

    public string prefsValue;

    public TextMeshProUGUI labelToUpdate;

    private Slider thisSlider;

    void Start()
    {
        thisSlider = GetComponent<Slider>();
        thisSlider.maxValue = maxValue;
        
        if (prefs)
        {
            thisSlider.value = PlayerPrefs.GetFloat(prefsValue);
        }
    }

    void Update()
    {
        if (labelToUpdate != null)
        {
            labelToUpdate.text = "" + Mathf.Round(thisSlider.value);
        }
    }
}
