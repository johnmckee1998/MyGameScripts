using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MotarScript : MonoBehaviour
{

    public GameObject bullet;

    public Transform BulletSpawnPoint;

    public float Spread = 0f;
    [Tooltip("Direct hit damage")]
    public float damage = 50f;
    public float range = 300f;
    public float bulletSpeed = 5f;

    public float turnRate = 10;
    private float nextTurn = 0f;

    public Material lookAtMat;
    private Material defaultMat;

    private AudioSource[] sounds;
    private AudioSource fireSound;

    private AudioSource clickSound;

    private Transform player;

    private float xRot;
    private float xRotTracker;
    private float yRot;
    private float yRotTracker;

    private TextMeshPro[] dTexts;
    private TextMeshPro XdegreeText;
    private TextMeshPro YdegreeText;

    // Start is called before the first frame update
    void Start()
    {
        sounds = GetComponents<AudioSource>();

        fireSound = sounds[0];
        clickSound = sounds[1];

        defaultMat = gameObject.GetComponent<Renderer>().material;

        player = FindObjectOfType<CharacterControllerScript>().transform;
        

        dTexts = GetComponentsInChildren<TextMeshPro>();

        XdegreeText = dTexts[0];
        YdegreeText = dTexts[1];

        transform.localEulerAngles = new Vector3(5 , 0, transform.eulerAngles.z);
        xRot = transform.localEulerAngles.x;
        yRot = transform.localEulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        XdegreeText.text = xRot.ToString();
        YdegreeText.text = yRot.ToString();
    }

    private void OnMouseOver()
    {
        xRotTracker = xRot;
        yRotTracker = yRot;
        if (Vector3.Distance(transform.position, player.position) < 2.0f) {

            gameObject.GetComponent<Renderer>().material = lookAtMat;

            if (Input.GetMouseButtonDown(0)) //Left Click
            {

            }
            else if (Input.GetMouseButtonDown(1)) //Right Click
            {
                //AngleMortar
            }
            else if (Input.GetMouseButtonDown(2) && Time.timeScale>0) //Middle Mouse //single shot
            {
                FireMortar();
            }
            else if (Input.GetKey("j") && Time.timeScale > 0) //crazy fast fire
            {
                FireMortar();
            }
            //Turning
            else if (Input.GetKey("i") && Time.time >= nextTurn && Time.timeScale > 0)
            {
                nextTurn = Time.time + 1.0f / turnRate;
                xRot++;
            }
            else if (Input.GetKey("o") && Time.time >= nextTurn && Time.timeScale > 0)
            {
                nextTurn = Time.time + 1.0f / turnRate;
                xRot--;
            }
            else if (Input.GetKey("k") && Time.time >= nextTurn && Time.timeScale > 0)
            {
                nextTurn = Time.time + 1.0f / turnRate;
                yRot--;
            }
            else if (Input.GetKey("l") && Time.time >= nextTurn && Time.timeScale > 0)
            {
                nextTurn = Time.time + 1.0f / turnRate;
                yRot++;
            }
            xRot = Mathf.Clamp(xRot, 5f, 50f);
            yRot = Mathf.Clamp(yRot, -45f, 45f);

            if(xRot != xRotTracker || yRot != yRotTracker) //if the value is changed 
                clickSound.PlayOneShot(clickSound.clip, clickSound.volume); //play click sound

            transform.localEulerAngles = new Vector3(xRot, yRot, transform.eulerAngles.z);
            //Old Rotation method
            /*else if (Input.GetAxis("Mouse ScrollWheel") > 0f) //Middle Mouse
            {
                //transform.rotation = new Quaternion(transform.rotation.x + 1, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                transform.eulerAngles += transform.right;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) //Middle Mouse
            {
                //transform.rotation = new Quaternion(transform.rotation.x - 1, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                transform.eulerAngles -= transform.right;
            }*/
        }
    }

    private void OnMouseExit()
    {
        gameObject.GetComponent<Renderer>().material = defaultMat;
    }

    private void FireMortar()
    {
        fireSound.PlayOneShot(fireSound.clip, fireSound.volume);

        GameObject bul = Instantiate(bullet, (BulletSpawnPoint.position/* - (0.5f * transform.forward)*/), new Quaternion(0, 0, 0, 0)/*Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z)*/);
        float ranX;
        float ranY;
        float ranZ;

        ranX = Random.Range(-Spread, Spread);
        ranY = Random.Range(-Spread, Spread);
        ranZ = Random.Range(-Spread, Spread);

        GrenadeRoundScript bulScript = bul.GetComponent<GrenadeRoundScript>();

        bulScript.SetTravelDir((transform.up + new Vector3(ranX, ranY, ranZ)) * bulletSpeed);
        bulScript.damage = damage;
        bulScript.range = range;
    }
}
