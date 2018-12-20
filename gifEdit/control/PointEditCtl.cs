using csharpHelp.services;
using gifEdit.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gifEdit.control {
	public class PointEditCtl {
		public ObservableCollection<PointEditVM> lstVM = new ObservableCollection<PointEditVM>();
		//public List<AttrMd> lstAttrMd = new List<AttrMd>();

		public void load(string path) {
			path = path.Trim(new char[] { '/', '\\', ' ', '\t' });
			string xmlPath = path + "/config.gife";

			PointEditModel md = new PointEditModel();
			md.path = path;
			md.name = new Regex(".*?([^\\/]*$)").Replace(path, "$1");

			XmlModelServer srv = new XmlModelServer(md, xmlPath);
			md.srv = srv;
			md.srv.loadFromXml();

			MainModel.ins.pointEditModel = md;

			//md.srv.save();

			lstVM.Clear();
			for(int i = 0; i < md.lstResource.Count; ++i) {
				string imgPath = md.lstResource[i].path;
				ImageSource img = loadImage(imgPath, path);
				PointEditVM vm = new PointEditVM() {
					Image = img
				};
				lstVM.Add(vm);
			}

			if(lstVM.Count > 0) {
				//lstVM.Last().IsLast = true;
			}
		}

		//public void initAttr(int idx) {
		//	PointEditModel md = MainModel.ins.pointEditModel;

		//	lstAttrMd.Clear();

		//	if(idx < 0 || idx >= md.lstResource.Count) {
		//		return;
		//	}

		//	PointResourceModel res = md.lstResource[idx];
			
		//	lstAttrMd.Add(new AttrMd("x"					, "坐标x"				, res.x));
		//	lstAttrMd.Add(new AttrMd("xFloat"				, "坐标x浮动"			, res.xFloat));
		//	lstAttrMd.Add(new AttrMd("y"					, "坐标y"				, res.y));
		//	lstAttrMd.Add(new AttrMd("yFloat"				, "坐标y浮动"			, res.yFloat));
		//	lstAttrMd.Add(new AttrMd("gravityValue"			, "重力"				, res.gravityValue));
		//	lstAttrMd.Add(new AttrMd("gravityAngle"			, "重力方向"			, res.gravityAngle));
		//	lstAttrMd.Add(new AttrMd("startSpeed"			, "开始速度"			, res.startSpeed));
		//	lstAttrMd.Add(new AttrMd("startSpeedFloat"		, "开始速度浮动"		, res.startSpeedFloat));
		//	lstAttrMd.Add(new AttrMd("startSpeedAngle"		, "开始速度方向"		, res.startSpeedAngle));
		//	lstAttrMd.Add(new AttrMd("startSpeedAngleFloat"	, "开始速度方向浮动"	, res.startSpeedAngleFloat));
		//	lstAttrMd.Add(new AttrMd("rotateSpeed"			, "旋转速度"			, res.rotateSpeed));
		//	lstAttrMd.Add(new AttrMd("rotateSpeedFloat"		, "旋转速度浮动"		, res.rotateSpeedFloat));
		//	lstAttrMd.Add(new AttrMd("directionSpeed"		, "分离速度"			, res.directionSpeed));
		//	lstAttrMd.Add(new AttrMd("directionSpeedFloat"	, "分离速度方向"		, res.directionSpeedFloat));
		//	lstAttrMd.Add(new AttrMd("pointCount"			, "粒子数"				, res.pointCount));
		//	lstAttrMd.Add(new AttrMd("pointLife"			, "粒子生命周期"		, res.pointLife));
		//	lstAttrMd.Add(new AttrMd("pointLifeFloat"		, "粒子生命周期浮动"	, res.pointLifeFloat));
		//	lstAttrMd.Add(new AttrMd("pointStartSize"		, "粒子开始大小"		, res.pointStartSize));
		//	lstAttrMd.Add(new AttrMd("pointStartSizeFloat"	, "粒子开始大小浮动"	, res.pointStartSizeFloat));
		//	lstAttrMd.Add(new AttrMd("pointEndSize"			, "粒子结束大小"		, res.pointEndSize));
		//	lstAttrMd.Add(new AttrMd("pointEndSizeFloat"	, "粒子结束大小浮动"	, res.pointEndSizeFloat));
		//	lstAttrMd.Add(new AttrMd("pointStartAngle"		, "粒子开始角度"		, res.pointStartAngle));
		//	lstAttrMd.Add(new AttrMd("pointStartAngleFloat"	, "粒子开始角度浮动"	, res.pointStartAngleFloat));
		//	lstAttrMd.Add(new AttrMd("pointEndAngle"		, "粒子结束角度"		, res.pointEndAngle));
		//	lstAttrMd.Add(new AttrMd("pointEndAngleFloat"	, "粒子结束角度浮动"	, res.pointEndAngleFloat));
		//}

		private ImageSource loadImage(string path, string basePath) {
			if(path.Length < 2 || path[1] != ':') {
				path = basePath + "/" + path;
			}
			if(!File.Exists(path)) {
				return null;
			}

			try {
				ImageSource img = new BitmapImage(new Uri(path));
				return img;
			} catch(Exception) { }

			return null;
		}
	}
	
	public class PointEditVM : INotifyPropertyChanged {
		//image
		ImageSource _Image = null;
		public ImageSource Image {
			get { return _Image; }
			set { _Image = value; updatePro("Image"); }
		}

		//Is Select
		bool _IsSelect = false;
		public bool IsSelect {
			get { return _IsSelect; }
			set { _IsSelect = value; updatePro("IsSelect"); }
		}

		//IsLast
		bool _IsLast = false;
		public bool IsLast {
			get { return _IsLast; }
			set { _IsLast = value; updatePro("IsLastVisibile"); }
		}

		//IsLastVisibile
		public Visibility IsLastVisibile {
			get { return _IsLast ? Visibility.Collapsed : Visibility.Visible; }
		}

		//
		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual void updatePro(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public static SolidColorBrush createBrush(string color) {
			return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
		}
	}

	//public class AttrMd {
	//	public string name;
	//	public string desc;
	//	public string value;

	//	public AttrMd() { }

	//	public AttrMd(string _name, string _desc, string _value) {
	//		name = _name;
	//		desc = _desc;
	//		value = _value;
	//	}

	//	public AttrMd(string _name, string _desc, double _value) {
	//		name = _name;
	//		desc = _desc;
	//		value = _value.ToString();
	//	}

	//	public AttrMd(string _name, string _desc, int _value) {
	//		name = _name;
	//		desc = _desc;
	//		value = _value.ToString();
	//	}
	//}
}
