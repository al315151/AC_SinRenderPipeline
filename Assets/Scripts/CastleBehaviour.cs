using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleBehaviour : MonoBehaviour
{

    float castleLife = 1250f;
    GameObject[] objectiveDoors;
    int[] indexDoors;

    public GameObject squad_GO;

    //Squad Management
    [HideInInspector]
    public int numberOfSquadsAvailable;
    float numberOfReserveSquads = 0.0f;
    List<SquadBehaviour> squadsAssigned;
    float dispatchTimer = 0.0f;

    public GameObject[] spawnerLocations;
    [HideInInspector]
    public bool allSquadsDefeated;

    [Header("Defense References")]
    public EnemyTurretBehaviour[] defenseTurrets;

    float contactTimer = 0.0f;
    float influenceArea = 90f;

    [Header("UI References")]
    public Canvas castle_Canvas;
    public Slider CastleLife_Slider;
    public Image CastleLife_SliderFill_Image;

    // Start is called before the first frame update
    void Start()
    {
        squadsAssigned = new List<SquadBehaviour>();
        indexDoors = new int[2];
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.currentInstance != null && WaveManager.currentInstance.gameStarted &&
            castle_Canvas.worldCamera == null)
        {
            castle_Canvas.worldCamera =
            WaveManager.currentInstance.player_Reference_GO.GetComponent<Camera>();
            CastleLife_Slider.maxValue = castleLife;
            CastleLife_Slider.value = castleLife;
            FindClosestDoors();
            // print(name + " closest doors: " + objectiveDoors[0].name
            //       + " , " + objectiveDoors[1].name);
        }
                     
        if (WaveManager.currentInstance != null && WaveManager.currentInstance.gameStarted)
        {
            for (int i = 0; i < squadsAssigned.Count; i++)
            {
                if (squadsAssigned[i] == null)
                { squadsAssigned.RemoveAt(i); }
            }  
            
            if (defenseTurrets.Length != 0)
            { PlayerInDistance(); }

            if (numberOfSquadsAvailable > 0)
            {
                dispatchTimer += Time.deltaTime;
                if (dispatchTimer > 4.0f)
                {
                    print("Dispatching new squad");
                    DispatchSquads();
                    dispatchTimer = 0.0f;
                }
            }

            if (squadsAssigned.Count == 0)
            {  allSquadsDefeated = true;        }
            else { allSquadsDefeated = false;   }

        }

        ChangeColorCastleLife();
    }

    public void ReceiveDamage(float damage)
    {
        castleLife -= damage;
        if (castleLife < 0.0f)
        {
            WaveManager.currentInstance.CreateCastleParticles(transform.position);
            SaveSquadsFromDestruction();
            Destroy(this.gameObject);

        }
    }

    void PlayerInDistance()
    {
        GameObject player = WaveManager.currentInstance.player_Reference_GO;       
        for (int i = 0; i < defenseTurrets.Length; i++)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < influenceArea)
            {
                defenseTurrets[i].GetComponent<EnemyTurretBehaviour>().ShutOnTurret();
                contactTimer += Time.deltaTime;
                
                if (contactTimer > 10.0f )
                {
                    if (squadsAssigned != null && squadsAssigned[0].castleObjectiveOverride == false)
                    {
                        SquadDefenseMode();
                        print("Modo Defensa Castillo activado");
                    }
                    contactTimer = 0.0f;
                }
            }
            else
            {
                defenseTurrets[i].GetComponent<EnemyTurretBehaviour>().ShutOffTurret();
            }
        }
        //print(Vector3.Distance(player.transform.position, transform.position));


    }
    
    void ChangeColorCastleLife()
    {
        castle_Canvas.gameObject.transform.LookAt(WaveManager.currentInstance.player_Reference_GO.transform.position);

        CastleLife_Slider.value = castleLife;
        if (castleLife < CastleLife_Slider.maxValue * 0.4f)
        { CastleLife_SliderFill_Image.color = 
                new Color(1.0f, 0.0f, 0.0f, 0.7f); }
        else if (castleLife < CastleLife_Slider.maxValue * 0.7f)
        { CastleLife_SliderFill_Image.color = 
            new Color(1.0f, 0.92f, 0.016f, 0.7f);  }
        else
        { CastleLife_SliderFill_Image.color =
                new Color(0.0f, 1.0f, 0.0f, 0.7f); }
       
    }

    void FindClosestDoors()
    {
        objectiveDoors = new GameObject[2];

        GameObject[] doors = WaveManager.currentInstance.DoorPositions;
        float[] doorsDistance = new float[doors.Length];

        for (int i = 0; i < doors.Length; i++)
        {  doorsDistance[i] = Vector3.Distance(transform.position, doors[i].transform.position);    }

        int minIndex = 0;
        for (int i = 1; i < doors.Length; i++)
        {  if (doorsDistance[i] < doorsDistance[minIndex])
            {   minIndex = i;    }
        }
        objectiveDoors[0] = doors[minIndex];
        indexDoors[0] = minIndex;

        int second = -1;
        for (int i = 0; i < doors.Length; i++)
        {  if (i != minIndex)
            {
                if (second == -1)
                {   second = i;   }
                else if (doorsDistance[i] < doorsDistance[second])
                { second = i; }
            }
        }
        objectiveDoors[1] = doors[second];
        indexDoors[1] = second;

    }

    public void CreateSquadsFromStats()
    {
        if (objectiveDoors == null)
        { FindClosestDoors(); }
        if (numberOfReserveSquads > 0)
        { numberOfSquadsAvailable += (int)Mathf.Ceil(numberOfReserveSquads); }
        //Cada 4 rondas, cada castillo tendrá una squad más de reserva por si es atacado.
        numberOfReserveSquads = Mathf.Ceil(WaveManager.currentInstance.currentWave / 4);
        float doorStat_1 = WaveManager.currentInstance.DoorDamageSuccessRatio[indexDoors[0]];
        float doorStat_2 = WaveManager.currentInstance.DoorDamageSuccessRatio[indexDoors[1]];

        //A más grande, más disparidad entre squads para cada puerta.
        float squadRatioPerDoor = Mathf.Abs(doorStat_2 - doorStat_1) / 2;

        int numberOfSquads_Door_1;
        int numberOfSquads_Door_2;
        if (doorStat_1 < doorStat_2)
        {
            numberOfSquads_Door_1 = (int)Mathf.Ceil(numberOfSquadsAvailable * (1 - squadRatioPerDoor));
            numberOfSquads_Door_2 = (int)Mathf.Ceil(numberOfSquadsAvailable - numberOfSquads_Door_1);
        }
        else
        {
            numberOfSquads_Door_2 = (int)Mathf.Ceil(numberOfSquadsAvailable * (1 - squadRatioPerDoor));
            numberOfSquads_Door_1 = (int)Mathf.Ceil(numberOfSquadsAvailable - numberOfSquads_Door_2);
        }
        //Si es menor a la ronda 7, habrá la mitad de squads de Ranged y la mitad de Suicidal.

        //print(name + " squads assigned to door 1: " + numberOfSquads_Door_1);
        //print(name + " squads assigned to door 2: " + numberOfSquads_Door_2);
        
        if (WaveManager.currentInstance.currentWave < 7)
        {
            //Aaahora creamos las squads.
            for (int i = 0; i < numberOfSquads_Door_1; i++)
            {
                GameObject new_Squad_GO = Instantiate(squad_GO, transform);
                new_Squad_GO.SetActive(true);
                //SquadBehaviour newSquad = new_Squad_GO.GetComponent<SquadBehaviour>();
                new_Squad_GO.GetComponent<SquadBehaviour>().SetInitialObjective(objectiveDoors[0]);
                //new_Squad_GO.GetComponent<SquadBehaviour>().ChangeObjectiveToAllMembers(objectiveDoors[0]);
                if (i < numberOfSquads_Door_1 / 2)
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Suicidal;        }
                else
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Ranged;        }

                if (i % 2 == 0)
                { new_Squad_GO.transform.position = spawnerLocations[0].transform.position; }
                else
                { new_Squad_GO.transform.position = spawnerLocations[1].transform.position; }

                new_Squad_GO.SetActive(false);
                squadsAssigned.Add(new_Squad_GO.GetComponent<SquadBehaviour>());

            }
            for (int i = 0; i < numberOfSquads_Door_2; i++)
            {
                GameObject new_Squad_GO = Instantiate(squad_GO, transform);
                new_Squad_GO.SetActive(true);
                //SquadBehaviour newSquad = new_Squad_GO.GetComponent<SquadBehaviour>();
                new_Squad_GO.GetComponent<SquadBehaviour>().SetInitialObjective(objectiveDoors[1]);
                //new_Squad_GO.GetComponent<SquadBehaviour>().ChangeObjectiveToAllMembers(objectiveDoors[1]);
                if (i < numberOfSquads_Door_2 / 2)
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Ranged; }
                else
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Suicidal; }

                if (i % 2 == 0)
                { new_Squad_GO.transform.position = spawnerLocations[0].transform.position; }
                else
                { new_Squad_GO.transform.position = spawnerLocations[1].transform.position; }

                new_Squad_GO.SetActive(false);
                squadsAssigned.Add(new_Squad_GO.GetComponent<SquadBehaviour>());
            }

        }
        else
        {
            for (int i = 0; i < numberOfSquads_Door_1; i++)
            {
                GameObject new_Squad_GO = Instantiate(squad_GO, transform);
                new_Squad_GO.SetActive(true);
                //SquadBehaviour newSquad = new_Squad_GO.GetComponent<SquadBehaviour>();
                new_Squad_GO.GetComponent<SquadBehaviour>().SetInitialObjective(objectiveDoors[0]);
               //new_Squad_GO.GetComponent<SquadBehaviour>().ChangeObjectiveToAllMembers(objectiveDoors[0]);
                if (i < numberOfSquads_Door_1 / 2)
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Balanced; }
                else
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Ranged; }

                if (i % 2 == 0)
                { new_Squad_GO.transform.position = spawnerLocations[0].transform.position; }
                else
                { new_Squad_GO.transform.position = spawnerLocations[1].transform.position; }

                new_Squad_GO.SetActive(false);
                squadsAssigned.Add(new_Squad_GO.GetComponent<SquadBehaviour>());
            }

            for (int i = 0; i < numberOfSquads_Door_2; i++)
            {
                GameObject new_Squad_GO = Instantiate(squad_GO, transform);
                new_Squad_GO.SetActive(true);
                //SquadBehaviour newSquad = new_Squad_GO.GetComponent<SquadBehaviour>();
                new_Squad_GO.GetComponent<SquadBehaviour>().SetInitialObjective(objectiveDoors[1]);
                //new_Squad_GO.GetComponent<SquadBehaviour>().ChangeObjectiveToAllMembers(objectiveDoors[1]);
                if (i < numberOfSquads_Door_2 / 2)
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Balanced; }
                else
                { new_Squad_GO.GetComponent<SquadBehaviour>().type = SquadType.Ranged; }

                if (i % 2 == 0)
                { new_Squad_GO.transform.position = spawnerLocations[0].transform.position; }
                else
                { new_Squad_GO.transform.position = spawnerLocations[1].transform.position; }

                new_Squad_GO.SetActive(false);
                squadsAssigned.Add(new_Squad_GO.GetComponent<SquadBehaviour>());
            }
                                          
        }

    }

    public void DispatchSquads()
    {
        for (int i = 0; i < squadsAssigned.Count; i++)
        {
            if (squadsAssigned[i].SquadDispatched == false)
            {
                squadsAssigned[i].SquadDispatched = true;
                squadsAssigned[i].gameObject.SetActive(true);
                break;
            }
        }
    }

    void DispatchReserveSquad()
    {
        SquadBehaviour newSquad = new SquadBehaviour();
        newSquad.SetInitialObjective(WaveManager.currentInstance.player_Reference_GO);
        newSquad.ChangeObjectiveToAllMembers(WaveManager.currentInstance.player_Reference_GO);
        newSquad.type = SquadType.Balanced; 

        newSquad.gameObject.transform.position = spawnerLocations[1].transform.position; 
        newSquad.gameObject.SetActive(true);

        numberOfReserveSquads--;

    }

    void SquadDefenseMode()
    {
        for (int i = 0; i < squadsAssigned.Count; i++)
        {
            squadsAssigned[i].SetInitialObjective(WaveManager.currentInstance.player_Reference_GO);
            squadsAssigned[i].ChangeObjectiveToAllMembers(WaveManager.currentInstance.player_Reference_GO);
            squadsAssigned[i].castleObjectiveOverride = true;
        }
    }

    void SaveSquadsFromDestruction()
    {
        for (int i = 0; i < squadsAssigned.Count; i++)
        {
            squadsAssigned[i].gameObject.transform.SetParent(transform.parent);
        }
    }


}
