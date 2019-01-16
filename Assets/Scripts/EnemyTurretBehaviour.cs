using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurretBehaviour : MonoBehaviour
{
    CastleBehaviour owner;
    public GameObject turretHead;
   
  
    [Header("Mechanics Variables")]
    public GameObject projectile;
    public GameObject launchProjectileZone;
    GameObject objective;
    
    public float turretLife;

    float timerShoot = 0.0f;

    bool turretActivated = false;


    // Update is called once per frame
    void Update()
    {
        if (turretActivated)
        {            
            timerShoot += Time.deltaTime;

            if (timerShoot > 0.75f)
            {
                objective = WaveManager.currentInstance.player_Reference_GO;
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
    }

  
    void ShootAtObjective()
    {
        launchProjectileZone.transform.LookAt(objective.transform.position);
        GameObject proj = Instantiate(projectile, launchProjectileZone.transform.position,
                                              launchProjectileZone.transform.rotation, this.transform);

        proj.name = gameObject.name + " Missile";
        proj.GetComponent<ProjectileBehaviour>().father = this.gameObject;
        proj.GetComponent<ProjectileBehaviour>().type = ProjectileType.EnemyProjectile;

        // Vector3 playerPosition = WaveManager.currentInstance.player_Reference_GO.transform.position;
        //Vector3 playerDirection = (playerPosition - transform.position);
        // playerDirection.y = 0f;
        // playerDirection /= 5f;
        //playerDirection.Normalize();

        // proj.GetComponent<Rigidbody>().AddForce(playerDirection * 100f);
        turretHead.transform.LookAt(objective.transform.position);
        proj.GetComponent<Rigidbody>().AddForce(launchProjectileZone.transform.forward * 1500f);
        //Por si acaso, para eitar cosas injustas haremos que desaparezca en 5 seg si no choca con nada.
        Destroy(proj, 5f);
    }

   

    public void ShutOffTurret()
    {
        turretActivated = false;
    }

    public void ShutOnTurret()
    {
        turretActivated = true;
    }
}
