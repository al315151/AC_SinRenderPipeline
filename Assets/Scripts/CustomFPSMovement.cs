using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class CustomFPSMovement : MonoBehaviour
{
    public float movementSpeed = 10f;
    float rotationSpeed = 2.5f;

    float groundDistance = 6f;
    CapsuleCollider cameraCollider;

    public NavMeshAgent cameraAgent;

    public GameObject launchProjectileZone;
    public GameObject projectile;

    //SHooting Cooldown behaviour
    float timerShoot = 0.0f;
    float shootCoolDown = 0.2f;



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraCollider = GetComponent<CapsuleCollider>();
        groundDistance = cameraCollider.bounds.extents.y + 0.1f;
        cameraAgent.updateRotation = false;
        cameraAgent.updateUpAxis = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.currentInstance.gameStarted)
        {
            timerShoot += Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                //El doble de lo normal.
                movementSpeed = 20f;
            }
            else { movementSpeed = 7.5f; }

            PlayerMovement();
            PlayerRotation();

            ShootingBehaviour();
            if (Input.GetKey(KeyCode.E))
            {
                WaveManager.currentInstance.turret_GO.SetActive(true);
                RaycastHit hit;
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {                    
                    WaveManager.currentInstance.turret_GO.transform.position = hit.point;
                }
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                if (WaveManager.currentInstance.availableTurrets > 0)
                {
                    GameObject newTurret = Instantiate(WaveManager.currentInstance.turret_GO,
                                                        WaveManager.currentInstance.turret_GO.transform.position,
                                                        WaveManager.currentInstance.turret_GO.transform.rotation);
                    WaveManager.currentInstance.availableTurrets--;
                    WaveManager.currentInstance.UpdateAvailableTurretsText();
                    newTurret.GetComponent<TurretBehaviour>().ShutOnTurret();
                }
                WaveManager.currentInstance.turret_GO.SetActive(false);
            }
        }
    }

    void PlayerRotation()
    {
        //Based on:
        //https://gamedev.stackexchange.com/questions/136174/im-rotating-an-object-on-two-axes-so-why-does-it-keep-twisting-around-the-thir
        
        float rotX = Input.GetAxis("Mouse X")  * rotationSpeed;
        float rotY = Input.GetAxis("Mouse Y")  * rotationSpeed;
      
        Quaternion yaw = Quaternion.Euler(0f, rotX, 0f);
        transform.rotation = yaw * transform.rotation;
        Quaternion pitch = Quaternion.Euler(-rotY, 0f, 0f);
        transform.rotation = transform.rotation * pitch;
        
    }

    void PlayerMovement()
    {
        Vector3 wantedForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
        Vector3 wantedRight = new Vector3(transform.right.x, 0f, transform.right.z);
        transform.position += wantedRight * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        transform.position += wantedForward * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;

        //Only for navmesh
        //wantedForward = wantedForward * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        //wantedRight = wantedRight * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        //cameraAgent.SetDestination(transform.position + (wantedForward + wantedRight));
        //cameraAgent.Move(wantedForward + wantedRight);
        
    }

    bool CheckGrounded()
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, -Vector3.up, out hit, groundDistance) && 
               hit.transform.gameObject.name != gameObject.name;
    }

    void ShootAtObjective()
    {
        GameObject proj = Instantiate(projectile, launchProjectileZone.transform.position,
                                      launchProjectileZone.transform.rotation);

        proj.name = gameObject.name + " Missile";
        proj.GetComponent<ProjectileBehaviour>().type = ProjectileType.PlayerProjectile;
        proj.GetComponent<Rigidbody>().AddForce(transform.forward * 1500f);
        //Por si acaso, para eitar cosas injustas haremos que desaparezca en 4 seg si no choca con nada.
        Destroy(proj, 3f);
    }

    void ShootingBehaviour()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && Input.GetKey(KeyCode.E) == false)
        {
            if (timerShoot > shootCoolDown)
            {
                ShootAtObjective();
                timerShoot = 0.0f;
            }
        }
    }


}
