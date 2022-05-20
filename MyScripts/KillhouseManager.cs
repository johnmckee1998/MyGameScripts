using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillhouseManager : MonoBehaviour
{
    public bool waitToClear; //if true, each room is only unlocked when the previous is cleared, if false then all doors are disabled and player can move freely
    [System.Serializable]
    public struct KillRoom
    {
        public KillhouseTarget[] targets;
        public KillhouseTarget[] noShootTarget; //used to score at the end
        public GameObject door;
    }

    public KillRoom[] rooms;
    private int currentRoom;
    private bool finished;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!finished)
            CheckRoom();
    }

    private void CheckRoom() //check if all targets for current room are hit
    {
        bool allhit = true;
        for(int i=0; i<rooms[currentRoom].targets.Length; i++)
        {
            if (!rooms[currentRoom].targets[i].hit)
            {
                allhit = false;
                return;
            }
        }
        //if it reaches here, then all targets are hit
        if (waitToClear)
            rooms[currentRoom].door.SetActive(false);
        if (currentRoom < rooms.Length - 1)
            currentRoom++;
        else
        {
            //all rooms cleared
            finished = true;
            Debug.Log("DONE!!!");
        }
    }

    public void ResetAllRooms()
    {
        for(int r =0; r<rooms.Length; r++)
        {
            for (int i = 0; i < rooms[r].targets.Length; i++)
                rooms[r].targets[i].ResetTarget();
            for (int i = 0; i < rooms[r].noShootTarget.Length; i++)
                rooms[r].noShootTarget[i].ResetTarget();
            if (waitToClear)
                rooms[r].door.SetActive(true);
        }
    }
}
