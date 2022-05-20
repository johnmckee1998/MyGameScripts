using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderScript : MonoBehaviour
{
    public LayerMask rayCastIgnore;
    public float climbSpeed = 10f;
    private bool inside;

    public bool vertical;

    private CharacterControllerScript player;
    private void FixedUpdate()
    {
        if (player == null)
            player = CharacterControllerScript.instance;

        if (inside)
            UpdatePlayer();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            inside = true;
            player.ignoreGravity = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            inside = false;
            player.ignoreGravity = false;
        }
    }

    private void UpdatePlayer()
    {
        //if (vertical && !CharacterControllerScript.characterController.isGrounded) //while not grouded, oppose gravity
        //    player.AddMove(new Vector3(0f, player.gravity *Time.fixedDeltaTime, 0f));
        Vector3 pMove = player.GetMoveDir();

        if (Mathf.Abs(player.GetMoveDir().x) >1.5f || Mathf.Abs(player.GetMoveDir().z) > 1.5f) //if trying to move
        {
            
            RaycastHit hit;

            // raycast in movedir, if moving towards ladder
            if (Physics.Raycast(player.transform.position, new Vector3(player.GetMoveDir().x, 0f, player.GetMoveDir().z), out hit, 1f, ~rayCastIgnore))
            {
                if (hit.transform.gameObject.Equals(gameObject)) 
                {
                    //Debug.Log("Angle " + (CameraMove.instance.GetPitch() / -90f));
                    pMove.y = climbSpeed * (CameraMove.instance.GetPitch()/-90f); //BETTER WAY - may climbspeed scale by look direction -> if looking up move up look down move down
                    //player.AddMove(new Vector3(0f, climbSpeed *Time.fixedDeltaTime*10f, 0f));
                }
                else
                {
                    pMove.y = 0f;
                }
            }
            else //not moving towards ladder
            {
                pMove.y = 0f;
                //player.AddMove(new Vector3(0f, -climbSpeed * Time.fixedDeltaTime, 0f));
            }

            
        }
        else
            pMove.y = 0f;
        player.SetMove(pMove);

    }
}
