using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class PlayerMortarScript : MonoBehaviour
{
    public Transform mortarTarget;

    public Transform projSpawn;
    public GameObject projectile;
    [Tooltip("The radius around target that shells fall - the lower the more accurate")]
    public float accuracy = 5f;
    public float shellHeight = 100f;
    public float timeToTarget = 2f;
    public Vector2 minMaxRange = new Vector2(15f, 100f);

    public float fireRate = 15f; //randomised fire rate so groups of mortars dont always fire in sync every shot
    public float rotSpeed = 45f;
    public float targetMoveSpeed = 10f;


    public VisualEffect shotVFX;

    public AudioSource shotSFX;

    public bool dontShootInterRound = true;

    public Transform playerPosition;

    public bool disablePcam = true;

    public UnityEvent exitEvent;

    private float fireRatePerSec;
    private float lastShotTime;

    private bool rotating;

    private bool active = false;

    private Camera pCam;
    // Start is called before the first frame update
    void Start()
    {
        fireRatePerSec = fireRate/60f;

        mortarTarget.gameObject.SetActive(false);

        //pCam = CharacterControllerScript.instance.pCam.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        //UpdateRotation();
        if(active && Time.timeScale>0)
            DetectInputs();
    }

    private void FixedUpdate()
    {
        //if (!rotating && (!dontShootInterRound || TowerDefenceWaveManager.instance.WaveStatus()))
        //{
        //    float dist = Vector3.Distance(transform.position, mortarTarget);
        //    if (dist > minMaxRange.x && Vector3.Distance(transform.position, mortarTarget) < minMaxRange.y)
        //        Firing();
        //}
    }

    private void DetectInputs()
    {
        
        if (CharacterControllerScript.instance.health <= 0 || Input.GetButtonDown("Interact")) 
            Leave();
        //Rotation of mortar and z movement of target
        transform.eulerAngles += (Vector3.up*  Input.GetAxis("Mouse X") * rotSpeed);
        //transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotSpeed);
        mortarTarget.localPosition += Vector3.forward* Input.GetAxis("Mouse Y") * targetMoveSpeed;
        //Debug.Log("Rot: " + Vector3.up * Input.GetAxis("Mouse X") * rotSpeed);

        //clamp mortar target within min/max range
        Vector3 mortarTarPos = mortarTarget.localPosition;
        mortarTarPos.z = Mathf.Clamp(mortarTarPos.z, minMaxRange.x, minMaxRange.y);

        mortarTarget.localPosition = mortarTarPos;

        //firing input
        if (Input.GetButton("Fire1") && Time.timeScale > 0)
            Firing();


    }



    /*private void UpdateRotation()
    {
        Vector3 rotationTarget = Quaternion.LookRotation(transform.position - mortarTarget).eulerAngles;
        //only want yaw rotation
        rotationTarget.x = transform.eulerAngles.x;
        rotationTarget.z = transform.eulerAngles.z;


        rotating = (Mathf.Abs(rotationTarget.y - transform.eulerAngles.y) > 1f); //if more than 1 degree difference, count it as rotating

        transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, rotationTarget, Mathf.Deg2Rad * (rotSpeed * Time.deltaTime), 1.0f);
    }*/


    private void Firing()
    {
        if (lastShotTime < Time.time && Time.timeScale > 0)
        {
            if (shotSFX != null)
                shotSFX.Play();
            if (shotVFX != null)
                shotVFX.Play();
            StartCoroutine(SpawnShell(timeToTarget));

            
            lastShotTime = Time.time + 1f / fireRatePerSec;
        }
    }

    private IEnumerator SpawnShell(float time)
    {
        //calculate target at the start - this way if the target moves the shots use the old positiong if they have already fired
        Vector3 target = mortarTarget.position + new Vector3(Random.Range(-accuracy, accuracy), shellHeight, Random.Range(-accuracy, accuracy));
        yield return new WaitForSeconds(timeToTarget);

        //GameObject shot = 
        Instantiate(projectile, target, Quaternion.LookRotation(Vector3.down));
    }

    private void Leave()
    {
        active = false;
        pCam.enabled = true;
        mortarTarget.gameObject.SetActive(false);

        WeaponSelection.instance.gameObject.SetActive(true);

        CharacterControllerScript.Active = true;

        
        CharacterControllerScript.instance.transform.parent = null;

        CharacterControllerScript.instance.transform.position -= transform.forward;

        CameraMove.instance.Active = true;
        //CameraMove.instance.transform.parent.localEulerAngles = Vector3.zero;

        //if (disablePcam)
         //   CameraMove.instance.gameObject.SetActive(true);

        OverlayCameraScript.instance.gameObject.SetActive(true);

        CanvasScript.reticle.GetComponent<RectTransform>().localPosition = Vector3.zero;

        StartCoroutine(ExitInvoke());
    }

    private IEnumerator ExitInvoke()
    {
        yield return new WaitForSeconds(0.25f); //just adds a delay when leaving to prevent instantly entering 
        exitEvent?.Invoke();
    }

    public void Enter()
    {
        pCam = CharacterControllerScript.instance.pCam.GetComponentInChildren<Camera>();
        pCam.enabled = false;
        active = true;

        mortarTarget.gameObject.SetActive(true);

        WeaponSelection.instance.gameObject.SetActive(false);

        CharacterControllerScript.Active = false;

        CharacterControllerScript.isCrouching = false;

        
        CharacterControllerScript.instance.transform.parent = transform;

        CharacterControllerScript.instance.transform.position = playerPosition.position;
        CharacterControllerScript.instance.transform.rotation = playerPosition.rotation;

        //CameraMove.instance.ChangePitch(-CameraMove.instance.getCamRot().x);

        //if(disablePcam)
         //   CharacterControllerScript.instance.pCam.;

        CameraMove.instance.Active = false;

        //CameraMove.instance.transform.localEulerAngles = new Vector3(0, 180, 0);

        //pCam.fieldOfView = hipFov;

        OverlayCameraScript.instance.gameObject.SetActive(false);
    }
}
