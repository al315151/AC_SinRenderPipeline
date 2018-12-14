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
    public GameObject player_Reference_GO;
    public GameObject turret_GO;
    public GameObject ParticleSystem_GO;
    float doorLife = 300f;
    float playerLife = 150f;


    [Header("Game variables")]
    public int currentWave;
    public static WaveManager currentInstance;
    public GameObject[] spawners;
    public Transform enemyPool;

    public int availableTurrets;

    [Header("UI variables")]
    public GameObject doorUIHolder;
    public Slider doorUI;
    public Image doorUIFill;
    public GameObject playerUIHolder;
    public Slider playerUI;
    public Image playerUIFill;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (playerUIHolder.gameObject.activeInHierarchy && doorUIHolder.gameObject.activeInHierarchy)
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
            //Si no quedan enemigos, pasamos a la siguiente ronda.
            if (currentWaveEnemies.Count == 0)
            { ProceedToNextWave(); }

            //Spawneamos los enemigos.
            if (whileSpawn)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer > spawnInterval)
                {
                    //pr si el jugador elimina a alguien cuando todos no están aun spawneados.
                    if (activeEnemyIndex >= currentWaveEnemies.Count)
                    { whileSpawn = false; }
                    else
                    {
                        currentWaveEnemies[activeEnemyIndex].SetActive(true);
                        activeEnemyIndex++;
                        spawnTimer = 0f;
                    }
                }
            }

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
        else if (objective.name == doorGameObject.name && sender.tag != "Player")
        {
            if (sender.name.Contains("Missile"))
            { doorLife -= 5f; }
            else { doorLife -= 10f; }
            
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
        if ((doorLife <= 0.0f || playerLife <= 0.0f) && GameOverUIHolder.activeInHierarchy == false)
        {
            //print("Se llama");
            GameOverAnimation(); }
        
    }

    public void ProceedToNextWave()
    {
        availableTurrets += 2;
        UpdateAvailableTurretsText();

        currentWave++;
        float numberOfEnemies = currentWave * 10;
        activeEnemyIndex = 0;
        spawnInterval = 1 / (currentWave - (currentWave / 2));
        whileSpawn = true;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            int spawnerIndex = Random.Range(0, spawners.Length);
            if (i % 2 == 0)
            {
                GameObject newEnemy = Instantiate(enemyTypes[0], spawners[spawnerIndex].transform.position, 
                                                                 Quaternion.identity, enemyPool);
                newEnemy.name = "Melee_" + i / 2;
                currentWaveEnemies.Add(newEnemy);
                newEnemy.SetActive(false);
                //newEnemy.GetComponent<EnemyBehaviour>().SetNewTarget(doorGameObject);
            }
            else
            {
                GameObject newEnemy = Instantiate(enemyTypes[1], spawners[spawnerIndex].transform.position,
                                                                Quaternion.identity, enemyPool);
                newEnemy.name = "Ranged_" + i / 2;
                currentWaveEnemies.Add(newEnemy);
                newEnemy.SetActive(false);
                //newEnemy.GetComponent<EnemyBehaviour>().SetNewTarget(doorGameObject);
            }
        }

    }

    public void SetUIProperties()
    {
        doorUI.maxValue = doorLife;
        doorUI.value = doorLife;
        doorUIFill.color = Color.green;

        playerUI.maxValue = playerLife;
        playerUI.value = playerLife;
        playerUIFill.color = Color.green;

    }

    void UpdateUIProperties()
    {
        doorUI.value = doorLife;
        if (doorLife / doorUI.maxValue > 0.7)
        {
            doorUIFill.color = Color.green;
        }
        else if (doorLife / doorUI.maxValue > 0.3)
        {
            doorUIFill.color = Color.yellow;
        }
        else { doorUIFill.color = Color.red; }

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
        doorUIHolder.gameObject.SetActive(false);
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
        ProceedToNextWave();

        doorUIHolder.SetActive(true);
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


}
