using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public struct RadialPath
{
    public float m_innerRadius { get; set; }
    public float m_outerRadius { get; set; }
    public float m_rotatedRadius { get; set; }
    public float m_area;
    public float m_availableSpace;
    public float m_annulusSpacingFactor;
    public Vector3 m_centerPos { get; set; }

    public int m_numCreaturesOnPath;
    private float m_angleBetweenCreatures;
    public float m_currentRotationAngle;

    public bool m_rebalanceTrigger;

    public RadialPath(Vector3 center, float innerRadius)
    {
        m_centerPos = center;
        m_innerRadius = innerRadius;
        m_outerRadius = innerRadius + 1.0f; // will be adjusted based on sprite size when creatures are added
        m_rotatedRadius = (m_outerRadius + m_innerRadius) / 2.0f;
        m_area = Mathf.PI * ((m_outerRadius * m_outerRadius) - (m_innerRadius * m_innerRadius));
        m_availableSpace = Mathf.PI * 2.0f * m_rotatedRadius;
        m_numCreaturesOnPath = 0;
        m_angleBetweenCreatures = 0f;
        m_currentRotationAngle = 0f;
        m_annulusSpacingFactor = 1.0f; // self-explanatory right?
        m_rebalanceTrigger = false;
    }

    public float getMaxAvailableSpace()
    {
        return Mathf.PI * 2.0f * ((m_outerRadius + m_innerRadius) / 2.0f);
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

        float x = m_centerPos.x + m_rotatedRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = m_centerPos.y + m_rotatedRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        return new Vector3(x, y, m_centerPos.z);
    }

    public void RotatePath(float rotationSpeed, float deltaTime)
    {
        m_currentRotationAngle += rotationSpeed * deltaTime;
        // Keep angle within 0-360 degrees
        if (m_currentRotationAngle >= 360f) m_currentRotationAngle -= 360f;
        if (m_currentRotationAngle < 0f) m_currentRotationAngle += 360f;
    }

    // creatureSize in world units, not pixels
    public bool canAcceptNewCreature(Vector2 creatureSize)
    {
        // picking the max between x and y is not great. The alternative is to rebalance the swarm 
        // when the creature's larger axis crosses the normal with the core, bleh, dont feel like doing that... 
        float spaceRequired = Mathf.Max(creatureSize.x, creatureSize.y) * m_annulusSpacingFactor;
        float spaceRemaining = m_availableSpace - spaceRequired;
        Debug.Log($"Checking if path can accept new creature. Required space: {spaceRequired}, Available space: {m_availableSpace}, Remaining space: {spaceRemaining}");
        return spaceRemaining > 0f;
    }

    public void addCreature(int pathIndex, Enemy newCreature)
    {
        // if the new creature is larger than the path's annulus length, we need to expand the path's radius
        float maxCreatureSize = Mathf.Max(newCreature.m_spriteSize.x, newCreature.m_spriteSize.y) / newCreature.m_ppu;
        
        if (maxCreatureSize > (m_outerRadius - m_innerRadius))
        {
            Debug.LogWarning($"Creature size {maxCreatureSize} exceeds path radius {m_outerRadius - m_innerRadius}. Expanding path Radius");
            float newOuterRadius = maxCreatureSize + m_innerRadius;
            setNewRadii(m_innerRadius, newOuterRadius);
            m_rebalanceTrigger = true;
        }

        // add the creature
        float spaceRequired = Mathf.Max(newCreature.m_spriteSize.x, newCreature.m_spriteSize.y) / newCreature.m_ppu * m_annulusSpacingFactor;
        m_availableSpace -= spaceRequired;
        m_numCreaturesOnPath++;

        // no good way to keep track of where each creature is in the path yet
        newCreature.m_pathIndex = pathIndex;
        newCreature.m_indexInPath = m_numCreaturesOnPath - 1;
        RecalculateDesignatedPositions();
        Debug.Log($"Added creature to path {pathIndex}. Available space: {m_availableSpace}, Num creatures: {m_numCreaturesOnPath}");
    }

    public void removeCreature(Enemy creatureToRemove)
    {
        float spaceRequired = Mathf.Max(creatureToRemove.m_spriteSize.x, creatureToRemove.m_spriteSize.y) / creatureToRemove.m_ppu * m_annulusSpacingFactor;
        m_availableSpace += spaceRequired;
        Debug.Assert(m_availableSpace <= getMaxAvailableSpace(), "Available space exceeded max space after removing creature!");
        m_numCreaturesOnPath--;

        creatureToRemove.m_pathIndex = -1;
        creatureToRemove.m_indexInPath = -1;
        RecalculateDesignatedPositions();
    }
    
    public void setNewRadii(float innerRadius, float outerRadius)
    {
        float oldMaxAvailableSpace = getMaxAvailableSpace();

        m_innerRadius = innerRadius;
        m_outerRadius = outerRadius;
        m_rotatedRadius = (m_outerRadius + m_innerRadius) / 2.0f;
        m_area = (Mathf.PI * 2.0f * m_outerRadius) - (Mathf.PI * 2.0f * m_innerRadius);

        float currentAvailableSpace = m_availableSpace;
        float newMaxAvailableSpace = getMaxAvailableSpace();

        // i dunno if there is a situation where the radius would get smaller and we need to expel creatures 
        if (newMaxAvailableSpace > oldMaxAvailableSpace)
        {
            m_availableSpace = newMaxAvailableSpace - currentAvailableSpace;
        }
        Debug.Log($"Set new radii: Inner Radius = {m_innerRadius}, Outer Radius = {m_outerRadius}, maxAvailableSapce = {getMaxAvailableSpace()}, Available Space = {m_availableSpace}");

        RecalculateDesignatedPositions();
    }
}

