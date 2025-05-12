using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemiesManager : MonoBehaviour
{
    
    public GameObject enemy;
    public Vector2 spawnArea;
    public float spawnTimer;

    private float timer;

    public GameObject target;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0f) {
            spawnEnemy();
            timer = spawnTimer;
        }   
    }

    private void spawnEnemy() {
        Vector3 newSpawnPosition = new Vector3(
            UnityEngine.Random.Range(-spawnArea.x, spawnArea.y),
            UnityEngine.Random.Range(-spawnArea.x, spawnArea.y),
            0f
        );

        // better if it's near the player i guess?
        newSpawnPosition += target.transform.position;

        GameObject newEnemy = Instantiate(enemy);
        newEnemy.transform.position = newSpawnPosition;
        newEnemy.GetComponent<Enemy>().setTarget(target);
    }
}
