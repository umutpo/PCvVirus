using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public struct RadialPath
{
    public float m_innerRadius { get; set; }
    public float m_outerRadius { get; set; }
    public float m_area;
    public float m_availableSpace;
    public float m_annulusSpacingFactor;
    public Vector3 m_centerPos { get; set; }

    private int m_numCreaturesOnPath;
    private float m_angleBetweenCreatures;
    public float m_currentRotationAngle;

    public RadialPath(Vector3 center, float outerRadius)
    {
        m_centerPos = center;
        m_innerRadius = outerRadius * 0.5f; // just take half for now?
        m_outerRadius = outerRadius;
        m_area = Mathf.PI * ((m_outerRadius * m_outerRadius) - (m_innerRadius * m_innerRadius));
        m_availableSpace = -1.0f;
        m_numCreaturesOnPath = 0;
        m_angleBetweenCreatures = 0f;
        m_currentRotationAngle = 0f;
        m_annulusSpacingFactor = 1.4f; // self-explanatory right?
    }

    public float getMAxAvailableSpace()
    {
        return Mathf.PI * 2.0f * ((m_outerRadius - m_innerRadius) / 2.0f);
    }

    public void RecalculateDesignatedPositions()
    {
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

    public bool canAcceptNewCreature(Vector2 creatureSize)
    {
        if (m_availableSpace < 0f)
        {
            m_availableSpace = getMAxAvailableSpace();
        }

        // picking the max between x and y is not great. The alternative is to rebalance the swarm 
        // when the creature's larger axis crosses the normal with the core, bleh, dont feel like doing that... 
        float spaceRequired = Mathf.Max(creatureSize.x, creatureSize.y) * m_annulusSpacingFactor;
        float spaceRemaining = m_availableSpace - spaceRequired;

        return spaceRemaining > 0f;
    }

    public void addCreature(int pathIndex, Enemy newCreature)
    {
        float spaceRequired = Mathf.Max(newCreature.m_spriteSize.x, newCreature.m_spriteSize.y) * m_annulusSpacingFactor;
        m_availableSpace -= spaceRequired;
        m_numCreaturesOnPath++;

        newCreature.m_pathIndex = pathIndex;
        newCreature.m_indexInPath = m_numCreaturesOnPath - 1;
        RecalculateDesignatedPositions();
    }

    public void removeCreature(Enemy creatureToRemove)
    {
        float spaceRequired = Mathf.Max(creatureToRemove.m_spriteSize.x, creatureToRemove.m_spriteSize.y) * m_annulusSpacingFactor;
        m_availableSpace += spaceRequired;
        Debug.Assert(m_availableSpace <= getMAxAvailableSpace(), "Available space exceeded max space after removing creature!");
        m_numCreaturesOnPath--;

        creatureToRemove.m_pathIndex = -1;
        creatureToRemove.m_indexInPath = -1;
        RecalculateDesignatedPositions();
    }
    
    public void setNewRadii(float innerRadius, float outerRadius)
    {
        float oldMaxAvailableSpace = getMAxAvailableSpace();

        m_innerRadius = innerRadius;
        m_outerRadius = outerRadius;
        m_area = (Mathf.PI * 2.0f * m_outerRadius) - (Mathf.PI * 2.0f * m_innerRadius);

        float currentAvailableSpace = m_availableSpace;
        float newMaxAvailableSpace = getMAxAvailableSpace();

        // i dunno if there is a situation where the radius would get smaller and we need to expel creatures 
        if (newMaxAvailableSpace > oldMaxAvailableSpace)
        {
            m_availableSpace = newMaxAvailableSpace - currentAvailableSpace;
        }

        RecalculateDesignatedPositions();
    }
}

public class SwarmController : MonoBehaviour
{
    public List<RadialPath> m_radialPaths;
    public Vector3 m_targetCenterPos;

    public float m_pathRotationSpeed = 10.0f;
    public float m_spaceBetweenRadialPaths = 2.0f;
    ;
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

    public void addRadialPath(Vector3 centerPos, float outerRadius)
    {
        RadialPath newPath = new RadialPath(centerPos, outerRadius);
        m_radialPaths.Add(newPath);
        Debug.Log($"added new radial path. total paths: {m_radialPaths.Count}");
    }

    public void addCreatureToLastRadialPath(Enemy newEnemy)
    {
        // assuming the paths are balanced, a simple first step is to always add new enemies at the outer-most ring
        RadialPath targetRadialPath = m_radialPaths.Last();

        bool canFitIntoPath = targetRadialPath.canAcceptNewCreature(newEnemy.m_spriteSize);
        if (canFitIntoPath)
        {
            targetRadialPath.addCreature(m_radialPaths.Count() - 1, newEnemy);
            m_radialPaths[m_radialPaths.Count - 1] = targetRadialPath;
        }
        else
        {
            // add a new radial path
            addRadialPath(m_targetCenterPos, getNextOuterRadius());
            addCreatureToLastRadialPath(newEnemy);
        }
    }

    public float getNextOuterRadius()
    {
        if (m_radialPaths.Count == 0)
        {
            return 2.0f; // default radius for the first path
        }
        else
        {
            RadialPath lastPath = m_radialPaths.Last();
            return lastPath.m_outerRadius + m_spaceBetweenRadialPaths; // increase radius by the spacing factor
        }
    }

    public void rebalanaceRadialPaths()
    {
        Debug.Log("Rebalancing radial paths...");
        float lastOuterRadius = 0f;
        float lastInnerRadius = 0f;
        for (int i = 0; i < m_radialPaths.Count; i++)
        {
            RadialPath path = m_radialPaths[i];

            // start from the first path, get it's outer radius, and then set the next path's inner radius to that outer radius
            if (lastOuterRadius == 0f)
            {
                lastOuterRadius = path.m_outerRadius;
                lastInnerRadius = path.m_innerRadius;
                continue;
            }

            // set the inner radius of the current path to the outer radius of the last path
            path.m_innerRadius = lastOuterRadius;
            path.m_outerRadius = lastOuterRadius + m_spaceBetweenRadialPaths;
            path.m_area = (Mathf.PI * 2.0f * path.m_outerRadius) - (Mathf.PI * 2.0f * path.m_innerRadius);
            path.m_availableSpace = path.getMAxAvailableSpace();
        }
    }

    public void assignCreatureToSwarm(Enemy newEnemy)
    {

        if (m_radialPaths.Count == 0)
        {
            addRadialPath(m_targetCenterPos, getNextOuterRadius());
        }

        addCreatureToLastRadialPath(newEnemy);
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