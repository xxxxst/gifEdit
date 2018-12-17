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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gifEdit.control {
	public class PointEditCtl {
		public ObservableCollection<PointEditVM> lstVM = new ObservableCollection<PointEditVM>();

		public void load(string path) {
			string xmlPath = path + "/config.gife";

			PointEditModel md = new PointEditModel();
			md.path = xmlPath;
			md.name = Path.GetDirectoryName(xmlPath);

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
}
