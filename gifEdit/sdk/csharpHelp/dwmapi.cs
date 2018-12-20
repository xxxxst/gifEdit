using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class dwmapi {
		[DllImport("dwmapi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();

		[StructLayout(LayoutKind.Sequential)]
		public struct MARGINS {
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}
	}
}
