using gifEdit.model;
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
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>
	/// ExportWin.xaml 的交互逻辑
	/// </summary>
	public partial class ExportWin : Window {
		public ExportWin() {
			InitializeComponent();
		}

		public void show(Window parent, PrjoectType type) {
			Owner = parent;

			ShowDialog();

			switch(type) {
				case PrjoectType.Spirit: {
					break;
				}

				case PrjoectType.Particle: {
					winApng.Visibility = Visibility.Visible;
					winApng.onShow();
					break;
				}
			}
		}
	}
}
