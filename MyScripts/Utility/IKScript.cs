using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class IKScript : MonoBehaviour
{
    
    public int ChainLength = 2;

    public Transform target;
    public Transform Pole; //influences pivot

    public int Iterations = 10; //solver iterations per update
    public float Delta = 0.001f;//distance when solver stops
    [Range(0, 1)]
    public float SnapBackStrength = 1f;

    protected float[] BonesLength;
    protected float CompleteLength;
    protected Transform[] Bones;
    protected Vector3[] Positions;
    //rotate shit fucking crap bitch ass cunt
    protected Vector3[] StartDirSucc; //start direction successor
    protected Quaternion[] StartRotBone;
    protected Quaternion StartRotTarget;
    protected Quaternion StartRotRoot;

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        Bones = new Transform[ChainLength + 1];
        Positions = new Vector3[ChainLength + 1];
        BonesLength = new float[ChainLength];

        StartDirSucc = new Vector3[ChainLength + 1];
        StartRotBone = new Quaternion[ChainLength + 1];

        if (target == null)
        {
            target = new GameObject(gameObject.name + " Target").transform;
            target.position = transform.position;
        }
        StartRotTarget = target.rotation;

        CompleteLength = 0;

        var current = transform;
        for(var i = Bones.Length -1; i>=0; i--)
        {
            Bones[i] = current;
            StartRotBone[i] = current.rotation;

            if(i == Bones.Length - 1)
            {
                //leaf
                StartDirSucc[i] = target.position - current.position;
            }
            else
            {
                //mid bone
                StartDirSucc[i] = Bones[i + 1].position - current.position;
                BonesLength[i] = (Bones[i + 1].position - current.position).magnitude;
                CompleteLength += BonesLength[i];
            }

            current = current.parent;
        }
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void ResolveIK()
    {
        if (target == null)
            return;
        else if (BonesLength.Length != ChainLength)
            Init();

        for(int i=0; i<Bones.Length; i++) //get pos
        {
            Positions[i] = Bones[i].position;
        }


        var RootRot = (Bones[0].parent != null) ? Bones[0].parent.rotation : Quaternion.identity;
        var RootRotDiff = RootRot * Quaternion.Inverse(StartRotRoot);

        //Calculate
        if ((target.position - Bones[0].position).sqrMagnitude >= CompleteLength * CompleteLength)
        {
            //stretch it
            var direction = (target.position - Positions[0]).normalized;
            //set everything after root
            for(int i =1; i<Positions.Length; i++)
            {
                Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
            }
        }
        else
        {
            for (int i = 0; i < Positions.Length-1; i ++) //was pos.length, added -1 coz out of bounds error
            {
                Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + RootRotDiff * StartDirSucc[i], SnapBackStrength);
            }


            for(int iteration =0; iteration < Iterations; iteration++)
            {
                //backward loop
                for(int i = Positions.Length-1; i>0; i--) //not using i>=0 coz you leave root alone
                {
                    if (i == Positions.Length - 1)
                        Positions[i] = target.position; //set end to target pos
                    else
                        Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i];
                }

                //forward loop - both are used to improve accuracy
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                //Stop if close enough
                if ((Positions[Positions.Length - 1] - target.position).sqrMagnitude < Delta * Delta) //sqr magnitude and delta*delta is used coz magnitude uses srqroot and its faster to just sqr delta
                    break;
            }
        }

        //move towards pole
        if (Pole != null)
        {
            for(int i =1; i<Positions.Length-1; i++)
            {
                var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
            }
        }


        for(int j=0; j<Positions.Length; j++) //set pos
        {
            if (j == Positions.Length - 1)
                Bones[j].rotation = target.rotation * Quaternion.Inverse(StartRotTarget) * StartRotBone[j];
            else
                Bones[j].rotation = Quaternion.FromToRotation(StartDirSucc[j], Positions[j + 1] - Positions[j]) * StartRotBone[j];

            Bones[j].position = Positions[j];
        }
    }

    /*
    private void OnDrawGizmos()
    {
        var current = this.transform;
        for(int i =0; i<ChainLength && current!=null && current.parent !=null; i++)
        {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            

            current = current.parent;
        }
    }
    */
}
