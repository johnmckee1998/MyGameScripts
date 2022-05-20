using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightMovement : MonoBehaviour
{//NEED TO ADD A METHOD FOR LOOKING AT PLAYER ******************************************************
    public Transform pitchObject;

    public float rotationRate;
    public Vector2 waitTime;
    public bool useLimits;
    public Vector2 yawMinMaxRotation;
    public Vector2 pitchMinMaxRotation;


    private Vector3 yawDest;
    private Vector3 pitchDest;

    private bool waiting;

    private Vector3 yawRotRef;
    private Vector3 pitchRotRef;

    private float yawRate;
    private float pitchRate;

    private float timer;

    [Space]
    public bool connectToDrone;
    public LargeDroneAI drone;
    public Light spotlight;
    private bool seePlayer;
    private Color lightRGB;


    private Vector3 yawStartRot;
    private Vector3 pitchStartRot;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RandomRotate());

        lightRGB.r = spotlight.color.r;
        lightRGB.g = spotlight.color.g;
        lightRGB.b = spotlight.color.b;
        lightRGB.a = spotlight.color.a;


        yawStartRot = transform.localEulerAngles;
        pitchStartRot = pitchObject.localEulerAngles;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (false)
        {
            if (!waiting && CheckRotation())
            {
                NewRotationPoints();

                StartCoroutine(WaitToRotate());
            }

            if (!waiting)
                Rotate();
        }

        if (connectToDrone)
            UpdateLight();

    }

    private bool CheckRotation() //check if each rotation is within +- 0.5 degrees
    {
        if (transform.localEulerAngles.y > yawDest.y + 0.5f || transform.localEulerAngles.y < yawDest.y - 0.5f)
            return false;
        if (pitchObject.localEulerAngles.x > pitchDest.y + 0.5f || pitchObject.localEulerAngles.x < pitchDest.y - 0.5f)
            return false;
        return true;
    }

    private void NewRotationPoints()
    {
        yawDest = new Vector3(transform.localEulerAngles.x, Random.Range(yawMinMaxRotation.x, yawMinMaxRotation.y), transform.localEulerAngles.z);
        pitchDest = new Vector3(pitchObject.localEulerAngles.x, Random.Range(pitchMinMaxRotation.x, pitchMinMaxRotation.y), pitchObject.localEulerAngles.z);
    }

    private void Rotate()
    {
        transform.localEulerAngles = Vector3.SmoothDamp(transform.localEulerAngles, yawDest, ref yawRotRef, 2f, rotationRate, Time.fixedDeltaTime);
        pitchObject.localEulerAngles = Vector3.SmoothDamp(pitchObject.localEulerAngles, pitchDest, ref pitchRotRef, 2f, rotationRate, Time.fixedDeltaTime);
    }

    IEnumerator WaitToRotate()
    {
        waiting = true;

        yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));

        waiting = false;
    }

    IEnumerator RandomRotate()
    {
        timer += Random.Range(waitTime.x, waitTime.y) * 2f;
        yawRate = Random.Range(-1f, 1f);
        pitchRate = Random.Range(-1f, 1f);
        while (true)
        {
            
            if (CheckRandRotate())
            {
                timer -= Time.fixedDeltaTime;
                transform.localEulerAngles += Vector3.up * yawRate * rotationRate * Time.fixedDeltaTime;
                pitchObject.localEulerAngles += Vector3.right * pitchRate * rotationRate * Time.fixedDeltaTime;

                if (useLimits)
                    ClampRotation();
                if (timer > 0)
                    yield return new WaitForFixedUpdate();
                else
                {
                    yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));
                    timer += Random.Range(waitTime.x, waitTime.y) * 2f;
                    yawRate = Random.Range(0.2f, 1f) * (Random.Range(0, 2) * 2 - 1); //random between 0.2 and 1, then randomly make it pos or neg - this method is better than ran between -1 and 1 coz that can return 0 or near zero which isnt great
                    pitchRate = Random.Range(0.2f, 1f) * (Random.Range(0, 2) * 2 - 1);
                }
            }
            else
            {
                /*
                Debug.Log("Looking at player");
                //look at player
                Vector3 yawDir = (CharacterControllerScript.instance.transform.position - transform.position);
                Vector3 pitchDir = (CharacterControllerScript.instance.transform.position - pitchObject.position);

                Vector3 yawLookRot = Quaternion.LookRotation(yawDir.normalized).eulerAngles;

                yawLookRot.x = transform.eulerAngles.x; //undo x and z rot coz yaw is only y
                yawLookRot.z = transform.eulerAngles.z;

                Vector3 pitchLookRot = Quaternion.LookRotation(pitchDir.normalized).eulerAngles;

                pitchLookRot.y = transform.eulerAngles.y; //undo y and z rot coz pitch is only x
                pitchLookRot.z = transform.eulerAngles.z;


                transform.eulerAngles = Vector3.MoveTowards(transform.eulerAngles, yawLookRot, rotationRate * Time.fixedDeltaTime * 5f);
                pitchObject.eulerAngles = Vector3.MoveTowards(pitchObject.eulerAngles, pitchLookRot, rotationRate * Time.fixedDeltaTime * 5f);
                */
                transform.LookAt(CharacterControllerScript.instance.transform.position);
                pitchObject.LookAt(CharacterControllerScript.instance.transform.position);



                if (useLimits)
                    ClampRotation();

                yield return new WaitForFixedUpdate();
            }
        }
    }

    private void ClampRotation()
    {
        float yaw = transform.localEulerAngles.y;
        float pitch = pitchObject.localEulerAngles.x;

        if (pitch > 180) //shouldnt realistically pass 90 degree rotation unless going negative
            pitch -= 360f;
        if (yaw > 180) //shouldnt realistically pass 90 degree rotation unless going negative
            yaw -= 360f;

        pitch = Mathf.Clamp(pitch, pitchMinMaxRotation.x, pitchMinMaxRotation.y);
        yaw = Mathf.Clamp(yaw, yawMinMaxRotation.x, yawMinMaxRotation.y);
        
        //convert back to normal if was negative
        if (pitch < 0)
            pitch += 360;
        if (yaw < 0)
            yaw += 360;


        transform.localEulerAngles = new Vector3(yawStartRot.x, yaw , yawStartRot.z);
        pitchObject.localEulerAngles = new Vector3(pitch, pitchStartRot.y, pitchStartRot.z);

    }

    private bool CheckRandRotate() //checks if the spotlight should rotate randomly - basically checks if either there is no connected drone, or if the drone detection is less than 100
    {
        if (!connectToDrone || !seePlayer)
            return true;

        if (drone.GetDetection() < 50)
            return true;

        return false; // detect must be >90
    }

    public void SeePlayer(bool b)
    {
        seePlayer = b;
    }

    private void UpdateLight()
    {
        Color tempLight = new Color();
        float detect = drone.GetDetection();
        detect = Mathf.Clamp(detect, 0f, 100f);
        tempLight.r = Mathf.Lerp(lightRGB.r, 255f, detect / 100f);
        tempLight.g = Mathf.Lerp(0f, lightRGB.g, 1f-(detect / 100f));
        tempLight.b = Mathf.Lerp(0f, lightRGB.b, 1f-(detect / 100f));
        tempLight.a = spotlight.color.a;

        if (!seePlayer)
            detect = 0f; //better way would be to record detect and then slowly reduce it when not seePlayer
        //spotlight.color = tempLight;
        spotlight.color = Color.Lerp(lightRGB, Color.red, detect / 100f);
    }
}
