using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image DamageEffect;
    public Image HealthBarFill;
    [SerializeField] private float decreaseSpeed;
    public void UpdateHealthBar(float ratio)
    {
        HealthBarFill.fillAmount = ratio;
    }
    private void Update(){
        if (DamageEffect.fillAmount > HealthBarFill.fillAmount){
            DamageEffect.fillAmount -= decreaseSpeed;
            Debug.Log(DamageEffect.fillAmount);
            Debug.Log(HealthBarFill.fillAmount);
            Debug.Log("Decreasing" + decreaseSpeed);
        }else{
            DamageEffect.fillAmount = HealthBarFill.fillAmount;
        }
    }
}
