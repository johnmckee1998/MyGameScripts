using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunBase : MonoBehaviour
{
    [Tooltip("Unique Identifier")]
    public string gunID;
    [Tooltip("If the gun uses SmartAi based shot detection")]
    public bool UsesShotDetection = false;
    public float detectRange;

    [Header("Recoil Pattern Settings")]
    public bool separateADSRecoil;
    public bool randomPosNegADS;
    public RecoilPattern hipRecoilPattern;
    public RecoilPattern adsRecoilPattern;

    [Space]
    public float adsSpeed = 0.1f;
    protected Vector3 adsRef;

    [Header("IK Positions")]
    public Transform rightHand;
    public Transform leftHand;
    

    [Header("Ammo Attibutes")]
    public Text AmmoText;
    public int AmmoPool = 100;
    [HideInInspector]
    public int MaxAmmo = 10;
    public int MagSize = 10;
    [HideInInspector]
    public int CurMag;

    //Note: A lot of these hideininspector-public variables are like that because i didnt know about protected - however other scripts may use them so i need to check that before changing them to protected

    [HideInInspector]
    public bool reloading = false;
    [HideInInspector]
    public bool done = false; //triggers end of reload
    [HideInInspector]
    public bool cycling = false; //used to signal when gun is cycling action

    protected float shotDetectTimer; //used to prevent excessive use of shot detectiong. waits a couple seconds before doing it again so repeated shots are treated like 1
    public void ShotDetection()
    {
        if (Time.time > shotDetectTimer)
        {
            shotDetectTimer = Time.time + 2f;
            //Debug.Log("Shot Detect");
            Vector3 shotPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(shotPos, detectRange); //Cast out detection sphere
            float randomChance = 0f;
            bool firstResponse = false;
            int responders = 0;
            SmartAI[] aiHits = new SmartAI[3];
            foreach (Collider hit in colliders)
            {
                //Rigidbody rbH = hit.GetComponent<Rigidbody>();
                randomChance = Random.Range(0f, 5f); //Dont alert everyone, randomly select some (though always one, thats why firstResponse is used)
                if (hit.gameObject.CompareTag("SmartEnemy") && (randomChance > 2f || !firstResponse))
                {
                    //Debug.Log("Correct Tag");
                    firstResponse = true; //record that at least one person has been alerted
                    SmartAI ai;
                    if (hit.GetComponent<UniversalStats>() != null) //if it hits unistats, must be the parent object
                        ai = hit.gameObject.GetComponent<SmartAI>();
                    else if (hit.GetComponent<HitboxScript>() != null) // if it hits hitbox then must be child so get from parent
                        ai = hit.gameObject.GetComponentInParent<SmartAI>();
                    else
                        return; //otherwise its not set up right, return 
                    

                    if (ai!=null && !ai.getDead())
                    {
                        bool alreadyHit = false; //This loop is used because enemies may have multiple colliders - like if using hitboxes - so multiple valid hits are found for the same enemy, this makes sure only 1 message is sent to an enemy
                        for (int i = 0; i < responders; i++)
                        {
                            if (ai == aiHits[i])
                            {//already detected
                                alreadyHit = true;
                                break;
                            }
                        }
                        if (!alreadyHit)
                        {
                            Debug.Log("Heard Shot");
                            aiHits[responders] = ai;
                            float distRelative = Vector3.Distance(transform.position, hit.transform.position);
                            float ranXoffset = Random.Range(0.1f * distRelative, -0.1f * distRelative);
                            float ranZoffset = Random.Range(0.1f * distRelative, -0.1f * distRelative);
                            int amountWander = Random.Range(1, 4);
                            //Debug.Log("Sound heard! " + hit.gameObject.name);
                            ai.setWanderDest(new Vector3(transform.position.x + ranXoffset, transform.position.y, transform.position.z + ranZoffset), amountWander); //Add random value to make response more realisitc - not knowing exact location
                            responders++;
                        }
                    }

                    
                    if (responders >= 3) //Maximum 3 responders
                        break;
                }

            }
        }
    }

    public void FinishReload()
    {
        done = true;
    }

    public void FinishCycle()
    {
        cycling = false;
    }

    public void RefillAmmo()
    {
        AmmoPool = MaxAmmo;
    }
    public int GetMag()
    {
        return CurMag;
    }
}

[System.Serializable]
public class RecoilPattern
{
    public float RecoilCamRotXMin = 0f;
    public float RecoilCamRotXMax = 0f;
    public float RecoilCamRotYMin = 0f;
    public float RecoilCamRotYMax = 0f;
}
