using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingScript : MonoBehaviour
{
    public GameObject endText;
    private bool ended = false;

    [Tooltip("If the player needs to activate something to complete level")]
    public bool activated = false;
    private bool act;

    // Start is called before the first frame update
    void Start()
    {
        endText.SetActive(false);
        act = !activated;
    }

    // Update is called once per frame
    void Update()
    {
        if(ended)
            Time.timeScale = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (act)
        {
            endText.SetActive(true);
            Time.timeScale = 0f;
            ended = true;
        }
    }

    public void setAct(bool b)
    {
        act = b;
    }

}
