namespace SharpZipLib.U3DEditor.Zip.Compression
{
	/// <summary>
	/// This class stores the pending output of the Deflater.
	/// 
	/// author of the original java version : Jochen Hoenicke
	/// </summary>
	public class DeflaterPending : PendingBuffer
	{
		/// <summary>
		/// Construct instance with default buffer m_Size
		/// </summary>
		public DeflaterPending() : base(DeflaterConstants.PENDING_BUF_SIZE)
		{
		}
	}
}