public class SwarmController : MonoBehaviour
{
    [SerializeField]
    public List<RadialPath> m_radialPaths;
    public Vector3 m_targetCenterPos;

    public float m_pathRotationSpeed = 100.0f;
    public float m_spaceBetweenRadialPaths = 0.2f;

    void Awake()
    {
        if (m_radialPaths == null)
        {
            m_radialPaths = new List<RadialPath>();
        }
    }

    void Update()
    {
        bool rebalanceRequired = false;
        for (int i = 0; i < m_radialPaths.Count; i++)
        {
            // this feels very stupid...please excuse me
            RadialPath currentPath = m_radialPaths[i];
            currentPath.RotatePath(m_pathRotationSpeed, Time.deltaTime);
            if (currentPath.m_rebalanceTrigger)
            {
                rebalanceRequired = true;
                currentPath.m_rebalanceTrigger = false;
            }
            m_radialPaths[i] = currentPath;
        }
        if (rebalanceRequired)
        {
            rebalanaceRadialPaths();
        }
    }

    public void addRadialPath(Vector3 centerPos, float innerRadius)
    {
        RadialPath newPath = new RadialPath(centerPos, innerRadius);
        m_radialPaths.Add(newPath);
        Debug.Log($"added new radial path. IR: {newPath.m_innerRadius}, OR {newPath.m_outerRadius}, RR: {newPath.m_rotatedRadius}, Free Space: {newPath.m_availableSpace}, total paths: {m_radialPaths.Count}");
    }

    public void addCreatureToLastRadialPath(Enemy newEnemy)
    {
        // assuming the paths are balanced, a simple first step is to always add new enemies at the outer-most ring
        RadialPath targetRadialPath = m_radialPaths.Last();

        if (targetRadialPath.m_numCreaturesOnPath == 0)
        {
            // if the path is empty, we can add the creature without checking for space
            targetRadialPath.addCreature(m_radialPaths.Count - 1, newEnemy);
            m_radialPaths[m_radialPaths.Count - 1] = targetRadialPath;
            newEnemy.m_pathIndex = m_radialPaths.Count - 1;
            newEnemy.m_indexInPath = 0;
            return;
        }

        bool canFitIntoPath = targetRadialPath.canAcceptNewCreature(newEnemy.m_spriteSize / newEnemy.m_ppu);
        if (canFitIntoPath)
        {
            int pathIndex = m_radialPaths.Count - 1;
            targetRadialPath.addCreature(pathIndex, newEnemy);
            m_radialPaths[m_radialPaths.Count - 1] = targetRadialPath;
            newEnemy.m_pathIndex = pathIndex;
            newEnemy.m_indexInPath = targetRadialPath.m_numCreaturesOnPath - 1;
        }
        else
        {
            // add a new radial path
            addRadialPath(m_targetCenterPos, getNextInnerRadius());
            addCreatureToLastRadialPath(newEnemy); // drama alert: recursion
        }
    }

    public float getNextInnerRadius()
    {
        if (m_radialPaths.Count == 0)
        {
            return 0.75f; // default radius for the first path
        }
        else
        {
            RadialPath lastPath = m_radialPaths.Last();
            return lastPath.m_outerRadius + m_spaceBetweenRadialPaths; // increase radius by the spacing factor
        }
    }

    public void rebalanaceRadialPaths()
    {
        if (m_radialPaths.Count < 2)
        {
            Debug.LogWarning("Not enough radial paths to rebalance.");
            return;
        }

        Debug.Log("Rebalancing radial paths...");
        float lastOuterRadius = 0f;

        for (int i = 0; i < m_radialPaths.Count; i++)
        {
            Debug.Log($"Rebalancing path {i}.");
            RadialPath path = m_radialPaths[i];
            Debug.Log($"Rebalancing path {i}: Inner Radius = {path.m_innerRadius}, Outer Radius = {path.m_outerRadius}");

            // start from the first path, get it's outer radius, and then set the next path's inner radius to that outer radius
            if (lastOuterRadius == 0f)
            {
                lastOuterRadius = path.m_outerRadius;
                continue;
            }

            // set the inner radius of the current path to the outer radius of the last path
            float currentAnnulusLength = path.m_outerRadius - path.m_innerRadius;
            float newInnerRadius = lastOuterRadius + m_spaceBetweenRadialPaths;
            float newOuterRadius = path.m_innerRadius + currentAnnulusLength;
            path.setNewRadii(newInnerRadius, newOuterRadius);
            lastOuterRadius = path.m_outerRadius;
        }
    }

    public void assignCreatureToSwarm(Enemy newEnemy)
    {

        if (m_radialPaths.Count == 0)
        {
            addRadialPath(m_targetCenterPos, getNextInnerRadius());
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
