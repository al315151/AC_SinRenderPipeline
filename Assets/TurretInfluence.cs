using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretInfluence : MonoBehaviour
{
    public TurretBehaviour father;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && other.GetComponent<EnemyBehaviour>() != null)
        {
            father.ChangeCurrentObjective(other.gameObject);
        }
    }

}
