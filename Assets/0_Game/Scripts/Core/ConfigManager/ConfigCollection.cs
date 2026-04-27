using UnityEngine;

public abstract class ConfigCollection : ScriptableObject
{
    public string ID => GetType().FullName;
}
