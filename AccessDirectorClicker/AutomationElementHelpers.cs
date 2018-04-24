using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace AccessDirectorClicker
{
    // taken from : https://blogs.msdn.microsoft.com/oldnewthing/20141013-00/?p=43863
    internal static class AutomationElementHelpers
    {
        public static AutomationElement Find(this AutomationElement root, string name)
        {
            return root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name));
        }

        public static IEnumerable<AutomationElement> EnumChildButtons(this AutomationElement parent)
        {
            return parent == null
                ? Enumerable.Empty<AutomationElement>()
                : parent.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty,
                        ControlType.Button)).Cast<AutomationElement>();
        }

        public static bool InvokeButton(this AutomationElement button)
        {
            var invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            invokePattern?.Invoke();
            return invokePattern != null;
        }
    }
}