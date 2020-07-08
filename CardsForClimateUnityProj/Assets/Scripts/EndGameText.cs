using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameText : MonoBehaviour
{
    public static EndGameText Instance = null;

    public int endGameResult;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one GameManager present in the scene");
        Instance = this;
        DontDestroyOnLoad (gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetEnding(int ending)
    {
        endGameResult = ending;
    }

    public string GetEnding()
    {
        switch (endGameResult)
        {
            case 0:
                return "Carbon levels are too high now! Game Over";
            case 1:
                return "We have run out of money for further action! Game Over";
            case 2:
                return "The planet has run out of time! Game Over";
            case 3:
                return "People have lost too much to continue! Game Over";
            default:
                return "You've Lost! Game Over";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
