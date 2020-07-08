using System.Collections;
using System.Collections.Generic;
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
    public int ActiveCardIndex = 2;

    /// <summary>
    /// Holds the UI objects that display action card data.
    /// </summary>
    private List<ActionCardDisplay> cardDisplayers = new List<ActionCardDisplay>();

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of HandManager present");
        Instance = this;
    }

    private void Start()
    {
        ActiveCardIndex = transform.childCount / 2;
        for (int i = 0; i < transform.childCount; i++)
        {
            ActionCardDisplay thisDisplay = transform.GetChild(i).GetComponent<ActionCardDisplay>();
            cardDisplayers.Add(thisDisplay);
            thisDisplay.ToggleMoneyAndCarbonDisplays(thisDisplay.DisplayCardIndex == ActiveCardIndex);
        }
        cardDisplayers.Sort((x, y) => x.DisplayCardIndex.CompareTo(y.DisplayCardIndex));
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

    /// <summary>
    /// Called by the card-scroll buttons on either side of the player's hand. Changes the order of the cards so that
    /// the desired card is on top and fully visible, and all other cards are below it in a logical manner.
    /// </summary>
    /// <param name="left">True if we're scrolling left, false if we're scrolling right</param>
    public void ScrollCards(bool left)
    {
        int originalIndex = ActiveCardIndex;
        do
        {
            ActiveCardIndex = left ? ActiveCardIndex-1 : ActiveCardIndex+1;
            if (ActiveCardIndex < 0 || ActiveCardIndex >= transform.childCount)
            {
                ActiveCardIndex = originalIndex;
                return;
            }
        } while (!cardDisplayers[ActiveCardIndex].gameObject.activeSelf);

        // Find the difference between the card index and the index of the active card for each card in the hand
        List<KeyValuePair<ActionCardDisplay, int>> cardDiffs = new List<KeyValuePair<ActionCardDisplay, int>>();
        for (int i = 0; i < cardDisplayers.Count; i++)
        {
            int difference = 
                Mathf.Abs(ActiveCardIndex - cardDisplayers[i].DisplayCardIndex);
            cardDiffs.Add(new KeyValuePair<ActionCardDisplay, int>(cardDisplayers[i], difference));
        }
        // Sort cards based on their differences from the active card, then set their display order appropriately
        cardDiffs.Sort((x, y) => y.Value.CompareTo(x.Value));
        for (int i = 0; i < cardDiffs.Count; i++)
        {
            cardDiffs[i].Key.transform.SetSiblingIndex(i);
            cardDiffs[i].Key.ToggleMoneyAndCarbonDisplays(cardDiffs[i].Key.DisplayCardIndex == ActiveCardIndex);
        }
    }

    /// <summary>
    /// Scrolls the active card in a random direction that is possible to move into
    /// </summary>
    public void ScrollRandom()
    {
        if (ActiveCardIndex <= 0) ScrollCards(false);
        else if (ActiveCardIndex >= transform.childCount - 1) ScrollCards(true);
        else ScrollCards(Random.value > 0.5 ? true : false);
    }
}
