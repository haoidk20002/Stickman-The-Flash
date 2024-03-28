using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro damageText;
    public float moveSpeed = 50f;
    public float destroyTime = 1f;

    private void Awake()
    {
        damageText = transform.GetComponent<TextMeshPro>();

    }
    private void Start()
    {

    }

    public void SetDamageText(string text)
    {
        damageText.text = text;
    }

    private void Update()
    {
        // Move the pop-up upwards
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // Destroy the pop-up after a certain time
        Destroy(gameObject, 1f);
    }
}
