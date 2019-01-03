using System;
using System.Collections.Generic;
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
	/// <summary>
	/// AttrText.xaml 的交互逻辑
	/// </summary>
	public partial class AttrText : UserControl {
		object model = null;

		List<string> lstDesc = new List<string>();
		List<string> lstHintInfo = new List<string>();
		List<Func<object, string>> lstFunToString = new List<Func<object, string>>();
		List<Action<object, string>> lstFunToObject = new List<Action<object, string>>();

		bool isUpdatTextInner = false;

		public AttrText() {
			InitializeComponent();
		}

		//TextChangedByUser
		public static readonly RoutedEvent TextChangedByUserProperty = EventManager.RegisterRoutedEvent("TextChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AttrText));
		public event RoutedEventHandler TextChangedByUser {
			//将路由事件添加路由事件处理程序
			add { AddHandler(TextChangedByUserProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(TextChangedByUserProperty, value); }
		}


		public void setData(object _model, string[] arrDesc, string[] arrHintInfo, Func<object, string>[] arrFunToString, Action<object, string>[] arrFunToObject) {
			List<string> _lstDesc = new List<string>(arrDesc);
			List<string> _lstTipInfo = new List<string>(arrHintInfo);
			List<Func<object, string>> _lstFunToString = new List<Func<object, string>>(arrFunToString);
			List<Action<object, string>> _lstFunToObject = new List<Action<object, string>>(arrFunToObject);

			setData(_model, _lstDesc, _lstTipInfo, _lstFunToString, _lstFunToObject);
		}

		public void setData(object _model, List<string> _lstDesc, List<string> _lstHintInfo, List<Func<object, string>> _lstFunToString, List<Action<object, string>> _lstFunToObject) {
			model = _model;
			lstDesc = _lstDesc;
			lstHintInfo = _lstHintInfo;
			lstFunToString = _lstFunToString;
			lstFunToObject = _lstFunToObject;

			string desc = "";
			for(int i = 0; i < lstDesc.Count; ++i) {
				desc += (i == 0) ? "" : "\r\n";
				desc += lstDesc[i];
			}
			txtAttrDesc.Text = desc;

			updateText();

			txtAttrValue.IsUndoEnabled = false;
			txtAttrValue.IsUndoEnabled = true;
		}

		public void setModel(object _model) {
			model = _model;

			updateText();

			txtAttrValue.IsUndoEnabled = false;
			txtAttrValue.IsUndoEnabled = true;
		}

		public void updateText() {
			if(model == null) {
				setText("");
				updateHint();
				return;
			}

			string text = "";
			for(int i = 0; i < lstFunToString.Count; ++i) {
				text += (i == 0) ? "" : "\r\n";
				text += lstFunToString[i](model);
			}

			setText(text);
			updateHint();
		}

		private void updateHint(string[] _arr = null) {
			string str = txtAttrValue.Text;

			string[] arr = _arr;

			if(arr == null) {
				arr = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			}

			int count = Math.Min(arr.Length, lstHintInfo.Count);

			string hint = "";
			for(int i = 0; i < count; ++i) {
				hint += (i == 0) ? "" : "\r\n";
				hint += arr[i] != "" ? "" : lstHintInfo[i];
			}
			for(int i = count; i < lstHintInfo.Count; ++i) {
				hint += (hint.Length == 0) ? "" : "\r\n";
				hint += lstHintInfo[i];
			}

			txtHintInfo.Text = hint;
		}

		private void setText(string text) {
			isUpdatTextInner = true;
			txtAttrValue.Text = text;
			isUpdatTextInner = false;
		}
		
		private void txtAttrValue_ScrollChanged(object sender, ScrollChangedEventArgs e) {
			txtAttrDesc.ScrollToVerticalOffset(e.VerticalOffset);
			txtHintInfo.ScrollToVerticalOffset(e.VerticalOffset);
		}

		private void txtAttrValue_TextChanged(object sender, TextChangedEventArgs e) {
			if(isUpdatTextInner) {
				return;
			}

			updateAttrByText();
			//updateAttrText();
		}

		private void updateAttrByText() {
			if(model == null) {
				return;
			}

			string str = txtAttrValue.Text;

			string[] arr = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			updateHint(arr);

			//List<string> lstStr = new List<string>(arr);
			//for(int i = lstStr.Count; i < attrCount; ++i) {
			//	lstStr.Add("");
			//}
			int count = Math.Min(arr.Length, lstFunToObject.Count);
			for(int i = 0; i < count; ++i) {
				lstFunToObject[i](model, arr[i]);
			}
			for(int i = count; i < lstFunToObject.Count; ++i) {
				lstFunToObject[i](model, "");
			}

			//发送事件
			RoutedEventArgs arg = new RoutedEventArgs(TextChangedByUserProperty, this);
			RaiseEvent(arg);
		}

	}
}
