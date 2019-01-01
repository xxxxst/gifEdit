using gifEdit.control;
using gifEdit.model;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace gifEdit.view {
	/// <summary>新建项目</summary>
	public partial class NewProjectWin : Window {
		public string projectPath = "";

		public NewProjectWin() {
			InitializeComponent();
		}

		public void show(Window parent) {
			Owner = parent;

			ShowDialog();
		}

		private void btnSprite_Click(object sender, RoutedEventArgs e) {
			btnSprite.IsSelect = false;
			btnPoint.IsSelect = false;

			btnSprite.IsSelect = true;
		}

		private void btnPoint_Click(object sender, RoutedEventArgs e) {
			btnSprite.IsSelect = false;
			btnPoint.IsSelect = false;

			btnPoint.IsSelect = true;
		}

		private void btnOk_Click(object sender, RoutedEventArgs e) {
			string path = txtPath.Text + "/" + txtProjectName.Text;

			lblErrorInfo.Content = "";

			PrjoectType type = PrjoectType.Spirit;
			if (btnPoint.IsSelect) {
				type = PrjoectType.Particle;
			}

			var rst = MainCtl.ins.createProject(path, type);
			switch (rst) {
				case MainCtl.CreateProjectResult.DirectoryExist: {
					lblErrorInfo.Content = "路径已存在";
					break;
				}
				case MainCtl.CreateProjectResult.Ok: {
					Close();
					break;
				}
				default: {
					lblErrorInfo.Content = "创建失败";
					break;
				}
			}

		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
