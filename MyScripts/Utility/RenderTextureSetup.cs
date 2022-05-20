using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureSetup : MonoBehaviour
{

    /*This Script updates the render texture to be relative to the screen size so that different aspect ratios and resolutions will work just as well as 16:9
     * 
     * 
     * 
     * 
     * 
     */
    public Camera switcherCam;


    public Material switcherRenderMat;
    // Start is called before the first frame update
    void Start()
    {
        if(switcherCam.targetTexture != null)
        {
            switcherCam.targetTexture.Release();
        }
        switcherCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);

        switcherRenderMat.mainTexture = switcherCam.targetTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
