using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.util {
	public class ComUtil {
		public static string loadEmbedText(string path) {
			try {
				var asm = Assembly.GetExecutingAssembly();
				using(var rs = asm.GetManifestResourceStream(path))
				using(var sr = new StreamReader(rs)) {
					return sr.ReadToEnd();
				}
			} catch(Exception) { }

			return "";
		}
		public static string[] loadEmbedLines(string path) {
			List<string> lst = new List<string>();

			try {
				var asm = Assembly.GetExecutingAssembly();
				using(var rs = asm.GetManifestResourceStream(path))
				using(var sr = new StreamReader(rs)) {
					while(!sr.EndOfStream) {
						lst.Add(sr.ReadLine());
					};
				}
			} catch(Exception) { }
			
			return lst.ToArray();
		}

		public static string[] loadEmbedShader(string path) {
			path = "gifEdit.resource.gifEditShader." + path;
			List<string> lst = new List<string>();

			try {
				var asm = Assembly.GetExecutingAssembly();
				using(var rs = asm.GetManifestResourceStream(path))
				using(var sr = new StreamReader(rs)) {
					while(!sr.EndOfStream) {
						string str = sr.ReadLine().Trim();
						if(str == "") {
							continue;
						}
						str += "\n";
						lst.Add(str);
					};
				}
			} catch(Exception) { }

			return lst.ToArray();
		}
	}
}
