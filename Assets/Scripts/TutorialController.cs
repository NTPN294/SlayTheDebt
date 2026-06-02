using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// The on click method for when the player clicks on the tutorial screen
    /// </summary>
    public void OnTutorialClick()
        => SceneManager.LoadScene("Phase1Scene");
}
