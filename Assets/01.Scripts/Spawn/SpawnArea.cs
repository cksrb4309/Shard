using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnArea", menuName = "Spawn/SpawnArea")]
[System.Serializable]
public class SpawnArea : ScriptableObject
{
    public List<Area> frontSpawnArea;
    public List<Area> backSpawnArea;
    public List<Area> midSpawnArea;
}
