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

			grdText.Margin = new Thickness(textGapLeft, 0, 0, 0);
			bdFrameBox.Margin = new Thickness(textGapLeft, 0, 0, 0);
			bdSelectFrame.Margin = new Thickness(-oneFrameWidth, 0, 0, 0);
			bdThumbPoint.Margin = new Thickness(-thumbPointWidth, 0, 0, 0);
			bdFrameBox.Width = 0;
		}

		//SelectFrame
		public static readonly DependencyProperty SelectFrameProperty = DependencyProperty.Register("SelectFrame", typeof(int), typeof(KeyFrame), new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(OnSelectFrameChanged)));
		public int SelectFrame {
			get { return (int)GetValue(SelectFrameProperty); }
			set { SetCurrentValue(SelectFrameProperty, value); }
		}
		
		private static void OnSelectFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as KeyFrame;
			if(ele == null) {
				return;
			}

			ele.updateSelectFramePos();
			
			//发送事件
			RoutedEventArgs arg = new RoutedEventArgs(SelectFrameChangedProperty, ele);
			ele.RaiseEvent(arg);
		}

		//FramePointColor
		public static readonly DependencyProperty FramePointColorProperty = DependencyProperty.Register("FramePointColor", typeof(Brush), typeof(KeyFrame), new PropertyMetadata(new BrushConverter().ConvertFromString("#26ae63")) );
		public Brush FramePointColor {
			get { return (Brush)GetValue(FramePointColorProperty); }
			set { SetCurrentValue(FramePointColorProperty, value); }
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

			ele.updateThumbSize();
			ele.updatePos();
		}

		//SelectFrameChanged
		public static readonly RoutedEvent SelectFrameChangedProperty = EventManager.RegisterRoutedEvent("SelectFrameChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KeyFrame));
		public event RoutedEventHandler SelectFrameChanged {
			add { AddHandler(SelectFrameChangedProperty, value); }
			remove { RemoveHandler(SelectFrameChangedProperty, value); }
		}

		int startPos = 6;
		//int boxStartPos = 6;
		//int boxFrameIdx = 0;
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

		//const int boxGapLeft = 4;
		const int textGapLeft = 8;
		const int textGapRight = 50;
		const int textFrameGap = 5;

		private void updateSelectFramePos() {
			int idx = SelectFrame;
			if(idx < MinFrame) {
				bdSelectFrame.Margin = new Thickness(-oneFrameWidth, 0, 0, 0);
				return;
			}

			idx = Math.Max(MinFrame, Math.Min(idx, MaxFrame));
			if(MaxFrame <= MinFrame) {
				idx = MinFrame - 1;
			}

			int pos = startPos + idx * oneFrameWidth;
			bdSelectFrame.Margin = new Thickness(pos, 0, 0, 0);

			updateThumbPos();
		}

		int minThumbPointWidth = 5;
		int thumbPointWidth = 5;
		private void updateThumbSize() {
			int totalCount = MaxFrame - MinFrame + 1;
			if(totalCount <= MinFrame) {
				thumbPointWidth = (int)grdThumb.ActualWidth;
			} else {
				thumbPointWidth = (int)(grdThumb.ActualWidth / totalCount);
				thumbPointWidth = Math.Max(thumbPointWidth, minThumbPointWidth);
			}

			bdThumbPoint.Width = thumbPointWidth;

			updateThumbPos();
		}

		private void updateThumbPos() {
			const int thumbWidth = 4;

			int idx = SelectFrame;
			if(idx < MinFrame) {
				bdThumbPoint.Margin = new Thickness(-thumbPointWidth, 0, 0, 0);
				return;
			}

			idx = Math.Max(MinFrame, Math.Min(idx, MaxFrame));
			int totalCount = MaxFrame - MinFrame + 1;

			if(totalCount <= 0) {
				idx = MinFrame - 1;
			}
			
			int pos = (int)((float)(idx - MinFrame) / totalCount * (ActualWidth - thumbWidth));
			bdThumbPoint.Margin = new Thickness(pos, 0, 0, 0);
		}

		private void updatePos() {
			int w = (int)ActualWidth;
			int textGap = oneFrameWidth * textFrameGap;
			int textMaxFrame = MaxFrame + 1;

			int count = textMaxFrame - MinFrame + 1;
			int totalWidth = oneFrameWidth * count;
			int r = startPos + totalWidth;
			if(r + textGapRight < w) {
				startPos = w - textGapRight - totalWidth;
			}
			if(startPos > textGapLeft) {
				startPos = textGapLeft;
			}

			//int startBoxIdx = -(startPos - textGapLeft) / oneFrameWidth;
			//startBoxIdx = Math.Max(startBoxIdx, 0);
			//int newBoxFrameIdx = (startPos - textGapLeft) % oneFrameWidth + textGapLeft;
			//if(boxStartPos != newBoxFrameIdx) {
			//	boxStartPos = newBoxFrameIdx;
			//	bdFrameBox.Margin = new Thickness(boxStartPos, 0, 0, 0);
			//}

			int startTextIdx = -(startPos - textGapLeft) / oneFrameWidth;
			startTextIdx = (startTextIdx / 5) * 5;
			startTextIdx = Math.Max(startTextIdx, 0);

			bool isStartFrameIdxUpdate = (textFrameIdx != startTextIdx);
			textFrameIdx = startTextIdx;
			int newTextStartPos = (startPos - textGapLeft) % textGap + textGapLeft;
			//if(textStartPos != newTextStartPos) {
			if(textStartPos != newTextStartPos) {
				textStartPos = newTextStartPos;
				grdText.Margin = new Thickness(textStartPos, 0, 0, 0);
				bdFrameBox.Margin = new Thickness(textStartPos, 0, 0, 0);
			}

			//int showBoxCount = (int)Math.Ceiling((w - textGapLeft + textGapRight) / (float)oneFrameWidth) + 1;
			//if((startBoxIdx + showBoxCount - 1) > MaxFrame) {
			//	showBoxCount = MaxFrame - startTextIdx + 1;
			//}
			//bdFrameBox.Width = showBoxCount * oneFrameWidth;

			int showTextCount = (int)Math.Ceiling((w - textGapLeft + textGapRight) / (float)textGap) + 1;
			//showCount = Math.Max(0, Math.Min(showCount, (MaxFrame + 6 - startFrameIdx) / textFrameGap));
			if((startTextIdx + (showTextCount - 1) * textFrameGap) > textMaxFrame) {
				showTextCount = (textMaxFrame - startTextIdx) / textFrameGap + 1;
			}
			showTextCount = Math.Max(0, showTextCount);

			bdFrameBox.Width = (showTextCount) * oneFrameWidth * textFrameGap;
			updateSelectFramePos();

			if(lstLabel.Count < showTextCount) {
				for(int i = lstLabel.Count; i < showTextCount; ++i) {
					Label lbl = createLabel(i);
					grdText.Children.Add(lbl);
					lstLabel.Add(lbl);
				}
			} else if(lstLabel.Count > showTextCount) {
				for(int i = showTextCount; i < lstLabel.Count; ++i) {
					grdText.Children.Remove(lstLabel[i]);
				}
				lstLabel.RemoveRange(showTextCount, lstLabel.Count - showTextCount);
			} else {
				if(!isStartFrameIdxUpdate) {
					return;
				}
			}

			for(int i = 0; i < lstLabel.Count; ++i) {
				lstLabel[i].Content = (startTextIdx + i * textFrameGap).ToString();
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

		MouseDownType mouseDownType = MouseDownType.None;
		int downStartPos = 0;
		int downX = 0;
		Window parentWin = null;
		private void bdText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			//Window win = Window.GetWindow(this);
			//if(win == null) {
			//	return;
			//}
			//parentWin = win;
			//mouseDownType = MouseDownType.Text;

			//downStartPos = startPos;
			//downX = (int)Mouse.GetPosition(bdText).X;

			//parentWin.MouseLeftButtonUp += Win_MouseLeftButtonUp; ;
			//parentWin.MouseMove += Win_MouseMove;
			//parentWin.CaptureMouse();
			mouseDownHandler(MouseDownType.Text);
		}

		private void mouseDownHandler(MouseDownType type) {
			Window win = Window.GetWindow(this);
			if(win == null) {
				return;
			}
			parentWin = win;
			mouseDownType = type;

			downStartPos = startPos;
			downX = (int)Mouse.GetPosition(bdText).X;

			parentWin.MouseLeftButtonUp += Win_MouseLeftButtonUp; ;
			parentWin.MouseMove += Win_MouseMove;
			parentWin.CaptureMouse();
		}

		private void Win_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			mouseDownType = MouseDownType.None;

			if(parentWin == null) {
				return;
			}

			parentWin.ReleaseMouseCapture();
			parentWin.MouseLeftButtonUp -= Win_MouseLeftButtonUp;
			parentWin.MouseMove -= Win_MouseMove;
			parentWin = null;
		}

		private void Win_MouseMove(object sender, MouseEventArgs e) {
			switch(mouseDownType) {
				case MouseDownType.Frame: {
					Point pos = e.GetPosition(grdFrameBox);
					int idx = (int)((pos.X - startPos) / oneFrameWidth);
					if(idx < MinFrame || idx > MaxFrame) {
						return;
					}
					if(idx == SelectFrame) {
						break;
					}

					SelectFrame = idx;
					break;
				}
				case MouseDownType.Text: {
					int x = (int)Mouse.GetPosition(bdText).X;
					startPos = -(x - downX) + downStartPos;
					updatePos();
					break;
				}
				case MouseDownType.Thumb: {
					if(grdThumb.ActualWidth <= 0) {
						return;
					}
					int totalCount = MaxFrame - MinFrame + 1;
					if(totalCount <= 0) {
						return;
					}

					Point pos = e.GetPosition(grdThumb);
					int idx = (int)Math.Floor(pos.X / grdThumb.ActualWidth * totalCount) + MinFrame;
					idx = Math.Max(MinFrame, Math.Min(idx, MaxFrame));
					if(idx == SelectFrame) {
						break;
					}
					setCenterPosByFrameIdx(idx);

					SelectFrame = idx;
					break;
				}
			}
		}

		public void setSelectFrameCenter() {
			startPos = (int)(-(SelectFrame * oneFrameWidth) + bdText.ActualWidth / 2);
			updatePos();
		}

		private void setCenterPosByFrameIdx(int idx) {
			startPos = (int)(-(idx * oneFrameWidth) + bdText.ActualWidth / 2);
			updatePos();
		}

		private void GrdFrameBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			//Point pos = e.GetPosition(grdFrameBox);
			//int idx = (int)((pos.X - startPos) / oneFrameWidth);
			//if(idx < MinFrame || idx > MaxFrame) {
			//	return;
			//}

			//SelectFrame = idx;

			mouseDownHandler(MouseDownType.Frame);
		}

		private void GrdThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			//if(grdThumb.ActualWidth <= 0) {
			//	return;
			//}
			//int totalCount = MaxFrame - MinFrame + 1;
			//if(totalCount <= 0) {
			//	return;
			//}

			//Point pos = e.GetPosition(grdThumb);
			//int idx = (int)Math.Floor(pos.X / grdThumb.ActualWidth * totalCount) + MinFrame;
			//idx = Math.Max(MinFrame, Math.Min(idx, MaxFrame));
			
			//SelectFrame = idx;

			mouseDownHandler(MouseDownType.Thumb);
		}

		private void GrdThumb_SizeChanged(object sender, SizeChangedEventArgs e) {
			updateThumbSize();
		}

		private void GrdBox_MouseWheel(object sender, MouseWheelEventArgs e) {
			const int scrollWidth = 30;

			if(e.Delta > 0) {
				startPos += scrollWidth;
			} else {
				startPos -= scrollWidth;
			}
			updatePos();
		}

		enum MouseDownType {
			None, Frame, Text, Thumb
		}
	}

}
