using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMoney : MonoBehaviour
{
    public static int Money = 500; //start at 500;
    public int startMoney = 500;
    public TextMeshProUGUI moneyText;
    public AudioSource buySound;
    public float scaleResetSpeed = 0.5f;
    private int prevMoney;
    private Vector3 startScale;
    void Start()
    {
        Money = startMoney;

        prevMoney = Money;

        if(moneyText!=null)
            startScale = moneyText.rectTransform.localScale;
    }

    private void Update()
    {
        if (moneyText != null)
        {
            moneyText.text = "Dosh: " + Money.ToString();

            moneyText.rectTransform.localScale = Vector3.Lerp(moneyText.rectTransform.localScale, startScale, scaleResetSpeed*Time.deltaTime);

            if (Money > prevMoney) //money went up
                moneyText.rectTransform.localScale = startScale * 1.1f;
            else if (Money < prevMoney && buySound != null) //money went down
                buySound.Play();
        }

        prevMoney = Money;
    }

    public void AddMoney(int m)
    {
        Money += m;
    }
}
