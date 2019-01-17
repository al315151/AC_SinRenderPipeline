using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy variables")]
    public GameObject[] enemyTypes;
    public List<GameObject> currentWaveEnemies;

    float spawnTimer = 0f;
    float spawnInterval = 0f;
    int activeEnemyIndex = 0;
    bool whileSpawn = false;

    [Header("Environment & player variables")]
    public GameObject doorGameObject;

    //Estos tres siguientes siempre tendran el mismo tamaño, dado que se trata de que 
    //cada indice se corresponda.
    public GameObject[] DoorPositions;
    float[] doorCurrentLife;
    public float[] DoorDamageSuccessRatio;
    //El success ratio aumentará conforme enemigos consigan atacar dicho punto. 
    //Si el usuario deja atacar mucho un punto, en la siguiente tanda se mandarán más squads a esa zona.
    // Estos datos se reiniciarán cada ronda.

    public List<CastleBehaviour> castles;
    public float squadsPerRound = 6;


    public GameObject player_Reference_GO;
    public GameObject turret_GO;
    public GameObject ParticleSystem_GO;
    float doorLife = 400f;
    float playerLife = 200f;


    [Header("Game variables")]
    public int currentWave;
    public static WaveManager currentInstance;
    public GameObject[] spawners;
    public Transform enemyPool;
    public Transform playerBulletPool;
       
    public int availableTurrets;

    [Header("Health UI variables")]
    public GameObject[] doorUIHolder;
    public Slider[] doorUI;
    public Image[] doorUIFill;

    public GameObject playerUIHolder;
    public Slider playerUI;
    public Image playerUIFill;

    [Header("Other UI variables")]
    public GameObject currentWave_GO;
    public Text CurrentWaveText;

    public GameObject GameOverUIHolder;
    public Text completedWaves;
    public Text GameOverText;
    float timerToReset = 0.0f;

    public GameObject availableTurretsUIHolder;
    public Text availableTurretsText;


    public GameObject MainMenu_GO;
    [HideInInspector]
    public bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        currentInstance = this;
        gameStarted = false;
        MainMenu_GO.SetActive(true);
        turret_GO.SetActive(false);
        availableTurretsUIHolder.SetActive(false);

        DoorDamageSuccessRatio = new float[4];

    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (playerUIHolder.gameObject.activeInHierarchy)
            { UpdateUIProperties(); }
            if (currentWave_GO.activeInHierarchy)
            { CurrentWaveText.text = "Completed Waves: " + currentWave + " "; }

            //Si borramos algun enemigo, lo quitamos de aqui.
            for (int i = 0; i < currentWaveEnemies.Count; i++)
            {
                if (currentWaveEnemies[i] == null)
                { currentWaveEnemies.RemoveAt(i); }
                else if (whileSpawn == false)
                { currentWaveEnemies[i].SetActive(true); }
            }
            
            for (int j = 0; j < castles.Count; j++)
            {
                if (castles[j] == null)
                { castles.RemoveAt(j); }
            }

            //Si no quedan enemigos, pasamos a la siguiente ronda.
            /**/
            //if (currentWaveEnemies.Count == 0)
            if (CheckAllSquadsDefeated() && GameOverUIHolder.activeInHierarchy == false)
            { ProceedToNextWave(); }
           
            if (GameOverUIHolder.activeInHierarchy)
            {
                float alpha = GameOverUIHolder.GetComponent<Image>().color.a + (Time.deltaTime / 1.5f);
                GameOverUIHolder.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, alpha);
                if (alpha > 0.3f)
                { GameOverText.gameObject.SetActive(true); }
                if (alpha > 0.7f)
                {
                    UpdateWaveNumber(currentWave);
                    completedWaves.gameObject.SetActive(true);
                }
                if (alpha >= 1f)
                {
                    timerToReset += Time.deltaTime;
                    if (timerToReset > 10f)
                    { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
                }
            }
        }
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }

    public void ReduceLifeFromObjective(GameObject objective, GameObject sender)
    {
        if (objective.tag == "Enemy" && sender.tag != "Enemy")
        {
            objective.GetComponent<EnemyBehaviour>().ReceiveDamage(20f, sender);
        }
        else if (objective.name.Contains("Door") && (sender.tag == "Enemy"))
        {
            for (int i = 0; i < DoorPositions.Length; i++)
            {
                if (objective.name == DoorPositions[i].name)
                {
                    if (sender.name.Contains("Missile"))
                    {
                        doorCurrentLife[i] -= 5f;
                        DoorDamageSuccessRatio[i] += 0.05f;
                    }
                    else
                    {
                        doorCurrentLife[i] -= 20f;
                        DoorDamageSuccessRatio[i] += 0.1f;
                    }
                }
            }            
        }
        if (objective.tag == "MainCamera")
        {
            if (sender.name.Contains("Missile"))
            { playerLife -= 5f; }
            else { playerLife -= 10f; }
           
        }
        else if (objective.tag == "Finish")
        {
            if (sender.name.Contains("Missile"))
            { objective.GetComponent<TurretBehaviour>().ReceiveDamage(5f); }
            else { objective.GetComponent<TurretBehaviour>().ReceiveDamage(10f); }
        }
        if (objective.tag == "Castle" && sender.tag != "Enemy")
        {
            for (int i = 0; i < castles.Count; i++)
            {
                if (objective.name == castles[i].name)
                {
                    castles[i].GetComponent<CastleBehaviour>().ReceiveDamage(10f);
                }
            }
        }
        

        if (GameOverUIHolder.activeInHierarchy == false)
        {
            for (int i = 0; i < DoorPositions.Length; i++)
            {
                if (doorCurrentLife[i] <= 0.0f || playerLife <= 0.0f)
                {
                    GameOverAnimation();
                    break;
                }
            }
            
            if (castles.Count == 0)
            {
                print("ALL CASTLES DEFEATED");
                GameOverText.text = "You have completed the training!!!";
                GameOverAnimation();
            }
            
        }
        
    }

    public void ProceedToNextWave()
    {
        availableTurrets += 3;
        currentWave++;
        UpdateAvailableTurretsText();        

        ManageSquadsOntoCastles();

    }

    public void SetUIProperties()
    {
        doorCurrentLife = new float[4];

        for (int i= 0; i < doorUIHolder.Length; i++)
        {
            doorUI[i].maxValue = doorLife;
            doorUI[i].value = doorLife;
            doorUIFill[i].color = Color.green;
            doorCurrentLife[i] = doorLife;
        }    

        playerUI.maxValue = playerLife;
        playerUI.value = playerLife;
        playerUIFill.color = Color.green;
    }

    void UpdateUIProperties()
    {
        for (int i = 0; i < doorUIHolder.Length; i++)
        {
            doorUI[i].value = doorCurrentLife[i];
            if (doorCurrentLife[i] / doorUI[i].maxValue > 0.7)
            { doorUIFill[i].color = Color.green; }
            else if (doorCurrentLife[i] / doorUI[i].maxValue > 0.3)
            { doorUIFill[i].color = Color.yellow; }
            else { doorUIFill[i].color = Color.red; }
        }
        

        playerUI.value = playerLife;
        if (playerLife / playerUI.maxValue > 0.7)
        {
            playerUIFill.color = Color.green;
        }
        else if (playerLife / playerUI.maxValue > 0.3)
        {
            playerUIFill.color = Color.yellow;
        }
        else { playerUIFill.color = Color.red; }



    }

    void UpdateWaveNumber(int number)
    {
        completedWaves.text = "Completed Waves: " + number + "  ";
    }

    public void UpdateAvailableTurretsText()
    {
        availableTurretsText.text = "Available turrets: " + availableTurrets;
    }

    void GameOverAnimation()
    {
        for (int i = 0; i < doorUIHolder.Length; i++)
        {
            doorUIHolder[i].gameObject.SetActive(false);
        }
        
        playerUIHolder.gameObject.SetActive(false);
        availableTurretsUIHolder.SetActive(false);
        GameOverUIHolder.gameObject.SetActive(true);
        GameOverText.gameObject.SetActive(false);
        completedWaves.gameObject.SetActive(false);
        currentWave_GO.SetActive(false);
        GameOverUIHolder.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    public void StartGame()
    {
        MainMenu_GO.SetActive(false);

        availableTurretsUIHolder.SetActive(true);
        availableTurrets = 1;
        UpdateAvailableTurretsText();

        currentWave = 0;

        //Comentado para hacer 
        ProceedToNextWave();

        for (int i = 0; i < doorUIHolder.Length; i++)
        {
            doorUIHolder[i].SetActive(true);
        }       
        playerUIHolder.SetActive(true);
        SetUIProperties();
        GameOverUIHolder.gameObject.SetActive(false);
        gameStarted = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (player_Reference_GO == null)
        { player_Reference_GO = GameObject.FindGameObjectWithTag("MainCamera"); }
    }

    public void QuitGame()
    {   Application.Quit();    }

    public void CreateParticles(Vector3 position)
    {
        GameObject GO = Instantiate(ParticleSystem_GO, position, Quaternion.identity);
        Destroy(GO, 1.9f);
    }

    public void CreateCastleParticles(Vector3 position)
    {
        GameObject GO = Instantiate(ParticleSystem_GO, position, Quaternion.identity);
        GO.transform.localScale = new Vector3(20f, 20f, 20f);
        Destroy(GO, 1.9f);
    }

    public void ManageSquadsOntoCastles()
    {
        squadsPerRound = (currentWave + 1) * 1.5f;
        //print("Round: " + currentWave + " total squads: " + squadsPerRound);
        
       for (int i = 0; i < castles.Count; i++)
       {
           castles[i].numberOfSquadsAvailable = ((int)Mathf.Ceil(squadsPerRound)) / castles.Count;
       }
       if (squadsPerRound % castles.Count != 0)
        {
            for (int i = 0; i < castles.Count; i++)
            {
                castles[i].numberOfSquadsAvailable++;
            }
        }

        for (int j = 0; j < castles.Count; j++)
        {
            //print(castles[j] +"number of squads assigned: " + castles[j].numberOfSquadsAvailable);
            //Decirles a los castillos que creen sus propios squads, con las estadisticas.
            castles[j].CreateSquadsFromStats();
        }

        //Resetear las stats de esta ronda. 
        DoorDamageSuccessRatio = new float[4];


    }

    bool CheckAllSquadsDefeated()
    {
        
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].allSquadsDefeated == false)
            { return false; }
        }

        return true;
    }



}
