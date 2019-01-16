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

namespace gifEdit.view.util {
	/// <summary>关键帧</summary>
	public partial class KeyFrame : UserControl {
		int oneFrameWidth = 8;
		//double scale = 1;
		
		SolidColorBrush coText = null;

		public KeyFrame() {
			InitializeComponent();

			coText = FindResource("conTextColor") as SolidColorBrush;
		}

		//SelectFrame
		public static readonly DependencyProperty SelectFrameProperty = DependencyProperty.Register("SelectFrame", typeof(int), typeof(KeyFrame), new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(OnValueChanged)));
		public int SelectFrame {
			get { return (int)GetValue(SelectFrameProperty); }
			set { SetCurrentValue(SelectFrameProperty, value); }
		}

		//MinFrame
		public static readonly DependencyProperty MinFrameProperty = DependencyProperty.Register("MinFrame", typeof(int), typeof(KeyFrame), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnValueChanged)));
		public int MinFrame {
			get { return (int)GetValue(MinFrameProperty); }
			set { SetCurrentValue(MinFrameProperty, value); }
		}

		//MaxFrame
		public static readonly DependencyProperty MaxFrameProperty = DependencyProperty.Register("MaxFrame", typeof(int), typeof(KeyFrame), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnValueChanged)));
		public int MaxFrame {
			get { return (int)GetValue(MaxFrameProperty); }
			set { SetCurrentValue(MaxFrameProperty, value); }
		}


		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as KeyFrame;
			if(ele == null) {
				return;
			}

			ele.updatePos();
		}

		int startPos = 6;
		int textStartPos = 6;
		int textFrameIdx = 0;
		List<Label> lstLabel = new List<Label>();
		private void updateFrame() {
			//int count = MaxFrame - MinFrame + 1;
			//double showOneFrameWidth = oneFrameWidth * scale;

			//double totalWidth = showOneFrameWidth * count;

			//double w = Width;
			//int showCount = (int)(w / showOneFrameWidth) + 1;
			//if(showCount > count) {
			//	showCount = count;
			//}
			
		}

		const int showGap = 6;
		const int textFrameGap = 5;
		private void updatePos() {
			int w = (int)ActualWidth;
			int textGap = oneFrameWidth * textFrameGap;

			int count = MaxFrame - MinFrame + 1;
			int totalWidth = oneFrameWidth * count;
			int r = startPos + totalWidth;
			if(r + showGap < w) {
				startPos = w - showGap - totalWidth;
			}
			if(startPos > showGap) {
				startPos = showGap;
			}

			int startFrameIdx = -(startPos - showGap) / oneFrameWidth;
			startFrameIdx = (startFrameIdx / 5) * 5;
			startFrameIdx = Math.Max(startFrameIdx, 0);

			bool isStartFrameIdxUpdate = (textFrameIdx != startFrameIdx);
			textFrameIdx = startFrameIdx;
			textStartPos = (startPos - showGap) % textGap + showGap;
			grdText.Margin = new Thickness(textStartPos, 0, 0, 0);

			int showCount = ((w - showGap * 2) / textGap) + 1;
			showCount = Math.Max(0, Math.Min(showCount, (MaxFrame + 1 - startFrameIdx) / 5));

			if(lstLabel.Count < showCount) {
				for(int i = lstLabel.Count; i < showCount; ++i) {
					Label lbl = createLabel(i);
					grdText.Children.Add(lbl);
					lstLabel.Add(lbl);
				}
			} else if(lstLabel.Count > showCount) {
				for(int i = showCount; i < lstLabel.Count; ++i) {
					grdText.Children.Remove(lstLabel[i]);
				}
				lstLabel.RemoveRange(showCount, lstLabel.Count - showCount);
			} else {
				if(!isStartFrameIdxUpdate) {
					return;
				}
			}

			for(int i = 0; i < lstLabel.Count; ++i) {
				lstLabel[i].Content = startFrameIdx + i * textFrameGap;
			}
		}

		private Label createLabel(int idx) {
			Label lbl = new Label();
			lbl.FontSize = 8;
			lbl.VerticalContentAlignment = VerticalAlignment.Center;
			lbl.Padding = new Thickness(0);
			lbl.Foreground = coText;
			lbl.Margin = new Thickness(idx * oneFrameWidth * textFrameGap, 0, 0, 0);
			//lbl.Content = frameIdx;

			return lbl;
		}

		private void userControl_SizeChanged(object sender, SizeChangedEventArgs e) {
			updatePos();
		}

		int downStartPos = 0;
		int downX = 0;
		Window parentWin = null;
		private void grdText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			parentWin = Window.GetWindow(this);
			if(parentWin == null) {
				return;
			}

			downStartPos = startPos;
			downX = (int)Mouse.GetPosition(bdText).X;

			parentWin.MouseLeftButtonUp += Win_MouseLeftButtonUp; ;
			parentWin.MouseMove += Win_MouseMove;
		}

		private void Win_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if(parentWin == null) {
				return;
			}

			parentWin.MouseLeftButtonUp -= Win_MouseLeftButtonUp;
			parentWin.MouseMove -= Win_MouseMove;
		}

		private void Win_MouseMove(object sender, MouseEventArgs e) {
			int x = (int)Mouse.GetPosition(bdText).X;
			startPos = (x - downX) + downStartPos;
			updatePos();
		}
	}
}
