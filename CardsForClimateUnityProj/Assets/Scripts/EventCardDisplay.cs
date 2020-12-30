using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays information about event cards in UI form.
/// </summary>
public class EventCardDisplay : MonoBehaviour
{
    /// <summary>
    /// EventCardDisplay singleton instance. Use this to reference EventCardDisplay functions.
    /// </summary>
    public static EventCardDisplay Instance;

    /// <summary>
    /// The EventCard data object that underpins all this card displayer's information.
    /// </summary>
    public EventCard MyCard { get; private set; }

    [Header("Information Slots and Icons")]
    public GameObject Title;
    public GameObject Description;
    public GameObject Money;
    public GameObject Carbon;
    public GameObject HopeIcon;
    // Commenting CardArt out because it isn't used in the current design, but may be used in future designs
    //public GameObject CardArt;

    public Vector3 startPos;
    public Vector3 endPos;

    private Animator myAnimator;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of EventCardDisplay present");
        Instance = this;

        myAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        startPos = transform.localPosition;
        endPos = new Vector3(-55.9f, -672, 0);
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(endPos, startPos, GameManager.Instance.TimeRemaining / GameManager.Instance.TimeBetweenTurns);
    }

    /// <summary>
    /// Fills in the card information and turns on the proper icons on the card displayer.
    /// </summary>
    /// <param name="thisCard"></param>
    public void SetCardAndDisplay(EventCard thisCard)
    {
        myAnimator.SetTrigger("New Card");

        MyCard = thisCard;
        Title.GetComponent<TextMeshProUGUI>().text = thisCard.cardName;
        Description.GetComponent<TextMeshProUGUI>().text = thisCard.cardDesc;
        Money.GetComponent<TextMeshProUGUI>().text =
                                                (thisCard.costMoney > 0 ? "+" : "") + thisCard.costMoney.ToString();
        Carbon.GetComponent<TextMeshProUGUI>().text =
                                                (thisCard.costCarbon > 0 ? "+" : "") + thisCard.costCarbon.ToString();
        //CardArt.GetComponent<Image>().sprite = thisCard.cardImage;
        HopeIcon.SetActive(thisCard.hope < 0);
    }
}
