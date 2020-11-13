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

    [Header("Hope Icons")]
    public Image HopeIcon1;
    public Image HopeIcon2;
    public Image HopeIcon3;

    [Header("Sprites")]
    public Sprite HopeSprite;
    public Sprite NoHopeSprite;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of HopeDisplay present");
        Instance = this;
    }

    /// <summary>
    /// Sets the hope icon for the given count.
    /// </summary>
    public void UpdateHope(HopeCount count) {
        switch (count)
        {
            case HopeCount.Full:
                HopeIcon1.sprite = HopeSprite;
                HopeIcon2.sprite = HopeSprite;
                HopeIcon3.sprite = HopeSprite;
                break;
            case HopeCount.Two:
                HopeIcon1.sprite = NoHopeSprite;
                HopeIcon2.sprite = HopeSprite;
                HopeIcon3.sprite = HopeSprite;
                break;
            case HopeCount.One:
                HopeIcon1.sprite = NoHopeSprite;
                HopeIcon2.sprite = NoHopeSprite;
                HopeIcon3.sprite = HopeSprite;
                break;
            case HopeCount.Empty:
                HopeIcon1.sprite = NoHopeSprite;
                HopeIcon2.sprite = NoHopeSprite;
                HopeIcon3.sprite = NoHopeSprite;
                break;
        }
    }
}
