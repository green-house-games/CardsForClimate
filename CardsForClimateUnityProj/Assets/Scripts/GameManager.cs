﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int money;
    /// <summary>
    /// Capital-M Money is a property that controls access to and updating of lowercase-m money, the backing value
    /// </summary>
    public int Money {
        get { return money; }
        set { // Makes sure we set the money UI value whenever Money is updated
            money = value;
            MoneyText.text = ((int)value).ToString();
        }
    }

    private int carbon;
    /// <summary>
    /// Capital-C Carbon is a property that controls access to and updating of lowercase-c carbon, the backing value
    /// </summary>
    public int Carbon {
        get { return carbon; }
        set { // Makes sure we set the slider UI value whenever Carbon is updated
            carbon = value;
            CarbonSlider.value = value;
        }
    }

    private int momentum;
    public int Momentum { get {
            return momentum;
        }
        set {
            momentum = value;
            MomentumDisplay.Instance.UpdateMomentum((MomentumCount)value);
        }
    }

    /// <summary>
    /// Constant values to delineate the max/min hope values.
    /// </summary>
    private int MAX_HOPE = (int) HopeCount.Full;
    private int MIN_HOPE = (int) HopeCount.Empty;

    /// <summary>
    /// Capital-H Hope is a property that controls access to and updating of
    /// lowercase-h hope, the backing value. It also ensures that the Hope UI
    /// stays in sync with the hope value.
    /// Starts at Full and can be decremented until it is Empty.
    /// </summary>
    private int hope = (int) HopeCount.Full;
    public int Hope {
        get {
            return hope;
        }
        set {
            if (value <= MIN_HOPE) {
                hope = MIN_HOPE;
                // GameEnd();
            }
            else if (value >= MAX_HOPE) hope = MAX_HOPE;
            else hope = value;
            HopeDisplay.Instance.UpdateHope((HopeCount)hope);
        }
    }

    public int CurrentTurnNumber { get; private set; } = 0;

    //Current cards in player's hand and previously played cards
    public List<ActionCard> PlayerHand { get; private set; }
    private List<ActionCard> newCards;
    public List<Card> PlayedCards { get; private set; }

    //Keeps track of what cards come next from the decks, receives data from master lists
    public List<EventCard> CurrentEventDeck { get; private set; }
    public List<ActionCard> CurrentActionDeck { get; private set; }

    //Active event card
    public EventCard activeEventCard;
    //Active player cards
    public ActionCard[] activePlayerCards = new ActionCard[3];
    public int activePlayerCardCount { get; private set; } = 0;
    //Current player card
    public ActionCard currentCard;
    private int currentCardIndex = 0;

    //Boolean for if turn is finished
    private bool turnActive = false;
    public bool TurnActive {
        get { return turnActive; }
        private set {
            RedrawButton.interactable = value;
            turnActive = value;
        }
    }

    //Boolean for if slot is a valid slot
    bool validPos = true;
    //Boolean for game lost status
    bool gameOver = false;

    public static GameManager Instance;

    GameObject EndTextHolder;

    [Header("Game UI Attributes")]
    public TextMeshProUGUI MoneyText;
    public Slider CarbonSlider;
    public Button RedrawButton;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one GameManager present in the scene");
        Instance = this;
    }

    /// <summary>
    /// Sets up the game by generating all decks and reseting default values
    /// </summary>
    public void SetupGame()
    {
        EndTextHolder = GameObject.Find("EndTextHolder");

        //Current cards in player's hand and previously played cards
        PlayerHand = new List<ActionCard>();
        PlayedCards = new List<Card>();

        //Keeps track of what cards come next from the decks, receives data from master lists
        CurrentEventDeck = new List<EventCard>();
        CurrentActionDeck = new List<ActionCard>();

        //Active event card
        activeEventCard = null;
        //Active player cards
        activePlayerCards = new ActionCard[3];
        activePlayerCardCount = 0;
        //Current player card

        currentCardIndex = 0;

        //Boolean for if turn is finished
        TurnActive = false;
        Momentum = 0;
        //Boolean for if slot is a valid slot
        validPos = true;
        //Boolean for game lost status
        gameOver = false;

        //Money and Carbon start at 20 every game
        Money = 20;
        Carbon = 20;

        //Start the game by generating the decks players will draw from
        GenerateActionDeck();
        GenerateEventDeck();

        //fill the player's hand
        DrawCards();

        Debug.Log("Cards For Climate!");
        Debug.Log("Press P to start your turn and advance further");

        BeginTurn();
    }

    /// <summary>
    /// Turns the entire list of action cards into a shuffled deck
    /// </summary>
    public void GenerateActionDeck()
    {
        do
        {
            //Generate random number to choose a card from master queue
            int ranNum = Random.Range(0, CardDataCompiler.Instance.MasterActionDeck.Count);
            //Add chosen card to queue of action deck that player draws from
            CurrentActionDeck.Add(CardDataCompiler.Instance.MasterActionDeck[ranNum]);
            //Remove added card from master queue to avoid duplicates
            CardDataCompiler.Instance.MasterActionDeck.RemoveAt(ranNum);
        } while (CardDataCompiler.Instance.MasterActionDeck.Count > 0);
    }

    /// <summary>
    /// Turns the entire list of event cards into a shuffled deck
    /// </summary>
    public void GenerateEventDeck()
    {
        do
        {
            //Generate random number to choose a card from master queue
            int ranNum = Random.Range(0, CardDataCompiler.Instance.MasterEventDeck.Count);
            //Add chosen card to queue of action deck that player draws from
            CurrentEventDeck.Add(CardDataCompiler.Instance.MasterEventDeck[ranNum]);
            //Remove added card from master queue to avoid duplicates
            CardDataCompiler.Instance.MasterEventDeck.RemoveAt(ranNum);
        } while (CardDataCompiler.Instance.MasterEventDeck.Count > 0);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        //Testing key for turn beginning
        if (!gameOver && Input.GetKeyDown(KeyCode.P))
        {
            // Call end turn (if we're not starting the game for the first time) 
            // and begin turn to intialize event card, redraw hand, and apply active cards
            if (CurrentTurnNumber > 0) EndTurn();
            else BeginTurn();
        }
        //Check which key is pressed to determine the card number played, if any
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseCardNumber(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) UseCardNumber(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) UseCardNumber(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) UseCardNumber(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) UseCardNumber(5);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) ReDrawHand();
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            //Forfeit the game
            gameOver = true;
            TurnActive = false;
            EndTurn();
            Debug.Log("You forfeit");
        }
    }

    /// <summary>
    /// Called to set up the beginning of a turn and the event card of that turn
    /// </summary>
    public void BeginTurn()
    {
        Debug.Log("Turn has begun");

        //Temp testing info
        Debug.Log("Global Stats: Money: " + Money + " | C02: " + Carbon + " | Hope : " + Hope);

        //Pull an event card and remove it from the deck
        activeEventCard = CurrentEventDeck[0];
        CurrentEventDeck.RemoveAt(0);

        //Display Card info
        Debug.Log("Card Event: " + activeEventCard.cardName);
        Debug.Log("Card Description: " + activeEventCard.cardDesc);
        Debug.Log("Card Stats Money: " + activeEventCard.costMoney +
            " | CO2: " + activeEventCard.costCarbon +
            " | Hope: " + activeEventCard.hope);

        EventCardDisplay.Instance.SetCardAndDisplay(activeEventCard);

        //Update Money, Carbon and Hope
        Money += activeEventCard.costMoney;
        Carbon += activeEventCard.costCarbon;
        Hope += activeEventCard.hope;

        //Display updated stats
        Debug.Log("Global Stats: Money: " + Money + " | C02: " + Carbon + " | Hope : " + Hope);

        //Warn if hope card must be played
        if (PlayerMustPlayHope()) Debug.Log("A positive hope card must be played");

        HandManager.Instance.SetCardDisplays(newCards);
        PrintPlayerHand();

        // The game should not *automatically* end here
        // Left this in in case its used in the future 
        /*
        if (PlayerMustPlayHope() && !PlayerHasHopeCard() && Money < 5)
        {
            
            Debug.Log("There are no valid hope cards in your hand and you " +
                      "do not have enough money to redraw your hand. You lose");
            // gameOver = true;
            // TurnActive = false;
            // EndTurn();
        } else
        {
            TurnActive = true;
        }
        */

        TurnActive = true;
        CurrentTurnNumber += 1;
    }

    /// <summary>
    /// Called to play the card that the player has indicated to play (if valid)
    /// </summary>
    public void PlayCard()
    {
        if (!TurnActive || !validPos) return;

        // Either the card has passed the appropriate hope checks to be played or
        // the player currently has momentum then check what card was played and
        // if the position is a valid one to check

        //If this is a momentum run, make sure the card played has momentum
        if ((Momentum > 0 && currentCard.momentum > 0) || Momentum == 0)
        {
            //Display the name of the card the player has played
            Debug.Log("Card played Name: " + currentCard.cardName +
            " Description: " + currentCard.cardDesc +
            " Money: " + currentCard.costMoney +
            " CO2:" + currentCard.costCarbon +
            " Hope:" + currentCard.hope +
            " Momentum:" + currentCard.momentum);

            //Add that card to the activePlayedCards array for stat checking after turn completion
            activePlayerCards[activePlayerCardCount] = currentCard;
            //Increment the activePlayerCardCount
            activePlayerCardCount++;
            PlayerHand.RemoveAt(currentCardIndex);
            Debug.Log("Playerhand count = " + PlayerHand.Count);


            //Check if end of turn
            if (currentCard.momentum == 0 || activePlayerCardCount == 3)
            {
                // if the card does not have momentum or
                // if the max number of allowed cards to be played is reached then end the turn
                turnActive = false;
                EndTurn();
            }
            //Card played has momentum
            else
            {
                Momentum += 1;    
                
                //If player has no other momentum cards to play, end turn
                if (!PlayerCardsMomentum())
                {
                    // If the player sees they have no more mementum cards to play, they can end the turn themselves
                    Debug.Log("Out of momentum cards to play");
                    turnActive = true;
                    //turnActive = false;
                    //EndTurn();
                }
                else
                {
                    //Display updated hand and message
                    Debug.Log("Card played has momentum, play another card with momentum!");
                    PrintPlayerHand();
                }
            }
        }
        else
        {
            Debug.Log("Card played must have momentum during a momentum run.");
        }
    }

    /// <summary>
    /// Checks if the card number pressed is valid in the player's hand, and if so, assigns that card to be used
    /// </summary>
    public void UseCardNumber(int cardNum)
    {
        Debug.Log("Pressed " + cardNum);
        //If the card number has decreased then check if there are enough cards for this key to be valid
        if (PlayerHand.Count > cardNum - 1)
        {
            currentCard = PlayerHand[cardNum - 1];
            currentCardIndex = cardNum - 1;
            validPos = true;
            PlayCard();
        } else
        {
            //No card is in this position of the hand display a message and redisplay hand
            Debug.Log("No card is this slot, try again");
            PrintPlayerHand();
            //This position is not valid
            validPos = false;
        }
    }

    /// <summary>
    /// Called by a UI card displayer when the player signals they want to use that card.
    /// </summary>
    public void UseCardByUI(ActionCard card)
    {
        if (!TurnActive)
        {
            return;
        } else
        {
            currentCard = card;
            currentCardIndex = PlayerHand.IndexOf(card);
            validPos = true;
            if (ValidCard(currentCard)) {
                PlayCard();
                Debug.Log("Used card " + card.cardName);
            }
        }
    }

    /// <summary>
    /// Called to end the player's current turn and set up for next turn
    /// </summary>
    public void EndTurn()
    {
        //Display that the turn has ended
        Debug.Log("Turn Ended");

        //Check if the card has a super event card
        if (CardDataCompiler.Instance.SuperNegativeEventCards.ContainsKey(activeEventCard.cardName))
        {
            //Increment the cards supercastrophe potential
            CurrentEventDeck.Insert(Random.Range(0, CurrentEventDeck.Count),
                CardDataCompiler.Instance.SuperNegativeEventCards[activeEventCard.cardName]);
        }


        //Update based on player cards
        for (int i = 0; i < activePlayerCardCount; i++)
        {
            //update money
            Money += activePlayerCards[i].costMoney;
            //update carbon cost
            Carbon += activePlayerCards[i].costCarbon;
            //update hope card
            Hope += activePlayerCards[i].hope;
        }

        //check momentum for super positive event and if there are remaining super positive cards to be played
        if(activePlayerCardCount >= 3 && CardDataCompiler.Instance.PositiveEventCards.Count > 0)
        {
            CurrentEventDeck.Insert(Random.Range(0, CurrentEventDeck.Count),
                CardDataCompiler.Instance.PositiveEventCards[
                    Random.Range(0, CardDataCompiler.Instance.PositiveEventCards.Count)]);
        }

        //Clear activeplayercards and reset counter
        for(int j = 0; j < activePlayerCards.Length; j++)
        {
            activePlayerCards[j] = null;
        }
        CardCatcher.Instance.ClearCaughtCards();
        //reset counter
        activePlayerCardCount = 0;
        //reset values
        TurnActive = false;
        Momentum = 0;
        validPos = true;

        //Temporarily display stats
        Debug.Log("Global Stats: Money: " + Money + " | C02: " + Carbon + " | Hope : " + Hope);

        //reset currentCard values
        currentCard = null;
        currentCardIndex = 0;
        //Replenish player hand
        DrawCards();

        // CHECK GAME END CONDITIONS

        // Check if money is less than zero
        if (Money < 0)
        {
            GameEnd();
        }

        // Check if carbon is 30 or greater
        if (Carbon >= 30)
        {
            GameEnd();
        }

        // Check if all hope is gone
        if (Hope <= MIN_HOPE)
        {
            GameEnd();
        }


        //If game is not over, begin next turn
        if (!gameOver)
        {
            BeginTurn();
        }
    }

    /// <summary>
    /// Draw cards to replenish the player's hand until it's full
    /// </summary>
    public void DrawCards()
    {
        newCards = new List<ActionCard>();
        while (PlayerHand.Count < 5)
        {
            //Check if there are no futher cards
            if (CurrentActionDeck.Count == 0)
            {
                Debug.Log("No more action cards to draw!");
                break;
            }
            //Draw a card and add it to the player hand
            PlayerHand.Add(CurrentActionDeck[0]);
            newCards.Add(CurrentActionDeck[0]);
            CurrentActionDeck.RemoveAt(0);
        }
    }

    /// <summary>
    /// Pays 5 money and replaces the player's hand of cards with new cards.
    /// </summary>
    public void ReDrawHand()
    {
        if (TurnActive)
        {
            //Take away 5 money from player
            Money -= 5;

            // If money is less than 0 now, end game
            if (Money < 0)
            {
                GameEnd();
            }

            //Clear the players hand
            do
            {
                PlayerHand.RemoveAt(0);
            } while (PlayerHand.Count > 0);

            //Redraw the player hand
            DrawCards();

            //Display new playerhand
            HandManager.Instance.SetCardDisplays(newCards, true);
            PrintPlayerHand();

            // I don't think there should be any automatic turn ending or evaluation happening here
            // If they *see* they have no hope cards and are going to lose, let them 
            // I kept this code commented here in case we find a use for it later
            /*
            if (PlayerHasHopeCard())
            {
                HandManager.Instance.SetCardDisplays(newCards, true);
                PrintPlayerHand();
            } else
            {
                //Display message based on if the player has enough money for redraw
                if (Money >= 5)
                {
                    Debug.Log("There are no valid hope cards in your new hand. " +
                        "You can either pay 5 money to redraw your hand by pressing 6 or forfeit the game by pressing 0");
                    Debug.Log("Money: " + Money);
                } else
                {
                    gameOver = true;
                    TurnActive = false;
                    EndTurn();
                    Debug.Log("There are no valid hope cards in your hand and " +
                        "you do not have enough money to redraw your hand. You lose");
                }
            }
            */
        }
;    }

    /// <summary>
    /// Outputs information to the log about each card the player is holding.
    /// </summary>
    public void PrintPlayerHand()
    {
        Debug.Log("_________________________________");
        Debug.Log("Player Hand");
        for (int i = 0; i < PlayerHand.Count; i++)
        {
            Debug.Log("Card Number: " + (i + 1) +
                " Name: " + PlayerHand[i].cardName +
                " Description: " + PlayerHand[i].cardDesc +
                " Money: " + PlayerHand[i].costMoney +
                " CO2:" + PlayerHand[i].costCarbon +
                " Hope:" + PlayerHand[i].hope +
                " Momentum:" + PlayerHand[i].momentum);
        }
    }

    /// <summary>
    /// Checks if the player must play a hope card in their next play. This is not currently used.
    /// </summary>
    public bool PlayerMustPlayHope() {
        return (Hope == MIN_HOPE + 1 && activePlayerCardCount == 0);
    }


    /// <summary>
    /// Checks if any card in the player's hand has positive hope.
    /// </summary>
    public bool PlayerHasHopeCard()
    {
        //Loop through player hand
        for(int i = 0; i < PlayerHand.Count; i++)
        {
            //If any card in the player's hand has a hope value then return true
            if(PlayerHand[i].hope != 0) return true;
        }
        //If no card was found then return false
        return false;
    }

    /// <summary>
    /// Checks if any card in the player's hand has momentum
    /// </summary>
    public bool PlayerCardsMomentum()
    {
        //Check if it is not a momentum run
        if (Momentum == 0) return true;

        //Loop through player hand
        for(int i = 0; i < PlayerHand.Count; i++)
        {
            // If any card in the player's hand has momentum, return true
            if (PlayerHand[i].momentum != 0) return true;
        }
        //If no card was found and momentum is true, return false
        return false;
    }

    /// <summary>
    /// Checks if the played card will work given whatever hope and momentum constraints are placed by the event card.
    /// </summary>
    /// <param name="playedCard"></param>
    public bool ValidCard(ActionCard playedCard)
    {
        // Make sure card played is valid given momentum constraints
        if (Momentum > 0 && playedCard.momentum <= 0)
        {
            Debug.Log("Card played must have momentum during a momentum run.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Method for ending the game
    /// </summary>
    public void GameEnd()
    {
        //Check the loss conditions for the game
        //If Carbon reaches 30 then gameover
        //if money reaches 0 then gameover
        //if hope reaches 0 then gameover
        //If eventdeck runs out of cards then gameover
        if(Carbon >= 30 || Money <= 0 || Hope <= MIN_HOPE || CurrentEventDeck.Count == 0)
        {
            //Change status of gameover
            gameOver = true;

            //Display message based on loss condition
            if(Carbon >= 30)
            {
                Debug.Log("Carbon levels are too high now! Game Over");
                EndTextHolder.GetComponent<EndGameText>().SetEnding(0);
                SceneManager.LoadScene("BadEndMenu");
            } else if (Money <= 0)
            {
                Debug.Log("We have run out of money for further action! Game Over");
                EndTextHolder.GetComponent<EndGameText>().SetEnding(1);
                SceneManager.LoadScene("BadEndMenu");
            } else if(Hope <= MIN_HOPE)
            {
                Debug.Log("The planet has run out of time! Game Over");
                EndTextHolder.GetComponent<EndGameText>().SetEnding(2);
                SceneManager.LoadScene("BadEndMenu");
            } else if(CurrentEventDeck.Count == 0)
            {
                Debug.Log("People have lost too much to continue! Game Over");
                EndTextHolder.GetComponent<EndGameText>().SetEnding(3);
                SceneManager.LoadScene("BadEndMenu");
            }
        //Check game win conditions
        } else if(Carbon <= 0)
        {
            Debug.Log("The Planet is Saved! " +
                "We have begun reducing carbon emissions and are on track to help the world! You Win!");
            SceneManager.LoadScene("GoodEndMenu");
        }
    }
}
