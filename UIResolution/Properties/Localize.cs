namespace UIResolution
{
	public class Localize
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static ModsCommon.LocalizeManager LocaleManager {get;} = new ModsCommon.LocalizeManager("Localize", typeof(Localize).Assembly);

		/// <summary>
		/// Change game UI resolution
		/// </summary>
		public static string Mod_Description => LocaleManager.GetString("Mod_Description", Culture);

		/// <summary>
		/// [NEW] Added UI scale setting (game graphics tab).
		/// </summary>
		public static string Mod_WhatsNewMessage1_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_1", Culture);

		/// <summary>
		/// [NEW] Added missing dependency checker.
		/// </summary>
		public static string Mod_WhatsNewMessage1_1_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_1_1", Culture);

		/// <summary>
		/// [TRANSLATION] Added Italian and Korean translations.
		/// </summary>
		public static string Mod_WhatsNewMessage1_1_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_1_2", Culture);

		/// <summary>
		/// [UPDATED] Added Plazas & Promenades DLC support.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_2", Culture);

		/// <summary>
		/// UI Scale ({0}%)
		/// </summary>
		public static string UIScale => LocaleManager.GetString("UIScale", Culture);
	}
}