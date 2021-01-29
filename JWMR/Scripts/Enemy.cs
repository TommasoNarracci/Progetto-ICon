using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public int lifepoints = 100;
    public int currentLP;
    public Vector3 StartPos;    //posizione di partenza
    private float randomRangeX_Neg = -7f;       //vari range per l'area di respawn random 
    private float randomRangeX_Pos = 7f;
    private float randomRangeZ_Neg = -7f;
    private float randomRangeZ_Pos = 7f;
    private int startRange = 80;     //range di attivazione inseguimento dell'agente
    private float speed = 7f;        //velocità di movimento
    public EnvironmentParameters environmentParameters;    //parametri di ambiente
    public MyAgent Agent;         //riferimento all'agente
    private NavMeshAgent navAgent;     //componente per l'inseguimento automatico 
    public void getShot(int damage, MyAgent shooter)  //l'Enemy è stato colpito
    {
        ApplyDamage(damage, shooter);
    }

    private void ApplyDamage(int damage, MyAgent shooter)   //Applicazione danno
    {
        currentLP -= damage;
        if (currentLP <= 0)
        {
            Die(shooter);
        }
    }
    private void Die(MyAgent shooter)     //Enemy sconfitto
    {
        shooter.KillCount();
        Debug.Log("Morto");
        this.gameObject.SetActive(false);
    }
    private void Respawn()      //respawn dell'Enemy in una posizione random nel suo range
    { 
        this.gameObject.SetActive(true);   //reset vari parametri
        currentLP = lifepoints;
        navAgent.speed = speed;
        transform.position = new Vector3(StartPos.x + Random.Range(randomRangeX_Neg, randomRangeX_Pos), StartPos.y, StartPos.z + Random.Range(randomRangeZ_Neg, randomRangeZ_Pos));
    }
    public void FixedUpdate()    //metodo chiamato ad ogni step
    {
        if(Vector3.Distance(this.transform.position, Agent.transform.position) < startRange)   //controllo se l'agente è nel range,attiva inseguimento
        {                                                                                      //se fuori range disattiva inseguimento
            this.GetComponent<NavMeshAgent>().enabled = true;
            navAgent.SetDestination(Agent.transform.position);   //riferimento posizione di destinazione
        }
        else
        {
            this.GetComponent<NavMeshAgent>().enabled = false;
        }
        
    }
    #region Debug

    /*public void OnMouseDown()
    {
        getShot(lifepoints);
    }*/
    #endregion
    // Start is called before the first frame update
    void Start()  //metodo che parte all'avvio del programma
    {
        StartPos = transform.position; //impostazioni di vari parametri Enemy e di ambiente
        currentLP = lifepoints;
        environmentParameters = Academy.Instance.EnvironmentParameters;
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = speed;
        Agent.OnEnvironmentReset += Respawn;  //ogni volta che riparte un nuovo episodio respawna i nemici
    }
    // Update is called once per frame
    void Update()
    {

    }
}
