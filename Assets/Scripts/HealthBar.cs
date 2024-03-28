using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill;
    public void UpdateHealthBar(float ratio)
    {
        healthBarFill.fillAmount = ratio;
    }
}
