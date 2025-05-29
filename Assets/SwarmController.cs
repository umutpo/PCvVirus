using System.Collections.Generic;
using UnityEngine;

public struct RadialPath
{
    public float m_innerRadius { get; set; }
    public float m_outerRadius { get; set; }
    public Vector3 m_centerPos { get; set; }

    private int m_numCreaturesOnPath;
    private float m_angleBetweenCreatures; 
    public float m_currentRotationAngle; 

    public RadialPath(Vector3 center, float outerRadius)
    {
        m_centerPos = center;
        m_innerRadius = outerRadius * 0.5f; // just take half for now?
        m_outerRadius = outerRadius;
        m_numCreaturesOnPath = 0;
        m_angleBetweenCreatures = 0f;
        m_currentRotationAngle = 0f;
    }

    public void RecalculateDesignatedPositions(int numCreatures)
    {
        m_numCreaturesOnPath = numCreatures;
        if (m_numCreaturesOnPath == 0)
        {
            m_angleBetweenCreatures = 0f;
            return;
        }
        m_angleBetweenCreatures = 360f / m_numCreaturesOnPath;
    }

    public Vector3 GetDesignatedPosition(int creatureIndex)
    {
        if (creatureIndex < 0 || creatureIndex >= m_numCreaturesOnPath)
        {
            return m_centerPos;
        }

        float angle = (creatureIndex * m_angleBetweenCreatures) + m_currentRotationAngle;

        float x = m_centerPos.x + m_outerRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = m_centerPos.y + m_outerRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        return new Vector3(x, y, m_centerPos.z);
    }

    public void RotatePath(float rotationSpeed, float deltaTime)
    {
        m_currentRotationAngle += rotationSpeed * deltaTime;
        // Keep angle within 0-360 degrees
        if (m_currentRotationAngle >= 360f) m_currentRotationAngle -= 360f;
        if (m_currentRotationAngle < 0f) m_currentRotationAngle += 360f;
    }
}

public class SwarmController : MonoBehaviour
{
    public List<RadialPath> m_radialPaths;

    public float m_pathRotationSpeed = 10.0f;

    void Awake()
    {
        if (m_radialPaths == null)
        {
            m_radialPaths = new List<RadialPath>();
        }
    }

    void Update()
    {
        for (int i = 0; i < m_radialPaths.Count; i++)
        {
            // this feels very stupid...please excuse me
            RadialPath currentPath = m_radialPaths[i];
            currentPath.RotatePath(m_pathRotationSpeed, Time.deltaTime);
            m_radialPaths[i] = currentPath;
        }
    }

    public void AddRadialPath(Vector3 centerPos, float outerRadius, float enemySizeMagnitude)
    {
        RadialPath newPath = new RadialPath(centerPos, outerRadius);
        m_radialPaths.Add(newPath);
        Debug.Log($"added new radial path. total paths: {m_radialPaths.Count}");
    }

    public Vector3 assignCreatureToPath(Vector3 centerPos, float outerRadius, float enemySizeMagnitude, int creatureIndex)
    {
        int pathIndex = 0; 
        if (m_radialPaths.Count == 0)
        {
            AddRadialPath(centerPos, outerRadius, enemySizeMagnitude);
        }

        if (pathIndex >= 0 && pathIndex < m_radialPaths.Count)
        {
            RadialPath targetPath = m_radialPaths[pathIndex];
            
            // need to recalculate all positions. this isn't very smooth but i like the final result
            targetPath.RecalculateDesignatedPositions(creatureIndex + 1); 
            m_radialPaths[pathIndex] = targetPath; // Update the struct in the list

            // return the position in the radial path to set it on the target
            return targetPath.GetDesignatedPosition(creatureIndex);
        }
        Debug.LogError("failed to assign creature to non-existent radial path index: " + pathIndex);
        return Vector3.zero; // is this gonna just make them teleport somewhere? should never hit it anyway
    }
    public Vector3 GetCurrentDesignatedPosition(int pathIndex, int creatureIndex)
    {
        if (pathIndex >= 0 && pathIndex < m_radialPaths.Count)
        {
            return m_radialPaths[pathIndex].GetDesignatedPosition(creatureIndex);
        }
        return Vector3.zero;
    }
}