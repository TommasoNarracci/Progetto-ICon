using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class muro_trigger : MonoBehaviour
{
    public MyAgent Agent;   //riferimento all'agente

    void OnTriggerEnter(Collider other)    //metodo che controlla se il muro indicatore è stato passato dall'agente,se si lo disattiva fino al prossimo episodio
    {                                      // (evita che l'agente faccia avanti e indietro nello stesso muro per prendere ricompense infinite)
        if (other.GetComponent<MyAgent>())
        {
            this.gameObject.SetActive(false);
            Agent.WallPassed();        //richiama metodo agente
        }
        


        
    }
    public void ReActivate()      //riattivazione muro indicatore
    {
        this.gameObject.SetActive(true);
    }




    // Start is called before the first frame update
    void Start()
    {
        Agent.OnEnvironmentReset += ReActivate;   //riattiva i muri disattivati ogni inizio episodio 

    }

    // Update is called once per frame
    void Update()
    {
       
        
    }
}
