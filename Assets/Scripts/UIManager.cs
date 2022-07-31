using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform chalkSlider;
    [SerializeField] private RectTransform staminaSlider;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text highscore;
    [SerializeField] private GameObject failScreen;
    [SerializeField] private GameObject loadingScreen;

    public static UIManager instance { get; private set; }

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void Start()
    {
        failScreen.SetActive(false);
        loadingScreen.SetActive(true);
        loadingScreen.GetComponent<Animator>().speed = 0;
    }

    public void SetChalk(float chalk, float max)
    {
        float new_width = Utils.Remap(chalk, 0, max, 0, 1);
        chalkSlider.localScale = new Vector3(new_width, 1, 0);
    }
    
    public void SetStamina(float stamina, float max)
    {
        float new_width = Utils.Remap(stamina, 0, max, 0, 1);
        staminaSlider.localScale = new Vector3(new_width, 1, 0);
    }

    public void SetScore(int points)
    {
        score.text = points.ToString("D7");
    }

    public void SetHighschore(int points)
    {
        highscore.text = "(" + points.ToString("D7") + ")";
    }


    public void OpenFailScreen(int points)
    {
        failScreen.transform.GetChild(1).GetComponent<TMP_Text>().text = points.ToString("D7");
        failScreen.SetActive(true);
    }

    public void CloseLoadingScreen()
    {
        loadingScreen.GetComponent<Animator>().speed = 1;
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
