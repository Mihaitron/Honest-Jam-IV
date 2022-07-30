using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform chalkSlider;
    [SerializeField] private RectTransform staminaSlider;
    [SerializeField] private TMP_Text score;

    public static UIManager instance { get; private set; }

    private void Start()
    {
        if (!instance)
        {
            instance = this;
        }
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

    public void SetPoints(int points)
    {
        score.text = points.ToString("D7");
    }
}
