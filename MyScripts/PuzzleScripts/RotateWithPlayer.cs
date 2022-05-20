using UnityEngine;

public class RotateWithPlayer : MonoBehaviour
{
    public float offset = 0;
    public bool randomiseOffset;
    public bool lockTo90Increments;
    public bool debug;
    // Update is called once per frame
    private void Start()
    {
        if (randomiseOffset)
            offset = Random.Range(0f, 360f);
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < 100)
            if (!lockTo90Increments)
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, CameraMove.instance.getCamRot().y + offset, transform.eulerAngles.z);
            else
            {
                float playerY = CameraMove.instance.getCamRot().y + offset;
                if (playerY > 360)
                    playerY -= 360;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, (Mathf.Round(playerY / 90f) * 90f), transform.eulerAngles.z); //Mathf.MoveTowards(transform.eulerAngles.y, (Mathf.Round(playerY / 90f) * 90f), 10f)
            }

        if(debug)
            Debug.Log(CameraMove.instance.getCamRot().y + " Y " + Mathf.Round((CameraMove.instance.getCamRot().y + offset) / 90f) * 90f + " Move: " + Mathf.MoveTowards(transform.eulerAngles.y, (Mathf.Round(CameraMove.instance.getCamRot().y / 90f) * 90f) +offset, 10f));

        //note
    }
}
