using csharpHelp.util;
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
	/// <summary>
	/// PointEditBox.xaml 的交互逻辑
	/// </summary>
	public partial class PointEditBox : UserControl {
		//PointRenderWin win = null;
		//System.Windows.Forms.Panel pnlRender = new System.Windows.Forms.Panel();

		public PointEditBox() {
			InitializeComponent();

			initAttrDesc();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {

		}

		public void load(string path) {
			PointEditCtl ctl = MainCtl.ins.pointEditCtl;
			
			ctl.load(path);

			lstRes.ItemsSource = ctl.lstVM;

			PointEditModel md = MainModel.ins.pointEditModel;
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

			//EventServer.ins.mainWinExitedEvent += () => {
			//	try {
			//		win.Close();
			//	} catch(Exception) { }
			//};
			pointRenderBox.init();
			
			updateAttr(0);
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

		private void updateAttr(int idx) {
			PointEditCtl ctl = MainCtl.ins.pointEditCtl;
			PointEditModel md = MainModel.ins.pointEditModel;
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
				item.pointCount.ToString(),
				item.pointLife.ToString(),
				item.pointLifeFloat.ToString(),
				item.pointStartSize.ToString(),
				item.pointStartSizeFloat.ToString(),
				item.pointEndSize.ToString(),
				item.pointEndSizeFloat.ToString(),
				item.pointStartAngle.ToString(),
				item.pointStartAngleFloat.ToString(),
				item.pointRotateSpeed.ToString(),
				item.pointRotateSpeedFloat.ToString(),
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
			
			txtAttrValue.Text = strAttrValue;
		}

		private void txtAttrValue_ScrollChanged(object sender, ScrollChangedEventArgs e) {
			txtAttrDesc.ScrollToVerticalOffset(e.VerticalOffset);
		}
	}
}
