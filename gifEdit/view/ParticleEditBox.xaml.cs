using csharpHelp.util;
using FreeImageAPI;
using gifEdit.control;
using gifEdit.control.glEngine;
using gifEdit.model;
using gifEdit.services;
using Microsoft.Win32;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace gifEdit.view {
	/// <summary>粒子系统</summary>
	public partial class ParticleEditBox : UserControl {
		//PointRenderWin win = null;
		//System.Windows.Forms.Panel pnlRender = new System.Windows.Forms.Panel();
		private ParticleEditBoxVM vm = new ParticleEditBoxVM();
		private ParticleEditCtl ctl = null;

		private string cachePath = "";
		private bool isEngineInited = false;
		//private bool isUpdatTextInner = false;
		private int selectEmitter = -1;

		private bool isEditGlobalAttrByText = false;
		private bool isEditGlobalAttrByUI = false;

		private Timer timer = null;
		//private int nowTime = 0;

		private Brush coFrameDef = null;
		private Brush coFrameLoop = null;
		private ExportCtl imageDataCtl = new ExportCtl();

		public ParticleEditBox() {
			InitializeComponent();

			DataContext = vm;

			coFrameDef = keyFrame.FramePointColor;
			coFrameLoop = FindResource("coFrameLoop") as Brush;

			imageDataCtl.getImageModelByFrame = particleRenderBox.renderToBuffer;

			EventServer.ins.pointEngineInitedEvent += () => {
				isEngineInited = true;

				if(cachePath != "") {
					string path = cachePath;
					cachePath = "";
					load(path);
				}
			};

			EventServer.ins.updateFPSEvent += (fps) => {
				lblFps.Content = fps;
			};

			EventServer.ins.copyToClipboard += () => {
				ParticleEditModel md = MainModel.ins.particleEditModel;
				if(md == null || md.fps <= 0) {
					return;
				}

				//ImageModel md = particleRenderBox.renderToBuffer();
				//saveImage(md, "bbb.png");
				//saveToClipboard(md);

				//imageDataCtl.saveToClipboard();

				//imageDataCtl.saveAsPng("bbb.png", 60);

				//int frameTime = (int)(1000f / md.fps + 0.5f);
				//imageDataCtl.saveAsGif("bbb.gif", 0, 60, frameTime);

				imageDataCtl.saveAsAPNG("ccc.png", 0, 240, md.fps);
			};

			initAttr();
			//initAttrDesc();

			timer = new Timer(new TimerCallback(timerProc), null, 0, 15);
			//timer.Interval = TimeSpan.FromMilliseconds(8);
			//timer.Tick += Timer_Tick;
			//timer.Start();
		}

		bool isRenderPause = false;
		int oldFrameIdx = 0;
		//int totalFrame = 0;
		bool isFrameInLoop = false;

		int renderTime = 0;
		int oldMaxRenderTime = 0;
		int oldFPS = 0;
		GlTime glTime = new GlTime();
		private void timerProc(object value) {
			if(!isEngineInited) {
				return;
			}
			int maxRenderTime = particleRenderBox.getMaxRenderTime();
			//updateMaxFrame(maxRenderTime);

			if(isRenderPause) {
				return;
			}

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.fps <= 0) {
				return;
			}

			float gapTime = glTime.getTime();
			if(maxRenderTime == 0) {
				return;
			}

			//if(maxRenderTime != oldMaxRenderTime) {
			//	oldMaxRenderTime = maxRenderTime;
			//	updateMaxFrame();
			//}

			if(md.frameCount == 0) {
				return;
			}

			renderTime = (int)(renderTime + gapTime);
			if(renderTime >= maxRenderTime) {
				renderTime = maxRenderTime + renderTime % maxRenderTime;
			} else {
				renderTime = renderTime % maxRenderTime;
			}

			particleRenderBox.setRenderTime(renderTime);

			int frameIdx = (int)(renderTime / 1000f * md.fps);
			//frameIdx = frameIdx % totalFrame;
			//if(frameIdx == oldFrameIdx) {
			//	return;
			//}
			//oldFrameIdx = frameIdx;
			updateFrame(frameIdx);

			//lblTest.Content = frameIdx;

			//if(frameIdx > 237) {
			//	Debug.WriteLine(frameIdx);
			//}

			//particleRenderBox.setRenderTime((int)(oldFrameIdx * 1000f / md.fps));
		}

		private void updateRenderTimeByFrame(int frameIdx) {
			if(!isEngineInited) {
				return;
			}
			int maxRenderTime = particleRenderBox.getMaxRenderTime();

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.fps <= 0) {
				return;
			}

			renderTime = (int)((float)frameIdx / md.fps * 1000);
			if(renderTime >= maxRenderTime) {
				renderTime = maxRenderTime + renderTime % maxRenderTime;
			} else {
				renderTime = renderTime % maxRenderTime;
			}

			particleRenderBox.setRenderTime(renderTime);
		}
		
		private void updateFrame(int frameIdx) {
			if(oldFrameIdx == frameIdx) {
				return;
			}
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			frameIdx = frameIdx % md.frameCount;

			Dispatcher.Invoke(() => {
				oldFrameIdx = frameIdx;
				//bool isLoop = frameIdx >= md.frameCount / 2;
				//if(isLoop != isFrameInLoop) {
				//	isFrameInLoop = isLoop;
				//	keyFrame.FramePointColor = isLoop ? coFrameLoop : coFrameDef;
				//}

				keyFrame.SelectFrame = frameIdx;
				updateFrameLabel();
			});
		}

		private void updateFrameLabel() {
			if(oldFrameIdx < 0) {
				nowFrame.Content = "";
			} else {
				nowFrame.Content = oldFrameIdx;
			}
		}

		private void updateMaxFrame() {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.fps <= 0) {
				oldMaxRenderTime = 0;
				oldFrameIdx = 0;
				md.frameCount = 0;
				return;
			}

			int maxRenderTime = particleRenderBox.getMaxRenderTime();

			if(maxRenderTime == oldMaxRenderTime && md.fps == oldFPS) {
				return;
			}

			oldMaxRenderTime = maxRenderTime;
			oldFPS = md.fps;

			md.frameCount = (int)Math.Ceiling(oldMaxRenderTime / 1000f * md.fps) * 2;
			//Debug.WriteLine(totalFrame);
			oldFrameIdx = oldFrameIdx % md.frameCount;
			
			Dispatcher.Invoke(() => {
				keyFrame.MaxFrame = md.frameCount - 1;
			});
		}

		//private void saveToClipboard(ImageModel md) {
		//	System.Drawing.Bitmap pic = new System.Drawing.Bitmap(md.width, md.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

		//	//System.Drawing.Image img = System.Drawing.Image.FromStream();

		//	for(int x = 0; x < md.width; x++) {
		//		for(int y = 0; y < md.height; y++) {
		//			int idx = (y * md.width + x) * md.pxChannel;
		//			System.Drawing.Color c = System.Drawing.Color.FromArgb(
		//				md.data[idx + 3],
		//				md.data[idx + 2],
		//				md.data[idx + 1],
		//				md.data[idx + 0]
		//			);
		//			pic.SetPixel(x, md.height - 1 - y, c);
		//		}
		//	}

		//	//BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
		//	//	pic.GetHbitmap(),
		//	//	IntPtr.Zero,
		//	//	Int32Rect.Empty,
		//	//	BitmapSizeOptions.FromWidthAndHeight(md.width, md.height)
		//	//);

		//	//Clipboard.SetImage(bs);

		//	ClipboardImageCtl.SetClipboardImage(pic, null, null);
		//}
		
		//private void saveImage(ImageModel md, string outPath) {
		//	//FIMEMORY ms = new FIMEMORY();

		//	FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
		//	IntPtr pFibData = FreeImage.GetBits(dib);
		//	Marshal.Copy(md.data, 0, pFibData, md.data.Length);

		//	FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, outPath, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
		//	//FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, ms, FREE_IMAGE_SAVE_FLAGS.DEFAULT);

		//	//int size = FreeImage.TellMemory(ms);
		//	//byte[] buffer = new byte[size];
		//	//FreeImage.SeekMemory(ms, 0, SeekOrigin.Begin);
		//	//FreeImage.ReadMemory(buffer, (uint)size, (uint)size, ms);

		//	FreeImage.Unload(dib);
		//}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {

		}

		private void initAttr() {
			atxProject.setData(null,
				new string[] {
					"背景颜色",
					"宽度",
					"高度",
					"FPS",
					"隐藏超出部分",
				},
				new string[] {
					"4D4D4D",
					"400",
					"400",
					"30",
					"true | false",
				},
				new Func<object, string>[] {
					(m) => ((ParticleEditModel)m).background,
					(m) => ((ParticleEditModel)m).width.ToString(),
					(m) => ((ParticleEditModel)m).height.ToString(),
					(m) => ((ParticleEditModel)m).fps.ToString(),
					(m) => ((ParticleEditModel)m).isMaskBox.ToString().ToLower(),
				},
				new Action<object, string>[] {
					(m, s)=> ((ParticleEditModel)m).background = s,
					(m, s)=> ((ParticleEditModel)m).width = getInt(s, 0),
					(m, s)=> ((ParticleEditModel)m).height = getInt(s, 0),
					(m, s)=> ((ParticleEditModel)m).fps = getInt(s, 0),
					(m, s)=> ((ParticleEditModel)m).isMaskBox = s.Trim().ToLower().FirstOrDefault() == 't',
				}
			);

			atxEmitter.setData(null,
				new string[] {
					 "坐标x"              ,
					 "坐标x浮动"            ,
					 "坐标y"              ,
					 "坐标y浮动"            ,
					 "开始透明度"            ,
					 "结束透明度"            ,
					 "重力"                   ,
					 "重力方向"             ,
					 "开始速度"             ,
					 "开始速度浮动"           ,
					 "开始速度方向"           ,
					 "开始速度方向浮动"     ,
					 "旋转速度"             ,
					 "旋转速度浮动"           ,
					 "分离速度"             ,
					 "分离速度方向"           ,
					 "粒子数"              ,
					 "粒子生命周期"           ,
					 "粒子生命周期浮动"     ,
					 "粒子开始大小"           ,
					 "粒子开始大小浮动"     ,
					 "粒子结束大小"           ,
					 "粒子结束大小浮动"     ,
					 "粒子开始角度"           ,
					 "粒子开始角度浮动"     ,
					 "粒子旋转速度"           ,
					 "粒子旋转速度浮动"     ,
				},
				new string[] {
					 "坐标x"              ,
					 "坐标x浮动"            ,
					 "坐标y"              ,
					 "坐标y浮动"            ,
					 "开始透明度"            ,
					 "结束透明度"            ,
					 "重力"                   ,
					 "重力方向"             ,
					 "开始速度"             ,
					 "开始速度浮动"           ,
					 "开始速度方向"           ,
					 "开始速度方向浮动"     ,
					 "旋转速度"             ,
					 "旋转速度浮动"           ,
					 "分离速度"             ,
					 "分离速度方向"           ,
					 "粒子数"              ,
					 "粒子生命周期"           ,
					 "粒子生命周期浮动"     ,
					 "粒子开始大小"           ,
					 "粒子开始大小浮动"     ,
					 "粒子结束大小"           ,
					 "粒子结束大小浮动"     ,
					 "粒子开始角度"           ,
					 "粒子开始角度浮动"     ,
					 "粒子旋转速度"           ,
					 "粒子旋转速度浮动"     ,
				},
				new Func<object, string>[] {
					(m) => ((ParticleResourceModel)m).x.ToString(),
					(m) => ((ParticleResourceModel)m).xFloat.ToString(),
					(m) => ((ParticleResourceModel)m).y.ToString(),
					(m) => ((ParticleResourceModel)m).yFloat.ToString(),
					(m) => ((ParticleResourceModel)m).startAlpha.ToString(),
					(m) => ((ParticleResourceModel)m).endAlpha.ToString(),
					(m) => ((ParticleResourceModel)m).gravityValue.ToString(),
					(m) => ((ParticleResourceModel)m).gravityAngle.ToString(),
					(m) => ((ParticleResourceModel)m).startSpeed.ToString(),
					(m) => ((ParticleResourceModel)m).startSpeedFloat.ToString(),
					(m) => ((ParticleResourceModel)m).startSpeedAngle.ToString(),
					(m) => ((ParticleResourceModel)m).startSpeedAngleFloat.ToString(),
					(m) => ((ParticleResourceModel)m).rotateSpeed.ToString(),
					(m) => ((ParticleResourceModel)m).rotateSpeedFloat.ToString(),
					(m) => ((ParticleResourceModel)m).directionSpeed.ToString(),
					(m) => ((ParticleResourceModel)m).directionSpeedFloat.ToString(),
					(m) => ((ParticleResourceModel)m).particleCount.ToString(),
					(m) => ((ParticleResourceModel)m).particleLife.ToString(),
					(m) => ((ParticleResourceModel)m).particleLifeFloat.ToString(),
					(m) => ((ParticleResourceModel)m).particleStartSize.ToString(),
					(m) => ((ParticleResourceModel)m).particleStartSizeFloat.ToString(),
					(m) => ((ParticleResourceModel)m).particleEndSize.ToString(),
					(m) => ((ParticleResourceModel)m).particleEndSizeFloat.ToString(),
					(m) => ((ParticleResourceModel)m).particleStartAngle.ToString(),
					(m) => ((ParticleResourceModel)m).particleStartAngleFloat.ToString(),
					(m) => ((ParticleResourceModel)m).particleRotateSpeed.ToString(),
					(m) => ((ParticleResourceModel)m).particleRotateSpeedFloat.ToString(),
				},
				new Action<object, string>[] {
					(m, s) => ((ParticleResourceModel)m).x = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).xFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).y = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).yFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).startAlpha = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).endAlpha = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).gravityValue = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).gravityAngle = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).startSpeed = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).startSpeedFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).startSpeedAngle = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).startSpeedAngleFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).rotateSpeed = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).rotateSpeedFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).directionSpeed = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).directionSpeedFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleCount = getInt(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleLife = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleLifeFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleStartSize = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleStartSizeFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleEndSize = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleEndSizeFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleStartAngle = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleStartAngleFloat = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleRotateSpeed = getFloat(s, 0),
					(m, s) => ((ParticleResourceModel)m).particleRotateSpeedFloat = getFloat(s, 0),
				}
			);
		}

		public void load(string path) {
			if(!isEngineInited) {
				cachePath = path;
				return;
			}

			selectEmitter = -1;
			vm.IsSelectProjectName = true;

			path = MainCtl.getFullPath(path);

			ctl = MainCtl.ins.particleEditCtl;

			ctl.load(path);

			lstRes.ItemsSource = ctl.lstVM;

			ParticleEditModel md = MainModel.ins.particleEditModel;
			lblProjName.Content = md.name;

			atxProject.setModel(md);
			setCpkBackgroundColor(md.background);

			//dgrdAttr.ItemsSource = ctl.lstAttrMd;

			//ctl.initAttr(0);

			//formHost.Child = pnlRender;

			//win = new PointRenderWin();
			//win.onLoaded = () => {
			//	Dispatcher.Invoke(() => {
			//		initRenderWin();
			//	});
			//};
			//win.Show();

			EventServer.ins.mainWinExitedEvent += () => {
				try {
					//win.Close();
					ctl.save();
				} catch(Exception) { }
			};
			particleRenderBox.init();
			updateMaxFrame();

			//if (ctl.md.lstResource.Count > 0) {
			//	selectEmitter = 0;
			//	updateAttrText();
			//}
		}

		private float getFloat(string str, float def) {
			if(str == "") {
				return def;
			}

			bool isOk = float.TryParse(str, out float rst);
			return isOk ? rst : def;
		}

		private int getInt(string str, int def) {
			if(str == "") {
				return def;
			}

			bool isOk = Int32.TryParse(str, out int rst);
			return isOk ? rst : def;
		}

		private void grdProjectName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			setSelectProject();
		}

		private void grdEmitterBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstRes.SelectedIndex;

			setSelectEmitter(idx);
		}

		private void setSelectProject() {
			if(vm.IsSelectProjectName) {
				return;
			}

			if(selectEmitter >= 0 && selectEmitter < ctl.lstVM.Count) {
				ctl.lstVM[selectEmitter].IsSelect = false;
			}

			selectEmitter = -1;
			lstRes.SelectedIndex = -1;
			atxEmitter.Visibility = Visibility.Collapsed;

			atxProject.Visibility = Visibility.Visible;
			grdSetUIProject.Visibility = Visibility.Visible;

			vm.IsSelectProjectName = true;
		}

		private void setSelectEmitter(int idx) {
			if(idx < 0 || idx >= ctl.lstVM.Count) {
				return;
			}

			if(idx == selectEmitter) {
				return;
			}

			if(vm.IsSelectProjectName) {
				vm.IsSelectProjectName = false;
				atxProject.Visibility = Visibility.Collapsed;
				grdSetUIProject.Visibility = Visibility.Collapsed;

				atxEmitter.Visibility = Visibility.Visible;
			}

			if(selectEmitter >= 0 && selectEmitter < ctl.lstVM.Count) {
				ctl.lstVM[selectEmitter].IsSelect = false;
			}

			selectEmitter = idx;
			lstRes.SelectedIndex = idx;
			ctl.lstVM[idx].IsSelect = true;
			//updateAttrText();

			ParticleEditModel md = MainModel.ins.particleEditModel;
			atxEmitter.setModel(md.lstResource[idx]);
		}

		private void setCpkBackgroundColor(string color) {
			uint.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out uint coBackground);

			isEditGlobalAttrByText = true;
			cpkBackground.Value = coBackground;
			isEditGlobalAttrByText = false;
		}

		private void atxProject_TextChangedByUser(object sender, RoutedEventArgs e) {
			if(!isEngineInited) {
				return;
			}
			if(isEditGlobalAttrByUI) {
				return;
			}

			particleRenderBox.updateGlobalAttr();
			updateMaxFrame();

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			setCpkBackgroundColor(md.background);
		}

		private void atxEmitter_TextChangedByUser(object sender, RoutedEventArgs e) {
			if(!isEngineInited) {
				return;
			}
			if(isEditGlobalAttrByUI) {
				return;
			}

			int idx = selectEmitter;
			if(idx < 0) {
				return;
			}

			particleRenderBox.updateEmitterAttr(idx);
			updateMaxFrame();
		}

		private void cpkBackground_ValueChanged(object sender, RoutedEventArgs e) {
			if(isEditGlobalAttrByText) {
				return;
			}

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			uint co = cpkBackground.Value;
			co = co & 0xffffff;
			md.background = Convert.ToString(co, 16);

			particleRenderBox.updateGlobalAttr();

			isEditGlobalAttrByUI = true;
			atxProject.updateText();
			isEditGlobalAttrByUI = false;
		}

		private void imgEmitter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			ParticleEditVM vm = (sender as Image)?.Tag as ParticleEditVM;
			if(vm == null) {
				return;
			}

			int idx = vm.idx;

			string path = selectFile(vm.md.path);
			setEmitterImage(idx, path);
		}

		private void setEmitterImage(int idx, string path) {
			if(path == "") {
				return;
			}

			if(idx < 0 || idx >= ctl.lstVM.Count) {
				return;
			}

			ParticleEditVM vm = ctl.lstVM[idx];

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			vm.md.path = MainCtl.formatPath(md.path, path);

			MainCtl.ins.particleEditCtl.updateImage(idx);
			particleRenderBox.updateEmitterImage(idx);
		}

		//选择文件
		private string selectFile(string defPath = "") {
			//选择文件
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "图片(*.png;*.jpg;*.bmp;*.jpeg)|*.png;*.jpg;*.bmp;*.jpeg|所有文件(*.*)|*.*";
			//if(File.Exists(txt.Text)) {
			if(defPath != "" && File.Exists(defPath)) {
				//ofd.InitialDirectory = System.IO.Path.GetDirectoryName(txt.Text);
				ofd.InitialDirectory = System.IO.Path.GetDirectoryName(defPath);
			}
			ofd.ValidateNames = true;
			ofd.CheckPathExists = true;
			ofd.CheckFileExists = true;
			if(ofd.ShowDialog() != true) {
				return "";
			}

			//txt.Text = ofd.FileName;
			return ofd.FileName;
		}

		private void grdEmitterBox_DragOver(object sender, DragEventArgs e) {
			e.Effects = DragDropEffects.Link;
			e.Handled = true;
		}

		private void grdEmitterBox_Drop(object sender, DragEventArgs e) {
			ParticleEditVM vm = (sender as Grid)?.Tag as ParticleEditVM;
			if(vm == null) {
				return;
			}

			int idx = vm.idx;

			string path = "";
			//拖拽文件
			try {
				string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);
				if(docPath.Length > 0) {
					//txt.Text = docPath[0];
					path = docPath[0];
				}
			} catch(Exception) { }

			if(path == "") {
				return;
			}

			setEmitterImage(idx, path);
		}

		private void btnAddEmitter_Click(object sender, RoutedEventArgs e) {
			int idx = ctl.createEmitter();
			particleRenderBox.createEmitter(idx);

			setSelectEmitter(idx);
		}

		private void btnRemoveEmitter_Click(object sender, RoutedEventArgs e) {
			int idx = lstRes.SelectedIndex;
			if(idx < 0 || idx >= ctl.lstVM.Count) {
				return;
			}

			particleRenderBox.removeEmitter(idx);
			ctl.removeEmitter(idx);

			lstRes.SelectedIndex = -1;

			selectEmitter = -1;
			if(idx <= ctl.lstVM.Count - 1) {
				setSelectEmitter(idx);
			} else if(idx > 0) {
				setSelectEmitter(idx - 1);
			} else {
				setSelectProject();
			}
			//Dispatcher.Invoke(() => {
			//});
		}

		private void btnTest_Click(object sender, RoutedEventArgs e) {
			//EventServer.ins.onCopyToClipboard();
		}

		private void KeyFrame_SelectFrameChanged(object sender, RoutedEventArgs e) {
			int idx = keyFrame.SelectFrame;

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			bool isLoop = idx >= md.frameCount / 2;
			if(isLoop != isFrameInLoop) {
				isFrameInLoop = isLoop;
				keyFrame.FramePointColor = isLoop ? coFrameLoop : coFrameDef;
			}

			if(oldFrameIdx == idx) {
				return;
			}
			if(idx < 0) {
				return;
			}

			pauseFrame();
			//Debug.WriteLine(keyFrame.SelectFrame);
			oldFrameIdx = idx;
			updateRenderTimeByFrame(idx);
			updateFrameLabel();
		}

		private void BtnPreFrame_Click(object sender, RoutedEventArgs e) {
			pauseFrame();

			int idx = oldFrameIdx - 1;
			if(idx < 0) {
				return;
			}

			updateRenderTimeByFrame(idx);
			updateFrame(idx);
			keyFrame.setSelectFrameCenter();
		}

		private void BtnStartFrame_Click(object sender, RoutedEventArgs e) {
			glTime.getTime();

			isRenderPause = false;
			btnStartFrame.Visibility = Visibility.Collapsed;
			btnPauseFrame.Visibility = Visibility.Visible;
		}

		private void pauseFrame() {
			isRenderPause = true;
			btnPauseFrame.Visibility = Visibility.Collapsed;
			btnStartFrame.Visibility = Visibility.Visible;
		}

		private void BtnPauseFrame_Click(object sender, RoutedEventArgs e) {
			pauseFrame();
		}

		private void BtnNextFrame_Click(object sender, RoutedEventArgs e) {
			pauseFrame();
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			int idx = oldFrameIdx + 1;
			if(idx >= md.frameCount) {
				return;
			}

			updateRenderTimeByFrame(idx);
			updateFrame(idx);
			keyFrame.setSelectFrameCenter();
		}
	}

	class ParticleEditBoxVM : INotifyPropertyChanged {
		private bool _IsSelectProjectName = true;
		public bool IsSelectProjectName {
			get { return _IsSelectProjectName; }
			set { _IsSelectProjectName = value; updatePro("IsSelectProjectName"); }
		}

		//
		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual void updatePro(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
