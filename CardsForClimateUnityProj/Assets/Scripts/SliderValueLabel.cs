using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderValueLabel : MonoBehaviour
{
    public TextMeshProUGUI ValueLabel;

    public void UpdateLabel(float value)
    {
        ValueLabel.text = ((int)value).ToString();
    }
}
