using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleBehaviour : MonoBehaviour
{

    float castleLive;

    //Squad Management

    List<SquadBehaviour> squadsAssigned;


    //Self-Defense variables
    float influenceArea;
    float PatrolRadio;



    // Start is called before the first frame update
    void Start()
    {
        squadsAssigned = new List<SquadBehaviour>();


    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.currentInstance.gameStarted)
        {
            for (int i = 0; i < squadsAssigned.Count; i++)
            {
                if (squadsAssigned[i] == null)
                { squadsAssigned.RemoveAt(i); }
            }









        }
    }
}
