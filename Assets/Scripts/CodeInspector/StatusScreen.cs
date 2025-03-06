using UnityEngine;
using UnityEngine.UI;

public class StatusScreen : MonoBehaviour
{
    [SerializeField]
    string _taskId;
    public string TaskId => _taskId;


    [SerializeField]
    Image _imgInvalid;


    [SerializeField]
    Image _imgValid;



    [SerializeField]
    Image _star1;
    [SerializeField]
    Image _star1Inner;
    [SerializeField]
    Image _star2;
    [SerializeField]
    Image _star2Inner;

    [SerializeField]
    Image _star3;
    [SerializeField]
    Image _star3Inner;

    [SerializeField]
    Image _star4;
    [SerializeField]
    Image _star4Inner;

    [SerializeField]
    Image _star5;
    [SerializeField]
    Image _star5Inner;

    [SerializeField]
    Image bg;
    private void Awake()
    {
        HideAll();
        SetInvalid();

    }

    private void Start()
    {
        CodeVanguardManager.Instance.OnTaskEnd += OnTaskEnd;
    }

    private void OnTaskEnd(string id, GradingResult result)
    {
        if (id.Equals(_taskId))
        {
            UpdateStatus(result);
        }
    }

    public void HideAll()
    {
        _imgInvalid.gameObject.SetActive(false);
        _imgValid.gameObject.SetActive(false);
        _star1.gameObject.SetActive(false);
        _star2.gameObject.SetActive(false);
        _star3.gameObject.SetActive(false);
        _star4.gameObject.SetActive(false);
        _star5.gameObject.SetActive(false);
        _star1Inner.gameObject.SetActive(false);
        _star2Inner.gameObject.SetActive(false);
        _star3Inner.gameObject.SetActive(false);
        _star4Inner.gameObject.SetActive(false);
        _star5Inner.gameObject.SetActive(false);
    }

    public void SetOptimal()
    {
        _imgValid.gameObject.SetActive(true);

        bg.color = Color.green;

    }
    public void SetInvalid()
    {
        _imgInvalid.gameObject.SetActive(true);
        bg.color = Color.red;
    }

    public void SetStarRating(int i)
    {
        //bg.color = Color.green;

        // Ensure all stars are active
        _star1.gameObject.SetActive(true);
        _star2.gameObject.SetActive(true);
        _star3.gameObject.SetActive(true);
        _star4.gameObject.SetActive(true);
        _star5.gameObject.SetActive(true);
        _star1Inner.gameObject.SetActive(true);
        _star2Inner.gameObject.SetActive(true);
        _star3Inner.gameObject.SetActive(true);
        _star4Inner.gameObject.SetActive(true);
        _star5Inner.gameObject.SetActive(true);
        // Set the stars' colors based on the rating
        _star1Inner.color = (i >= 1) ? Color.white : Color.gray;
        _star2Inner.color = (i >= 2) ? Color.white : Color.gray;
        _star3Inner.color = (i >= 3) ? Color.white : Color.gray;
        _star4Inner.color = (i >= 4) ? Color.white : Color.gray;
        _star5Inner.color = (i >= 5) ? Color.white : Color.gray;

        bg.color = (i >= 5) ? Color.green : Color.yellow;
    }


    public void UpdateStatus(GradingResult res)
    {
        HideAll();

        if (CodeVanguardManager.Instance.UseStarSystem)
        {
            if (!res.Correct)
            {
                SetInvalid();
            }
            else
            {
                var userScore = res.TotalScore;
                if (userScore < 25f)
                    SetStarRating(1);
                else if (userScore < 50f)
                    SetStarRating(2);
                else if (userScore < 75f)
                    SetStarRating(3);
                else if (userScore < 100f)
                    SetStarRating(4);
                else if (userScore == 100f) // Only perfect score gets 5 stars
                    SetStarRating(5);

            }
        }
        else
        {
            if (!res.Correct)
            {
                SetInvalid();
            }
            else
            {

                SetOptimal();


            }
        }
    }
}
