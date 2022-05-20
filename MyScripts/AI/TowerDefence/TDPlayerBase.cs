using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TDPlayerBase : MonoBehaviour
{
    public static TDPlayerBase instance;

    //public float baseHealth = 10000f;
    //private UniversalStats unistats;
    private float shieldHealth = -1f; //REMOVE
    private float shieldReChargeRate = 50f;
    [Tooltip("Time between being damaged and recharging shield")]
    public float shieldRechargeWait = 10f;
    public Transform shootTarget;//used as target for gun enemies
    //to add - effect for shield (could use a dissolving shader, dissolve as damaged), maybe a pulse effect when hit, 

    [System.Serializable]
    public struct BaseBuilding
    {
        public Transform building;
        public GameObject destroyedBuilding; //to be used
        public Transform target;
        public float buildingHealth;
        public Image healthBar;
        [HideInInspector]
        public float startHealth;
        public UnityEvent deathEvent;
        [HideInInspector]
        public bool deathEventOccured;
    }
    public BaseBuilding[] buildings;

    [Header("UI Stuff")]
    public Image baseHealthBar;
    //public Image shieldHealthBar;

    private bool won;
    private bool lost;

    private float startShield;
    private float startHealth;
    private float rechargeTimer;
    private float totalStartHealth; //total health of all buildings
    void Start()
    {
        instance = this;
        //unistats = GetComponent<UniversalStats>();

        //startHealth = unistats.health;
        startShield = shieldHealth;
        //temp thing to disable shield
        shieldHealth = 0;
        startShield = 1;

        for (int i = 0; i < buildings.Length; i++)
        {
            buildings[i].startHealth = buildings[i].buildingHealth;
            totalStartHealth += buildings[i].buildingHealth;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (baseHealthBar != null)
        //    baseHealthBar.fillAmount = (unistats.health / startHealth);
        //if (shieldHealthBar != null)
        //    shieldHealthBar.fillAmount = (shieldHealth / startShield);

        UpdateBuildingHealth();

        if (!CheckBuildings())
        {
            //death/loss
            CanvasScript.instance.Lose();
            lost = true;
        }

        if (rechargeTimer <= 0 && shieldHealth < startShield) //recharge shield
        {
            shieldHealth += shieldReChargeRate * Time.fixedDeltaTime;
            shieldHealth = Mathf.Clamp(shieldHealth,0f, startShield);
        }
        else if (rechargeTimer > 0)
            rechargeTimer -= Time.fixedDeltaTime;



        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i].buildingHealth <= 0)
                if (buildings[i].deathEvent != null && !buildings[i].deathEventOccured)
                {
                    buildings[i].deathEvent.Invoke();
                    buildings[i].deathEventOccured = true;
                }
        }

    }


    public void DamageBase(float dam, int baseIndex)
    {
        if (!lost)
        {
            if (shieldHealth > 0)
            {
                if (shieldHealth - dam > 0)
                    shieldHealth -= dam;
                else //in this case, dam is greater than total remain shield, so damage both shield and health
                {
                    //unistats.health -= (dam - shieldHealth);
                    shieldHealth = 0;
                }
            }
            else if(buildings[baseIndex].buildingHealth>0) //only bother damaging non dead buildings
                buildings[baseIndex].buildingHealth -= dam;
            rechargeTimer = shieldRechargeWait;
        }
    }

    private bool CheckBuildings() //returns true if at least 1 building is alive
    {
        for (int i = 0; i < buildings.Length; i++)
            if (buildings[i].buildingHealth > 0)
                return true;
        return false;
    }

    public int GetClosestBase(Vector3 pos)
    {
        if (!lost)
        {
            float closestDist = float.MaxValue;
            int closestInt = 0;
            for (int i = 0; i < buildings.Length; i++)
            {
                if (buildings[i].buildingHealth > 0 && Vector3.Distance(buildings[i].building.position, pos) < closestDist)
                    closestInt = i;
            }
            return closestInt;
        }
        else
            return 0;
    }

    private void UpdateBuildingHealth()
    {
        float currentTotalHealth = 0;
        for (int i = 0; i < buildings.Length; i++)
            if (buildings[i].buildingHealth <= 0)
            {
                if (buildings[i].healthBar.gameObject.activeSelf)
                    buildings[i].healthBar.gameObject.SetActive(false);    //disable health bar when building is destoyed
            }
            else
            {
                buildings[i].healthBar.fillAmount = buildings[i].buildingHealth / buildings[i].startHealth;
                currentTotalHealth += buildings[i].buildingHealth;
            }

        baseHealthBar.fillAmount = (currentTotalHealth / totalStartHealth);
    }

}
