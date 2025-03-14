using UnityEngine;
using UnityEngine.UI;

public class OpenSurvey : MonoBehaviour
{
    [SerializeField]
    Button button;
    string surveyBaseURL = "https://docs.google.com/forms/d/e/1FAIpQLSdeo_8RoGg88DAkYu3BLeHb6Itb5sIuTgP9vJ1KpaMSt4_OnQ/viewform?usp=pp_url&entry.1251095461="; // Replace with your actual survey URL
    private void Awake()
    {
        button.onClick.AddListener(() => OpenSurveyWithSessionID(CodeVanguardManager.Instance.UniqueId));
    }
    public void OpenSurveyWithSessionID(string sessionID)
    {
        string fullURL = surveyBaseURL + sessionID;
        Application.OpenURL(fullURL);
    }
}
