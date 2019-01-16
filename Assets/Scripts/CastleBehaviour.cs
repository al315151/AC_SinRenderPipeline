using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleBehaviour : MonoBehaviour
{

    float castleLife = 2000f;
    GameObject[] objectiveDoors;


    //Squad Management

    List<SquadBehaviour> squadsAssigned;
    
    //Self-Defense variables
    float influenceArea = 125f;
    float PatrolRadio;

    [Header("Defense References")]
    public EnemyTurretBehaviour[] defenseTurrets;



    [Header("UI References")]
    public Canvas castle_Canvas;
    public Slider CastleLife_Slider;
    public Image CastleLife_SliderFill_Image;

    // Start is called before the first frame update
    void Start()
    {
        squadsAssigned = new List<SquadBehaviour>();        
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.currentInstance != null && WaveManager.currentInstance.gameStarted &&
            castle_Canvas.worldCamera == null)
        {
            castle_Canvas.worldCamera =
            WaveManager.currentInstance.player_Reference_GO.GetComponent<Camera>();
            CastleLife_Slider.maxValue = castleLife;
            CastleLife_Slider.value = castleLife;

            FindClosestDoors();
            print(name + " closest doors: " + objectiveDoors[0].name
                  + " , " + objectiveDoors[1].name);
        }
                     
        if (WaveManager.currentInstance != null && WaveManager.currentInstance.gameStarted)
        {
            for (int i = 0; i < squadsAssigned.Count; i++)
            {
                if (squadsAssigned[i] == null)
                { squadsAssigned.RemoveAt(i); }
            }  
            
            if (defenseTurrets.Length != 0)
            { PlayerInDistance(); }


        }

        ChangeColorCastleLife();
    }

    public void ReceiveDamage(float damage)
    {
        castleLife -= damage;
        if (castleLife < 0.0f)
        {
            WaveManager.currentInstance.CreateCastleParticles(transform.position);
            Destroy(this.gameObject);

        }
    }

    void PlayerInDistance()
    {
        GameObject player = WaveManager.currentInstance.player_Reference_GO;       
        for (int i = 0; i < defenseTurrets.Length; i++)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < influenceArea)
            {
                defenseTurrets[i].GetComponent<EnemyTurretBehaviour>().ShutOnTurret();
            }
            else
            {
                defenseTurrets[i].GetComponent<EnemyTurretBehaviour>().ShutOffTurret();
            }
        }
        //print(Vector3.Distance(player.transform.position, transform.position));


    }
    
    void ChangeColorCastleLife()
    {
        castle_Canvas.gameObject.transform.LookAt(WaveManager.currentInstance.player_Reference_GO.transform.position);

        CastleLife_Slider.value = castleLife;
        if (castleLife < CastleLife_Slider.maxValue * 0.4f)
        { CastleLife_SliderFill_Image.color = 
                new Color(1.0f, 0.0f, 0.0f, 0.7f); }
        else if (castleLife < CastleLife_Slider.maxValue * 0.7f)
        { CastleLife_SliderFill_Image.color = 
            new Color(1.0f, 0.92f, 0.016f, 0.7f);  }
        else
        { CastleLife_SliderFill_Image.color =
                new Color(0.0f, 1.0f, 0.0f, 0.7f); }
       
    }

    void FindClosestDoors()
    {
        objectiveDoors = new GameObject[2];

        GameObject[] doors = WaveManager.currentInstance.DoorPositions;
        float[] doorsDistance = new float[doors.Length];

        for (int i = 0; i < doors.Length; i++)
        {  doorsDistance[i] = Vector3.Distance(transform.position, doors[i].transform.position);    }

        int minIndex = 0;
        for (int i = 1; i < doors.Length; i++)
        {  if (doorsDistance[i] < doorsDistance[minIndex])
            {   minIndex = i;    }
        }
        objectiveDoors[0] = doors[minIndex];

        int second = -1;
        for (int i = 0; i < doors.Length; i++)
        {  if (i != minIndex)
            {
                if (second == -1)
                {   second = i;   }
                else if (doorsDistance[i] < doorsDistance[second])
                { second = i; }
            }
        }
        objectiveDoors[1] = doors[second];

    }





}
