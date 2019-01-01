using desktopDate.util;
using gifEdit.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gifEdit.control {
	public class MainCtl {
		public static MainCtl ins = new MainCtl();

		public ParticleEditCtl particleEditCtl = new ParticleEditCtl();
		public LastProjectCtl lastProjectCtl = new LastProjectCtl();

		public CreateProjectResult createProject(string path, PrjoectType type) {
			CreateProjectResult rst = CreateProjectResult.Ok;

			if (type == PrjoectType.Spirit) {
				return rst;
			}

			if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length > 0) {
				rst = CreateProjectResult.DirectoryExist;
				return rst;
			}

			try {
				Directory.CreateDirectory(path);

				lastProjectCtl.create(path, type);

				//pointEditCtl.load(path);

				var win = MainModel.ins.mainWin;
				win.openProject(path, type);

			} catch (Exception) {
				rst = CreateProjectResult.Error;
			}

			return rst;
		}

		public static string getFullPath(string path) {
			if (isAbsolutePath(path)) {
				return path;
			}

			return SysConst.rootPath() + "/" + path;
		}

		public static string formatPath(string path) {
			string rootPath = SysConst.rootPath();

			if (!isAbsolutePath(path)) {
				return path;
			}

			path = Path.GetFullPath(path);
			if (path.IndexOf(rootPath) == 0) {
				path = path.Substring(rootPath.Length);
			}

			path = new Regex("[/\\\\]+").Replace(path, "/");

			return path;
		}

		public static bool isAbsolutePath(string path) {
			return path.Length >= 2 && path[1] == ':';
		}

		public void openProject(int idx) {
			var lst = MainModel.ins.configModel.lstLastProject;
			if (idx < 0 || idx >= lst.Count) {
				return;
			}
			var md = lst[idx];

			if (md.type == PrjoectType.Spirit) {
				return;
			}

			lastProjectCtl.updatTime(idx);

			//pointEditCtl.load(getFullPath(md.path));

			var win = MainModel.ins.mainWin;
			win.openProject(md.path, md.type);

		}

		public enum CreateProjectResult {
			Ok,
			DirectoryExist,
			Error,
		}
	}
}
