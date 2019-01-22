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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>
	/// ExportApngWin.xaml 的交互逻辑
	/// </summary>
	public partial class ExportApngWin : UserControl {
		public ExportApngWin() {
			InitializeComponent();
		}

		public void onShow() {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			txtWidth.Text = md.width.ToString();
			txtHeight.Text = md.height.ToString();
			txtFps.Text = md.fps.ToString();
			txtFps.Text = md.fps.ToString();
			txtStartFrame.Text = "0";
			txtEndFrame.Text = (md.frameCount - 1).ToString();

			sldStartFrame.Minimum = 0;
			sldStartFrame.Maximum = md.frameCount - 1;
			sldStartFrame.Value = 0;
			sldEndFrame.Minimum = 0;
			sldEndFrame.Maximum = md.frameCount - 1;
			sldEndFrame.Value = md.frameCount - 1;
		}

		private void SldStartFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

		}

		private void SldEndFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {

		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e) {

		}
	}
}
