using csharpHelp.services;
using desktopDate.util;
using gifEdit.control;
using gifEdit.model;
using gifEdit.services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {

		public MainWindow() {
			InitializeComponent();

			MainModel.ins.mainWin = this;

			var md = MainModel.ins.configModel;
			try{
				md.srv.load(md, SysConst.configPath());
				md.brgSrv.load(md, this);
			} catch(Exception) {

			}

			var lastProjectCtl = MainCtl.ins.lastProjectCtl;
			lastProjectCtl.init();
			lstLastOpenProject.ItemsSource = lastProjectCtl.lstVM;
		}

		public IntPtr getHandle() {
			return new WindowInteropHelper(this).Handle;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			//viewPointEditBox.load(SysConst.rootPath() + "/project/aaa/");

			//string path = SysConst.rootPath() + "/project/aaa/";
			//openProject(path, PrjoectType.Point);
		}

		public void openProject(string path, PrjoectType type) {
			switch (type) {
				case PrjoectType.Spirit: {
					break;
				}
				case PrjoectType.Particle: {
					grdNav.Visibility = Visibility.Collapsed;
					viewParticleEditBox.Visibility = Visibility.Visible;
					viewParticleEditBox.load(path);
					break;
				}
			}
		}

		private void Window_Closed(object sender, EventArgs e) {
			var md = MainModel.ins.configModel;
			try {
				EventServer.ins.onMainWinExited();

				md.brgSrv.save();
				md.srv.save();
			} catch(Exception) {

			}
		}

		private void grdLastProject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			//Debug.WriteLine(lstLastOpenProject.SelectedIndex);

			int idx = lstLastOpenProject.SelectedIndex;
			MainCtl.ins.openProject(idx);
		}

		private void btnNewProject_Click(object sender, RoutedEventArgs e) {
			NewProjectWin win = new NewProjectWin();
			win.show(this);
		}

		private void btnExportProject_Click(object sender, RoutedEventArgs e) {

		}
	}
}
