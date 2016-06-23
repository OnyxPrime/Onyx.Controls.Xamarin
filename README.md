# Onyx.Controls.Xamarin

Onyx Controls is a suite of controls for use in Xamarin and Xamarin.Forms

Please let us know what's missing and what would make your life easier and we'll see what we can do!

Our Current Control Suite
-----
* WrapLayout - Inspired by Jason Smith's [custom layouts](https://evolve.xamarin.com/session/56e20f83bad314273ca4d81c) talk at Xamarin Evolve 2016
* More to come... Let me know what you'd like to see!


Usage
-----
* WrapLayout

```XAML
xmlns:custom="clr-namespace:Onyx.Controls.Xamarin;assembly=Onyx.Controls.Xamarin"

<custom:WrapLayout ItemsSource="{Binding Results}" SelectedItem="{Binding SelectedItem, Mode=TwoWay}" Spacing="5" Margin="5" HorizontalOptions="CenterAndExpand" >
	<custom:WrapLayout.ItemTemplate>
		<DataTemplate>
			<Image Source="{Binding ThumbnailUrl}" Aspect="AspectFill" WidthRequest="70" HeightRequest="70" VerticalOptions="FillAndExpand"/>					
		</DataTemplate>
	</custom:WrapLayout.ItemTemplate>
</custom:WrapLayout>
```
On **iOS**

```C#
AppDelegate.cs

using Onyx.Controls.Xamarin

public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
	Xamarin.Forms.Init();//platform specific init
	WrapLayout.Init();
	...
}
```
You must do this AFTER you call Xamarin.Forms.Init();
