using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RadItemsControl = Telerik.Windows.Controls.ItemsControl;

namespace ListBoxDragReorder
{
	public static class ControlExtensions
	{
		public static ItemsControl FindItemsConrolParent(this FrameworkElement target)
		{
			ItemsControl result = null;
			result = target.Parent as ItemsControl;
			if (result != null)
			{
				return result;
			}

			result = RadItemsControl.ItemsControlFromItemContainer(target);
			if (result != null)
			{
				return result;
			}

			return FindVisualParent<ItemsControl>(target);
		}

		public static T FindVisualParent<T>(FrameworkElement target) where T : FrameworkElement
		{
			var visParent = VisualTreeHelper.GetParent(target);
			var result = visParent as T;
			if (result != null)
			{
				return result;
			}
			return FindVisualParent<T>(visParent as FrameworkElement);
		}
	}
}
