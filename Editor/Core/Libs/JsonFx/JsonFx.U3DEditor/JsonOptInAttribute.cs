using System;
namespace JsonFx.U3DEditor
{
	/** Specifies that members of this class that should be serialized must be explicitly specified.
	 * Classes that this attribute is applied to need to explicitly
	 * declare every member that should be serialized with the JsonMemberAttribute.
	 * \see JsonMemberAttribute
	 */
	public class JsonOptInAttribute : Attribute
	{
		public JsonOptInAttribute ()
		{
			
		}
	}

	public class JsonUseTypeHintAttribute : Attribute
	{
		public JsonUseTypeHintAttribute () {}
	}

    /// <summary>
    /// 序列化的时候将类信息写入，反序列的时候直接生成对应类的对象
    /// </summary>
    public class JsonClassTypeAttribute : Attribute
    {
        public JsonClassTypeAttribute() { }
    }
}

