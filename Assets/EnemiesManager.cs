using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public struct RadialPath
{
    public float m_innerRadius { get; set; }
    public float m_outerRadius { get; set; }
    public float m_circumference { get; set; }
    public Vector3 m_centerPos { get; set; }
    public float m_spaceTaken { get; set; }

    List<Vector3> m_radialPositions;
    int m_numCreatures;

    public float spaceAvaiable()
    {
        return m_circumference - m_spaceTaken;
    }

    public bool GodPleaseTellMeIfIAmInTheRightPositionOrNot(Vector3 position)
    {

        return false;
    }

};

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

    List<RadialPath> m_targetRadialPaths;

    void Start()
    {
        if (m_enemy == null)
        {
            m_enemy = GameObject.Find("Enemy");
        }
        m_enemySize = m_enemy.GetComponent<SpriteRenderer>().sprite.rect.size;

        m_targetRadialPaths = new List<RadialPath>();

        
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

        float randomSign = Random.value > 0.5f ? -1.0f : 1.0f;

        if (Random.value > 0.5f)
        {
            ret.x = Random.Range(-m_spawnArea.x, m_spawnArea.y);
            ret.y = m_spawnArea.y * randomSign;
        }
        else
        {
            ret.x = m_spawnArea.x * randomSign;
            ret.y = Random.Range(-m_spawnArea.x, m_spawnArea.y);
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
            // newSpawnPosition += m_target.transform.position;

            /* --- swarm pathing --- */
            // if the previous radial path around the target is full or it's the first, start a new one

            Debug.Assert(m_targetRadialPaths != null);

            if (m_targetRadialPaths.Count == 0)
            {
                RadialPath rp = new RadialPath();
                rp.m_centerPos = m_target.transform.position;
                rp.m_innerRadius = 2.0f;
                rp.m_outerRadius = rp.m_innerRadius + m_enemySize.magnitude + 2.0f;
                rp.m_circumference = 2 * Mathf.PI * rp.m_outerRadius - 2 * Mathf.PI * rp.m_innerRadius;

                m_targetRadialPaths.Add(rp);
                Debug.Log("creatde a radial path.");
            }

            GameObject newEnemy = Instantiate(m_enemy);
            newEnemy.transform.position = newSpawnPosition;
            newEnemy.GetComponent<Enemy>().m_distanceToTarget = 0.0f;
            // Debug.Log("enemy: [" + newEnemy.name + "] Position: [" + newSpawnPosition + "]");
            newEnemy.GetComponent<Enemy>().setTarget(m_target);
            newEnemy.GetComponent<Enemy>().setRadialPath(m_targetRadialPaths.LastOrDefault());
            newEnemy.GetComponent<Enemy>().m_pathingType = PathingType.Radial; // is setting like this ok?

            newEnemy.gameObject.SetActive(true);

            RadialPath lastRadialPath = m_targetRadialPaths.Last();
            lastRadialPath.m_circumference -= m_enemySize.magnitude;
            m_numEnemiesSpawned++;
        }
    }
}
