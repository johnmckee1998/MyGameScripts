using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;

public class BotShooting : MonoBehaviour
{
    [Tooltip("Determines whether raycast or projectiles are used")]
    public bool UseRayCast = false;
    [Tooltip("Unused?")]
    public LayerMask rayCastIgnore;
    public float rayCastDropFactor = 1;

    [Header("Base Attributes")]
    [Tooltip("If true, then only spread will be changed, for the rest the prefabs settings will be used")]
    public bool dontChangeStats = false;
    public float damage = 10;
    public float range = 100;
    public float fireRate = 10.0f;
    private float fireRatePerSecond;
    public float bulletSpeed = 50f;
    public int numOfShots = 1;
    public float spreadModifier = 1;
    public bool isSemiAuto = false;

    //private bool aiming = false;

    [Header("Ammo Attibutes")]
    
    
    private int MaxAmmo = 10;

    public int MagSize = 10;
    public bool limitedAmmo;
    public int ammoPool = 100;
    private int startAmmoPool;
    public Image ammoDisplay;
    public GameObject reloadIcon;
    [Space]
    public bool randomMagSize;
    public Vector2Int magMinMax;
    [Space]
    public GameObject muzzleFlash;
    public float reloadTime = 1.0f;
    private int CurMag;
    private bool reloading = false;
    public Transform BulletSpawnPoint;

    //private Animator gunAnim;

    [Tooltip("0 = Standard AI bullet. 1 = AI Grenade round. 2 = AI Heavy Sniper round")]
    public int ammoType = 0;

    private float nextFire = 0f;
    private float spread = 0.025f;

    private ParticleSystem flash;
    private VisualEffect vfxFlash;

    public GameObject bullet;
    public bool usedPooledBullet;
    public string pooledBulletID;
    private AudioSource bang;

    [Header("recoil")]
    public Transform recoilTransform;
    public Vector3 recoilVector;
    public float recoilResetRate = 1;
    private Vector3 recoilStartPos;


    //[Header("Aim Assist")]
    //[Tooltip("AA requires another script to set the aa target")]
    [HideInInspector]
    public bool useAimAssist;
    [HideInInspector]
    public Vector3 aimAssistTarget;
    
    
    //NOT USED
    /*
    [Header("Recoil Properties")]
    public Vector3 recoilVector = Vector3.one;
    public float recoilForce = 0.2f;
    public float recoilRotForce = 10f;
    public float recoilResetTime = 0.1f;
    public Vector3 recoilResetForce;
    private float recoilResetRotRef;
    */
    
    /*
    [Header("Bolt Properties")]
    public GameObject boltObject;
    public Vector3 boltMovementVector = Vector3.one;
    public float boltRecoilForce = 0.2f;
    public float boltResetTime = 0.2f;
    private Vector3 boltResetVector;
    private Vector3 boltHomePos;
    [Header("To be used bolt properties")]
    public bool isOpenBolt = false;
    public Transform restPos; //position of the bolt when ready to fire
    public Transform firedPos; //position of the bolt when fired
    private bool useBoltPos = false;
    */

    //private bool started = false;
    
    public bool shoot = false;
    public bool alwaysShoot;
    //private bool ads = false;
    //private float adsSpread;
    //private float hipSpread;


    //used to smoothly rotate the gun when turning (just using yawSpeed directly gives jittery movement)


    [Header("Test")]
    public AudioClip[] shotSounds;
    private bool shotLastUpdate;
    private float timeOfLastShot;
    private float shotLenth;


    void Start()
    {
        fireRatePerSecond = fireRate / 60f;

        flash = transform.GetComponentInChildren<ParticleSystem>();
        vfxFlash = transform.GetComponentInChildren<VisualEffect>();
        bang = transform.GetComponent<AudioSource>();
        
        CurMag = MagSize;
         
        //started = true;


        shoot = false;

        if (recoilTransform != null)
            recoilStartPos = recoilTransform.localPosition;

        shotLenth = 1f / fireRatePerSecond;

        if(limitedAmmo)
            startAmmoPool = ammoPool;


        if (reloadIcon != null)
            reloadIcon.SetActive(reloading);
    }


