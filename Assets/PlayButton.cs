using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public Button playButton; // Reference to the Button component

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("PlayButton script is missing a reference to the Button component!");
        }
    }

    void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("Main");
    }
}
