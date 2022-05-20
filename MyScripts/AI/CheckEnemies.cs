using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckEnemies : MonoBehaviour
{
    public GameObject winUI;

    // Start is called before the first frame update
    void Start()
    {
        winUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform Go = transform.GetChild(i);
            if (Go.GetComponent<Guard>() != null)
            {
                if (Go.GetComponent<Guard>().getDead())
                {
                    //Child is dead
                    count++;
                }
            }
        }
        if (count == transform.childCount)
        {
            //Debug.Log("all Enemies dead");
            winUI.SetActive(true);
            Time.timeScale = 0;
        }

    }
}
