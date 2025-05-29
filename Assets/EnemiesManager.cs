using UnityEngine;

public enum PathingType
{
    None, Target, Radial
};

public class EnemiesManager : MonoBehaviour
{
    public GameObject m_enemy;
    public Vector2 m_enemySize;
    public Vector2 m_spawnArea;
    public float m_spawnTimer;
    public bool m_spawnEnemies;

    public int m_numEnemiesSpawned;
    public int m_maxSpawns;

    private float m_timer;

    public GameObject m_target;
    public SwarmController m_swarmController;

    void Start()
    {
        if (m_enemy == null)
        {
            Debug.LogWarning("m_enemy is not assigned...again...");
            m_enemy = GameObject.Find("Enemy");
        }
        SpriteRenderer enemySpriteRenderer = m_enemy.GetComponent<SpriteRenderer>();
        if (enemySpriteRenderer != null && enemySpriteRenderer.sprite != null)
        {
            m_enemySize = enemySpriteRenderer.sprite.rect.size;
        }
        else
        {
            Debug.LogError("enemy sprite renderer thing is not assigned");
            m_enemySize = Vector2.one;
        }
        
        if (m_swarmController == null)
        {
            // i asked ai and it said to do this
            m_swarmController = FindAnyObjectByType<SwarmController>();
            if (m_swarmController == null)
            {
                GameObject swarmControllerGO = new GameObject("SwarmController");
                m_swarmController = swarmControllerGO.AddComponent<SwarmController>();
            }
        }

        if (m_swarmController.m_radialPaths.Count == 0)
        {
            m_swarmController.AddRadialPath(m_target.transform.position, 2.0f, m_enemySize.magnitude);
        }
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer < 0f)
        {
            spawnEnemy();
            m_timer = m_spawnTimer;
        }
    }

    private Vector3 generateRandomPositionAtEdgesOfSpawnArea()
    {
        Vector3 ret = new Vector3();

        float randomSignX = Random.Range(0, 2) * 2 - 1; // -1 or 1
        float randomSignY = Random.Range(0, 2) * 2 - 1; // -1 or 1

        if (Random.value < 0.5f) // spawn on X edges
        {
            ret.x = m_spawnArea.x * randomSignX;
            ret.y = Random.Range(-m_spawnArea.y, m_spawnArea.y);
        }
        else // spawn on Y edges
        {
            ret.x = Random.Range(-m_spawnArea.x, m_spawnArea.y);
            ret.y = m_spawnArea.y * randomSignY;
        }

        ret.z = 0.0f;

        return ret;
    }

    private void spawnEnemy()
    {
        if (m_spawnEnemies && m_numEnemiesSpawned < m_maxSpawns)
        {
            Vector3 newSpawnPosition = generateRandomPositionAtEdgesOfSpawnArea();

            // better if it's near the player i guess?
            newSpawnPosition += m_target.transform.position;

            GameObject newEnemy = Instantiate(m_enemy, newSpawnPosition, Quaternion.identity);
            newEnemy.gameObject.SetActive(true);

            Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
            enemyComponent.setTarget(m_target);
            enemyComponent.m_pathingType = PathingType.Radial;

            enemyComponent.m_swarmController = m_swarmController;
            enemyComponent.m_pathIndex = 0;
            enemyComponent.m_indexInPath = m_numEnemiesSpawned;

            Vector3 designatedPos = m_swarmController.assignCreatureToPath(
                m_target.transform.position,
                2.0f,
                m_enemySize.magnitude,
                enemyComponent.m_indexInPath
            );

            enemyComponent.setDesignatedRadialPosition(designatedPos);
            m_numEnemiesSpawned++;
        }
    }
}