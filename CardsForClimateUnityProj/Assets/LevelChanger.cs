using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{

    public Animator animator;
    public float transitionTime = 10f;
    //private int nextSceneIndex;

    // Update is called once per frame
    void Update() {}

    public void LoadLevel(int nextSceneIndex)
    {
        StartCoroutine(FadeToLevel(nextSceneIndex));
    }

    IEnumerator FadeToLevel(int levelIndex)
    {
        // Play animation
        animator.SetTrigger("FadeOut");

        // Wait
        yield return new WaitForSeconds(transitionTime);

        // Load scene
        SceneManager.LoadScene(levelIndex);
    }
}

