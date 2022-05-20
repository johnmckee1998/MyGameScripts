using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using TMPro;

public class SixShooterScript : MonoBehaviour
{
    public float damage = 10;
    public float range = 100;
    public float fireRate = 10.0f;
    public float bulletSpeed = 50f;
    //public int numOfShots = 1;
    public float spreadModifier = 1;
    private float adsSpread;
    private float hipSpread;
    //public bool isSemiAuto = false;

    public bool isInside = false;

    [Tooltip("0 = Standard bullet. 1 = Grenade round. 2 = Heavy Sniper round")]
    public int ammoType = 0;

    public TextMeshProUGUI ammoText;

    //Used for fire rate timming
    private float nextFire = 0f;

    //base spread value - if spread modifier =1 then this is the spread amount
    private float spread = 0.025f;

    //Total ammo capacity
    private int ammoCap = 6;

    //private ParticleSystem flash;
    private VisualEffect flash;


    public GameObject bullet;
    private AudioSource bang;

    public Camera fpsCam;

    public GameObject Torch;

    [Header("Recoil Properties")]
    public Vector3 recoilVector = Vector3.one;
    public float recoilForce = 0.2f;
    private float recoilAngle;
    private float recoilSmoothDampVel;
    private Vector3 recoilResetForce;


    private bool hasShot = false;
    private bool ads = false;
    [Tooltip("Animator component of revolver")]
    public Animator sixAnim;

    //values used to reset gun after 'animating' 
    private Vector3 startPos;
    private float startYRot;

    //used to smoothly rotate the gun when turning (just using yawSpeed directly gives jittery movement)
    private float yRotAmount;
    private float yRotVel;
    private float xRotAmount;
    private float xRotVel;

    //position to move to when aiming
    public Transform aimPos;

    void Start()
    {
        //flash = transform.GetComponentInChildren<ParticleSystem>();
        flash = transform.GetComponentInChildren<VisualEffect>();

        bang = transform.GetComponent<AudioSource>();
        //bang.volume = 0;
        //Debug.Log(flash);
        //flash.Play();
        //spread *= spreadModifier;
        hipSpread = spread * spreadModifier;
        adsSpread = 0;

        if(ammoText != null)
            ammoText.text = ammoCap.ToString();

        fpsCam = GetComponentInParent<Camera>();

        //sixAnim = GetComponent<Animator>();

        startPos = transform.localPosition;

        startYRot = transform.localEulerAngles.y;
        yRotAmount = 0;

        xRotAmount = 0;
    }


    // Update is called once per frame
    void Update()
    {
        CharacterControllerScript.isAiming = ads;

        if (ammoText != null)
            ammoText.SetText(ammoCap.ToString());
        bang.pitch = Time.timeScale;
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFire && Time.timeScale > 0 && ammoCap > 0 && !hasShot)
        {
            //hasShot = true;
            //sixAnim.SetBool("Shoot", true);//trigger animation
            

            nextFire = Time.time + 1.0f / fireRate;
            Shoot();
            flash.Play();
            //Instantiate(flash.gameObject, flash.transform);
            
            bang.PlayOneShot(bang.clip, 1f);
        }

        if (Input.GetButtonDown("Fire2") && Time.timeScale > 0)
        {
            ads = !ads;

            sixAnim.SetBool("Aim", ads);//Start/Stop Aim Anim
        }

        if (Input.GetKeyDown("f") && Torch!=null)
            Torch.SetActive(!Torch.activeSelf);

