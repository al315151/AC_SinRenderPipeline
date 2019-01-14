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
                    numberOfMembers = WaveManager.currentInstance.currentWave * 4;
                    memberPositions =  new GameObject[numberOfMembers];
                    //Initial positions of squad.
                    for (int i = 0; i < numberOfMembers; i++)
                    {
                        //Ahora, el enemigo melee es el numero 1 en el array de enemyTypes.
                        memberPositions[i] = Instantiate(enemyTypes[1], transform);
                        memberPositions[i].transform.position = new Vector3(transform.position.x + i * 2f,
                                                                            transform.position.y,
                                                                            transform.position.z + i * 2f);

                    }
                    print("Squad created");

                    break;
                }




        }




    }


}
