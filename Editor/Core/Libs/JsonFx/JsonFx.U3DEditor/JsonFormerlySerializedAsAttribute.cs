using System;
namespace JsonFx.U3DEditor
{
	/** Indicate that this was previously serialized as [name] and can load values from fields with that name in a json file */
	public class JsonFormerlySerializedAsAttribute : Attribute
	{
		public string name;
		public JsonFormerlySerializedAsAttribute (string name)
		{
			this.name = name;
		}
	}
}

