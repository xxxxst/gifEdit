﻿using csharpHelp.util;
using gifEdit.control;
using gifEdit.model;
using gifEdit.services;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
		private bool isUpdatTextInner = false;
		private int selectEmitter = -1;

		public ParticleEditBox() {
			InitializeComponent();

			DataContext = vm;

			EventServer.ins.pointEngineInitedEvent += () => {
				isEngineInited = true;

				if (cachePath != "") {
					string path = cachePath;
					cachePath = "";
					load(path);
				}
			};

			initAttrDesc();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {

		}

		public void load(string path) {
			if (!isEngineInited) {
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
				} catch (Exception) { }
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

		private void initAttrDesc() {
			string[] arrDesc = new string[] {
				"x"						, "坐标x"				,
				"xFloat"				, "坐标x浮动"			,
				"y"						, "坐标y"				,
				"yFloat"				, "坐标y浮动"			,
				"startAlpha"            , "开始透明度"          ,
				"endAlpha"              , "结束透明度"          ,
				"gravityValue"			, "重力"				,
				"gravityAngle"			, "重力方向"			,
				"startSpeed"			, "开始速度"			,
				"startSpeedFloat"		, "开始速度浮动"		,
				"startSpeedAngle"		, "开始速度方向"		,
				"startSpeedAngleFloat"	, "开始速度方向浮动"	,
				"rotateSpeed"			, "旋转速度"			,
				"rotateSpeedFloat"		, "旋转速度浮动"		,
				"directionSpeed"		, "分离速度"			,
				"directionSpeedFloat"	, "分离速度方向"		,
				"pointCount"			, "粒子数"				,
				"pointLife"				, "粒子生命周期"		,
				"pointLifeFloat"		, "粒子生命周期浮动"	,
				"pointStartSize"		, "粒子开始大小"		,
				"pointStartSizeFloat"	, "粒子开始大小浮动"	,
				"pointEndSize"			, "粒子结束大小"		,
				"pointEndSizeFloat"		, "粒子结束大小浮动"	,
				"pointStartAngle"		, "粒子开始角度"		,
				"pointStartAngleFloat"	, "粒子开始角度浮动"	,
				"pointEndAngle"			, "粒子旋转速度"		,
				"pointEndAngleFloat"	, "粒子旋转速度浮动"    ,
			};
			string strAttrDesc = "";
			
			for(int i = 0; i < arrDesc.Length; i+=2) {
				strAttrDesc += (i == 0) ? "" : "\r\n";
				strAttrDesc += arrDesc[i + 1];
			}
			txtAttrDesc.Text = strAttrDesc;
		}

		private void updateAttrText() {
			int idx = selectEmitter;
			if (idx < 0) {
				return;
			}

			ParticleEditCtl ctl = MainCtl.ins.particleEditCtl;
			ParticleEditModel md = MainModel.ins.particleEditModel;
			var item = md.lstResource[idx];

			string[] arr = new string[] {
				item.x.ToString(),
				item.xFloat.ToString(),
				item.y.ToString(),
				item.yFloat.ToString(),
				item.startAlpha.ToString(),
				item.endAlpha.ToString(),
				item.gravityValue.ToString(),
				item.gravityAngle.ToString(),
				item.startSpeed.ToString(),
				item.startSpeedFloat.ToString(),
				item.startSpeedAngle.ToString(),
				item.startSpeedAngleFloat.ToString(),
				item.rotateSpeed.ToString(),
				item.rotateSpeedFloat.ToString(),
				item.directionSpeed.ToString(),
				item.directionSpeedFloat.ToString(),
				item.particleCount.ToString(),
				item.particleLife.ToString(),
				item.particleLifeFloat.ToString(),
				item.particleStartSize.ToString(),
				item.particleStartSizeFloat.ToString(),
				item.particleEndSize.ToString(),
				item.particleEndSizeFloat.ToString(),
				item.particleStartAngle.ToString(),
				item.particleStartAngleFloat.ToString(),
				item.particleRotateSpeed.ToString(),
				item.particleRotateSpeedFloat.ToString(),
			};

			//string strAttrDesc = "";
			string strAttrValue = "";
			//for(int i = 0; i < ctl.lstAttrMd.Count; ++i) {
			//	var item = ctl.lstAttrMd[i];

			//	if(i != 0) {
			//		strAttrDesc += "\r\n";
			//		strAttrValue += "\r\n";
			//	}

			//	strAttrDesc += item.desc;
			//	strAttrValue += item.value;
			//}

			//txtAttrDesc.Text = strAttrDesc;

			for(int i = 0; i < arr.Length; ++i) {
				strAttrValue += (i == 0) ? "" : "\r\n";
				strAttrValue += arr[i];
			}

			isUpdatTextInner = true;
			txtAttrValue.Text = strAttrValue;
			txtAttrValue.IsUndoEnabled = false;
			txtAttrValue.IsUndoEnabled = true;
			isUpdatTextInner = false;
		}

		private void updateAttrByText() {
			int idx = selectEmitter;
			if (idx < 0) {
				return;
			}

			string str = txtAttrValue.Text;

			const int attrCount = 27;

			string[] arr = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			List<string> lstStr = new List<string>(arr);
			for (int i = lstStr.Count; i < attrCount; ++i) {
				lstStr.Add("");
			}


			ParticleEditModel md = MainModel.ins.particleEditModel;
			var item = md.lstResource[idx];

			item.x = 						getFloat(lstStr[ 0], item.x);
			item.xFloat = 					getFloat(lstStr[ 1], item.xFloat);
			item.y = 						getFloat(lstStr[ 2], item.y);
			item.yFloat = 					getFloat(lstStr[ 3], item.yFloat);
			item.startAlpha = 				getFloat(lstStr[ 4], item.startAlpha);
			item.endAlpha = 				getFloat(lstStr[ 5], item.endAlpha);
			item.gravityValue = 			getFloat(lstStr[ 6], item.gravityValue);
			item.gravityAngle = 			getFloat(lstStr[ 7], item.gravityAngle);
			item.startSpeed = 				getFloat(lstStr[ 8], item.startSpeed);
			item.startSpeedFloat = 			getFloat(lstStr[ 9], item.startSpeedFloat);
			item.startSpeedAngle = 			getFloat(lstStr[10], item.startSpeedAngle);
			item.startSpeedAngleFloat = 	getFloat(lstStr[11], item.startSpeedAngleFloat);
			item.rotateSpeed = 				getFloat(lstStr[12], item.rotateSpeed);
			item.rotateSpeedFloat = 		getFloat(lstStr[13], item.rotateSpeedFloat);
			item.directionSpeed = 			getFloat(lstStr[14], item.directionSpeed);
			item.directionSpeedFloat = 		getFloat(lstStr[15], item.directionSpeedFloat);
			item.particleCount = 				getInt  (lstStr[16], item.particleCount);
			item.particleLife = 				getFloat(lstStr[17], item.particleLife);
			item.particleLifeFloat = 			getFloat(lstStr[18], item.particleLifeFloat);
			item.particleStartSize = 			getFloat(lstStr[19], item.particleStartSize);
			item.particleStartSizeFloat = 		getFloat(lstStr[20], item.particleStartSizeFloat);
			item.particleEndSize = 			getFloat(lstStr[21], item.particleEndSize);
			item.particleEndSizeFloat = 		getFloat(lstStr[22], item.particleEndSizeFloat);
			item.particleStartAngle = 			getFloat(lstStr[23], item.particleStartAngle);
			item.particleStartAngleFloat = 	getFloat(lstStr[24], item.particleStartAngleFloat);
			item.particleRotateSpeed = 		getFloat(lstStr[25], item.particleRotateSpeed);
			item.particleRotateSpeedFloat = 	getFloat(lstStr[26], item.particleRotateSpeedFloat);

			pointRenderBox.updateEmitterAttr(idx);
		}

		private float getFloat(string str, float def) {
			if (str == "") {
				return def;
			}

			bool isOk = float.TryParse(str, out float rst);
			return isOk ? rst : def;
		}

		private int getInt(string str, int def) {
			if (str == "") {
				return def;
			}

			bool isOk = Int32.TryParse(str, out int rst);
			return isOk ? rst : def;
		}

		private void txtAttrValue_ScrollChanged(object sender, ScrollChangedEventArgs e) {
			txtAttrDesc.ScrollToVerticalOffset(e.VerticalOffset);
		}

		private void txtAttrValue_TextChanged(object sender, TextChangedEventArgs e) {
			if (!isEngineInited) {
				return;
			}

			if (selectEmitter < 0) {
				return;
			}
			
			if (isUpdatTextInner) {
				return;
			}

			updateAttrByText();
			//updateAttrText();
		}

		private void grdProjectName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (vm.IsSelectProjectName) {
				return;
			}

			if (selectEmitter >= 0 && selectEmitter < ctl.lstVM.Count) {
				ctl.lstVM[selectEmitter].IsSelect = false;
			}

			selectEmitter = -1;
			lstRes.SelectedIndex = -1;
			grdParticleSetting.Visibility = Visibility.Collapsed;
			grdProjectSetting.Visibility = Visibility.Visible;

			vm.IsSelectProjectName = true;
		}

		private void grdEmitterBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstRes.SelectedIndex;
			if (idx < 0 || idx >= ctl.lstVM.Count) {
				return;
			}

			if (idx == selectEmitter) {
				return;
			}

			if (vm.IsSelectProjectName) {
				vm.IsSelectProjectName = false;
				grdProjectSetting.Visibility = Visibility.Collapsed;
				grdParticleSetting.Visibility = Visibility.Visible;
			}

			if (selectEmitter >= 0 && selectEmitter < ctl.lstVM.Count) {
				ctl.lstVM[selectEmitter].IsSelect = false;
			}

			selectEmitter = idx;
			ctl.lstVM[idx].IsSelect = true;
			updateAttrText();
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