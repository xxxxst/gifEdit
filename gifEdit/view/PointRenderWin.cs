using csharpHelp.util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using D3D11Device = SharpDX.Direct3D11.Device;
//using D3D11Device1 = SharpDX.Direct3D11.Device1;
//using DXGIDevice = SharpDX.DXGI.Device;
//using D2D1Device = SharpDX.Direct2D1.Device;
//using DXGIFactory = SharpDX.DXGI.Factory;
//using DeviceContext = SharpDX.Direct2D1.DeviceContext;
//using Factory = SharpDX.Direct2D1.Factory;
//using PixelFormat = SharpDX.WIC.PixelFormat;

namespace gifEdit.view {
	public partial class PointRenderWin : Form {
		public Action onLoaded = null;

		//D3D11Device d3DDevice = null;
		//DXGIDevice dxgiDevice = null;
		//D2D1Device d2DDevice = null;
		//DeviceContext d2DContext = null;
		//Factory d2DFactory;
		//ImagingFactory wicFactory;

		//RenderTarget renderTarget = null;
		//SwapChain swapChain = null;
		//Surface backBuffer = null;
		//Bitmap1 targetBitmap = null;

		//SolidColorBrush solidBrush = null;

		public PointRenderWin() {
			InitializeComponent();
		}

		public IntPtr getHandle() {
			return Handle;
		}

		private void PointRenderWin_Load(object sender, EventArgs e) {
			IntPtr Handle = getHandle();

			//隐藏边框
			int oldstyle = User32.GetWindowLong(Handle, User32.GWL_STYLE);
			User32.SetWindowLong(Handle, User32.GWL_STYLE, oldstyle & (~(User32.WS_CAPTION | User32.WS_CAPTION_2)) | User32.WS_EX_LAYERED);

			//不在Alt+Tab中显示
			int oldExStyle = User32.GetWindowLong(Handle, User32.GWL_EXSTYLE);
			User32.SetWindowLong(Handle, User32.GWL_EXSTYLE, oldExStyle & (~User32.WS_EX_APPWINDOW) | User32.WS_EX_TOOLWINDOW);

			try {
				//Debug.WriteLine("aa:" + pnlMain.Handle + "," + pnlMain.Width + "," + pnlMain.Height);
				initEngine();
				Render();
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}

			onLoaded?.Invoke();
		}

		private void initEngine() {
			//// 创建 Dierect3D 设备。
			//this.d3DDevice = new D3D11Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
			//this.dxgiDevice = d3DDevice.QueryInterface<D3D11Device1>().QueryInterface<DXGIDevice>();
			//// 创建 Direct2D 设备和工厂。
			//this.d2DDevice = new D2D1Device(dxgiDevice);
			//this.d2DContext = new DeviceContext(d2DDevice, DeviceContextOptions.None);
			//this.d2DFactory = this.d2DContext.Factory;
			//this.wicFactory = new ImagingFactory2();

			//this.renderTarget = new DeviceContext(d2DDevice, DeviceContextOptions.None);

			//// 创建 DXGI SwapChain。
			//SwapChainDescription swapChainDesc = new SwapChainDescription() {
			//	BufferCount = 1,
			//	Usage = Usage.RenderTargetOutput,
			//	OutputHandle = Handle,
			//	IsWindowed = true,
			//	// 这里宽度和高度都是 0，表示自动获取。
			//	ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.B8G8R8A8_UNorm),
			//	SampleDescription = new SampleDescription(1, 0),
			//	SwapEffect = SwapEffect.Discard
			//};
			//this.swapChain = new SwapChain(dxgiDevice.GetParent<Adapter>().GetParent<DXGIFactory>(),
			//	d3DDevice, swapChainDesc);
			//// 创建 BackBuffer。
			//this.backBuffer = Surface.FromSwapChain(this.swapChain, 0);
			//// 从 BackBuffer 创建 DeviceContext 可用的目标。
			//this.targetBitmap = new Bitmap1(this.d2DContext, backBuffer);
			//((DeviceContext)this.renderTarget).Target = targetBitmap;

			//solidBrush = new SolidColorBrush(renderTarget, new RawColor4(1, 1, 0, 1));

			GlControl glControl = new GlControl();

			Paint += PnlMain_Paint;
		}

		private void PnlMain_Paint(object sender, PaintEventArgs e) {
			Render();
		}

		private void Render() {
			//renderTarget.BeginDraw();
			//renderTarget.Clear(Color.White);
			//renderTarget.FillRectangle(new RectangleF(50, 50, 450, 150), solidBrush);
			//renderTarget.EndDraw();
			//this.swapChain.Present(0, PresentFlags.None);
		}

		public void updateData() {

		}

		private void PointRenderWin_Resize(object sender, EventArgs e) {

		}

		private void RenderControl_ContextCreated(object sender, GlControlEventArgs e) {
			Gl.MatrixMode(MatrixMode.Projection);
			Gl.LoadIdentity();
			Gl.Ortho(0.0, 1.0f, 0.0, 1.0, 0.0, 1.0);

			Gl.MatrixMode(MatrixMode.Modelview);
			Gl.LoadIdentity();
		}

		private void RenderControl_ContextDestroying(object sender, GlControlEventArgs e) {

		}

		private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e) {

		}

		private void RenderControl_Render(object sender, GlControlEventArgs e) {
			var senderControl = sender as GlControl;

			int vpx = 0;
			int vpy = 0;
			int vpw = senderControl.ClientSize.Width;
			int vph = senderControl.ClientSize.Height;

			Gl.Viewport(vpx, vpy, vpw, vph);
			Gl.Clear(ClearBufferMask.ColorBufferBit);

			if(Gl.CurrentVersion >= Gl.Version_110) {
				// Old school OpenGL 1.1
				// Setup & enable client states to specify vertex arrays, and use Gl.DrawArrays instead of Gl.Begin/End paradigm
				using(MemoryLock vertexArrayLock = new MemoryLock(_ArrayPosition))
				using(MemoryLock vertexColorLock = new MemoryLock(_ArrayColor)) {
					// Note: the use of MemoryLock objects is necessary to pin vertex arrays since they can be reallocated by GC
					// at any time between the Gl.VertexPointer execution and the Gl.DrawArrays execution

					Gl.VertexPointer(2, VertexPointerType.Float, 0, vertexArrayLock.Address);
					Gl.EnableClientState(EnableCap.VertexArray);

					Gl.ColorPointer(3, ColorPointerType.Float, 0, vertexColorLock.Address);
					Gl.EnableClientState(EnableCap.ColorArray);

					Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
				}
			} else {
				// Old school OpenGL
				Gl.Begin(PrimitiveType.Triangles);
				Gl.Color3(1.0f, 0.0f, 0.0f); Gl.Vertex2(0.0f, 0.0f);
				Gl.Color3(0.0f, 1.0f, 0.0f); Gl.Vertex2(0.5f, 1.0f);
				Gl.Color3(0.0f, 0.0f, 1.0f); Gl.Vertex2(1.0f, 0.0f);
				Gl.End();
			}
		}

		/// <summary>
		/// Vertex position array.
		/// </summary>
		private static readonly float[] _ArrayPosition = new float[] {
			0.0f, 0.0f,
			0.5f, 1.0f,
			1.0f, 0.0f
		};

		/// <summary>
		/// Vertex color array.
		/// </summary>
		private static readonly float[] _ArrayColor = new float[] {
			1.0f, 0.0f, 0.0f,
			0.0f, 1.0f, 0.0f,
			0.0f, 0.0f, 1.0f
		};

	}
}
