using SimAirport.Logging;

namespace TBFlash.Escalators
{
    internal static class TBFlash_Escalators_Utils
    {
		internal const bool isTBFlashDebug = false;

		internal static void TBFlashLogger(Log log)
		{
			if (isTBFlashDebug)
			{
				Game.Logger.Write(log);
			}
		}
	}
}
