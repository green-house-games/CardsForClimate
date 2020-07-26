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

    Image displayedImage;
    
    [Header("Icon")]
    public Sprite EmptyMomentumIcon;
    public Sprite OneMomentumIcon;
    public Sprite TwoMomentumIcon;
    public Sprite FullMomentumIcon;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of MomentumDisplay present");
        Instance = this;
        displayedImage = GetComponent<Image>();
    }

    /// <summary>
    /// Sets the momentum icon for the given count.
    /// </summary>
    public void UpdateMomentum(MomentumCount count) {
        switch (count)
        {
            case MomentumCount.Empty:
                displayedImage.sprite = EmptyMomentumIcon;
                break;
            case MomentumCount.One:
                displayedImage.sprite = OneMomentumIcon;
                break;
            case MomentumCount.Two:
                displayedImage.sprite = TwoMomentumIcon;
                break;
            case MomentumCount.Full:
                displayedImage.sprite = FullMomentumIcon;
                break;
        }
    }
}
