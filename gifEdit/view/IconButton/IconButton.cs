﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>Button</summary>
	public class IconButton : Button {
		static IconButton() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
		}

		public IconButton() {

		}

		//Image Padding
		public static readonly DependencyProperty ImagePaddingPro = DependencyProperty.Register("ImagePadding", typeof(Thickness), typeof(IconButton), new PropertyMetadata(new Thickness(0)));
		public Thickness ImagePadding {
			get { return (Thickness)GetValue(ImagePaddingPro); }
			set { SetCurrentValue(ImagePaddingPro, value); }
		}

		//Source
		public static readonly DependencyProperty SourcePro = DependencyProperty.Register("Source", typeof(ImageSource), typeof(IconButton), new PropertyMetadata(null));
		[Category("Appearance")]
		public ImageSource Source {
			get { return (ImageSource)GetValue(SourcePro); }
			set { SetCurrentValue(SourcePro, value); }
		}

		//Radius
		public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(CornerRadius), typeof(IconButton), new PropertyMetadata(new CornerRadius(0)));
		public CornerRadius Radius {
			get { return (CornerRadius)GetValue(RadiusProperty); }
			set { SetCurrentValue(RadiusProperty, value); }
		}

		//Over Color
		public static readonly DependencyProperty OverColorProperty = DependencyProperty.Register("OverColor", typeof(Brush), typeof(IconButton), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b8b8b8"))));
		public Brush OverColor {
			get { return (Brush)GetValue(OverColorProperty); }
			set { SetCurrentValue(OverColorProperty, value); }
		}

		//Is Select
		public static readonly DependencyProperty IsSelectProperty = DependencyProperty.Register("IsSelect", typeof(bool), typeof(IconButton), new PropertyMetadata(false));
		public bool IsSelect {
			get { return (bool)GetValue(IsSelectProperty); }
			set { SetCurrentValue(IsSelectProperty, value); }
		}

		//Select Color
		public static readonly DependencyProperty SelectColorProperty = DependencyProperty.Register("SelectColor", typeof(Brush), typeof(IconButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));
		public Brush SelectColor {
			get { return (Brush)GetValue(SelectColorProperty); }
			set { SetCurrentValue(SelectColorProperty, value); }
		}
	}
}
