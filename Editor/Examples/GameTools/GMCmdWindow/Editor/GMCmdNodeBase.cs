using EUTK;
using JsonFx.U3DEditor;
using UnityEngine;

namespace UGT
{
    public class GMCmdNodeBase : Node
    {
        [JsonMember]
        [SerializeField]
        public string cmd = "";

        public override int maxOutConnections
        {
            get { return 0; }
        }

        public override int maxInConnections
        {
            get { return 0; }
        }
    }
}
