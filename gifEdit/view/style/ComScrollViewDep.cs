using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace comStyle {
	public class ComScrollViewDep : DependencyObject {
		public static readonly DependencyProperty IsShowSideButtonProperty = DependencyProperty.RegisterAttached("IsShowSideButton", typeof(bool), typeof(ComScrollViewDep), new PropertyMetadata(true));

		public static bool GetIsShowSideButton(DependencyObject obj) {
			return (bool)obj.GetValue(IsShowSideButtonProperty);
		}

		public static void SetIsShowSideButton(DependencyObject obj, bool value) {
			obj.SetCurrentValue(IsShowSideButtonProperty, value);
		}

	}
}
