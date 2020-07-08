using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    /// <summary>
    /// HandManager singleton instance. Use this to reference HandManager functions.
    /// </summary>
    public static HandManager Instance;

    /// <summary>
    /// The index of the card that is currently fully visible
    /// </summary>
    public int ActiveCardIndex = 0;

    [Tooltip("The arc of movement in the y-axis that a card follows whil sliding.")]
    public AnimationCurve CardSlideCurve;

    [Tooltip("The time, in seconds, that a card sliding up or down will be in motion for.")]
    public float CardSlideTime = 0.5f;

    [Tooltip("How far upwards a sliding card will go to reach its resting hovered position.")]
    public float CardSlideDistance = 30;

    /// <summary>
    /// Holds the UI objects that display action card data.
    /// </summary>
    private List<ActionCardDisplay> cardDisplayers = new List<ActionCardDisplay>();

    /// <summary>
    /// The card currently being hovered over by the player's mouse
    /// </summary>
    private ActionCardDisplay topCard;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of HandManager present");
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            ActionCardDisplay thisDisplay = transform.GetChild(i).GetComponent<ActionCardDisplay>();
            cardDisplayers.Add(thisDisplay);
            thisDisplay.ToggleMoneyAndCarbonDisplays(thisDisplay.DisplayCardIndex == ActiveCardIndex);
        }
        cardDisplayers.Sort((x, y) => x.DisplayCardIndex.CompareTo(y.DisplayCardIndex));
    }

    // Here we calculate which card is currently being hovered over, and thus should be on top and slid upward.
    private void LateUpdate()
    {
        if (topCard != null && !topCard.gameObject.activeSelf)
        {
            topCard = null;
        }

        int topCardValue; // the display priority (lower is better) of the card currently being shown on top
        if (topCard != null && topCard.Hovered)
        {
            topCardValue = topCard.DisplayCardIndex;
        } else
        {
            topCardValue = int.MaxValue; // we should definitely have fewer cards in hand than this :)
        }

        bool topCardChanged = false;
        // loop over every card in the hand, and see if they qualify to be the new top card
        foreach (ActionCardDisplay card in cardDisplayers)
        {
            if (card.gameObject.activeSelf && card.Hovered && Mathf.Abs(card.DisplayCardIndex) < topCardValue)
            {
                if (topCard != null && !topCardChanged)
                {
                    StartCoroutine(topCard.Slide(false));
                    topCard.ToggleMoneyAndCarbonDisplays(false);
                }

                topCardChanged = true;
                topCard = card;
                topCardValue = card.DisplayCardIndex;
            }
        }

        if (topCardChanged) // execute all necessary animation changes for the new top card
        {
            StartCoroutine(topCard.Slide(true));
            topCard.transform.SetAsLastSibling();
            topCard.ToggleMoneyAndCarbonDisplays(true);
        } else if (topCard != null && !topCard.Hovered) // if player's not hovering over anything, slide top card down
        {
            StartCoroutine(topCard.Slide(false));
            topCard = null;
        }
    }

    /// <summary>
    /// Fills in display information for cards in the player's hand
    /// </summary>
    /// <param name="handCards">The actual data objects for the cards in the player's hand</param>
    public void SetCardDisplays(List<ActionCard> handCards)
    {
        for (int i = 0; i < cardDisplayers.Count; i++)
        {
            if (handCards.Count <= i)
            { // if we run out of card data to put on the card displayers, we start turning off any extra card displayers
                cardDisplayers[i].gameObject.SetActive(false);
            } else
            {
                cardDisplayers[i].gameObject.SetActive(true);
                cardDisplayers[i].SetCardAndDisplay(handCards[i]);
            }
        }
    }
}
