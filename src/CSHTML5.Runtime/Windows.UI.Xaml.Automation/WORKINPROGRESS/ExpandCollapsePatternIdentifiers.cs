#if WORKINPROGRESS
#if MIGRATION
namespace System.Windows.Automation
#else
namespace Windows.UI.Xaml.Automation
#endif
{
	//
	// Summary:
	//     Contains values used as automation property identifiers specifically for properties
	//     of the System.Windows.Automation.Provider.IExpandCollapseProvider pattern.
	public static partial class ExpandCollapsePatternIdentifiers
	{
		//
		// Summary:
		//     Identifies the System.Windows.Automation.Provider.IExpandCollapseProvider.ExpandCollapseState
		//     automation property.
		//
		// Returns:
		//     The automation property identifier.
		public static readonly AutomationProperty ExpandCollapseStateProperty;
	}
}
#endif