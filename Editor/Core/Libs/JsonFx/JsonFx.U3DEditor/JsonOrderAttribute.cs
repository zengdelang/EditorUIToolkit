using System;

namespace JsonFx.U3DEditor
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
	public class JsonOrderAttribute : Attribute
	{
		public int order;

		public JsonOrderAttribute (int order) {
			this.order = order;
		}
	}
}

