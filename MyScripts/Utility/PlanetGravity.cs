using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{

    Vector3 PlanetCentre;
    //Rigidbody playerRB;
    //GameObject playerGO;
    public float Gravity;

    private float pGrav = 0;
    // Start is called before the first frame update
    void Start()
    {
        PlanetCentre = transform.position;
        //playerRB = GameObject.Find("Player").GetComponent<Rigidbody>();
        //playerGO = GameObject.Find("Player");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //Disable normal player gravity
            //pGrav =  other.GetComponent<CharacterControllerScript>().gravity;
            //other.GetComponent<CharacterControllerScript>().gravity = 0;
            other.GetComponent<CharacterControllerScriptRigidBody>().useSphereGrav = true;
            other.GetComponent<CharacterControllerScriptRigidBody>().PlanetCentre = PlanetCentre;
            
            Debug.Log("Active");
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Enable normal player gravity
            //other.GetComponent<CharacterControllerScript>().gravity = pGrav;
            other.GetComponent<CharacterControllerScriptRigidBody>().useSphereGrav = false;
            Debug.Log("Deactivated");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //Debug.Log("Entered Atmosphere");
            //playerRB.AddForce((PlanetCentre - playerGO.transform.position).normalized * Gravity);

            //other.gameObject.GetComponent<Rigidbody>().AddForce((PlanetCentre - other.gameObject.transform.position).normalized * Gravity * Time.deltaTime); //Accelerates towards center - gravity
            
            //other.gameObject.transform.rotation = Quaternion.LookRotation(PlanetCentre - other.gameObject.transform.position, other.gameObject.transform.up); //causes rotation so the object is upright
        }
    }
}
