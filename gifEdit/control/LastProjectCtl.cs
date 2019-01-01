using desktopDate.util;
using gifEdit.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gifEdit.control {
	public class LastProjectCtl {
		public ObservableCollection<LastProjectVM> lstVM = new ObservableCollection<LastProjectVM>();

		public void init() {
			var lst = MainModel.ins.configModel.lstLastProject;

			string rootPath = SysConst.rootPath();

			for (int i = 0; i < lst.Count; ++i) {
				lst[i].path = MainCtl.formatPath(lst[i].path);
			}

			lst.Sort((a, b) => b.lastOpenTime.CompareTo(a.lastOpenTime));

			lstVM.Clear();
			for (int i = 0; i < lst.Count; ++i) {
				LastProjectVM vm = new LastProjectVM(lst[i]);

				lstVM.Add(vm);
			}

			if (lstVM.Count > 0) {
				lstVM.Last().IsLast = true;
			}
		}

		public void create(string path, PrjoectType type) {
			var lst = MainModel.ins.configModel.lstLastProject;
			LastProject md = new LastProject() {
				name = Path.GetFileNameWithoutExtension(path),
				path = MainCtl.formatPath(path),
				type = type,
				lastOpenTime = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")
			};
			LastProjectVM vm = new LastProjectVM(md);

			lst.Add(md);
			lstVM.Add(vm);
		}

		public void updatTime(int idx) {
			var lst = MainModel.ins.configModel.lstLastProject;
			if (idx < 0 || idx >= lstVM.Count) {
				return;
			}

			var vm = lstVM[idx];
			lstVM[idx].LastOpenTime = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
			lstVM.RemoveAt(idx);
			lstVM.Insert(0, vm);

			var md = lst[idx];
			lst.RemoveAt(idx);
			lst.Insert(0, md);
		}
	}

	public class LastProjectVM : INotifyPropertyChanged {
		public LastProject md = null;

		public LastProjectVM() { }

		public LastProjectVM(LastProject _md) {
			md = _md;
		}

		//Name
		public string Name {
			get { return md.name; }
			set { md.name = value; updatePro("Name"); }
		}

		//Type
		public PrjoectType Type {
			get { return md.type; }
			set { md.type = value; updatePro("Type"); updatePro("StrType"); }
		}

		//StrType
		public string StrType {
			get { return getType(md.type); }
		}

		//Path
		public string Path {
			get { return md.path; }
			set { md.path = value; updatePro("Path"); }
		}

		//LastOpenTime
		public string LastOpenTime {
			get { return md.lastOpenTime; }
			set { md.lastOpenTime = value; updatePro("LastOpenTime"); }
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

		private string getType(PrjoectType type) {
			switch (type) {
				case PrjoectType.Particle: return "粒子";
				case PrjoectType.Spirit: default: return "精灵";
			}
		}
	}
}
