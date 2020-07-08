using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Displays information about action cards in UI form.
/// </summary>
public class ActionCardDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// The ActionCard data object that underpins all this card displayer's information.
    /// </summary>
    public ActionCard MyCard { get; private set; }

    [Header("Information Slots and Icons")]
    public GameObject Title;
    public GameObject Description;
    public GameObject Money;
    public GameObject MoneyIcon;
    public GameObject CarbonIcon;
    public GameObject Carbon;
    public GameObject HopeIcon;
    public GameObject MomentumIcon;
    public GameObject CardArt;

    [Header("Other")]
    public int DisplayCardIndex;

    /// <summary>
    /// Whether or not the cursor is currently hovering over this card
    /// </summary>
    public bool Hovered { get; set; }

    /// <summary>
    /// Whether or not this card is currently sliding up or down as part of the hover animation
    /// </summary>
    public bool Sliding { get; private set; }

    /// <summary>
    /// Used to calculate where the card should be based on where the mouse is dragging it from
    /// </summary>
    private Vector3 mouseDiff;

    /// <summary>
    /// This card displayer's default screen position in the player's hand
    /// </summary>
    private Vector3 restingPos;

    /// <summary>
    /// This card displayer's default screen rotation in the player's hand
    /// </summary>
    private Vector3 restingAngle;

    private void Start()
    {
        restingPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        restingAngle = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    /// <summary>
    /// Called when the user hovers over the card with their mouse
    /// </summary>
    public void OnHover()
    {
        Hovered = true;
    }

    /// <summary>
    /// Called when the user's mouse leaves the card
    /// </summary>
    public void OnExit()
    {
        Hovered = false;
    }

    /// <summary>
    /// Fills in the card information and turns on the proper icons on the card displayer.
    /// </summary>
    /// <param name="thisCard"></param>
    public void SetCardAndDisplay(ActionCard thisCard)
    {
        MyCard = thisCard;
        Title.GetComponent<TextMeshProUGUI>().text = thisCard.cardName;
        Description.GetComponent<TextMeshProUGUI>().text = thisCard.cardDesc;
        Money.GetComponent<TextMeshProUGUI>().text =
                                                (thisCard.costMoney > 0 ? "+" : "") +  thisCard.costMoney.ToString();
        Carbon.GetComponent<TextMeshProUGUI>().text = 
                                                (thisCard.costCarbon > 0 ? "+" : "") + thisCard.costCarbon.ToString();
        CardArt.GetComponent<Image>().sprite = thisCard.cardImage;
        MomentumIcon.SetActive(thisCard.momentum > 0);
        HopeIcon.SetActive(thisCard.hope > 0);
    }

    /// <summary>
    /// Turns on/off the money and carbon icons when the card focus is switched.
    /// </summary>
    public void ToggleMoneyAndCarbonDisplays(bool on)
    {
        MoneyIcon.SetActive(on);
        CarbonIcon.SetActive(on);
    }

    /// <summary>
    /// Slides the card either up for better visibility or down back into the player's hand.
    /// </summary>
    /// <param name="up">Whether the card is sliding up (true) or down (false).</param>
    /// <returns>Because this function yield returns an IENumerator, it can run in the background on a separate thread,
    ///          then pick up later right where it left off. Perfect for animations!</returns>
    public IEnumerator Slide(bool up)
    {
        // wait until next frame to see if the card is still moving before we can start moving it a different way
        while (Sliding)
        {
            yield return new WaitForFixedUpdate();
        }

        float elapsedTime = 0;
        Sliding = true;
        // Move towards the desired position in the slide animation in increments proportional to the amount of
        // time that has passed since the last frame
        while (elapsedTime < HandManager.Instance.CardSlideTime)
        {
            if (!Sliding)
            { // if at any point in the animation the slide is interrupted by player action, let it happen!
                break;
            }

            float scaledTime = elapsedTime / HandManager.Instance.CardSlideTime;
            if (!up) scaledTime = 1 - scaledTime;
            scaledTime = HandManager.Instance.CardSlideCurve.Evaluate(scaledTime);

            transform.position = Vector3.Lerp(restingPos, 
                restingPos + new Vector3(0, HandManager.Instance.CardSlideDistance, 0), scaledTime);
            transform.localEulerAngles = 
                Vector3.Lerp(restingAngle, new Vector3(0,0, restingAngle.z > 180 ? 360 : 0), scaledTime);

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate(); // wait until next frame to continue movement
        }

        // Once time is up, finalize the positions and rotations of the cards
        transform.position = restingPos;
        if (up)
        {
            transform.position += new Vector3(0, HandManager.Instance.CardSlideDistance, 0);
            transform.localEulerAngles = Vector3.zero;
        } else
        {
            transform.localEulerAngles = restingAngle;
        }
        Sliding = false;
    }

    /// <summary>
    /// Used to tell when user is grabbing card on a mobile device
    /// </summary>
    public void OnTouch()
    {
        if (Input.touchCount > 0)
        {
            Hovered = true;
        }
    }

    public void OnEndTouch()
    {
        if (Input.touchSupported)
        {
            Hovered = false;
        }
    }

    /// <summary>
    /// Implemented by the IBeginDragHandler interface. Called when the user begins dragging the card with the mouse.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseDiff = 
            new Vector3(Input.mousePosition.x - transform.position.x, Input.mousePosition.y - transform.position.y, 0);
        Sliding = false; // cancel the sliding animation; player has control now
    }

    /// <summary>
    /// Implemented by the IDragHandler interface. Called each frame the user is dragging the card with the mouse.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition - mouseDiff;
    }

    /// <summary>
    /// Implemented by the IEndDragHandler interface. Called when the user stops dragging the card with the mouse.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        int currentTurn = GameManager.Instance.CurrentTurnNumber;
        // put card back in its hand position
        transform.position = restingPos;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        // Test to see if the user dropped the card in the card play square - if so, the user wants to play this card.
        // We test by casting a ray on all UI objects under the mouse position, and seeing if the ray hits the square.
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject.CompareTag("Card Play Square"))
            {
                // First, we make sure that this card is valid to be played
                if (GameManager.Instance.PlayerCardsHope() && GameManager.Instance.ValidCard(MyCard))
                {
                    // If valid, we add the card to the card catcher square's collection
                    // and tell the GameManager to play this card.
                    CardCatcher.Instance.CatchCard(this);
                    GameManager.Instance.UseCardByUI(MyCard);
                    // only turn the card off if a new turn hasn't been started, triggered by the last card; 
                    // otherwise this will cause issues with the hand refilling between turns.
                    if (currentTurn == GameManager.Instance.CurrentTurnNumber)
                    {
                        gameObject.SetActive(false);
                    }
                }
                break;
            }
        }
    }
}
