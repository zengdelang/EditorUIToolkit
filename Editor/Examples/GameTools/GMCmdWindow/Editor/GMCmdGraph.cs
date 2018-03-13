using System;
using EUTK;

namespace UGT
{
    public class GMCmdGraph : Graph
    {
        public override Type baseNodeType
        {
            get { return typeof(GMCmdNodeBase); }
        }
    }
}
