using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    public GameObject turretHead;


    [Header("Mechanics Variables")]
    public GameObject projectile;
    public GameObject launchProjectileZone;
    GameObject objective;
    EnemyBehaviour objectiveScript;
    
    public float turretLife;

    float timerShoot = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        turretLife = 300f;
    }

    // Update is called once per frame
    void Update()
    {
        timerShoot += Time.deltaTime;

        if (timerShoot > 0.5f)
        {
            SearchForEnemy();
            if (objective != null)
            {
                turretHead.transform.LookAt(objective.transform.position);
                RaycastHit hit;
                if (Physics.Raycast(transform.position, turretHead.transform.forward, out hit))
                {                    
                    timerShoot = 0.0f;
                    ShootAtObjective();   
                }
            }                
        }

    }

    public void ReceiveDamage(float damage)
    {
        turretLife -= damage;
        if (turretLife <= 0.0f)
        {
            Destroy(this.gameObject);
        }

    }

    void ShootAtObjective()
    {
        GameObject proj = Instantiate(projectile, launchProjectileZone.transform.position,
                                              launchProjectileZone.transform.rotation);

        proj.name = gameObject.name + " Missile";
        proj.GetComponent<ProjectileBehaviour>().father = this.gameObject;
        Vector3 playerDirection = (objectiveScript.transform.position - transform.position);
        playerDirection.y = 0f;
        playerDirection.Normalize();

        proj.GetComponent<Rigidbody>().AddForce(playerDirection * 2000f);
        //Por si acaso, para eitar cosas injustas haremos que desaparezca en 5 seg si no choca con nada.
        Destroy(proj, 5f);
    }





    void SearchForEnemy()
    {
        List<GameObject> enemies = WaveManager.currentInstance.currentWaveEnemies;
        float minDistance = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(this.transform.position,
                                                  enemies[i].transform.position);
                if (distance < minDistance)
                {
                    objective = enemies[i];
                    objectiveScript = enemies[i].GetComponent<EnemyBehaviour>();
                }
            }
        }

    }




}
