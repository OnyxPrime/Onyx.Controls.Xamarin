using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

/*
 * Thank you Jason Smith for the base setup for this layout.
 */

namespace Onyx.Controls.Xamarin
{
	public class WrapLayout : Layout<View>
	{
		public static void Init()
		{
		}

		static bool userTap = false;

		#region Bindable Properties
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(WrapLayout), null, propertyChanged: ItemTemplatePropertyChanged);

		static void ItemTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ApplyItemsSourceToTemplate((WrapLayout)bindable);
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(WrapLayout), null, propertyChanged: SelectedItemPropertyChanged);

		static void SelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// Need to handle the scenario where the SelectedItem property is changed programmatically.
			if (!userTap)
			{
				if (SelectedItemView != null) SelectedItemView = null;
				var self = (WrapLayout)bindable;
				for (int i = 0; i < self.Children.Count; i++)
				{
					var x = self.Children[i] as WrapLayoutSelectedTemplate;
					if (newValue != null && newValue.Equals(x.BindingContext))
					{
						SelectedItemView = x;
						SelectedItemView.IsSelected = true;
					}
				}
				userTap = false;
			}
		}

		public object SelectedItem
		{
			get { return (object)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, (object)value); }
		}

		protected void SelectedItemChanged(object oldValue, object newValue)
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				var x = this.Children[i] as WrapLayoutSelectedTemplate;
				if (newValue != null && newValue.Equals(x.BindingContext))
					x.IsSelected = !x.IsSelected;
				else if (x.IsSelected)
					x.IsSelected = false;
			}
		}

		static WrapLayoutSelectedTemplate selectedItemView = null;
		static WrapLayoutSelectedTemplate SelectedItemView
		{
			get { return selectedItemView; }
			set { selectedItemView = value; }
		}

		public static readonly BindableProperty SpacingProperty =
			BindableProperty.Create(nameof(Spacing), typeof(double), typeof(WrapLayout), 0.0, propertyChanged: OnSpacingChanged);

		static void OnSpacingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (WrapLayout)bindable;
			self.InvalidateMeasure();
		}

		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<object>), typeof(WrapLayout), null, propertyChanged: OnItemsSourceChanged);

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ApplyItemsSourceToTemplate((WrapLayout)bindable);
		}

		public IEnumerable<object> ItemsSource
		{
			get { return GetValue(ItemsSourceProperty) as IEnumerable<object>; }
			set { SetValue(ItemsSourceProperty, value); }
		}

		#endregion

		#region Overrides

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			var layout = ComputeNiaveLayout(widthConstraint, heightConstraint);
			var width = GetMaxRowWidth(layout);
			var last = layout[layout.Count - 1];
			var height = last[0].Bottom;
			return new SizeRequest(new Size(width, height));
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			var layout = ComputeLayout(width, height);
			int i = 0;
			foreach (var region in layout)
			{
				var child = Children[i];
				i++;
				LayoutChildIntoBoundingRegion(child, region);
			}
		}
		#endregion

		#region Static Methods
		static void ApplyItemsSourceToTemplate(WrapLayout layout)
		{
			layout.Children.Clear();
			var items = layout.ItemsSource;
			if (items == null)
				return;

			var template = layout.ItemTemplate ?? GetDefaultTemplate();

			for (int i = 0; i < items.Count(); i++)
			{
				var context = items.ElementAt(i);
				var view = CreateViewFromTemplate(template, context);
				view.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(layout.ItemTapped), CommandParameter = view } );
				layout.Children.Add(view);
			}
		}

		static DataTemplate GetDefaultTemplate()
		{
			var defaultDataTemplate = new DataTemplate(typeof(Label));
			defaultDataTemplate.SetBinding(Label.TextProperty, new Binding(".", converter: new ObjectConverter()));
			return defaultDataTemplate;
		}

		static View CreateViewFromTemplate(DataTemplate template, object context)
		{
			var view = new WrapLayoutSelectedTemplate(template.CreateContent() as View);
			view.BindingContext = context;
			return view;
		}

		#endregion

		#region Private Methods

		private void ItemTapped(object arg)
		{
			userTap = true;
			var view = arg as WrapLayoutSelectedTemplate;
			var context = view.BindingContext;
			if (SelectedItem != null && SelectedItem.Equals(context))
			{
				SelectedItemView.IsSelected = false;
				SelectedItem = null;
				selectedItemView = null;
			}
			else
			{
				if (SelectedItemView != null) SelectedItemView.IsSelected = false;
				SelectedItem = context;
				SelectedItemView = view;
				SelectedItemView.IsSelected = true;
			}
		}

		private double GetMaxRowWidth(IEnumerable<Row> rows)
		{
			double maxRowWidth = 0.0;
			for (int i = 0; i < rows.Count(); i++)
			{
				var val = rows.ElementAt(i).Width;
				if (maxRowWidth.Equals(0.0))
					maxRowWidth = val;
				else if (maxRowWidth < val)
					maxRowWidth = val;
			}
			return maxRowWidth;
		}

		private IEnumerable<Rectangle> ComputeLayout(double widthConstraint, double heightConstraint)
		{
			var layout = ComputeNiaveLayout(widthConstraint, heightConstraint);

			var computedLayout = new List<Rectangle>();
			for (int i = 0; i < layout.Count; i++)
				for (int x = 0; x < layout[i].Count; x++)
					computedLayout.Add(layout[i][x]);
			return computedLayout;
		}

		private List<Row> ComputeNiaveLayout(double widthConstraint, double heightConstraint)
		{
			var result = new List<Row>();
			var row = new Row();
			result.Add(row);

			var spacing = Spacing;
			double y = 0;
			foreach (var child in Children)
			{
				var request = child.Measure(double.PositiveInfinity, double.PositiveInfinity);

				if (row.Count == 0)
				{
					row.Add(new Rectangle(0, y, request.Request.Width, request.Request.Height));
					row.Height = request.Request.Height;
					continue;
				}

				var last = row[row.Count - 1];
				var x = last.Right + spacing;
				var childWidth = request.Request.Width;
				var childHeight = request.Request.Height;

				if (x + childWidth > widthConstraint)
				{
					y += row.Height + spacing;

					row = new Row();
					result.Add(row);
					x = 0;
				}

				row.Add(new Rectangle(x, y, childWidth, childHeight));
				row.Width = x + childWidth;
				row.Height = Math.Max(row.Height, childHeight);
			}

			return result;
		}
		#endregion
	}

	class Row : List<Rectangle>
	{
		public double Width { get; set;}
		public double Height { get; set;}
	}

    class ObjectConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return string.Empty;
			else
				return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}

