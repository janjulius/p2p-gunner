using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

[CreateAssetMenu(fileName = "NetworkInfo", menuName = "Infos/NetworkInfo", order = 1)]
public class NetworkInfo : ScriptableObject
{
    public GameObject[] networkPrefabs;

    private string[] networkPrefabsNames;
    
    public CharacterInfo[] Characters;
    
    public void Init()
    {
        networkPrefabsNames = new string[networkPrefabs.Length];

        for (int i = 0; i < networkPrefabs.Length; i++)
        {
            networkPrefabsNames[i] = networkPrefabs[i].name;
        }
    }

    public GameObject GetNetworkPrefabByName(string prefabName)
    {
        GameObject res = null;
        foreach(var gm in networkPrefabs)
        {
            if (gm.name == prefabName)
                res = gm;
        }
        return res;
        //return networkPrefabs[Array.IndexOf(networkPrefabsNames, prefabName)];
    }

    public int GetNetworkPrefabId(string prefabName)
    {
        return Array.IndexOf(networkPrefabsNames, prefabName);
    }
}