    // Update is called once per frame
    void Update()
    {
        if (alwaysShoot)
            shoot = true;

        if(bang!=null)
            bang.pitch = Time.timeScale;
        if (!isSemiAuto)
        {
            //Fullauto fire
            if (shoot && Time.time >= nextFire && Time.timeScale > 0 && CurMag > 0 && !reloading)
            {
                nextFire = Time.time + 1.0f / fireRatePerSecond;
                if (!UseRayCast)
                    Shoot();
                else
                    RayCastShoot();
                if (flash != null)
                    flash.Play();
                if (vfxFlash != null)
                    vfxFlash.Play();
                if (shotSounds.Length > 0)
                    PlayRandomSound();
                else if(bang!=null)
                    bang.PlayOneShot(bang.clip, bang.volume);

                CurMag--;
            }
        }
        else
        {
            //semi auto fire
            if (shoot && Time.time >= nextFire && Time.timeScale > 0 && CurMag > 0 && !reloading)
            {
                nextFire = Time.time + 1.0f / fireRatePerSecond;
                if (!UseRayCast)
                    Shoot();
                else
                    RayCastShoot();
                if (flash != null)
                    flash.Play();
                if (vfxFlash != null)
                    vfxFlash.Play();
                if (shotSounds.Length > 0)
                    PlayRandomSound();
                else if (bang != null)
                    bang.PlayOneShot(bang.clip, bang.volume);

                CurMag--;
            }
        }


        if (CurMag <= 0 && !reloading)
        {
            StartCoroutine(Reload());
        }

        if(false && shotSounds.Length>0 && !shoot && shotLastUpdate) //has stopped shooting
        {
           StartCoroutine( PlaySoundAfterTime(shotLenth - (Time.time - timeOfLastShot), 2)); //play end sound
        }

        shotLastUpdate = shoot;

        if (ammoDisplay != null && limitedAmmo)
            ammoDisplay.fillAmount = ((float)ammoPool + CurMag) / ((float)startAmmoPool + MagSize);
        
    }

    private void FixedUpdate()
    {
        if (recoilTransform != null)
            recoilTransform.localPosition = Vector3.MoveTowards(recoilTransform.localPosition, recoilStartPos, recoilResetRate*Time.fixedDeltaTime);

        
    }


