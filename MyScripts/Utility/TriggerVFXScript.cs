using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TriggerVFXScript : MonoBehaviour
{
    public VisualEffect effect;
    public KeyCode triggerKey;
    public GameObject player;
    public float triggerDist = 5;
    public Material hoverMat;

    private Material defaultMat;
    private Renderer render;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<CharacterControllerScript>().gameObject;

        render = GetComponent<Renderer>();
        defaultMat = render.material;
    }

    private void OnMouseOver()
    {
        if(Vector3.Distance(transform.position, player.transform.position) <= triggerDist)
        {
            render.material = hoverMat;

            if (Input.GetKeyDown(triggerKey))
                effect.Play();
        }
    }

    private void OnMouseExit()
    {
        render.material = defaultMat;
    }

}
