using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using UnityEngine.AI;

public class MyAgent : Agent
{
    public int score = 0;  //semplice variabile usata per vari controlli(es.numero muri passati)
    public Transform shotpoint;  //punto da dove partirà il raggio dello sparo
    public int timeBetweenShots = 50;  
    public int damage = 100;
    public bool shotReady = true;   
    public int shotwait = 0;    //usato come timer per regolare tempo tra 2 colpi
    public Vector3 startPos;    //posizione start dell'oggetto
    public Rigidbody RB;        
    private EnvironmentParameters EnvironmentParameters;  //parametri di ambiente mlagents 
    public event Action OnEnvironmentReset;               //e reset
    private float timeRemaining = 100f;   //tempo massimo dato all'agente per trovare il prossimo muro indicatore
    private float wallTouched = 10f;      //tempo massimo in cui l'agente può toccare un muro del labirinto(per evitare che resti incastrato)
    public GameObject finale;             //punto di arrivo del labirinto   
    public Projectile projectile;         //proiettile sparo
    public void Shoot()     //funzione che si occupa dello sparo
    {
        if (!shotReady)
        {
            return;
        }
        var direction = transform.forward;
        var layerMask = 1 << LayerMask.NameToLayer("Enemy");   //terrà in considerazione solamente elementi in questo layer nel raggio
        //Debug.Log("Sparo");
        //Debug.DrawRay(shotpoint.position, direction * 40f, Color.green, 2f);   //disegno del raggio in modo da essere visibile
        Projectile spawnedProjectile =(Projectile) Instantiate(projectile, shotpoint.position, Quaternion.Euler(0f, -90f, 0f));
        spawnedProjectile.SetDirection(direction,spawnedProjectile);
        if (Physics.Raycast(shotpoint.position, direction, out var hit, 40f, layerMask))   //controllo se il colpo è andato a segno o no
        {
            Debug.Log("Colpito");
            hit.transform.GetComponent<Enemy>().getShot(damage, this);    //richiama una funzione in Enemy
        }
        else
        {
            AddReward(-0.05f);    //colpo mancato,penalità
        }
        shotwait = timeBetweenShots;       //reimpostazione tempi per il prossimo sparo
        shotReady = false;
    }
    
    
    
    public void KillCount()
    {
        AddReward(0.5f);  //nemico colpito,ricompensa
    }
    
    
    
    public void FixedUpdate()   //metodo chiamato ad ogni step 
    {
        if (!shotReady)        //controllo timer sparo
        {
            shotwait--;
            if (shotwait <= 0)
            {
                shotReady = true;
            }
        }

        if (timeRemaining > 0)  //controllo tempo che ci mette a passare attraverso un altro muro indicatore
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            AddReward(-0.4f);       //tempo scaduto,penalità e fine episodio
            Debug.Log("Time has run out!");
            timeRemaining = 0;        
            EndEpisode();
        }
        AddReward(-1f / MaxStep);   
    }
    public override void OnActionReceived(float[] vectorAction)    //metodo che effettua azione in base agli input dati dal decisionrequester
    {
        if (Mathf.RoundToInt(vectorAction[0]) >= 1)
        {
            Shoot();
        }
        MoveAgent(vectorAction);

    }

    public void MoveAgent(float[] act)     //metodo per movimento dell'agente
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[1]);
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        RB.AddForce(dirToGo * 0.5f, ForceMode.VelocityChange);
    }

    public override void CollectObservations(VectorSensor sensor)  //metodo che collezione le osservazioni necessarie(da aggiungere a quelle prese in
    {                                                              //automatico dai RayPerceptionSensor
        sensor.AddObservation(shotReady);                          //osservazioni su colpo pronto e direzione dell'agente
        sensor.AddObservation(transform.InverseTransformDirection(RB.velocity));
       
    }
    public override void Initialize()      //metodo che parte all'inizio dell'esecuzione del programma
    {        
        startPos = transform.position;
        RB = GetComponent<Rigidbody>();
        EnvironmentParameters = Academy.Instance.EnvironmentParameters;
        RB.freezeRotation = true;       
    }
    public override void Heuristic(float[] actionsOut)     //metodo che permette di prendere azioni,il decisionrequester darà degli input poi tradotti
    {                                                      //in valori discreti
        actionsOut[0] = Input.GetKey(KeyCode.P) ? 1f : 0f;
        actionsOut[1] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            actionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[1] = 2;
        }

    }
    public override void OnEpisodeBegin()           //metodo che parte all'inizio di ogni episodio
    {
        score = 0;       //reset contatori e timer
        timeRemaining = 100f;
        wallTouched = 10f;
        OnEnvironmentReset.Invoke();   //reset parametri ambiente
        Debug.Log("Inizio Episodio");
        //timeBetweenShots = Mathf.FloorToInt(EnvironmentParameters.GetWithDefault("shootingFrequency", 30f));
        transform.position = startPos;   //reset posizioni,velocità e colpo pronto
        RB.velocity = Vector3.zero;
        shotReady = true;
    }
    
    private void OnCollisionEnter(Collision other)       //metodo che gestisce le collisioni 
    {
        if (other.gameObject.CompareTag("Enemy"))          //se un enemy tocca l'agente penalità e fine episodio
        {
            Debug.Log("Beccato");
            AddReward(-1f);
            EndEpisode();
        }
        else
        {
            if (other.gameObject.CompareTag("Terrain"))  //se invece tocca i muri del labirinto scatta il timer di muro toccato
            {                                            //se timer scaduto,penalità e fine episodio 
                wallTouched -= Time.deltaTime*5;
                //Debug.Log(wallTouched);
                AddReward(-0.0025f);
                if(wallTouched <= 0)
                {
                    Debug.Log("Muroooo");
                    AddReward(-0.4f);
                    EndEpisode();
                }
            }
        }
    }
    public void EndLab()      //Labirinto finito,ricompensa e fine episodio
    {
        AddReward(1f);
        Debug.Log("LABIRINTO FINITO!");
        EndEpisode();
    }
    public void WallPassed()      //muro indicatore passato,ricompensa e reset dei timer
    {
        score++;
        Debug.Log("Passato" + score);
        AddReward(0.2f);
        timeRemaining = 100f;
        wallTouched = 10f;
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
