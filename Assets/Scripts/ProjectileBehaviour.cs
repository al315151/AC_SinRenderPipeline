using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
    EnemyProjectile,
    TurretProjectile,
    PlayerProjectile
};


public class ProjectileBehaviour : MonoBehaviour
{
    public GameObject father;
    public ProjectileType type = ProjectileType.PlayerProjectile;

    private void OnTriggerEnter(Collider other)
    {
        //print("Entramos?");
        if (other.gameObject != father && other.tag != tag && other.tag != "IgnoreProjectile") // No es el mismo objeto.
        {
            if (other.tag != "Enemy" && type == ProjectileType.EnemyProjectile) // De enemigo a player o torreta.
            {
                WaveManager.currentInstance.ReduceLifeFromObjective(other.gameObject, this.gameObject);
                Destroy(this.gameObject);
            }
            if (other.tag == "Enemy" && type != ProjectileType.EnemyProjectile) // De player o torreta a enemigo.
            {
                WaveManager.currentInstance.ReduceLifeFromObjective(other.gameObject, this.gameObject);
                Destroy(this.gameObject);
            }
            if (other.tag == "Castle" && type != ProjectileType.EnemyProjectile)
            {
                WaveManager.currentInstance.ReduceLifeFromObjective(other.gameObject, this.gameObject);
                Destroy(this.gameObject);
            }
        }
    }
}
