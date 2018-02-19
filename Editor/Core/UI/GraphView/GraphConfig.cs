using System;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonOptIn]
    [Serializable]
    public class GraphConfig
    {
        [SerializeField]
        public bool allowClick = true;
    }
}
