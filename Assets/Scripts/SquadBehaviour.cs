using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SquadType
{
    Suicidal, 
    Healers,
    Balanced, 
    Ranged

};



public class SquadBehaviour : MonoBehaviour
{
    int numberOfMembers;
    List<GameObject> squadMembers;

    GameObject[] enemyTypes;

    public SquadType type;

    public GameObject currentObjective;
    public GameObject initialObjective;


    public bool SquadDispatched = false;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SquadDispatched)
        {
            if (enemyTypes == null && WaveManager.currentInstance != null)
            {
                enemyTypes = WaveManager.currentInstance.enemyTypes;
                PositionAndNumberAssignment(type);
            }

            //Check if anyone's dead. If it is, remove from the squad.
            for (int i = 0; i < squadMembers.Count; i++)
            {
                if (squadMembers[i] == null)
                {
                   // print("Referencia de enemigo caido quitada de lista de squad");
                    squadMembers.RemoveAt(i);
                    numberOfMembers--;
                }
            }
            if (squadMembers.Count == 0)
            { Destroy(this.gameObject); }
        }
    }

    void PositionAndNumberAssignment(SquadType type)
    {
        numberOfMembers = (WaveManager.currentInstance.currentWave) * 2;
        squadMembers = new List<GameObject>(numberOfMembers);
        switch (type)
        {
            case SquadType.Suicidal:
                {         
                    //Initial positions of squad.
                    for (int i = 0; i < numberOfMembers / 2; i++)
                    {
                        for (int j = 0; j < numberOfMembers - (numberOfMembers / 2); j++)
                        {
                            //Ahora, el enemigo melee es el numero 1 en el array de enemyTypes.
                            Vector3 memberInitialPosition = new Vector3(transform.position.x + (i * 2f),
                                                                        transform.position.y,
                                                                        transform.position.z + (j * 2f));
                            GameObject recruit = Instantiate(enemyTypes[1], memberInitialPosition,
                                                               Quaternion.identity, transform);
                            recruit.name = "Member " + i + " of " + name;
                            recruit.SetActive(true);
                            recruit.GetComponent<EnemyBehaviour>().mySquad = this;
                            squadMembers.Add(recruit);


                        }
                    }
                    print("Squad melee created");

                    break;
                }
            case SquadType.Ranged:
                {
                    
                    //Initial positions of squad.
                    for (int i = 0; i < numberOfMembers / 2; i++)
                    {
                        for (int j = 0; j < numberOfMembers - (numberOfMembers / 2); j++)
                        {
                            //Ahora, el enemigo melee es el numero 1 en el array de enemyTypes.
                            Vector3 memberInitialPosition = new Vector3(transform.position.x + (i * 2f),
                                                                        transform.position.y,
                                                                        transform.position.z + (j * 2f));
                            GameObject recruit = Instantiate(enemyTypes[0], memberInitialPosition,
                                                               Quaternion.identity, transform);
                            recruit.name = "Member " + i + " of " + name;
                            recruit.SetActive(true);
                            recruit.GetComponent<EnemyBehaviour>().mySquad = this;
                            squadMembers.Add(recruit);
                        }
                    }
                    print("Squad ranged created");

                    break;
                }

            case SquadType.Balanced:
                {
                   
                    //Initial positions of squad.
                    for (int i = 0; i < numberOfMembers / 2; i++)
                    {
                        for (int j = 0; j < numberOfMembers - (numberOfMembers / 2); j++)
                        {
                            //Ahora, el enemigo melee es el numero 1 en el array de enemyTypes.
                            Vector3 memberInitialPosition = new Vector3(transform.position.x + (i * 2f),
                                                                        transform.position.y,
                                                                        transform.position.z + (j * 2f));

                            int enemyTypeIndex;
                            if ((i+j) < numberOfMembers / 2)
                            {   enemyTypeIndex = 0; }
                            else
                            {   enemyTypeIndex = 1; }
                            GameObject recruit = Instantiate(enemyTypes[enemyTypeIndex], memberInitialPosition,
                                                               Quaternion.identity, transform);
                            recruit.name = "Member " + i + " of " + name;
                            recruit.SetActive(true);
                            recruit.GetComponent<EnemyBehaviour>().mySquad = this;
                            squadMembers.Add(recruit);
                        }
                    }
                    print("Squad balanced created");

                    break;
                }
        }

    }

    
    public void ChangeObjectiveToAllMembers(GameObject obj)
    {
        for (int i = 0; i < squadMembers.Count; i++)
        {
            squadMembers[i].GetComponent<EnemyBehaviour>().FollowOrders(obj);
        }


    }


    public void SetInitialObjective(GameObject objective)
    {
        initialObjective = objective;
    }






}
