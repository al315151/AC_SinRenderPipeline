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
    GameObject[] memberPositions;

    GameObject[] enemyTypes;

    public SquadType type;


    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyTypes == null && WaveManager.currentInstance != null)
        {
            enemyTypes = WaveManager.currentInstance.enemyTypes;
            PositionAndNumberAssignment(type);
        }




    }

    void PositionAndNumberAssignment(SquadType type)
    {
        switch (type)
        {
            case SquadType.Suicidal:
                {
                    numberOfMembers = (WaveManager.currentInstance.currentWave + 1) * 4;
                    memberPositions =  new GameObject[numberOfMembers];
                    //Initial positions of squad.
                    for (int i = 0; i < numberOfMembers / 2; i++)
                    {
                        for (int j = 0; j < numberOfMembers - (numberOfMembers / 2); j++)
                        {
                            //Ahora, el enemigo melee es el numero 1 en el array de enemyTypes.
                            Vector3 memberInitialPosition = new Vector3(transform.position.x + (i * 2f),
                                                                        transform.position.y,
                                                                        transform.position.z + (j * 2f));
                            memberPositions[i+j] = Instantiate(enemyTypes[1], memberInitialPosition, 
                                                               Quaternion.identity, transform);
                            memberPositions[i + j].SetActive(true);


                        }
                    }
                    print("Squad created");

                    break;
                }




        }




    }


}
