using csharpHelp.util;
using FreeImageAPI;
using gifEdit.control;
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

		public ParticleEditBox() {
			InitializeComponent();

			DataContext = vm;

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
				ImageModel md = pointRenderBox.renderToBuffer();
				//saveImage(md, "bbb.png");
				saveToClipboard(md);
			};

			initAttr();
			//initAttrDesc();
		}

		private void saveToClipboard(ImageModel md) {
			System.Drawing.Bitmap pic = new System.Drawing.Bitmap(md.width, md.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			//System.Drawing.Image img = System.Drawing.Image.FromStream();

			for(int x = 0; x < md.width; x++) {
				for(int y = 0; y < md.height; y++) {
					int idx = (y * md.width + x) * md.pxChannel;
					System.Drawing.Color c = System.Drawing.Color.FromArgb(
						md.data[idx + 3],
						md.data[idx + 2],
						md.data[idx + 1],
						md.data[idx + 0]
					);
					pic.SetPixel(x, md.height - 1 - y, c);
				}
			}

			//BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
			//	pic.GetHbitmap(),
			//	IntPtr.Zero,
			//	Int32Rect.Empty,
			//	BitmapSizeOptions.FromWidthAndHeight(md.width, md.height)
			//);

			//Clipboard.SetImage(bs);

			ClipboardImageCtl.SetClipboardImage(pic, null, null);
		}
		
		private void saveImage(ImageModel md, string outPath) {
			//FIMEMORY ms = new FIMEMORY();

			FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
			IntPtr pFibData = FreeImage.GetBits(dib);
			Marshal.Copy(md.data, 0, pFibData, md.data.Length);

			FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, outPath, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
			//FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, ms, FREE_IMAGE_SAVE_FLAGS.DEFAULT);

			//int size = FreeImage.TellMemory(ms);
			//byte[] buffer = new byte[size];
			//FreeImage.SeekMemory(ms, 0, SeekOrigin.Begin);
			//FreeImage.ReadMemory(buffer, (uint)size, (uint)size, ms);

			FreeImage.Unload(dib);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {

		}

		private void initAttr() {
			atxProject.setData(null,
				new string[] {
					"背景颜色",
					"宽度",
					"高度",
					"隐藏超出部分",
				},
				new string[] {
					"4D4D4D",
					"400",
					"400",
					"true | false",
				},
				new Func<object, string>[] {
					(m) => ((ParticleEditModel)m).background,
					(m) => ((ParticleEditModel)m).width.ToString(),
					(m) => ((ParticleEditModel)m).height.ToString(),
					(m) => ((ParticleEditModel)m).isMaskBox.ToString().ToLower(),
				},
				new Action<object, string>[] {
					(m, s)=> ((ParticleEditModel)m).background = s,
					(m, s)=> ((ParticleEditModel)m).width = getInt(s, 0),
					(m, s)=> ((ParticleEditModel)m).height = getInt(s, 0),
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
			pointRenderBox.init();

			//if (ctl.md.lstResource.Count > 0) {
			//	selectEmitter = 0;
			//	updateAttrText();
			//}
		}

		//private void initRenderWin() {
		//	//是否嵌入成功标志，用作返回值
		//	bool isEmbedSuccess = false;
		//	//外进程句柄
		//	IntPtr processHwnd = win.getHandle();
		//	//容器句柄
		//	IntPtr panelHwnd = pnlRender.Handle;
		//	//IntPtr panelHwnd = MainModel.ins.mainWin.getHandle();

		//	if(processHwnd != (IntPtr)0 && panelHwnd != (IntPtr)0) {
		//		//把本窗口句柄与目标窗口句柄关联起来
		//		int setTime = 0;
		//		while(!isEmbedSuccess && setTime < 5) {
		//			isEmbedSuccess = (User32.SetParent(processHwnd, panelHwnd) != IntPtr.Zero);
		//			Thread.Sleep(100);
		//			setTime++;
		//		}
		//		//设置初始尺寸和位置
		//		User32.MoveWindow(processHwnd, 0, 0, 0, 0, true);
		//		updateRenderSize();
		//	}

		//	//if(isEmbedSuccess) {
		//	//	_embededWindowHandle = _process.MainWindowHandle;
		//	//}

		//	//return isEmbedSuccess;
		//}

		//private void initAttrDesc() {
		//	string[] arrDesc = new string[] {
		//		"x"						, "坐标x"				,
		//		"xFloat"				, "坐标x浮动"			,
		//		"y"						, "坐标y"				,
		//		"yFloat"				, "坐标y浮动"			,
		//		"startAlpha"            , "开始透明度"          ,
		//		"endAlpha"              , "结束透明度"          ,
		//		"gravityValue"			, "重力"				,
		//		"gravityAngle"			, "重力方向"			,
		//		"startSpeed"			, "开始速度"			,
		//		"startSpeedFloat"		, "开始速度浮动"		,
		//		"startSpeedAngle"		, "开始速度方向"		,
		//		"startSpeedAngleFloat"	, "开始速度方向浮动"	,
		//		"rotateSpeed"			, "旋转速度"			,
		//		"rotateSpeedFloat"		, "旋转速度浮动"		,
		//		"directionSpeed"		, "分离速度"			,
		//		"directionSpeedFloat"	, "分离速度方向"		,
		//		"pointCount"			, "粒子数"				,
		//		"pointLife"				, "粒子生命周期"		,
		//		"pointLifeFloat"		, "粒子生命周期浮动"	,
		//		"pointStartSize"		, "粒子开始大小"		,
		//		"pointStartSizeFloat"	, "粒子开始大小浮动"	,
		//		"pointEndSize"			, "粒子结束大小"		,
		//		"pointEndSizeFloat"		, "粒子结束大小浮动"	,
		//		"pointStartAngle"		, "粒子开始角度"		,
		//		"pointStartAngleFloat"	, "粒子开始角度浮动"	,
		//		"pointEndAngle"			, "粒子旋转速度"		,
		//		"pointEndAngleFloat"	, "粒子旋转速度浮动"    ,
		//	};
		//	string strAttrDesc = "";

		//	for(int i = 0; i < arrDesc.Length; i+=2) {
		//		strAttrDesc += (i == 0) ? "" : "\r\n";
		//		strAttrDesc += arrDesc[i + 1];
		//	}
		//	txtAttrDesc.Text = strAttrDesc;
		//}

		//private void updateAttrText() {

		//}

		//private void updateAttrText() {
		//	int idx = selectEmitter;
		//	if (idx < 0) {
		//		return;
		//	}

		//	ParticleEditCtl ctl = MainCtl.ins.particleEditCtl;
		//	ParticleEditModel md = MainModel.ins.particleEditModel;
		//	var item = md.lstResource[idx];

		//	string[] arr = new string[] {
		//		item.x.ToString(),
		//		item.xFloat.ToString(),
		//		item.y.ToString(),
		//		item.yFloat.ToString(),
		//		item.startAlpha.ToString(),
		//		item.endAlpha.ToString(),
		//		item.gravityValue.ToString(),
		//		item.gravityAngle.ToString(),
		//		item.startSpeed.ToString(),
		//		item.startSpeedFloat.ToString(),
		//		item.startSpeedAngle.ToString(),
		//		item.startSpeedAngleFloat.ToString(),
		//		item.rotateSpeed.ToString(),
		//		item.rotateSpeedFloat.ToString(),
		//		item.directionSpeed.ToString(),
		//		item.directionSpeedFloat.ToString(),
		//		item.particleCount.ToString(),
		//		item.particleLife.ToString(),
		//		item.particleLifeFloat.ToString(),
		//		item.particleStartSize.ToString(),
		//		item.particleStartSizeFloat.ToString(),
		//		item.particleEndSize.ToString(),
		//		item.particleEndSizeFloat.ToString(),
		//		item.particleStartAngle.ToString(),
		//		item.particleStartAngleFloat.ToString(),
		//		item.particleRotateSpeed.ToString(),
		//		item.particleRotateSpeedFloat.ToString(),
		//	};

		//	//string strAttrDesc = "";
		//	string strAttrValue = "";
		//	//for(int i = 0; i < ctl.lstAttrMd.Count; ++i) {
		//	//	var item = ctl.lstAttrMd[i];

		//	//	if(i != 0) {
		//	//		strAttrDesc += "\r\n";
		//	//		strAttrValue += "\r\n";
		//	//	}

		//	//	strAttrDesc += item.desc;
		//	//	strAttrValue += item.value;
		//	//}

		//	//txtAttrDesc.Text = strAttrDesc;

		//	for(int i = 0; i < arr.Length; ++i) {
		//		strAttrValue += (i == 0) ? "" : "\r\n";
		//		strAttrValue += arr[i];
		//	}

		//	//isUpdatTextInner = true;
		//	//txtAttrValue.Text = strAttrValue;
		//	//txtAttrValue.IsUndoEnabled = false;
		//	//txtAttrValue.IsUndoEnabled = true;
		//	//isUpdatTextInner = false;
		//}

		//private void updateAttrByText() {
		//	int idx = selectEmitter;
		//	if (idx < 0) {
		//		return;
		//	}

		//	string str = txtAttrValue.Text;

		//	const int attrCount = 27;

		//	string[] arr = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
		//	List<string> lstStr = new List<string>(arr);
		//	for (int i = lstStr.Count; i < attrCount; ++i) {
		//		lstStr.Add("");
		//	}


		//	ParticleEditModel md = MainModel.ins.particleEditModel;
		//	var item = md.lstResource[idx];

		//	item.x = 						getFloat(lstStr[ 0], item.x);
		//	item.xFloat = 					getFloat(lstStr[ 1], item.xFloat);
		//	item.y = 						getFloat(lstStr[ 2], item.y);
		//	item.yFloat = 					getFloat(lstStr[ 3], item.yFloat);
		//	item.startAlpha = 				getFloat(lstStr[ 4], item.startAlpha);
		//	item.endAlpha = 				getFloat(lstStr[ 5], item.endAlpha);
		//	item.gravityValue = 			getFloat(lstStr[ 6], item.gravityValue);
		//	item.gravityAngle = 			getFloat(lstStr[ 7], item.gravityAngle);
		//	item.startSpeed = 				getFloat(lstStr[ 8], item.startSpeed);
		//	item.startSpeedFloat = 			getFloat(lstStr[ 9], item.startSpeedFloat);
		//	item.startSpeedAngle = 			getFloat(lstStr[10], item.startSpeedAngle);
		//	item.startSpeedAngleFloat = 	getFloat(lstStr[11], item.startSpeedAngleFloat);
		//	item.rotateSpeed = 				getFloat(lstStr[12], item.rotateSpeed);
		//	item.rotateSpeedFloat = 		getFloat(lstStr[13], item.rotateSpeedFloat);
		//	item.directionSpeed = 			getFloat(lstStr[14], item.directionSpeed);
		//	item.directionSpeedFloat = 		getFloat(lstStr[15], item.directionSpeedFloat);
		//	item.particleCount = 				getInt  (lstStr[16], item.particleCount);
		//	item.particleLife = 				getFloat(lstStr[17], item.particleLife);
		//	item.particleLifeFloat = 			getFloat(lstStr[18], item.particleLifeFloat);
		//	item.particleStartSize = 			getFloat(lstStr[19], item.particleStartSize);
		//	item.particleStartSizeFloat = 		getFloat(lstStr[20], item.particleStartSizeFloat);
		//	item.particleEndSize = 			getFloat(lstStr[21], item.particleEndSize);
		//	item.particleEndSizeFloat = 		getFloat(lstStr[22], item.particleEndSizeFloat);
		//	item.particleStartAngle = 			getFloat(lstStr[23], item.particleStartAngle);
		//	item.particleStartAngleFloat = 	getFloat(lstStr[24], item.particleStartAngleFloat);
		//	item.particleRotateSpeed = 		getFloat(lstStr[25], item.particleRotateSpeed);
		//	item.particleRotateSpeedFloat = 	getFloat(lstStr[26], item.particleRotateSpeedFloat);

		//	pointRenderBox.updateEmitterAttr(idx);
		//}

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

		//private void txtAttrValue_ScrollChanged(object sender, ScrollChangedEventArgs e) {
		//	txtAttrDesc.ScrollToVerticalOffset(e.VerticalOffset);
		//}

		//private void txtAttrValue_TextChanged(object sender, TextChangedEventArgs e) {
		//	if (!isEngineInited) {
		//		return;
		//	}

		//	if (selectEmitter < 0) {
		//		return;
		//	}

		//	if (isUpdatTextInner) {
		//		return;
		//	}

		//	updateAttrByText();
		//	//updateAttrText();
		//}

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
			UInt32.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out uint coBackground);

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

			pointRenderBox.updateGlobalAttr();

			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}
			//UInt32.TryParse(md.background, System.Globalization.NumberStyles.HexNumber, null, out uint coBackground);

			//isEditGlobalAttrByText = true;
			//cpkBackground.Value = coBackground;
			//isEditGlobalAttrByText = false;

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

			pointRenderBox.updateEmitterAttr(idx);
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

			pointRenderBox.updateGlobalAttr();

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
			pointRenderBox.updateEmitterImage(idx);
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
			pointRenderBox.createEmitter(idx);

			setSelectEmitter(idx);
		}

		private void btnRemoveEmitter_Click(object sender, RoutedEventArgs e) {
			int idx = lstRes.SelectedIndex;
			if(idx < 0 || idx >= ctl.lstVM.Count) {
				return;
			}

			pointRenderBox.removeEmitter(idx);
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
