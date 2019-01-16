using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretInfluence : MonoBehaviour
{
    public TurretBehaviour father;

    public List<GameObject> enemiesInRange;

    float changeObjectiveTimer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && other.GetComponent<EnemyBehaviour>() != null)
        {
            father.ChangeCurrentObjective(other.gameObject);
            enemiesInRange.Add(other.gameObject);
        }
    }

    private void Start()
    {
        enemiesInRange = new List<GameObject>();
    }

    private void Update()
    {
        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            if (enemiesInRange[i] == null)
            {
                enemiesInRange.RemoveAt(i);
            }
        }

        if (enemiesInRange != null && enemiesInRange.Count > 0)
        {
            changeObjectiveTimer += Time.deltaTime;
            if (changeObjectiveTimer > 2.0f)
            {
                int index = 0;
                float distance = Vector3.Distance(transform.position, enemiesInRange[0].transform.position);
                for (int i = 1; i < enemiesInRange.Count; i++)
                {
                    float aux = Vector3.Distance(transform.position, enemiesInRange[1].transform.position);
                    if (aux < distance)
                    { index = i; distance = aux; }
                }

                father.ChangeCurrentObjective(enemiesInRange[index]);
                changeObjectiveTimer = 0.0f;
            }
        }


    }



}
