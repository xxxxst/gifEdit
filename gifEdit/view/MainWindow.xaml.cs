using csharpHelp.services;
using desktopDate.util;
using gifEdit.model;
using gifEdit.services;
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
		}

		public IntPtr getHandle() {
			return new WindowInteropHelper(this).Handle;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			viewPointEditBox.load(SysConst.rootPath() + "/project/aaa/");
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

	}
}
