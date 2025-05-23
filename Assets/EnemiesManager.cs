using System.Collections.Generic;
using System.Linq;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public struct RadialPath
{
    public float m_innerRadius { get; set; }
    public float m_outerRadius { get; set; }
    public float m_circumference { get; set; }
    public Vector3 m_centerPos { get; set; }
    public float m_spaceTaken { get; set; }

    public float spaceAvaiable()
    {
        return m_circumference - m_spaceTaken;
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

    private void spawnEnemy()
    {
        if (m_spawnEnemies && m_numEnemiesSpawned < m_maxSpawns)
        {
            Vector3 newSpawnPosition = new Vector3(
                UnityEngine.Random.Range(-m_spawnArea.x, m_spawnArea.y),
                UnityEngine.Random.Range(-m_spawnArea.x, m_spawnArea.y),
                0f
            );

            // better if it's near the player i guess?
            newSpawnPosition += m_target.transform.position;

            // if the previous radial path around the target is full or it's the first, start a new one
            if (m_targetRadialPaths.Count == 0)
            {
                RadialPath rp = new RadialPath();
                rp.m_centerPos = m_target.transform.position;
                rp.m_innerRadius = 2.0f;
                rp.m_outerRadius = rp.m_innerRadius + m_enemySize.magnitude + 2.0f;
                rp.m_circumference = 2 * Mathf.PI * rp.m_outerRadius - 2 * Mathf.PI * rp.m_innerRadius;

                m_targetRadialPaths.Add(rp);
            }

            GameObject newEnemy = Instantiate(m_enemy);
            newEnemy.transform.position = newSpawnPosition;
            newEnemy.GetComponent<Enemy>().setTarget(m_target);
            newEnemy.GetComponent<Enemy>().setRadialPath(m_targetRadialPaths.LastOrDefault());
            newEnemy.gameObject.SetActive(true);

            RadialPath lastRadialPath = m_targetRadialPaths.Last();
            lastRadialPath.m_circumference -= m_enemySize.magnitude;
            m_numEnemiesSpawned++;
        }
    }
}
