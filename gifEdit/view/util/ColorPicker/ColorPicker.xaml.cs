using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
	/// <summary>ColorPicker</summary>
	public partial class ColorPicker : UserControl {
		Window win = null;

		public ColorPicker() {
			InitializeComponent();
		}

		private void initEvent() {
			Window _win = Window.GetWindow(this);
			if (win == _win) {
				return;
			}

			if (win != null) {
				win.MouseLeftButtonUp -= Win_MouseLeftButtonUp;
				win.MouseMove -= Win_MouseMove;
			}
			win = _win;

			win.MouseLeftButtonUp += Win_MouseLeftButtonUp;
			win.MouseMove += Win_MouseMove;
		}

		private float[] hsl2Rgb(float[] hsl) {
			// HSL取值范围(0, 1)
			float H = hsl[0];
			float S = hsl[1];
			float L = hsl[2];

			float[] rst = new float[3];

			float R, G, B;
			float q, p;
			float[] T = new float[3];

			if (S == 0) {
				R = G = B = L;
			} else {
				if (L < 0.5) {
					q = L * (1.0f + S);
				} else {
					q = L + S - L * S;
				}
				p = 2.0f * L - q;

				T[0] = H + 1f/3;
				T[1] = H;
				T[2] = H - 1f/3;
				for (int i = 0; i < 3; i++) {
					if (T[i] < 0) {
						T[i] += 1.0f;
					}
					if (T[i] > 1) {
						T[i] -= 1.0f;
					}
					if ((T[i] * 6) < 1) {
						T[i] = p + ((q - p) * 6.0f * T[i]);
					} else if ((T[i] * 2.0f) < 1) {
						T[i] = q;
					} else if ((T[i] * 3.0f) < 2) {
						T[i] = p + (q - p) * ((2.0f / 3.0f) - T[i]) * 6.0f;
					} else T[i] = p;
				}
				R = T[0];
				G = T[1];
				B = T[2];
			}
			R = ((R > 1) ? 1 : ((R < 0) ? 0 : R));//取值范围(0,1)
			G = ((G > 1) ? 1 : ((G < 0) ? 0 : G));//取值范围(0,1)
			B = ((B > 1) ? 1 : ((B < 0) ? 0 : B));//取值范围(0,1)

			rst[0] = R;
			rst[1] = G;
			rst[2] = B;
			return rst;
		}

		//private void ColorPicker_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
		//	if (e.ChangedButton == MouseButton.Left) {
		//		isDownMain = false;
		//	}
		//}

		private float xPercent = 0;	//v
		private float yPercent = 0;	//s
		private float hPercent = 0; //h
		private float tPercent = 1; //transparent

		private bool isDownMain = false;
		private bool isDownH = false;
		private bool isDownT = false;

		private void grdMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			initEvent();

			isDownMain = true;
			win?.CaptureMouse();
		}

		private void grdH_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			initEvent();

			isDownH = true;
			win?.CaptureMouse();
		}

		private void grdTransparent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			initEvent();

			isDownT = true;
			win?.CaptureMouse();
		}

		private void Win_MouseMove(object sender, MouseEventArgs e) {
			if (isDownMain) {
				//明度/饱和度
				double w = grdMain.ActualWidth;
				double h = grdMain.ActualHeight;
				if (w <= 0 || h <= 0) {
					return;
				}

				Point pos = e.GetPosition(grdMain);

				xPercent = (float)(pos.X / w);
				yPercent = (float)(pos.Y / h);

				xPercent = Math.Max(0, Math.Min(xPercent, 1));
				yPercent = Math.Max(0, Math.Min(yPercent, 1));

				sldMain.Margin = new Thickness(xPercent * w, yPercent * h, 0, 0);

			} else if (isDownH) {
				//色相
				double h = grdH.ActualHeight;
				if (h <= 0) {
					return;
				}
				Point pos = e.GetPosition(grdH);

				hPercent = (float)(pos.Y / h);
				hPercent = Math.Max(0, Math.Min(hPercent, 1));

				sldH.Margin = new Thickness(0, hPercent * h, 0, 0);

			} else if (isDownT) {
				//透明度
				double w = grdTransparent.ActualWidth;
				if (w <= 0) {
					return;
				}
				Point pos = e.GetPosition(grdTransparent);

				tPercent = (float)(pos.X / w);
				tPercent = Math.Max(0, Math.Min(tPercent, 1));

				sldT.Margin = new Thickness(tPercent * w, 0, 0, 0);

			}
		}

		private void updateSld() {
			double w = grdMain.ActualWidth;
			double h = grdMain.ActualHeight;
			if (w <= 0 || h <= 0) {
				return;
			}

			sldMain.Margin = new Thickness(xPercent * w, yPercent * h, 0, 0);
			sldH.Margin = new Thickness(0, hPercent * h, 0, 0);
			sldT.Margin = new Thickness(tPercent * w, 0, 0, 0);
		}

		private void Win_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			isDownMain = false;
			isDownH = false;
			isDownT = false;

			win?.ReleaseMouseCapture();
		}

		private void bdMain_SizeChanged(object sender, SizeChangedEventArgs e) {
			updateSld();
		}
	}
}
