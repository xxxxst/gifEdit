using gifEdit.control;
using gifEdit.model;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>
	/// ExportApngWin.xaml 的交互逻辑
	/// </summary>
	public partial class ExportApngWin : UserControl {
		private ExportCtl exportCtl = new ExportCtl();

		private ExportWin parentWin = null;
		private int maxFrameIdx = 0;

		private int progressPrecent = 0;
		public ExportApngWin() {
			InitializeComponent();

			exportCtl.getImageModelByFrame = MainCtl.ins.getImageModelByFrame;

			exportCtl.onUpdateProgress = (val, total) => {
				int precent = val * 100 / total;
				if(precent < progressPrecent) {
					return;
				}
				progressPrecent = precent;

				//Debug.WriteLine(val + "," + total);
				Dispatcher.Invoke(() => {
					if(precent >= 100) {
						btnOk.IsEnabled = true;
						lblProgress.Content = "导出成功";
					} else {
						lblProgress.Content = precent + "/100";
					}
				});
			};
		}

		public void onShow(ExportWin _parentWin) {
			parentWin = _parentWin;

			lblProgress.Content = "";
			progressPrecent = 0;

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			txtWidth.Text = md.width.ToString();
			txtHeight.Text = md.height.ToString();
			txtFps.Text = md.fps.ToString();
			txtFps.Text = md.fps.ToString();
			txtFilePath.Text = md.path;
			txtFileName.Text = md.name + ".png";

			maxFrameIdx = md.frameCount - 1;

			sldStartFrame.Minimum = 0;
			sldStartFrame.Maximum = maxFrameIdx;
			sldStartFrame.Value = 0;
			sldEndFrame.Minimum = 0;
			sldEndFrame.Maximum = maxFrameIdx;
			sldEndFrame.Value = maxFrameIdx;

			txtStartFrame.Text = "0";
			txtEndFrame.Text = maxFrameIdx.ToString();
		}

		private bool isChangedByInner = false;

		private int startFrame = 0;
		private int endFrame = 0;
		private void updateSelectFrame(int start, int end) {
			start = Math.Max(0, Math.Min(start, maxFrameIdx));
			end = Math.Max(0, Math.Min(end, maxFrameIdx));

			isChangedByInner = true;
			if(start != startFrame) {
				startFrame = start;
				sldStartFrame.Value = startFrame;
				txtStartFrame.Text = startFrame.ToString();
			}
			if(end != endFrame) {
				endFrame = end;
				sldEndFrame.Value = endFrame;
				txtEndFrame.Text = endFrame.ToString();
			}
			isChangedByInner = false;
		}
		
		private void SldStartFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(isChangedByInner) {
				return;
			}

			int start = (int)sldStartFrame.Value;
			int end = Math.Max(start, endFrame);
			updateSelectFrame(start, end);
		}

		private void SldEndFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(isChangedByInner) {
				return;
			}

			int end = (int)sldEndFrame.Value;
			int start = Math.Min(end, startFrame);
			updateSelectFrame(start, end);
		}

		private void TxtStartFrame_TextChanged(object sender, TextChangedEventArgs e) {
			if(isChangedByInner) {
				return;
			}

			bool isOk = int.TryParse(txtStartFrame.Text, out int start);
			if(!isOk) {
				return;
			}

			int end = Math.Max(start, endFrame);
			updateSelectFrame(start, end);
		}

		private void TxtEndFrame_TextChanged(object sender, TextChangedEventArgs e) {
			if(isChangedByInner) {
				return;
			}

			bool isOk = int.TryParse(txtEndFrame.Text, out int end);
			if(!isOk) {
				return;
			}

			int start = Math.Min(end, startFrame);
			updateSelectFrame(start, end);
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			string fileName = txtFileName.Text.Trim();
			if(fileName == "") {
				return;
			}

			string path = txtFilePath.Text.Trim();
			if(path != "") {
				path = path + "/" + fileName;
			}

			int start = Math.Max(0, Math.Min(startFrame, maxFrameIdx));
			int end = Math.Max(0, Math.Min(endFrame, maxFrameIdx));
			int count = end - start + 1;
			if(count <= 0) {
				return;
			}

			progressPrecent = 0;
			lblProgress.Content = "";
			btnOk.IsEnabled = false;
			exportCtl.saveAsAPNG(path, start, count, md.fps);
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e) {
			exportCtl.clearThread();

			parentWin.close();
		}

		public bool onCancel() {
			exportCtl.clearThread();

			return true;
		}

	}
}
