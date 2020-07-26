using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HopeCount {
  Empty = 0,
  One = 1,
  Two = 2,
  Full = 3
}
public class HopeDisplay : MonoBehaviour
{
    /// <summary>
    /// HopeDisplay singleton instance. Use this to reference HopeDisplay functions.
    /// </summary>
    public static HopeDisplay Instance;

    Image displayedImage;

    [Header("Icon")]
    public Sprite EmptyHopeIcon;
    public Sprite OneHopeIcon;
    public Sprite TwoHopeIcon;
    public Sprite FullHopeIcon;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of HopeDisplay present");
        Instance = this;
        displayedImage = GetComponent<Image>();
    }

    /// <summary>
    /// Sets the hope icon for the given count.
    /// </summary>
    public void UpdateHope(HopeCount count) {
        switch (count)
        {
            case HopeCount.Full:
                displayedImage.sprite = FullHopeIcon;
                break;
            case HopeCount.Two:
                displayedImage.sprite = TwoHopeIcon;
                break;
            case HopeCount.Empty:
                displayedImage.sprite = EmptyHopeIcon;
                break;
            case HopeCount.One:
                displayedImage.sprite = OneHopeIcon;
                break;
        }
    }
}
