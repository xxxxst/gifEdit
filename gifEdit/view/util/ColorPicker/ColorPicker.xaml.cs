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

		private bool isSerValueInner = false;

		public ColorPicker() {
			InitializeComponent();
		}

		//Enable Alpha
		public static readonly DependencyProperty EnableAlphaProperty = DependencyProperty.Register("EnableAlpha", typeof(bool), typeof(ColorPicker), new PropertyMetadata(true));
		public bool EnableAlpha {
			get { return (bool)GetValue(EnableAlphaProperty); }
			set { SetCurrentValue(EnableAlphaProperty, value); }
		}

		//Value
		//public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(uint), typeof(ColorPicker), new PropertyMetadata(0));
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(uint), typeof(ColorPicker), new FrameworkPropertyMetadata(0xffffffff, new PropertyChangedCallback(OnValueChanged)));
		public uint Value {
			get { return (uint)GetValue(ValueProperty); }
			set { SetCurrentValue(ValueProperty, value); }
		}
		
		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as ColorPicker;
			if(ele == null) {
				return;
			}

			if(ele.isSerValueInner) {
				return;
			}

			uint val = ele.Value;
			float a = (val & 0xff000000) >> 24;
			float r = (val & 0x00ff0000) >> 16;
			float g = (val & 0x0000ff00) >> 8;
			float b = (val & 0x000000ff);
			a /= 255;
			r /= 255;
			g /= 255;
			b /= 255;

			if(!ele.EnableAlpha) {
				a = 1;
			}

			Rgb rgb = new Rgb(r, g, b);
			Hsv hsv = rgb2hsv(rgb);

			ele.xPercent = hsv.s;
			ele.yPercent = 1 - hsv.v;
			ele.hPercent = 1 - hsv.h / 360;
			ele.tPercent = a;
			ele.updateColor();

			//Debug.WriteLine("" + ele.isSerValueInner + "," + Convert.ToString(ele.Value, 16));
		}

		//Value Changed Event
		public static readonly RoutedEvent ValueChangedProperty = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPicker));
		public event RoutedEventHandler ValueChanged {
			//将路由事件添加路由事件处理程序
			add { AddHandler(ValueChangedProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(ValueChangedProperty, value); }
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

		private static float[] hsl2rgb(float[] hsl) {
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

		class Hsv {
			public float h = 0;
			public float s = 0;
			public float v = 0;

			public Hsv() { }

			public Hsv(float _s, float _l, float _v) {
				h = _s;
				s = _l;
				v = _v;
			}
		}

		class Rgb {
			public float r = 0;
			public float g = 0;
			public float b = 0;

			public Rgb() { }

			public Rgb(float _r, float _g, float _b) {
				r = _r;
				g = _g;
				b = _b;
			}
		}

		private static Hsv rgb2hsv(Rgb rgb) {
			Hsv hsv = new Hsv();
			float min, max, delta;

			min = rgb.r < rgb.g ? rgb.r : rgb.g;
			min = min < rgb.b ? min : rgb.b;

			max = rgb.r > rgb.g ? rgb.r : rgb.g;
			max = max > rgb.b ? max : rgb.b;

			hsv.v = max;                                // v
			delta = max - min;
			if(delta < 0.00001) {
				hsv.s = 0;
				hsv.h = 0; // undefined, maybe nan?
				return hsv;
			}
			if(max > 0.0) { // NOTE: if Max is == 0, this divide would cause a crash
				hsv.s = (delta / max);                  // s
			} else {
				// if max is 0, then r = g = b = 0              
				// s = 0, h is undefined
				hsv.s = 0.0f;
				hsv.h = float.NaN;                            // its now undefined
				return hsv;
			}
			if(rgb.r >= max)                           // > is bogus, just keeps compilor happy
				hsv.h = (rgb.g - rgb.b) / delta;        // between yellow & magenta
			else
			if(rgb.g >= max)
				hsv.h = 2.0f + (rgb.b - rgb.r) / delta;  // between cyan & yellow
			else
				hsv.h = 4.0f + (rgb.r - rgb.g) / delta;  // between magenta & cyan

			hsv.h *= 60.0f;                              // degrees

			if(hsv.h < 0.0)
				hsv.h += 360.0f;

			return hsv;
		}

		private static Rgb hsv2rgb(Hsv hsv) {
			byte[] rst = new byte[3];

			float hh, p, q, t, ff;
			long i;
			Rgb rgb = new Rgb();

			if(hsv.s <= 0.0) {       // < is bogus, just shuts up warnhsvgs
				rgb.r = hsv.v;
				rgb.g = hsv.v;
				rgb.b = hsv.v;
				return rgb;
			}
			hh = hsv.h;
			if(hh >= 360.0) hh = 0.0f;
			hh /= 60.0f;
			i = (long)hh;
			ff = hh - i;
			p = hsv.v * (1.0f - hsv.s);
			q = hsv.v * (1.0f - (hsv.s * ff));
			t = hsv.v * (1.0f - (hsv.s * (1.0f - ff)));

			switch(i) {
				case 0:
					rgb.r = hsv.v;
					rgb.g = t;
					rgb.b = p;
					break;
				case 1:
					rgb.r = q;
					rgb.g = hsv.v;
					rgb.b = p;
					break;
				case 2:
					rgb.r = p;
					rgb.g = hsv.v;
					rgb.b = t;
					break;

				case 3:
					rgb.r = p;
					rgb.g = q;
					rgb.b = hsv.v;
					break;
				case 4:
					rgb.r = t;
					rgb.g = p;
					rgb.b = hsv.v;
					break;
				case 5:
				default:
					rgb.r = hsv.v;
					rgb.g = p;
					rgb.b = q;
					break;
			}

			//Debug.WriteLine("hsv:" + hsv.h + "," + hsv.s + "," + hsv.v);
			//Debug.WriteLine("rgb:" + rgb.r + "," + rgb.g + "," + rgb.b);
			//Debug.WriteLine("--------------");
			return rgb;
		}

		//private void ColorPicker_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
		//	if (e.ChangedButton == MouseButton.Left) {
		//		isDownMain = false;
		//	}
		//}

		private float xPercent = 0;	//s
		private float yPercent = 0;	//v
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
			if(!isDownMain && !isDownH && !isDownT) {
				return;
			}

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

				//sldMain.Margin = new Thickness(xPercent * w, yPercent * h, 0, 0);
			} else if (isDownH) {
				//色相
				double h = grdH.ActualHeight;
				if (h <= 0) {
					return;
				}
				Point pos = e.GetPosition(grdH);

				hPercent = (float)(pos.Y / h);
				hPercent = Math.Max(0, Math.Min(hPercent, 1));

				//sldH.Margin = new Thickness(0, hPercent * h, 0, 0);
			} else if (isDownT) {
				//透明度
				double w = grdTransparent.ActualWidth;
				if (w <= 0) {
					return;
				}
				Point pos = e.GetPosition(grdTransparent);

				tPercent = (float)(pos.X / w);
				tPercent = Math.Max(0, Math.Min(tPercent, 1));

				//sldT.Margin = new Thickness(tPercent * w, 0, 0, 0);
			}

			updateColor();
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

		private void updateColor() {
			updateSld();

			float h = (1 - hPercent) * 360;
			float s = xPercent;
			float v = 1 - yPercent;

			Rgb rgb = hsv2rgb(new Hsv(h, s, v));

			byte r = (byte)(Math.Max(0, Math.Min(255, 255 * rgb.r	))	);
			byte g = (byte)(Math.Max(0, Math.Min(255, 255 * rgb.g	))	);
			byte b = (byte)(Math.Max(0, Math.Min(255, 255 * rgb.b	))	);
			byte a = (byte)(Math.Max(0, Math.Min(255, 255 * tPercent))	);

			SolidColorBrush color = new SolidColorBrush(Color.FromArgb(a, r, g, b));
			color.Freeze();
			bdCurrentColor.Background = color;

			Rgb rgbSV = hsv2rgb(new Hsv(h, 1, 1));
			byte rSV = (byte)(Math.Max(0, Math.Min(255, 255 * rgbSV.r)));
			byte gSV = (byte)(Math.Max(0, Math.Min(255, 255 * rgbSV.g)));
			byte bSV = (byte)(Math.Max(0, Math.Min(255, 255 * rgbSV.b)));

			SolidColorBrush coSV = new SolidColorBrush(Color.FromRgb(rSV, gSV, bSV));
			coSV.Freeze();
			bdSV.Background = coSV;
			
			LinearGradientBrush lColor = new LinearGradientBrush(
				Color.FromArgb(  0, r, g, b),
				Color.FromArgb(255, r, g, b),
				new Point(0, 0.5),
				new Point(1, 0.5));
			lColor.Freeze();

			bdA.Background = lColor;

			isSerValueInner = true;
			Value = ((uint)(a) << 24) + ((uint)(r) << 16) + ((uint)(g) << 8) + b;
			isSerValueInner = false;

			updateValue();
		}

		private void updateValue() {

			//发送事件
			RoutedEventArgs arg = new RoutedEventArgs(ValueChangedProperty, this);
			RaiseEvent(arg);
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
