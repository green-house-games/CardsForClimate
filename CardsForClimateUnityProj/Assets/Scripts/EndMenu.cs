using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    void Start()
    {
  
    }

    void Update()
    {
        
    }

    public void NewGame()
    {
        Debug.Log("new game");
        SceneManager.LoadScene("MainGame");
    }

    public void GetInvolved()
    {
        Application.OpenURL("http://greenhousegame.com/");
    }
}
