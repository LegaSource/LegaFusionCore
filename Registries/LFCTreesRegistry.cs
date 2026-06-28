using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public class LFCTreesRegistry
{
    private static readonly HashSet<GameObject> TreeRegistry = [];

    public static void AddTree(GameObject treeObject) => TreeRegistry.Add(treeObject);
    public static void RemoveTree(GameObject treeObject) => TreeRegistry.Remove(treeObject);
    public static void ClearTrees() => TreeRegistry.Clear();
    public static bool TryGetTreeAtPosition(Vector3 position, out GameObject bestTree)
    {
        bestTree = null;
        float bestDistance = float.MaxValue;

        foreach (GameObject tree in GetTrees())
        {
            float distance = Vector3.Distance(position, tree.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTree = tree;
            }
        }

        return bestTree != null;
    }
    public static List<GameObject> GetTreesAtPosition(Vector3 position, float range = 5f)
    {
        List<GameObject> trees = [];

        foreach (GameObject treeObject in GetTrees())
        {
            if (Vector3.Distance(position, treeObject.transform.position) < range)
                trees.Add(treeObject);
        }

        return trees;
    }
    public static List<GameObject> GetTrees() => TreeRegistry.Where(t => t != null).ToList();
}
