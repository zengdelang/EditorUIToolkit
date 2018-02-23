using System;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class GraphConfig
    {
        [JsonMember] [SerializeField] public bool allowClick = true;
    }
}
