using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSelection : MonoBehaviour
{
    public static WeaponSelection instance;

    private int curWeapon = 0;
    private GunBase[] weapons;
    private WeaponSwitchAnimationManager[] gunParents;
    private GunBase CurWeapScript;
    public TextMeshProUGUI AmmoText;

    public static AudioSource ClickSound;
    public static bool IsReloading = false;
    

    [Tooltip("Used to set weapon cap -> 0 means unlimited")]
    public int gunLimit = 0;

    public bool allowSwitch;

    int prevWeap;

    //stuff for the ammotext pulse effect
    private Vector3 ammoTScale;
    private float prevAmmoCount;

    private string[] currentIDs;

    [Header("Grenade Stuff")]
    public GameObject grenade;
    public int grenadeCount;
    private int maxGrenadeCount;

    [Header("PlacementStuff")]
    public GameObject placementPreview;
    public GameObject placementObject;
    public string placementID;
    public int placementCount = 0;
    private GameObject tempPreview; //used to store a reference to the instatiated obj
    private ObjectPlacementPreview previewObjScript;
    private bool placing; //true when previewing placement - used to disable shooting as left click is used to place

    [Header("Support Call In")]
    public GameObject callIn;
    public Vector3 spawnOffset;
    public string callInID;
    public int callInAmount = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        IsReloading = false;
        ClickSound = GetComponent<AudioSource>();

        //SelectWeapon();
        

        weapons = new GunBase[transform.childCount];
        gunParents = new WeaponSwitchAnimationManager[transform.childCount];
        currentIDs = new string[20]; //20 should allow for all instances - probably could do better but this will work for now, ideally make it dynamic length
        //weapons = GetComponentsInChildren<ShootingScript>();
        int a = 0;
        foreach(Transform g in transform)
        {
            weapons[a] = g.GetComponentInChildren<GunBase>();
            gunParents[a] = g.GetComponent<WeaponSwitchAnimationManager>();
            currentIDs[a] = weapons[a].gunID;
            a++;
        }

        CurWeapScript = weapons[0];

        prevWeap = 0;
        //Debug.Log("Amount of weapons " + weapons.Length);

        

        StartWeap();


        prevAmmoCount = CurWeapScript.CurMag;

        //if(AmmoText==null)
            

        ammoTScale = AmmoText.transform.localScale;

        maxGrenadeCount = grenadeCount;
    }

    // Update is called once per frame
    void Update()
    {
        

        prevWeap = curWeapon;

        if(Input.GetAxis("Mouse ScrollWheel") > 0f && Time.timeScale>0)
        {
            if (curWeapon >= transform.childCount - 1)
                curWeapon = 0;
            else
                curWeapon++;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f && Time.timeScale > 0)
        {
            if (curWeapon <= 0)
                curWeapon = transform.childCount - 1;
            else
                curWeapon--;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && Time.timeScale > 0)
            curWeapon = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount > 1 && Time.timeScale > 0) 
            curWeapon = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount > 2 && Time.timeScale > 0)
            curWeapon = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount > 3 && Time.timeScale > 0)
            curWeapon = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount > 4 && Time.timeScale > 0)
            curWeapon = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6) && transform.childCount > 5 && Time.timeScale > 0)
            curWeapon = 5;

        CurWeapScript = weapons[curWeapon];

        if (AmmoText != null) {
            try
            {
                AmmoText.SetText((CurWeapScript.GetMag() + " / " + CurWeapScript.AmmoPool));
            }
            catch
            {
                //AmmoText.SetText(transform.GetChild(curWeapon).GetComponentInChildren<FlameThrowerScript>().GetMag() + " / " + transform.GetChild(curWeapon).GetComponentInChildren<FlameThrowerScript>().AmmoPool); //WTF this is way too long
            }
        }

        if (prevWeap != curWeapon)
            SelectWeapon();

        //pulse effect
        if (CurWeapScript.CurMag != prevAmmoCount)
            AmmoText.transform.localScale = ammoTScale * 1.5f;
        AmmoText.transform.localScale = Vector3.MoveTowards(AmmoText.transform.localScale, ammoTScale, 0.1f);

        prevAmmoCount = CurWeapScript.CurMag;


        if(Input.GetButtonDown("Grenade") && grenadeCount > 0)
        {
            grenadeCount--;
            Instantiate(grenade, CharacterControllerScript.instance.pCam.transform.position, Quaternion.identity);
        }

        //ObjectPlacement
        if (placing && tempPreview!=null)
        {
            if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && previewObjScript.IsValidPlace() && CharacterControllerScript.Active)
            {
                //Spawn Placement Obj
                Instantiate(placementObject, tempPreview.transform.position, tempPreview.transform.rotation);
                Invoke("DisablePlacing", 0.25f); //i disabled placing like this so that the player doesnt accidentally shoot when placing - gives a slight delay so the player can let go of mouse button
                try
                {
                    tempPreview.SendMessage("Placed", SendMessageOptions.DontRequireReceiver);
                }
                catch
                {

                }
                Destroy(tempPreview);
                placementCount--;
            }
        }

        //Object Placement Preview
        if (Input.GetKeyDown("i") && placementCount > 0 && Time.timeScale > 0)
        {
            if (placing) //if already placing
            {
                DisablePlacing();
            }
            else
            {
                placing = true;
                tempPreview = Instantiate(placementPreview);
                previewObjScript = tempPreview.GetComponent<ObjectPlacementPreview>();
            }
        }

    }

    public void DisablePlacing()
    {
        placing = false;
        if (tempPreview != null)
            Destroy(tempPreview);
    }

    private void StartWeap()
    {
        int i = 0;
        foreach (Transform weap in transform)
        {
            weap.gameObject.SetActive(i == curWeapon);
            i++;
        }
        //for(int i = 0; i < weapons.Length; i++)
        //{
        //    weapons[i].enabled = (i == curWeapon);
        //}

    }

    private void SelectWeapon()
    {
        try
        {
            transform.GetChild(prevWeap).GetComponent<WeaponSwitchAnimationManager>().SwitchGunAnim();
        }
        catch
        {

        }

        
        /*
        int i = 0;
        
        foreach (Transform weap in transform)
        {
            weap.gameObject.SetActive(i == curWeapon);
            i++;
        } 
        */
    }

    public void EquipGun()
    {
        /*
        for(int i=0; i<gunParents.Length; i++)
        {
            gunParents[i].EquipGun(i == curWeapon);
        }
        */

        int i = 0;
        foreach (Transform weap in transform)
        {
            weap.gameObject.SetActive(i == curWeapon);
            i++;
        }
        /*for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = (i == curWeapon);
        }*/
    }

    public void MaxAmmoAll()
    {
        for (int i =0; i<weapons.Length; i++)
            weapons[i].RefillAmmo();

        grenadeCount = maxGrenadeCount;
    }

    private void RefreshGuns()
    {
        weapons = new GunBase[transform.childCount];
        gunParents = new WeaponSwitchAnimationManager[transform.childCount];
        //weapons = GetComponentsInChildren<ShootingScript>();
        int a = 0;
        foreach (Transform g in transform)
        {
            weapons[a] = g.GetComponentInChildren<GunBase>();
            gunParents[a] = g.GetComponent<WeaponSwitchAnimationManager>();
            currentIDs[a] = weapons[a].gunID;
            a++;
        }
    }

    public void AddGun(GameObject g, bool replace = false, int index = -1) //index only relevant if replacing
    {
        if (index < 0)
            index = curWeapon;//used when no index is given, or invalid is given

        if ((gunLimit == 0 || transform.childCount < gunLimit) && !replace) //nomarl behaviour/weapon cap not reached
        {
            g.transform.parent = transform;
            RefreshGuns();
            curWeapon = transform.childCount - 1;

            EquipGun();
            SelectWeapon();
            //g.SetActive(true);
        }
        else if(transform.childCount >= gunLimit || replace) //limited weapon cap behaviour //reached cap
        {
            Debug.Log("CapReached");
            curWeapon = index;
            Destroy(gunParents[curWeapon].gameObject);//replace gun
            StartCoroutine(ReplaceGun(g));
            
        }
    }

    IEnumerator ReplaceGun(GameObject g) //maybe have this run in LateUpdate ?
    {
        yield return null;
        g.transform.parent = transform;
        g.transform.SetSiblingIndex(curWeapon);
        RefreshGuns();
        //curWeapon = transform.childCount - 1;
        //SelectWeapon();
        g.SetActive(true); //replace with equipgun?
        EquipGun();
    }

    //check if a given id is already present in currentIds -> checks if player has the given gun
    public bool CheckID(string ID)
    {
        for(int i = 0; i<weapons.Length; i++)
        {
            if (currentIDs[i].Equals(ID))
                return true;
        }
        return false;
    
    }

    public void SetCurrentWeapon(int i)//sets weapon at current index as the current weapon
    {
        if (i < transform.childCount)
        {
            curWeapon = i;
            EquipGun();
        }
        
    }

    public void RefillAmmo(string ID)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].gunID.Equals(ID))
                weapons[i].RefillAmmo();
        }
    }

    public string GetID()
    {
        return currentIDs[curWeapon];
    }

    public void ChangeGrenade(GameObject g, int amount)
    {
        grenade = g;
        grenadeCount = amount;
        maxGrenadeCount = amount;
    }

    public bool IsPlacing()
    {
        return placing;
    }

    public void SetPlacing(bool b) //previously placing was only used by this script when placing object, now is used as a generic way of preventing shooting
    {
        placing = b;
    }

    public bool CheckSupportID(string i) //returns true if the id is same as current all in
    {
        if (i.Equals(callInID))
            return true;
        return false;
    }

    public GunBase GetCurrentWeapon()
    {
        return CurWeapScript;
    }
}
