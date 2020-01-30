using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public struct RPCInfo
{
    public string name;
    public string declaringType;
    public MethodInfo info;

    public RPCInfo(string name, string declaringType, MethodInfo info)
    {
        this.name = name;
        this.declaringType = declaringType;
        this.info = info;
    }
}

public class RpcManager : MonoBehaviour
{
    public static RpcManager Instance;
    public Dictionary<string, RPCInfo> Rpcs = new Dictionary<string, RPCInfo>();

    private void Awake()
    {
        Instance = this;
        Assembly assembly = Assembly.GetExecutingAssembly();

        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(NetRPCAttribute), false).Length > 0)
            .ToArray();

        foreach (var info in methods)
        {
            var rInfo = new RPCInfo(info.Name, info.DeclaringType.FullName, info);
            Rpcs.Add(info.Name, rInfo);
            Debug.Log(rInfo.declaringType);
        }
    }

    public bool IsCharacter(string name)
    {
        return Rpcs[name].declaringType.Contains("Assets.Scripts.Character");
    }
    
    public bool IsAudio(string name)
    {
        return Rpcs[name].declaringType.Contains("AudioManager");
    }
}

public class NetRPCAttribute : Attribute
{
}