using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Managers;

public static class LFCMapObjectsManager
{
    public static void SpawnOutsideMapObjectsForServer(int min, int max, Action<Vector3, Quaternion> spawnAction)
    {
        System.Random random = new System.Random();
        for (int i = 0; i < random.Next(min, max); i++)
        {
            GameObject[] outsideAINodes = RoundManager.Instance.outsideAINodes;
            Vector3 position = outsideAINodes[random.Next(0, outsideAINodes.Length)].transform.position;
            position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, default, random) + Vector3.up;
            spawnAction(position, Quaternion.identity);
        }
    }

    public static void SpawnScatteredMapObjectsForServer(int mapObjectsAmount, int minInside, int minOutside, Action<Vector3, bool> spawnAction)
    {
        if (!LFCUtilities.IsServer || RoundManager.Instance == null) return;

        System.Random random = new System.Random();
        const float minDistance = 50f;
        List<Vector3> selectedPositions = [];
        StartOfRound.Instance.allPlayerScripts.Where(p => !p.isPlayerDead).ToList().ForEach(p => selectedPositions.Add(p.transform.position));

        List<GameObject> insideNodes = RoundManager.Instance.insideAINodes.Where(n => n != null).ToList() ?? [];
        List<GameObject> outsideNodes = RoundManager.Instance.outsideAINodes.Where(n => n != null).ToList() ?? [];

        LFCUtilities.Shuffle(insideNodes);
        LFCUtilities.Shuffle(outsideNodes);

        List<bool> mapObjectTypes = new(mapObjectsAmount);
        minInside = Mathf.Clamp(minInside, 0, mapObjectsAmount);
        minOutside = Mathf.Clamp(minOutside, 0, mapObjectsAmount - minInside);

        for (int i = 0; i < minInside; i++) mapObjectTypes.Add(false);
        for (int i = 0; i < minOutside; i++) mapObjectTypes.Add(true);

        while (mapObjectTypes.Count < mapObjectsAmount)
            mapObjectTypes.Add(random.Next(0, 2) == 0);

        foreach (bool isOutside in mapObjectTypes)
        {
            float maxDistance = float.MinValue;
            Vector3 bestPosition = Vector3.zero;
            GameObject lastNodeSaved = null;

            List<GameObject> nodes = isOutside ? outsideNodes : insideNodes;
            foreach (GameObject node in nodes)
            {
                Vector3 candidatePosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(node.transform.position, 10f, default, random) + Vector3.up;
                if (!Physics.Raycast(candidatePosition, Vector3.down, out RaycastHit hit, 5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) continue;

                Vector3 validPosition = hit.point;

                // Calculer la distance minimale avec les positions sélectionnées
                float minDistanceToSelected = selectedPositions.Count > 0
                    ? selectedPositions.Min(p => Vector3.Distance(p, validPosition))
                    : float.MaxValue;

                // Garder la position la plus éloignée des autres sélectionnées
                if (minDistanceToSelected > minDistance || minDistanceToSelected > maxDistance)
                {
                    maxDistance = minDistanceToSelected;
                    bestPosition = validPosition;
                    lastNodeSaved = node;

                    if (minDistanceToSelected > minDistance) break;
                }
            }

            if (bestPosition != Vector3.zero)
            {
                selectedPositions.Add(bestPosition);
                _ = nodes.Remove(lastNodeSaved);

                spawnAction(bestPosition, isOutside);
            }
        }
    }

    public static void AttachMapObjectForEveryone(PlayerControllerB player, GameObject mapObject)
    {
        if (Physics.Raycast(player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward, Vector3.down, out RaycastHit hitInfo, 80f, 1342179585, QueryTriggerInteraction.Ignore))
        {
            PlayerPhysicsRegion physicsRegion = hitInfo.collider.gameObject.transform.GetComponentInChildren<PlayerPhysicsRegion>();
            if (physicsRegion?.parentNetworkObject != null && physicsRegion.allowDroppingItems && physicsRegion.itemDropCollider.ClosestPoint(hitInfo.point) == hitInfo.point)
            {
                mapObject.transform.SetParent(physicsRegion.physicsTransform);
                mapObject.transform.localPosition = physicsRegion.physicsTransform.InverseTransformPoint(hitInfo.point + (Vector3.up * 0.04f) + physicsRegion.addPositionOffsetToItems);
                mapObject.transform.rotation = Quaternion.Euler(0f, player.transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);
            }
        }
    }
}
