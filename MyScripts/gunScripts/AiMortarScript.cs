using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AiMortarScript : MonoBehaviour
{
    public static Vector3 mortarTarget;

    public Transform projSpawn;
    public GameObject projectile;
    [Tooltip("The radius around target that shells fall - the lower the more accurate")]
    public float accuracy = 5f;
    public float shellHeight = 100f;
    public float timeToTarget = 2f;
    public Vector2 minMaxRange = new Vector2(15f,100f);

    public Vector2 fireRateRange = new Vector2(10f,20f); //randomised fire rate so groups of mortars dont always fire in sync every shot
    public float rotSpeed = 45f;


    public VisualEffect shotVFX;

    public AudioSource shotSFX;

    public bool dontShootInterRound = true;

    private float fireRatePerSec;
    private float lastShotTime;

    private bool rotating;
    // Start is called before the first frame update
    void Start()
    {
        fireRatePerSec = Random.Range(fireRateRange.x,fireRateRange.y) / 60f;
    }

    // Update is called once per frame
    void Update()
    {

        UpdateRotation();
        
        
    }

    private void FixedUpdate()
    {
        if (!rotating && (!dontShootInterRound || TowerDefenceWaveManager.instance.WaveStatus()))
        {
            float dist = Vector3.Distance(transform.position, mortarTarget);
            if (dist > minMaxRange.x && Vector3.Distance(transform.position, mortarTarget) < minMaxRange.y)
                Firing();
        }
    }



    private void UpdateRotation()
    {
        //old rotation calc - still used for rotating check
        Vector3 rotationTarget = Quaternion.LookRotation(mortarTarget- transform.position).eulerAngles;
        //only want yaw rotation
        rotationTarget.x = transform.eulerAngles.x;
        rotationTarget.z = transform.eulerAngles.z;

        rotating = (Mathf.Abs(rotationTarget.y - transform.eulerAngles.y) > 1f); //if more than 1 degree difference, count it as rotating

        //transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, rotationTarget, Mathf.Deg2Rad * (rotSpeed * Time.deltaTime), 1.0f);

        //new rotation calc
        Quaternion rotTar = Quaternion.LookRotation(mortarTarget - transform.position);

        rotTar.x = transform.rotation.x;
        rotTar.z = transform.rotation.z;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTar, (rotSpeed * Time.deltaTime));
        
    }


    private void Firing()
    {
        if(lastShotTime<Time.time && Time.timeScale > 0)
        {
            if(shotSFX!=null)
                shotSFX.Play();
            if(shotVFX!=null)
                shotVFX.Play();
            StartCoroutine(SpawnShell(timeToTarget));

            fireRatePerSec = Random.Range(fireRateRange.x, fireRateRange.y) / 60f;
            lastShotTime = Time.time + 1f/fireRatePerSec;
        }
    }

    private IEnumerator SpawnShell(float time)
    {
        //calculate target at the start - this way if the target moves the shots use the old positiong if they have already fired
        Vector3 target = mortarTarget + new Vector3(Random.Range(-accuracy, accuracy), shellHeight, Random.Range(-accuracy, accuracy));
        yield return new WaitForSeconds(timeToTarget);
        
        //GameObject shot = 
        Instantiate(projectile, target, Quaternion.LookRotation(Vector3.down));
    }

}
