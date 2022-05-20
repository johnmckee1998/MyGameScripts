using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePositionRotation : MonoBehaviour
{
    public float switchTime= 5f;
    public float moveTime = 0.25f;
    public AudioSource switchSound;

    public enum SwitchType { Position, Rotation, Both };
    [System.Serializable]
    public struct SwitchObject
    {
        public Transform obj;
        public SwitchType switchType;
        public Vector3 switchPosInfo;
        public Vector3 switchRotInfo;
        [HideInInspector]
        public Vector3 startPos;
        [HideInInspector]
        public Vector3 startRot;
        [HideInInspector]
        public Vector3 smoothref;
    }

    public SwitchObject[] objects;

    private float timer=0; //times the time between switches
    private float switchTimer; //times the actual switch motion
    private bool startPos = true; //used to track if object is in start pos/rot or switchpos/rot
    public bool switched;//simply used to track wehn switch happens so sound is only played once

    private Vector3 smoothRef;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].startPos = objects[i].obj.localPosition;
            objects[i].startRot = objects[i].obj.localEulerAngles;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timer >= switchTime)
        {
            
            if (switchSound != null && !switched)
                switchSound.Play();

            if (!switched)
            {
                switchTimer = moveTime * 1.1f;
                switched = true;
            }
            //Debug.Log(startPos +  " Switch! " + switchTimer);

            for (int i =0; i<objects.Length; i++)
            {
                if(objects[i].switchType == SwitchType.Position || objects[i].switchType == SwitchType.Both)
                {
                    if (startPos)//if in startpos, lerp to switchpos
                    {
                        objects[i].obj.localPosition = Vector3.SmoothDamp(objects[i].obj.localPosition, objects[i].switchPosInfo, ref objects[i].smoothref, moveTime);
                    }
                    else //if in switchpos, lerp to startpos
                    {
                        objects[i].obj.localPosition = Vector3.SmoothDamp(objects[i].obj.localPosition, objects[i].startPos, ref objects[i].smoothref, moveTime);
                    }
                }
                if (objects[i].switchType == SwitchType.Rotation || objects[i].switchType == SwitchType.Both)
                {
                    if (startPos)//if in startpos, lerp to switchpos
                    {
                        //objects[i].obj.localEulerAngles = Vector3.SmoothDamp(objects[i].obj.localEulerAngles, objects[i].switchRotInfo, ref objects[i].smoothref, moveTime);
                        objects[i].obj.localEulerAngles = Vector3.MoveTowards(objects[i].obj.localEulerAngles, objects[i].switchRotInfo, 10f);
                    }
                    else //if in switchpos, lerp to startpos
                    {
                        //objects[i].obj.localEulerAngles = Vector3.SmoothDamp(objects[i].obj.localEulerAngles, objects[i].startRot, ref objects[i].smoothref, moveTime);
                        objects[i].obj.localEulerAngles = Vector3.MoveTowards(objects[i].obj.localEulerAngles, objects[i].startRot, 10f);
                    }
                }
            }

            if (switchTimer <= 0)
            {
                timer = 0;
                switched = false;
                startPos = !startPos;
            }
            else
                switchTimer -= Time.fixedDeltaTime;
        }
        else //
            timer += Time.fixedDeltaTime;
    }
}
