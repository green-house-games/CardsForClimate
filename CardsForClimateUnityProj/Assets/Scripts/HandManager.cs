using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

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
    /// the ratio of X-axis movement over which the player will be assumed to be scrolling cards
    /// </summary>
    public float CardScrollRatio = 1.8f;

    /// <summary>
    /// the threshold of X-axis movement over which the player will be assumed to be scrolling cards
    /// </summary>
    public float CardScrollThreshold = 30;

    [Tooltip("The time, in seconds, that a card shifting its hand position will be in motion for.")]
    public float CardShiftTime = 0.75f;

    [Tooltip("The arc of movement that a card follows whil shifting.")]
    public AnimationCurve CardShiftCurve;

    [Tooltip("The x positions that a card can have, based on distance from center.")]
    public AnimationCurve CardXFrames;

    public AnimationCurve InvertedCardXFrames;

    [Tooltip("The y positions that a card can have, based on distance from center.")]
    public AnimationCurve CardYFrames;

    [Tooltip("The Z rotations that a card can have, based on distance from center.")]
    public AnimationCurve CardZRotationFrames;

    [Tooltip("The scale a card might have, based on distance from center.")]
    public AnimationCurve CardScales;

    /// <summary>
    /// Whether or not the user is currently scrolling cards in the hand
    /// </summary>
    public bool Scrolling { get; private set; }

    /// <summary>
    /// Holds the UI objects that display action card data.
    /// </summary>
    private List<ActionCardDisplay> cardDisplayers = new List<ActionCardDisplay>();

    /// <summary>
    /// Used to calculate where the mouse is compared to the Hand when dragging
    /// </summary>
    private Vector3 mouseStart;

    private List<int> displayIndices = new List<int> {-4, -3, -2, -1, 0, 1, 2, 3, 4};

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of HandManager present");
        Instance = this;

        for (int i = 0; i < transform.childCount; i++)
        { // Initialize all card displayer objects for interactions to come
            ActionCardDisplay thisDisplay = transform.GetChild(i).GetComponent<ActionCardDisplay>();
            cardDisplayers.Add(thisDisplay);
            thisDisplay.ToggleMoneyAndCarbonDisplays(thisDisplay.DisplayCardIndex == ActiveCardIndex);
            thisDisplay.Centered = thisDisplay.DisplayCardIndex == ActiveCardIndex;
        }
        cardDisplayers.Sort((x, y) => x.DisplayCardIndex.CompareTo(y.DisplayCardIndex));
    }

    /// <summary>
    /// Fills in display information for empty card slots in the player's hand
    /// </summary>
    /// <param name="newCards">The actual data objects for the new cards to add to the player's hand</param>
    /// <param name="redraw">Optional bool that tells the function if cards are being redrawn</param>
    public void SetCardDisplays(List<ActionCard> newCards, bool redraw = false)
    {
        foreach (ActionCardDisplay card in cardDisplayers)
        {
            if ((!card.ActiveCard || redraw) && newCards.Count > 0)
            {
                card.gameObject.SetActive(true);
                card.SetCardAndDisplay(newCards[0]);
                newCards.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Changes which card within the hand of cards is currently centered onscreen
    /// </summary>
    /// <param name="centeredCard">The new card to become centered</param>
    public void ShiftCenter(ActionCardDisplay centeredCard)
    {
        foreach (ActionCardDisplay card in cardDisplayers)
        {
            if (!card.Equals(centeredCard))
            {
                card.Centered = false;
                card.ToggleMoneyAndCarbonDisplays(false);
            }
            int cardDifference = card.DisplayCardIndex - centeredCard.DisplayCardIndex;
            StartCoroutine(card.ShiftCard(
                new Vector3(CardXFrames.Evaluate(cardDifference), CardYFrames.Evaluate(cardDifference), 0), 
                Quaternion.Euler(0, 0, CardZRotationFrames.Evaluate(cardDifference)), 
                new Vector3(CardScales.Evaluate(cardDifference), CardScales.Evaluate(cardDifference), 1)));
        }
    }

    /// <summary>
    /// Called when the hand feels someone begin to drag it to the side. Initializes values for the cards to drag/scroll
    /// </summary>
    /// <param name="turnOffCarbon">Do we want to turn off money/carbon displays on cards?</param>
    public void StartDraggingCards(bool turnOffCarbon = true)
    {
        mouseStart = new Vector3(Input.mousePosition.x - (Screen.width / 2), Input.mousePosition.y - (Screen.height / 2));
        if (turnOffCarbon)
        {
            foreach (ActionCardDisplay card in cardDisplayers)
            {
                card.ToggleMoneyAndCarbonDisplays(false);
            }
        }
    }

    /// <summary>
    /// Called each frame when the Hand feels the player dragging it one way or another. Used to scroll cards in real-
    /// time alongside the player's mouse/touch movement.
    /// </summary>
    public void DragCards()
    {
        Scrolling = true;
        Vector3 mouseDiff = new Vector3(Input.mousePosition.x - (Screen.width/2), Input.mousePosition.y - (Screen.height/2)) - mouseStart;
        foreach (ActionCardDisplay card in cardDisplayers) {
            float newTime = InvertedCardXFrames.Evaluate(mouseDiff.x + card.RestingPosLocal.x);
            card.transform.localPosition = new Vector3(CardXFrames.Evaluate(newTime), CardYFrames.Evaluate(newTime), 0);
            card.transform.localRotation = Quaternion.Euler(0, 0, CardZRotationFrames.Evaluate(newTime));
            card.transform.localScale = new Vector3(CardScales.Evaluate(newTime), CardScales.Evaluate(newTime), 1);
        }
    }

    /// <summary>
    /// Called on the frame that the Hand feels the player stop dragging it. Finalizes card positions according to
    /// their nearest "set" positions.
    /// </summary>
    public void StopDraggingCards()
    {
        // Used to identify which card slots are still available
        List<int> currentIndices = new List<int>(displayIndices);
        foreach (ActionCardDisplay card in cardDisplayers)
        {
            float unroundedTime = InvertedCardXFrames.Evaluate(card.transform.localPosition.x);
            int newTime = Mathf.Clamp(Mathf.RoundToInt(unroundedTime), currentIndices.Min(), currentIndices.Max());
            if (!currentIndices.Contains(newTime))
            {
                newTime = Mathf.FloorToInt(unroundedTime);
            } else if (newTime > unroundedTime && newTime == currentIndices.Max())
            {
                newTime = Mathf.FloorToInt(unroundedTime);
            }
            currentIndices.Remove(newTime);

            if (newTime == 0)
            {
                card.Centered = true;
                card.ToggleMoneyAndCarbonDisplays(true);
            }

            // Move each card to its nearest "set" position
            StartCoroutine(card.ShiftCard(new Vector3(CardXFrames.Evaluate(newTime), CardYFrames.Evaluate(newTime), 0),
                Quaternion.Euler(0, 0, CardZRotationFrames.Evaluate(newTime)),
                new Vector3(CardScales.Evaluate(newTime), CardScales.Evaluate(newTime), 1),
                CardShiftTime * .5f));
        }
        Scrolling = false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Details how cards should be positioned along the x-axis and y-axis, rotated along the z-axis, and sized in x and y.
    /// Update positions here, then use the editor buttons to clear and generate keyframes, and you should have updated
    /// "set" positions for cards in the hand.
    /// </summary>
    private Dictionary<int, float[]> cardPositionValues = new Dictionary<int, float[]>
    {
        { -4, new float[5] {-860, -450, 400, .3f, .3f } },
        { -3, new float[5] {-660, -300, 390, .5f, .5f} },
        { -2, new float[5] {-460, -150, 380, .7f, .7f} },
        { -1, new float[5] {-260, -46, 370, 1, 1} },
        { 0, new float[5] {0, 0, 360, 1.15f, 1.15f} },
        { 1, new float[5] {260, -46, 350, 1, 1} },
        { 2, new float[5] {460, -150, 340, .7f, .7f} },
        { 3, new float[5] {660, -300, 330, .5f, .5f} },
        { 4, new float[5] {860, -450, 320, .3f, .3f} },
    };

    /// <summary>
    /// Turn all the position/rotation/scale information above into keyframes on an animation curve, which can then be
    /// accessed and evaluated easily during runtime.
    /// </summary>
    public void GenerateKeyframes()
    {
        // Generate X position frames
        CardXFrames.AddKey(new Keyframe(-4, cardPositionValues[-4][0]));
        CardXFrames.AddKey(new Keyframe(-3, cardPositionValues[-3][0]));
        CardXFrames.AddKey(new Keyframe(-2, cardPositionValues[-2][0]));
        CardXFrames.AddKey(new Keyframe(-1, cardPositionValues[-1][0]));
        CardXFrames.AddKey(new Keyframe(0, cardPositionValues[0][0]));
        CardXFrames.AddKey(new Keyframe(1, cardPositionValues[1][0]));
        CardXFrames.AddKey(new Keyframe(2, cardPositionValues[2][0]));
        CardXFrames.AddKey(new Keyframe(3, cardPositionValues[3][0]));
        CardXFrames.AddKey(new Keyframe(4, cardPositionValues[4][0]));

        // Generate inverted X position frames - useful when trying to get an animation time from a position, which can
        // then be plugged into all other position/etc curves
        for (int i = 0; i < CardXFrames.length; i++)
        {
            Keyframe currentFrame = new Keyframe(CardXFrames.keys[i].value, CardXFrames.keys[i].time);
            currentFrame.inWeight = 0;
            currentFrame.outWeight = 0;
            InvertedCardXFrames.AddKey(currentFrame);
        }

        // Generate Y position frames
        CardYFrames.AddKey(new Keyframe(-4, cardPositionValues[-4][1]));
        CardYFrames.AddKey(new Keyframe(-3, cardPositionValues[-3][1]));
        CardYFrames.AddKey(new Keyframe(-2, cardPositionValues[-2][1]));
        CardYFrames.AddKey(new Keyframe(-1, cardPositionValues[-1][1]));
        CardYFrames.AddKey(new Keyframe(0, cardPositionValues[0][1]));
        CardYFrames.AddKey(new Keyframe(1, cardPositionValues[1][1]));
        CardYFrames.AddKey(new Keyframe(2, cardPositionValues[2][1]));
        CardYFrames.AddKey(new Keyframe(3, cardPositionValues[3][1]));
        CardYFrames.AddKey(new Keyframe(4, cardPositionValues[4][1]));

        // Generate Z rotation frames
        CardZRotationFrames.AddKey(new Keyframe(-4, cardPositionValues[-4][2]));
        CardZRotationFrames.AddKey(new Keyframe(-3, cardPositionValues[-3][2]));
        CardZRotationFrames.AddKey(new Keyframe(-2, cardPositionValues[-2][2]));
        CardZRotationFrames.AddKey(new Keyframe(-1, cardPositionValues[-1][2]));
        CardZRotationFrames.AddKey(new Keyframe(0, cardPositionValues[0][2]));
        CardZRotationFrames.AddKey(new Keyframe(1, cardPositionValues[1][2]));
        CardZRotationFrames.AddKey(new Keyframe(2, cardPositionValues[2][2]));
        CardZRotationFrames.AddKey(new Keyframe(3, cardPositionValues[3][2]));
        CardZRotationFrames.AddKey(new Keyframe(4, cardPositionValues[4][2]));

        // Generate scale frames
        CardScales.AddKey(new Keyframe(-4, cardPositionValues[-4][3]));
        CardScales.AddKey(new Keyframe(-3, cardPositionValues[-3][3]));
        CardScales.AddKey(new Keyframe(-2, cardPositionValues[-2][3]));
        CardScales.AddKey(new Keyframe(-1, cardPositionValues[-1][3]));
        CardScales.AddKey(new Keyframe(0, cardPositionValues[0][3]));
        CardScales.AddKey(new Keyframe(1, cardPositionValues[1][3]));
        CardScales.AddKey(new Keyframe(2, cardPositionValues[2][3]));
        CardScales.AddKey(new Keyframe(3, cardPositionValues[3][3]));
        CardScales.AddKey(new Keyframe(4, cardPositionValues[4][3]));
    }

    /// <summary>
    /// Deletes all the keyframes that were previously generated. Click this before generating new keyframes.
    /// </summary>
    public void ClearKeyframes()
    {
        CardXFrames = new AnimationCurve();
        CardYFrames = new AnimationCurve();
        CardZRotationFrames = new AnimationCurve();
        CardScales = new AnimationCurve();
        InvertedCardXFrames = new AnimationCurve();
    }
#endif
}

#if UNITY_EDITOR 
[CustomEditor(typeof(HandManager))]
public class HandManagerGUI : Editor
{
    /// <summary>
    /// Creates a custom GUI for the HandManager in the Unity editor. Allows us to insert buttons to generate and
    /// clear animation keyframes with.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        HandManager manager = (HandManager)target;
        if (GUILayout.Button("Generate keyframes"))
        {
            manager.GenerateKeyframes();
        }
        if (GUILayout.Button("Clear keyframes"))
        {
            manager.ClearKeyframes();
        }
    }
}
#endif