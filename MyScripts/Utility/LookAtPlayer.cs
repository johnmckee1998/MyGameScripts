using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            if(CharacterControllerScript.instance !=null )
                player = CharacterControllerScript.instance.transform;
        }
        else
        {
            transform.LookAt(player);
            transform.Rotate(0, 180f, 0); //flip y rot

            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);//undo x and z rot
        }
    }
}
