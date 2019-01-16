using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    //Animation and objective variables
    Animator enemyAnimator;
    float distanceToObjective;
    public GameObject objective;

    //Mechanics variables
    public GameObject projectile;
    public GameObject launchProjectileZone;
    public float currentEnemyLife;
    bool targetInRange;
    float shootTimer;

    NavMeshAgent enemyNavAgent;

    bool melee = false;

    //Squad Data
    public SquadBehaviour mySquad;
    float positionInSquad;


    // Start is called before the first frame update
    void Start()
    {
        currentEnemyLife = 80f;
        shootTimer = 0f;
        enemyAnimator = GetComponent<Animator>();
        enemyNavAgent = GetComponent<NavMeshAgent>();
        targetInRange = false;
        enemyNavAgent.updateUpAxis = false;

        if (launchProjectileZone == null) //SOMOS MELEE
        {
            enemyNavAgent.speed = enemyNavAgent.speed * 2.5f;
            currentEnemyLife = 50f;
            melee = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (objective == null)
        {
            FollowOrders(mySquad.initialObjective);
            //SetNewTarget(WaveManager.currentInstance.doorGameObject);
        }
        else
        {
            if (targetInRange == false)
            { enemyNavAgent.SetDestination(objective.transform.position); }
            Vector3 desiredObjectivePosition = new Vector3(objective.transform.position.x, transform.position.y,
                                                           objective.transform.position.z);
            float distance = Vector3.Distance(transform.position, desiredObjectivePosition);

            if (melee == false)
            {
                enemyAnimator.SetFloat("DistanceToTarget", distance);
                if (distance < 30f)
                {
                    targetInRange = true;
                    ShootingBehaviour();
                    //enemyNavAgent.isStopped = true;
                }
                else
                {
                    targetInRange = false;
                    //enemyNavAgent.isStopped = false;
                }
                
            }
            else // Si soy Melee...
            {
                //Valor antiguo == 6.5f
                if (distance < 8f)
                {
                    //KABOOM
                    WaveManager.currentInstance.ReduceLifeFromObjective(objective, this.gameObject);
                    Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 3.0f,
                                                  transform.position.z);
                    WaveManager.currentInstance.CreateParticles(newPos);
                    Destroy(this.gameObject);
                }
            }


        }

    }

    void ShootAtObjective()
    {
        launchProjectileZone.transform.LookAt(objective.transform.position);
        GameObject proj = Instantiate(projectile, launchProjectileZone.transform.position,
                                      launchProjectileZone.transform.rotation);

        proj.name = gameObject.name + " Missile";
        proj.GetComponent<ProjectileBehaviour>().type = ProjectileType.EnemyProjectile;
        //Vector3 playerDirection = (objective.transform.position - transform.position);
        //playerDirection.y = 0f;
        //playerDirection.Normalize();

        proj.GetComponent<Rigidbody>().AddForce(launchProjectileZone.transform.forward * 1000f);
        //Por si acaso, para eitar cosas injustas haremos que desaparezca en 5 seg si no choca con nada.
        Destroy(proj, 5f);
    }
    
    void ShootingBehaviour()
    {
        shootTimer += Time.deltaTime;
        if (targetInRange)
        {
            if (shootTimer > 1.5f)
            {
                ShootAtObjective();
                shootTimer = 0.0f;
            }
        }
        else
        {
            if (shootTimer > 3.0f)
            {
                ShootAtObjective();
                shootTimer = 0.0f;
            }
        }
    }

    public void ReceiveDamage(float hit, GameObject sender)
    {
        currentEnemyLife -= hit;
        //print("current " + gameObject.name + " life: " + currentEnemyLife);
        if (currentEnemyLife <= 0f)
        {
            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 3.0f,
                                                  transform.position.z);
            WaveManager.currentInstance.CreateParticles(newPos);
            Destroy(this.gameObject);
        }
        else
        {
            //SetNewTarget(sender);
            if (sender != objective)
            { mySquad.ChangeObjectiveToAllMembers(sender); }
        }

    }
    
    void SetNewTarget(GameObject obj)
    {       
        if (obj.tag == "Player")
        {
            enemyNavAgent.SetDestination(WaveManager.currentInstance.player_Reference_GO.transform.position);
            objective = WaveManager.currentInstance.player_Reference_GO;
        }
        else
        {
            enemyNavAgent.SetDestination(obj.transform.position);
            objective = obj;
        }

        if (obj.GetComponent<ProjectileBehaviour>() != null)
        {
            if (obj.GetComponent<ProjectileBehaviour>().type == ProjectileType.TurretProjectile)
            {
                //print("Torreta damaged");
                if (obj.GetComponent<ProjectileBehaviour>().father != null)
                {
                    enemyNavAgent.SetDestination(obj.GetComponent<ProjectileBehaviour>().father.transform.position);
                    objective = obj.GetComponent<ProjectileBehaviour>().father;
                }
            }
        }
              

        //print("Nuevo objetivo: " + objective.name);
    }
    
    public void FollowOrders(GameObject target)
    {
        SetNewTarget(target);
    }




}
