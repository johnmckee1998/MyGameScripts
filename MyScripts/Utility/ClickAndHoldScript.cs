using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickAndHoldScript : MonoBehaviour
{
    [Range(0.01f, 3f)]
    public float timeToBuy = 1f;
    public Image buyFillImage;

    public UnityEvent OnFillEvent;

    private float buyButtonFill;
    private bool holding;

    public void StartCounting()
    {
        holding = true;
        StartCoroutine(FillCheck());
    }

    private IEnumerator FillCheck()
    {
        while (holding)
        {
            buyButtonFill += Time.deltaTime / timeToBuy;
            buyFillImage.fillAmount = buyButtonFill;
            if (buyButtonFill >= 1)
            {
                OnFillEvent?.Invoke();
                break;
            }
            yield return null;
        }
    }

    public void StopCounting()
    {
        holding = false;
        buyButtonFill = 0;
        buyFillImage.fillAmount = buyButtonFill;
    }

}
