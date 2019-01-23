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
	public class ParticleEditCtl {
		public ObservableCollection<ParticleEditVM> lstVM = new ObservableCollection<ParticleEditVM>();
		//public List<AttrMd> lstAttrMd = new List<AttrMd>();

		public ParticleEditModel md = null;
		private XmlModelServer srv = null;

		public void load(string path) {
			path = path.Trim(new char[] { '/', '\\', ' ', '\t' });
			string xmlPath = path + "/config.gife";

			md = new ParticleEditModel();
			md.path = path;
			md.name = new Regex(".*?([^\\/]*$)").Replace(path, "$1");

			srv = new XmlModelServer(md, xmlPath);
			md.srv = srv;
			md.srv.loadFromXml();

			MainModel.ins.particleEditModel = md;

			//md.srv.save();

			lstVM.Clear();
			for(int i = 0; i < md.lstResource.Count; ++i) {
				string imgPath = md.lstResource[i].path;
				ImageSource img = loadImage(imgPath, path);
				ParticleEditVM vm = new ParticleEditVM() {
					idx = i,
					md = md.lstResource[i],
					Image = img,
					ImgName = Path.GetFileName(imgPath)
				};
				lstVM.Add(vm);
			}

			if(lstVM.Count > 0) {
				lstVM.Last().IsLast = true;
			}
		}

		public void updateImage(int idx) {
			if(idx < 0 || idx >= lstVM.Count) {
				return;
			}

			string imgPath = md.lstResource[idx].path;
			lstVM[idx].Image = loadImage(imgPath, md.path);
			lstVM[idx].ImgName = Path.GetFileName(imgPath);
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

		public int createEmitter() {
			ParticleResourceModel resMd = new ParticleResourceModel();
			md.lstResource.Add(resMd);

			int idx = lstVM.Count;
			if(idx > 0) {
				lstVM[idx - 1].IsLast = false;
			}

			ParticleEditVM vm = new ParticleEditVM() {
				idx = idx,
				md = resMd,
				Image = null,
				IsLast = true
			};
			lstVM.Add(vm);

			return idx;
		}

		public void removeEmitter(int idx) {
			if(idx < 0 || idx >= lstVM.Count) {
				return;
			}

			if(idx > 0 && idx == lstVM.Count - 1) {
				lstVM[idx - 1].IsLast = true;
			}

			lstVM.RemoveAt(idx);
			md.lstResource.RemoveAt(idx);

			for(int i = idx; i < lstVM.Count; ++i) {
				lstVM[i].idx = i;
			}
		}

		public void save() {
			srv?.save();
		}

	}
	
	public class ParticleEditVM : INotifyPropertyChanged {
		public int idx = 0;
		public ParticleResourceModel md = null;

		//image
		ImageSource _Image = null;
		public ImageSource Image {
			get { return _Image; }
			set { _Image = value; updatePro("Image"); }
		}

		//ImgName
		string _ImgName = null;
		public string ImgName {
			get { return _ImgName; }
			set { _ImgName = value; updatePro("ImgName"); }
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
			set { _IsLast = value; updatePro("IsLineVisibile"); }
		}

		//IsLineVisibile
		public Visibility IsLineVisibile {
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
