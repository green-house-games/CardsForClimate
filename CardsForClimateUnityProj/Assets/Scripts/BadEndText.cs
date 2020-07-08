using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BadEndText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EndGameText endTextObject = (EndGameText)GameObject.FindObjectOfType(typeof(EndGameText));
        string endText = endTextObject.GetEnding();
        Debug.Log(endText);
        GetComponent<TextMeshProUGUI>().SetText(endText);
    }
}
