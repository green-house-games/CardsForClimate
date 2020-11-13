using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MomentumCount {
  Empty = 0,
  One = 1,
  Two = 2,
  Full = 3
}

public class MomentumDisplay : MonoBehaviour
{
    /// <summary>
    /// MomentumDisplay singleton instance. Use this to reference MomentumDisplay functions.
    /// </summary>
    public static MomentumDisplay Instance;

    [Header("Momentum Icons")]
    public Image MomentumIcon1;
    public Image MomentumIcon2;
    public Image MomentumIcon3;

    [Header("Sprites")]
    public Sprite MomentumSprite;
    public Sprite NoMomentumSprite;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of MomentumDisplay present");
        Instance = this;
    }

    /// <summary>
    /// Sets the momentum icon for the given count.
    /// </summary>
    public void UpdateMomentum(MomentumCount count) {
        switch (count)
        {
            case MomentumCount.Empty:
                MomentumIcon1.sprite = NoMomentumSprite;
                MomentumIcon2.sprite = NoMomentumSprite;
                MomentumIcon3.sprite = NoMomentumSprite;
                break;
            case MomentumCount.One:
                MomentumIcon1.sprite = NoMomentumSprite;
                MomentumIcon2.sprite = NoMomentumSprite;
                MomentumIcon3.sprite = MomentumSprite;
                break;
            case MomentumCount.Two:
                MomentumIcon1.sprite = NoMomentumSprite;
                MomentumIcon2.sprite = MomentumSprite;
                MomentumIcon3.sprite = MomentumSprite;
                break;
            case MomentumCount.Full:
                MomentumIcon1.sprite = MomentumSprite;
                MomentumIcon2.sprite = MomentumSprite;
                MomentumIcon3.sprite = MomentumSprite;
                break;
        }
    }
}
