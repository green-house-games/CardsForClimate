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
    /// Whether or not this card is currently set to be the center card
    /// </summary>
    public bool Centered { get; set; }

    public bool ActiveCard { get; set; } = false;

    /// <summary>
    /// Whether or not this card is currently being shifted within the hand
    /// </summary>
    public bool Shifting { get; private set; }

    /// <summary>
    /// Used to calculate where the card should be based on where the mouse is dragging it from
    /// </summary>
    private Vector3 mouseDiff;

    /// <summary>
    /// This card displayer's default screen position at the time an interaction begins
    /// </summary>
    public Vector3 restingPos { get; private set; }

    /// <summary>
    /// This card displayer's default screen rotation in the player's hand
    /// </summary>
    private Vector3 restingAngle;

    private Image myImage;

    private void Start()
    {
        restingPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        restingAngle = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
        myImage = GetComponent<Image>();
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
        myImage.color = new Color(1, 1, 1, 1);
        ActiveCard = true;
    }

    public void Deactivate()
    {
        ActiveCard = false;
        Title.GetComponent<TextMeshProUGUI>().text = "";
        Description.GetComponent<TextMeshProUGUI>().text = "";
        Money.GetComponent<TextMeshProUGUI>().text = "";
        Carbon.GetComponent<TextMeshProUGUI>().text = "";
        CardArt.GetComponent<Image>().sprite = null;
        MomentumIcon.SetActive(false);
        HopeIcon.SetActive(false);
        myImage.color = new Color(1, 1, 1, .4f);
    }

    /// <summary>
    /// Turns on/off the money and carbon icons when the card focus is switched.
    /// </summary>
    public void ToggleMoneyAndCarbonDisplays(bool on)
    {
        MoneyIcon.SetActive(on);
        CarbonIcon.SetActive(on);
    }

    public IEnumerator ShiftCard(Vector3 position, Quaternion rotation, Vector3 scale, float shiftTime = -1)
    {
        Shifting = true;
        float elapsedTime = 0;
        if (shiftTime < 0)
        {
            shiftTime = HandManager.Instance.CardShiftTime;
        }

        Vector3 startPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        Quaternion startRot = transform.localRotation;
        Vector3 startScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        while (elapsedTime < shiftTime)
        {
            float scaledTime = HandManager.Instance.CardShiftCurve.Evaluate(elapsedTime / shiftTime);
            transform.localPosition = Vector3.Lerp(startPos, position, scaledTime);
            transform.localRotation = Quaternion.Slerp(startRot, rotation, scaledTime);
            transform.localScale = Vector3.Lerp(startScale, scale, scaledTime);
            

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        transform.localPosition = position;
        transform.localRotation = rotation;
        transform.localScale = scale;
        restingPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        Shifting = false;
    }

    /// <summary>
    /// Used to tell when user is grabbing card
    /// </summary>
    public void OnClick()
    {
        Centered = true;
        ToggleMoneyAndCarbonDisplays(true);
        HandManager.Instance.ShiftCenter(this);
    }

    public void OnEndTouch()
    {
        if (Input.touchSupported)
        {
            Centered = false;
        }
    }

    /// <summary>
    /// Implemented by the IBeginDragHandler interface. Called when the user begins dragging the card with the mouse.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ActiveCard && Centered)
        {
            mouseDiff =
            new Vector3(Input.mousePosition.x - transform.position.x, Input.mousePosition.y - transform.position.y, 0);
            restingPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
    }

    /// <summary>
    /// Implemented by the IDragHandler interface. Called each frame the user is dragging the card with the mouse.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (ActiveCard && Centered && (Input.mousePosition.y - mouseDiff.y) > restingPos.y)
        {
            transform.position = new Vector3(transform.position.x, Input.mousePosition.y - mouseDiff.y, transform.position.z);
        } else
        {
            transform.position = new Vector3(transform.position.x, restingPos.y, transform.position.z);
        }
    }

    /// <summary>
    /// Implemented by the IEndDragHandler interface. Called when the user stops dragging the card with the mouse.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
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
                if (GameManager.Instance.ValidCard(MyCard))
                {
                    // If valid, we add the card to the card catcher square's collection
                    // and tell the GameManager to play this card.
                    CardCatcher.Instance.CatchCard(this);
                    Deactivate();
                    GameManager.Instance.UseCardByUI(MyCard);
                }
                break;
            }
        }
    }
}
