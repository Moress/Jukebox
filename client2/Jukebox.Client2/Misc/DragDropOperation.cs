using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Jukebox.Client2.JukeboxService;

namespace ListBoxDragReorder
{
	public enum DropPosition
	{
		After,
		Before,
		Inside
	}

	/// <summary>
	/// Helper class, used in drag-reordering.
	/// </summary>
	public class DragDropOperation
	{
		public DropPosition DropPosition
		{
			get;
			set;
		}

		public object Payload
		{
			get;
			set;
		}
	}
}
