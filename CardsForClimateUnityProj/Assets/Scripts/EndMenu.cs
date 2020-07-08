using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    public void NewGame()
    {
        Debug.Log("new game");
        Destroy(((EndGameText)GameObject.FindObjectOfType(typeof(EndGameText))).gameObject);
        SceneManager.LoadScene("MainGame");
    }

    public void GetInvolved()
    {
        Application.OpenURL("http://greenhousegame.com/");
    }
}