        if(ads)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aimPos.localPosition, ref recoilResetForce, 0.1f);//reset recoil when aiming
        else
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startPos, ref recoilResetForce, 0.1f);//reset recoil

        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilSmoothDampVel, 0.1f);
        //recoilAngle -= 0.1f;
        Mathf.Clamp(recoilAngle, 0, 30);

        //Caluclations for the weapon 'sway'/movement when spining 
        if (CameraMove.yawSpeed != 0)
            yRotAmount = Mathf.SmoothDamp(yRotAmount, CameraMove.yawSpeed, ref yRotVel, 0.1f);
        else
            yRotAmount = Mathf.SmoothDamp(yRotAmount, 0, ref yRotVel, 0.5f);


        if (CameraMove.pitchSpeed != 0)
            xRotAmount = Mathf.SmoothDamp(xRotAmount, CameraMove.pitchSpeed, ref xRotVel, 0.1f);
        else
            xRotAmount = Mathf.SmoothDamp(xRotAmount, 0, ref xRotVel, 0.5f);

        //applying weapon sway/spin
        transform.localEulerAngles = new Vector3(1*recoilAngle + xRotAmount, startYRot + yRotAmount, -1 * recoilAngle);
        //transform.RotateAround(transform.position+transform.forward, new Vector3(0,1f,0), yRotAmount);
        //transform.RotateAround(transform.position + transform.forward, new Vector3(1f,0,0), xRotAmount);

        //Debug.Log("Angle: " + recoilAngle + " localEuler: " + transform.localEulerAngles);
        
    }


    private void Shoot()
    {
        //flash.Stop();
        //flash = transform.GetComponentInChildren<ParticleSystem>();
        //flash.Play();
        // 

        //Trigger Guard detection of gunshot - maybe add a thing to change the trigger distance depending on whether or not the player is in a building
        float DRange = 0;
        if (isInside)
            DRange = 40.0f;
        else
            DRange = 75.0f;

        Vector3 shotPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(shotPos, DRange); //Cast out detection sphere
        float randomChance = 0f;
        bool firstResponse = false;
        int responders = 0;
        foreach (Collider hit in colliders)
        {
            //Rigidbody rbH = hit.GetComponent<Rigidbody>();
            randomChance = Random.Range(0f, 5f); //Dont alert everyone, randomly select some (though always one)
            if (hit.gameObject.tag == "SmartEnemy" && (randomChance>2f || !firstResponse)) 
            {
                firstResponse = true; //record that at least one person has been alerted
                if (!hit.gameObject.GetComponent<SmartAI>().getDead())
                {
                    float distRelative = Vector3.Distance(transform.position, hit.transform.position);
                    float ranXoffset = Random.Range(0.1f*distRelative, -0.1f*distRelative);
                    float ranZoffset = Random.Range(0.1f*distRelative, -0.1f*distRelative);
                    int amountWander = Random.Range(1, 4);
                    //Debug.Log("Sound heard! " + hit.gameObject.name);
                    hit.gameObject.GetComponent<SmartAI>().setWanderDest(new Vector3(transform.position.x + ranXoffset, transform.position.y, transform.position.z +ranZoffset), amountWander); //Add random value to make response more realisitc - not knowing exact location
                }

                responders++;
                if (responders >= 3) //Maximum 3 responders
                    break;
            }

        }
        //Debug.Log("No. of responders: " + responders);


        //New Way of shooting - Raycast
        RaycastHit rayHit;
        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward, Color.white, 5.0f, false);
        if(Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range))
        {
            UniversalStats uniStats = rayHit.transform.GetComponent<UniversalStats>();

            if (uniStats != null) //Damage objects/enemies with universal stats
            {
                uniStats.health -= damage;

                Rigidbody uniRb = rayHit.transform.GetComponent<Rigidbody>();

                if (uniRb != null)
                    uniRb.AddForce(-80 * rayHit.transform.forward); //currently always hits target back - fix this as it is stupid
            }
            else if (rayHit.transform.tag == "SmartEnemy")
            {
                //ameObject b = Instantiate(blood, transform.position, transform.rotation); spawn Blood
                //b.GetComponent<ParticleSystem>().Play();
                rayHit.transform.GetComponent<SmartAI>().showBlood(rayHit.point, new Quaternion(fpsCam.transform.rotation.x, -fpsCam.transform.rotation.y, fpsCam.transform.rotation.z, 0f));
                rayHit.transform.GetComponent<SmartAI>().playHitSound();

                rayHit.transform.GetComponent<SmartAI>().health -= damage;
                rayHit.transform.GetComponent<Rigidbody>().AddForce(50 * fpsCam.transform.forward);
            }
        }



        /* OLD WAY - Uses projectile
        GameObject bul = Instantiate(bullet, (transform.position), transform.rotation);
        float ranX = Random.Range(-spread, spread);
        float ranY = Random.Range(-spread, spread);
        float ranZ = Random.Range(-spread, spread);

        if (ammoType == 0) //Standard Bullet
            {
            bul.GetComponent<BulletScript>().SetTravelDir((-transform.forward + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
            bul.GetComponent<BulletScript>().damage = damage;
            bul.GetComponent<BulletScript>().range = range;
        }
        else if (ammoType == 1) //Grenade Round
        {
            bul.GetComponent<GrenadeRoundScript>().SetTravelDir((-transform.forward + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
            bul.GetComponent<GrenadeRoundScript>().damage = damage;
            bul.GetComponent<GrenadeRoundScript>().range = range;
        }
        else if (ammoType == 2) //HeavySniper Round
        {
            bul.GetComponent<ExplosiveRoundScript>().SetTravelDir((-transform.forward + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
            bul.GetComponent<ExplosiveRoundScript>().damage = damage;
            bul.GetComponent<ExplosiveRoundScript>().range = range;
        }
        */

        ammoCap--;
        if (ads)
        {
            transform.localPosition -= new Vector3(Vector3.forward.x * recoilVector.x, Vector3.forward.y * recoilVector.y, Vector3.forward.z * recoilVector.z) * recoilForce / 10; //Recoil ADS dampened
            recoilAngle += 7;
        }
        else
        {
            transform.localPosition -= new Vector3(Vector3.forward.x * recoilVector.x, Vector3.forward.y * recoilVector.y, Vector3.forward.z * recoilVector.z) * recoilForce; //Recoil
            recoilAngle += 15;
        }
        //recoilAngle += 15;
        recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
    }

    public void SetShoot()
    {
        hasShot = false;
        //sixAnim.SetBool("Shoot", false);
    }

}
