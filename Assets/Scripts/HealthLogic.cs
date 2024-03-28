using System.Diagnostics;
using UnityEngine;

public class HealthLogic
{
    private int maxhealth = 10;

    private int currenthealth = 10;
    public int Current=> currenthealth;

    public float RatioHealth
    {
        get
        {
            //UnityEngine.Debug.Log(currenthealth/maxhealth);
            return (float) currenthealth/maxhealth;
        }
    }
    //    public float RatioHealth => (float) currenthealth/maxhealth; getter


    public void Init(int health){
        maxhealth = health;
        currenthealth = maxhealth;
    }

    public void TakeDamage(int dam){

        currenthealth -= dam;
        UnityEngine.Debug.Log(currenthealth);
    }

    public bool IsDead => Current <= 0;
}