    private void Shoot()
    {

        for (int i = 0; i < numOfShots; i++)
        {
            GameObject bul;
            if (!usedPooledBullet)
                bul = Instantiate(bullet, BulletSpawnPoint.position, new Quaternion(0, 0, 0, 0));
            else
                bul = SimpleObjectPool.instance.SpawnFromPool(pooledBulletID, BulletSpawnPoint.position, Quaternion.identity);
            float ranX;
            float ranY;
            float ranZ;

            ranX = Random.Range(-spread, spread);
            ranY = Random.Range(-spread, spread);
            ranZ = Random.Range(-spread, spread);
            


            if (ammoType == 0) //Standard Bullet
            {
                EnemyBulletScript bulSc = bul.GetComponent<EnemyBulletScript>();
                bulSc.SetTravelDir((-transform.forward + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
                bulSc.damage = damage;
                bulSc.range = range;
            }
            else if (ammoType == 1) //Grenade Round
            {
                GrenadeRoundScript bulScript = bul.GetComponent<GrenadeRoundScript>();

                bulScript.SetTravelDir((-transform.forward + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
                bulScript.damage = damage;
                bulScript.range = range;
            }
            else if (ammoType == 2) //HeavySniper Round
            {
                bul.GetComponent<ExplosiveRoundScript>().SetTravelDir((-transform.forward + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
                bul.GetComponent<ExplosiveRoundScript>().damage = damage;
                bul.GetComponent<ExplosiveRoundScript>().range = range;
            }
        }
    }

    private void RayCastShoot()
    {
        for (int i = 0; i < numOfShots; i++)
        {
            Vector3 direction;
            if (useAimAssist)
                direction = (aimAssistTarget - BulletSpawnPoint.position).normalized;
            else
                direction = BulletSpawnPoint.forward;

            float randomX = Random.Range(-spreadModifier, +spreadModifier);
            float randomY = Random.Range(-spreadModifier, +spreadModifier);
            //shot.transform.eulerAngles += new Vector3(randomX, randomY, 0f);

            GameObject shot;
            if (!usedPooledBullet) //instatiate bullet
                shot = Instantiate(bullet, BulletSpawnPoint.position, Quaternion.LookRotation(direction) * Quaternion.Euler(randomX, randomY, 0f));
            else //Get bullet from pool
                shot = SimpleObjectPool.instance.SpawnFromPool(pooledBulletID, BulletSpawnPoint.position, Quaternion.LookRotation(direction) *Quaternion.Euler(randomX, randomY, 0f));
            if (!dontChangeStats)
            {
                RaycastBullet rcB = shot.GetComponent<RaycastBullet>();
                rcB.SetNewStats(bulletSpeed,damage);
                rcB.range = range;
                rcB.dropFactor = rayCastDropFactor;
            }
            //rcB.spread = spreadModifier;
            //Apply Spread
            



            //Debug.Log("Damage: " + damage);

        }
        if (recoilTransform != null)
        {
            recoilTransform.localPosition += recoilVector;
        }
        if (muzzleFlash != null)
        {
            StartCoroutine(PlayMuzzleFlash());
        }
    }

    IEnumerator Reload()
    {
        reloading = true;
        //Debug.Log(gunAnim.GetCurrentAnimatorStateInfo(0).tagHash);
        //Debug.Log("Reload Start " + reloadTime);

        if (reloadIcon != null)
            reloadIcon.SetActive(reloading);

        if (limitedAmmo && ammoPool <=0) //no ammo left on limited ammo
        {
            while (ammoPool <= 0) //wait, checking every 2 second if it ahs reloaded
                yield return new WaitForSeconds(2f);
        }



        yield return new WaitForSeconds(reloadTime);
        if(randomMagSize)
            MagSize = Random.Range(magMinMax.x, magMinMax.y+1);//+1 is coz rand int, int is exlusive on the max value
        if (limitedAmmo)
        {
            if (MagSize - CurMag < ammoPool)
            {
                ammoPool -= (MagSize - CurMag); //use mag-curmag to allow for partial realoads
                CurMag = MagSize;
            }
            else if (ammoPool > 0) //dont think this is actually necessary - the previous bit actually does this fine
            {
                CurMag += ammoPool;
                ammoPool = 0;
            }
        }
        else
            CurMag = MagSize;

        reloading = false;

        if (reloadIcon != null)
            reloadIcon.SetActive(reloading);
        //gunAnim.SetBool("Reload", false);
    }

    public void InstantReload()//used in special cases
    {
        if (randomMagSize)
            MagSize = Random.Range(magMinMax.x, magMinMax.y + 1);//+1 is coz rand int, int is exlusive on the max value
        if (limitedAmmo)
        {
            if (MagSize - CurMag < ammoPool)
            {
                ammoPool -= (MagSize - CurMag); //use mag-curmag to allow for aprtial realoads
                CurMag = MagSize;
            }
            else if (ammoPool > 0) //not necessary?
            {
                CurMag += ammoPool;
                ammoPool = 0;
            }
        }
        else
            CurMag = MagSize;
    }

    private void PlayRandomSound()
    {

        /*
        if (!shotLastUpdate)//first shot of burst
        {
            Debug.Log("Fist SHot");
            bang.clip = shotSounds[0]; //full shot sound
            bang.PlayOneShot(bang.clip, bang.volume);
        }
        else //follow up shots
        {
            if (CurMag > 1)
            {//more than 1 shot left
                Debug.Log("Mid SHot");
                bang.clip = shotSounds[0]; //mid shot sound
                bang.PlayOneShot(bang.clip, bang.volume);
            }
            else //last shot, do full sound
            {
                Debug.Log("Last SHot");
                bang.clip = shotSounds[0]; //full shot sound
                bang.PlayOneShot(bang.clip, bang.volume);
            }
        }
        timeOfLastShot = Time.time;
        */




        int rand = Random.Range(0, shotSounds.Length);
        bang.clip = shotSounds[rand];
        bang.PlayOneShot(bang.clip, bang.volume);
    }

    IEnumerator PlaySoundAfterTime(float f, int i)
    {
        Debug.Log("Delay Sound");
        yield return new WaitForSeconds(f);
        bang.clip = shotSounds[i]; 
        bang.PlayOneShot(bang.clip, bang.volume);
    }

    public bool IsReloading()
    {
        return reloading;
    }

    public void RefillAmmoPool()
    {
        ammoPool = startAmmoPool;
        CurMag = MagSize;
    }

    IEnumerator PlayMuzzleFlash()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.02f);
        muzzleFlash.SetActive(false);
    }
}
