using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents the square in the upper-middle of the screen where cards go to be played.
/// </summary>
public class CardCatcher : MonoBehaviour
{
    public static CardCatcher Instance;

    [Tooltip("The scale that 'caught', or 'played' cards will be shrunk down to in order to fit nicely in their positions.")]
    public Vector3 caughtCardScale = new Vector3(.25f, .25f, .25f);

    [Tooltip("How far away from the card play square, and each subsequent card, that a 'caught' (played) card will be.")]
    public float CardDistanceIncrement = 30;

    /// <summary>
    /// The current distance away from origin in the card catcher that caught cards should be.
    /// </summary>
    private float runningCardDistance = 30;
    private float startingCardDistance;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one CardCatcher found in scene");
        Instance = this;
    }

    private void Start()
    {
        startingCardDistance = runningCardDistance;
    }

    /// <summary>
    /// Called when a UI action card is moved over the card catcher square. Visually queues the card up to be used.
    /// </summary>
    public void CatchCard(ActionCardDisplay card)
    {
        GameObject caughtCard = Instantiate(card.gameObject, transform.parent);
        // Do all the visual tricks to position the card nicely where it's meant to go, and also keep 
        // it from interfering with the game.
        caughtCard.transform.SetAsLastSibling();
        caughtCard.transform.localScale = caughtCardScale;
        caughtCard.transform.localEulerAngles = Vector3.zero;
        caughtCard.GetComponent<Image>().raycastTarget = false; // keep players from being able to click the card

        caughtCard.transform.localPosition = new Vector3(0, runningCardDistance, 0);
        runningCardDistance -= CardDistanceIncrement;
    }

    /// <summary>
    /// At the end of the turn, deletes all the card objects queued up in the card catcher square.
    /// </summary>
    public void ClearCaughtCards()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform thisChild = transform.parent.GetChild(i);
            if (!thisChild.CompareTag("Card Play Square")) {
                Destroy(transform.parent.GetChild(i).gameObject);
            }
            runningCardDistance = startingCardDistance; // set distance card will be from card catcher back to default
        }
    }
}
