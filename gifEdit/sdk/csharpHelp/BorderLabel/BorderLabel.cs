using System;
using System.Collections.Generic;
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

namespace csharpHelp.ui {
	/// <summary>BorderLabel</summary>
	public class BorderLabel : Button {
		static BorderLabel() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderLabel), new FrameworkPropertyMetadata(typeof(BorderLabel)));
		}

		//Radius
		public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(CornerRadius), typeof(BorderLabel), new PropertyMetadata(new CornerRadius(0)));
		public CornerRadius Radius {
			get { return (CornerRadius)GetValue(RadiusProperty); }
			set { SetCurrentValue(RadiusProperty, value); }
		}
	}
}
