using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public static WaterManager instance;
    private Material waterMat;

    //WaveStuff
    public float amplitude = 1f;
    public float length = 2f;
    public float speed = 1f;
    public float offset = 0;

    private float XInten = 0;
    private float ZInten = 0;

    private Vector2 meshOffset;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if(instance != this)
        {
            Debug.Log("TOO MANY WAHA");
            Destroy(this);
        }
        waterMat = GetComponent<MeshRenderer>().material;
        XInten = waterMat.GetFloat("x_wave_inten");
        ZInten = waterMat.GetFloat("z_wave_inten");

        //meshOffset = GlobalManager.Global.Player_XZ_Speed;

        Debug.Log("X: " + XInten + " Z: " + ZInten);
    }

    private void Update()
    {
        offset += Time.deltaTime * speed;

        meshOffset += new Vector2(CharacterControllerScript.instance.GetRealPlayerSpeed().x, CharacterControllerScript.instance.GetRealPlayerSpeed().y);

        waterMat.SetVector("P_Speed", meshOffset);
        //Debug.Log(meshOffset);
    }

    public float GetWaveHeight(float x, float z)
    {
        float y = 0;
        //X sine wave
        y = Mathf.Sin((x*XInten) / length + offset);
        //z sine wave
        y += Mathf.Sin((z*ZInten) / length + offset);
        y *= amplitude;
        return y;
    }
}
