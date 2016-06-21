using System;
using Xamarin.Forms;

namespace Onyx.Controls.Xamarin
{
	public class WrapLayoutSelectedTemplate : Grid
	{
		
		private bool isSelected = false;
		public bool IsSelected
		{
			get { return isSelected; }
			set { isSelected = value; SelectedValueChanged();  }
		}

		public WrapLayoutSelectedTemplate(View content) : base()
		{
			this.Padding = new Thickness(0);
			this.BackgroundColor = Color.Transparent;		
			this.Children.Add(content);
		}

		private void SelectedValueChanged()
		{
			if (IsSelected)
			{
				this.Padding = new Thickness(2);
				this.BackgroundColor = Color.Blue;
			}
			else
			{
				this.Padding = new Thickness(0);
				this.BackgroundColor = Color.Transparent;
			}
		}


	}
}

