using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finale : MonoBehaviour
{
    public MyAgent Agent;   //riferimento all'agente

    void OnTriggerEnter(Collider other)   //script applicato al muro indicatore di arrivo,richiama il metodo di fine labirinto in agente
    {
        if (other.GetComponent<MyAgent>())
        {
            Agent.EndLab();
        }




    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
