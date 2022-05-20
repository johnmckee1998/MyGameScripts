using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBuyEvents : MonoBehaviour
{
    public int cost = 100;
    public void BuyHealth()
    {
        if (PlayerMoney.Money >= cost)
        {
            PlayerMoney.Money -= cost;
            CharacterControllerScript.instance.FillHealth();
        }
    }

    public void BuyAllAmmo()
    {
        if (PlayerMoney.Money >= cost)
        {
            PlayerMoney.Money -= cost;
            WeaponSelection.instance.MaxAmmoAll();
        }
    }

    public void BuyAmmo(string gunID)
    {
        if (PlayerMoney.Money >= cost)
        {
            PlayerMoney.Money -= cost;
            WeaponSelection.instance.RefillAmmo(gunID);
        }
    }
}
