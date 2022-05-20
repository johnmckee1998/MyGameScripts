using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TDBuyMenu : MonoBehaviour
{
    public static TDBuyMenu instance;

    [System.Serializable]
    public struct PlacementPurchase
    {
        public string name;
        public string placementID;
        public GameObject placePreview;
        public GameObject placeObj;
        public int cost;
        public string description;
    }

    public PlacementPurchase[] buyablePlacements;

    [System.Serializable]
    public struct GunPurchase
    {
        public string name;
        public string ID;
        public GameObject gun;
        public int cost;
        public string description;
    }

    public GunPurchase[] buyableGuns;

    [System.Serializable]
    public struct BotPurchase
    {
        public string name;
        public GameObject bot;
        public int cost;
        public string description;
    }

    public BotPurchase[] buyableBots;
    public Transform botSpawn;
    public float botSpawnRadius;
    [Space]
    public TMP_Dropdown placementDropdown;
    public TMP_Dropdown gunDropdown;
    public TMP_Dropdown botDropdown;
    [Space]
    public TextMeshProUGUI placementPrice;
    public TextMeshProUGUI gunPrice;
    public TextMeshProUGUI botPrice;
    [Space]
    public TextMeshProUGUI placementDescription;
    public TextMeshProUGUI gunDescription;
    public TextMeshProUGUI botDescription;
    [Space]
    public TextMeshProUGUI botCounter;
    [Header("Sounds")]
    public AudioSource invalidPurchaseSound;
    public AudioSource noMoneySound;
    public TextMeshProUGUI invalidPurchaseText;
    public TextMeshProUGUI noMoneyText;
    private float IPTextLerp=1; //used to lerp text colour for invalid purchase
    private float NMTextLerp=1; //same but for no money


    private WeaponSelection weapSelect;

    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        placementDropdown.ClearOptions();
        for(int i=0; i< buyablePlacements.Length; i++)
        {
            placementDropdown.AddOptions(new List<string> {buyablePlacements[i].name});
        }

        gunDropdown.ClearOptions();
        for (int i = 0; i < buyableGuns.Length; i++)
        {
            gunDropdown.AddOptions(new List<string> { buyableGuns[i].name });
        }

        botDropdown.ClearOptions();
        for (int i = 0; i < buyableBots.Length; i++)
        {
            botDropdown.AddOptions(new List<string> { buyableBots[i].name });
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (weapSelect == null)
            if(WeaponSelection.instance!=null)
                weapSelect = WeaponSelection.instance;

        //Update Prices
        if (gunPrice != null)
            gunPrice.text = "Cost: " + buyableGuns[gunDropdown.value].cost.ToString();

        if (placementPrice != null)
            placementPrice.text = "Cost: " + buyablePlacements[placementDropdown.value].cost.ToString();

        if (botPrice != null)
            botPrice.text = "Cost: " + buyableBots[botDropdown.value].cost.ToString();

        //update description
        if (gunDescription != null)
            gunDescription.text = buyableGuns[gunDropdown.value].description;

        if (placementDescription != null)
            placementDescription.text = buyablePlacements[placementDropdown.value].description.ToString();

        if (botDescription != null)
            botDescription.text = buyableBots[botDropdown.value].description;

        if (botCounter != null)
            botCounter.text = "Bot slots Available: " + AICoverpointManager.instance.FreePointsAvaliable().ToString();

        if (invalidPurchaseText != null)
        {
            IPTextLerp += Time.deltaTime;
            IPTextLerp = Mathf.Clamp01(IPTextLerp);
            invalidPurchaseText.color = Color.Lerp(Color.red, Color.clear, IPTextLerp);
            NMTextLerp += Time.deltaTime;
            NMTextLerp = Mathf.Clamp01(NMTextLerp);
            noMoneyText.color = Color.Lerp(Color.red, Color.clear, NMTextLerp);
        }

        //close menu when pressing escape which also opens pause menu
        if (Input.GetKeyDown("escape"))
            gameObject.SetActive(false);
    }

    public void BuyPlacement()
    {
        if (buyablePlacements[placementDropdown.value].cost <= PlayerMoney.Money)
        {
            if (buyablePlacements[placementDropdown.value].placementID != weapSelect.placementID || weapSelect.placementCount<=0) //if owned placement isnt the same or none are left
            {
                PlayerMoney.Money -= buyablePlacements[placementDropdown.value].cost;
                weapSelect.placementPreview = buyablePlacements[placementDropdown.value].placePreview;
                weapSelect.placementObject = buyablePlacements[placementDropdown.value].placeObj;
                weapSelect.placementID = buyablePlacements[placementDropdown.value].placementID;
                weapSelect.placementCount = 1;
            }
            else
                InvalidPurchase();
        }
        else //Not enough money
            NotEnoughMoney();
    }

    public void BuyGun(int weapSlot)
    {
        if (buyableGuns[gunDropdown.value].cost <= PlayerMoney.Money)
        {
            if (!weapSelect.CheckID(buyableGuns[gunDropdown.value].ID))
            {
                PlayerMoney.Money -= buyableGuns[gunDropdown.value].cost;
                weapSelect.AddGun(Instantiate(buyableGuns[gunDropdown.value].gun, weapSelect.transform.position, weapSelect.transform.rotation), true, weapSlot);
            }
            else//enough money but already own gun
                InvalidPurchase();
        }
        else
            NotEnoughMoney();
    }

    public void BuyBot()
    {
        if (buyableBots[botDropdown.value].cost <= PlayerMoney.Money)
        {
            if (AICoverpointManager.instance.ArePointsAvaliable())
            {
                PlayerMoney.Money -= buyableBots[botDropdown.value].cost;
                Vector3 offset = new Vector3(Random.Range(-botSpawnRadius, botSpawnRadius), 0f, Random.Range(-botSpawnRadius, botSpawnRadius));
                Instantiate(buyableBots[botDropdown.value].bot, botSpawn.position + offset, botSpawn.rotation, botSpawn);
            }
            else //enough money but no avaliable points
                InvalidPurchase();
        }
        else //not enough money
            NotEnoughMoney();
    }

    private void OnEnable()
    {
        if (CharacterControllerScript.instance != null && CharacterControllerScript.Active)
        {
            CanvasScript.instance.pReticle.SetActive(false);
            CharacterControllerScript.instance.LockMovement(true);
        }
    }


    private void OnDisable()
    {
        if (CharacterControllerScript.instance != null && !CharacterControllerScript.Active)
        {
            CanvasScript.instance.pReticle.SetActive(true);
            CharacterControllerScript.instance.UnlockMovement();
        }
    }

    private void NotEnoughMoney()
    {
        //also add visual effect like red flash or something
        noMoneySound.Play();
        noMoneyText.color = Color.red;
        NMTextLerp = 0f;
    }

    private void InvalidPurchase()
    {
        //also add visual effect like orange flash or something
        invalidPurchaseSound.Play();
        invalidPurchaseText.color = Color.red;
        IPTextLerp = 0;
    }

}
