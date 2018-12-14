using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public GameObject father;

    private void OnTriggerEnter(Collider other)
    {
        //print("Entramos?");
        if (other.gameObject != father)
        {
            WaveManager.currentInstance.ReduceLifeFromObjective(other.gameObject, this.gameObject); 
            Destroy(this.gameObject);
        }
    }
}
