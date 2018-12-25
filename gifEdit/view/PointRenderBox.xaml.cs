using FreeImageAPI;
using gifEdit.control;
using gifEdit.control.glEngine;
using gifEdit.model;
using gifEdit.services;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>
	/// PointRenderBox.xaml 的交互逻辑
	/// </summary>
	public partial class PointRenderBox : UserControl {
		//public byte[] imageData = null;
		//MemoryLock imageLock = null;
		//IntPtr pImageData1 = IntPtr.Zero;

		//IntPtr pImageData = IntPtr.Zero;
		//int imageWidth = 0;
		//int imageHeight = 0;
		bool isEngineInited = false;

		int renderTime = 0;
		int maxRenderTime = 1;
		//long totalRenderIdx = 0;
		//int renderIdx = 0;
		//int maxRenderIdx = 1;
		GlTime glTime = new GlTime();
		FpsCtl fpsCtl = new FpsCtl();

		//long oldTime = 0;
		//long oldfpsCount = 0;
		//long fpsCount = 0;

		//private GlProgram _Program;
		private float[] mMVP = null;

		//uint texId = 0;
		//PointEmitter pe = null;
		List<PointEmitter> lstEmitter = new List<PointEmitter>();

		//Timer timer = null;

		public PointRenderBox() {
			InitializeComponent();

			//byte[] data = new byte[64 * 64 * 4];
			//for(int i = 0; i < 64; ++i) {
			//	for(int j = 0; j < 64; ++j) {
			//		int idx = i * 64 * 4 + j * 4;
			//		byte r = (byte)((double)i / 63 * 255);
			//		//uint val = ((uint)r) << 24 + 0x00FFFFFF;
			//		//if(i == 63 && j == 0) {
			//		//	Debug.WriteLine(r);
			//		//}
			//		data[idx] = r;
			//		data[idx + 1] = r;
			//		data[idx + 2] = 255;
			//		data[idx + 3] = 170;
			//		if(j > 20 && j < 40) {
			//			data[idx + 2] = 0;
			//			data[idx + 3] = 0;
			//		}
			//	}
			//}
			//imageData = data;
			//imageLock = new MemoryLock(imageData);
			//pImageData1 = imageLock.Address;

			//imageWidth = 64;
			//imageHeight = 64;

			//timer = new Timer((obj)=>onTimerProc(), null, Timeout.Infinite, 15);

			//EventServer.ins.mainWinExitedEvent += () => {
			//	timer?.Change(Timeout.Infinite, 15);
			//	timer?.Dispose();
			//	timer = null;
			//};

			//glControl.AnimationTime = 1;
			if(DesignerProperties.GetIsInDesignMode(this)) {
				glControl.Animation = false;
			}
		}

		//private void onTimerProc() {
		//	Dispatcher.Invoke(() => {
		//		++renderIdx;
		//		renderIdx = renderIdx % maxRenderIdx;
		//		//lblFps.Content = "" + renderIdx;
		//		update();
		//	});
		//}

		public void init() {
			try {
				//maxRenderIdx = 66 * 5;

				PointEditModel md = MainModel.ins.pointEditModel;
				//pe = new PointEmitter(md.lstResource[0]);

				for(int i = 0; i < md.lstResource.Count; ++i) {
					PointEmitter emt = new PointEmitter(md.lstResource[i]);
					lstEmitter.Add(emt);
				}

				//maxRenderTime = 5 * 1000;
				maxRenderTime = 0;
				for(int i = 0; i < md.lstResource.Count; ++i ) {
					int ms = (int)(md.lstResource[i].pointLife * 1000);
					maxRenderTime = Math.Max(maxRenderTime, ms);
				}

				//udpateImage();
				updateRenderBoxSize();
				//Debug.WriteLine("aa:" + Gl.CurrentVersion);
				
				if(isEngineInited) {
					update();
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void updateRenderBoxSize() {
			PointEditModel md = MainModel.ins.pointEditModel;
			if(md == null) {
				return;
			}

			int width = md.width;
			int height = md.height;

			width = Math.Min(width, (int)grdRenderBox.ActualWidth - 2);
			height = Math.Min(height, (int)grdRenderBox.ActualHeight - 2);

			//win.Width = width;
			//win.Height = height;
			formHost.Width = width;
			formHost.Height = height;
		}

		//public void udpateImage() {
		//	string path = @"E:\00workself\csharp\project\gifEdit\gifEdit\resource\image\snow.png";
		//	//var fif = FreeImage FreeImage_GetFileType(FileName, 0);

		//	var type = FreeImage.GetFileType(path, 0);
		//	if(type == FREE_IMAGE_FORMAT.FIF_UNKNOWN) {
		//		return;
		//	}

		//	if(!FreeImage.FIFSupportsReading(type)) {
		//		return;
		//	}
			
		//	var dib = FreeImage.Load(type, path, FREE_IMAGE_LOAD_FLAGS.DEFAULT);

		//	imageWidth = (int)FreeImage.GetWidth(dib);
		//	imageHeight = (int)FreeImage.GetHeight(dib);

		//	//var dib2 = FreeImage.Rescale(dib, 64, 64, FREE_IMAGE_FILTER.FILTER_BILINEAR);
		//	var bits = FreeImage.GetBits(dib);
		//	pImageData = bits;
		//}

		private void grdRenderBox_SizeChanged(object sender, SizeChangedEventArgs e) {
			updateRenderBoxSize();
		}
		
		private void glControl_ContextCreated(object sender, GlControlEventArgs e) {
			Gl.Enable(EnableCap.AlphaTest);
			Gl.Enable(EnableCap.Blend);
			Gl.Enable(EnableCap.Texture2d);
			Gl.Enable(EnableCap.PolygonSmooth);
			//Gl.Enable(EnableCap.Multisample);
			//Gl.Enable(EnableCap.PrimitiveRestart);
			Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			//_Program = new GlProgram(_VertexSourceGL, _FragmentSourceGL);

			//const int GL_NEAREST = 0x2600;
			//const int GL_LINEAR = 0x2601;
			//const int GL_REPEAT = 0x2901;
			//const int GL_REPLACE = 0x1E01;
			//Gl.ActiveTexture(TextureUnit.Texture0);
			//texId = Gl.GenTexture();
			//Gl.BindTexture(TextureTarget.Texture2d, texId);
			//Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
			//Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
			//Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, new int[] { GL_LINEAR });
			//Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, new int[] { GL_LINEAR });
			//Gl.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, GL_REPLACE);

			//Gl.BindTexture(TextureTarget.Texture2d, 0);

			updateGlSize();

			//Gl.ClearColor(27 / 255f, 28 / 255f, 32 / 255f, 1.0f);
			//glClear("1e2027");
			glClear("24252c", 1);
			//glClear(0, 1);

			isEngineInited = true;

			//timer?.Change(100, 15);
		}

		private void glClear(string strColor) {
			uint color = 0;
			try {
				color = Convert.ToUInt32(strColor, 16);
			} catch(Exception) { }
			glClear(color);
		}

		private void glClear(string strColor, float alpha) {
			uint color = 0;
			try {
				color = Convert.ToUInt32(strColor, 16);
			} catch(Exception) { }
			glClear(color, alpha);
		}

		private void glClear(uint color) {
			uint a = ((color & 0xff000000) >> 24);
			uint r = ((color & 0x00ff0000) >> 16);
			uint g = ((color & 0x0000ff00) >> 8);
			uint b = ((color & 0x000000ff));
			Gl.ClearColor(r / 255f, g / 255f, b / 255f, a);
		}

		private void glClear(uint color, float alpha) {
			uint r = ((color & 0x00ff0000) >> 16);
			uint g = ((color & 0x0000ff00) >> 8);
			uint b = ((color & 0x000000ff));
			Gl.ClearColor(r / 255f, g / 255f, b / 255f, alpha);
		}

		private void glControl_ContextDestroying(object sender, GlControlEventArgs e) {
			//_Program?.Dispose();
			//_Program = null;

			//pe?.Dispose();
			//pe = null;

			for(int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].Dispose();
			}
			lstEmitter.Clear();
		}

		int idx = 0;
		private void update() {
			//++fpsCount;

			//if(oldTime == 0) {
			//	oldTime = DateTime.Now.ToFileTimeUtc();
			//}

			//long time = DateTime.Now.ToFileTimeUtc();
			//int ms = (int)((double)(time - oldTime) / (1000 * 10));
			//int idx = ms / 20;
			//if(idx <= totalRenderIdx) {
			//	return;
			//}
			//totalRenderIdx = idx;
			//renderIdx = (int)(totalRenderIdx % maxRenderIdx);


			//fps
			//if(fpsCount - oldfpsCount > 66) {
			//	const int second = 1 * 1000 * 1000 * 10;

			//	int val = (int)((fpsCount - oldfpsCount) / ((double)(time - oldTime) / second));
			//	Dispatcher.Invoke(() => {
			//		lblFps.Content = val;
			//	});

			//	if(time > 10 * second) {
			//		oldTime = time;
			//		fpsCount = oldfpsCount = 0;
			//	}
			//}
			float gapTime = glTime.getTime();
			renderTime = (int)(renderTime + gapTime);
			renderTime = renderTime % maxRenderTime;

			fpsCtl.update();
			++idx;
			if(idx > 120) {
				lblFps.Content = fpsCtl.getFps();
				idx = 0;
			}

			//return;
			//if(pImageData == IntPtr.Zero) {
			//	return;
			//}
			if(!isEngineInited) {
				return;
			}
			//if(pe == null) {
			//	return;
			//}

			//const int GL_NEAREST = 0x2600;
			//const int GL_LINEAR = 0x2601;
			//const int GL_REPEAT = 0x2901;
			//const int GL_REPLACE = 0x1E01;

			var senderControl = glControl;

			int vpx = 0;
			int vpy = 0;
			int vpw = senderControl.ClientSize.Width;
			int vph = senderControl.ClientSize.Height;

			Gl.Viewport(vpx, vpy, vpw, vph);
			Gl.Clear(ClearBufferMask.ColorBufferBit);

			//pe.render(mMVP, renderTime);
			for(int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].render(mMVP, renderTime);
			}

			//Gl.UseProgram(_Program.ProgramName);

			//using(MemoryLock vertexArrayLock = new MemoryLock(_ArrayPosition))
			////using(MemoryLock vertexColorLock = new MemoryLock(_ArrayColor))
			//using(MemoryLock vertexTexCoordLock = new MemoryLock(_ArrayTexCoord)) {
			//	//Gl.VertexPointer(2, VertexPointerType.Float, 0, vertexArrayLock.Address);
			//	//Gl.EnableClientState(EnableCap.VertexArray);

			//	//Gl.ColorPointer(3, ColorPointerType.Float, 0, vertexColorLock.Address);
			//	//Gl.EnableClientState(EnableCap.ColorArray);

			//	Gl.VertexAttribPointer((uint)_Program.LocationPosition, 2, VertexAttribType.Float, false, 0, vertexArrayLock.Address);
			//	Gl.EnableVertexAttribArray((uint)_Program.LocationPosition);

			//	//Gl.TexCoordPointer(2, TexCoordPointerType.Float, 0, vertexTexCoordLock.Address);
			//	//Gl.EnableClientState(EnableCap.TextureCoordArray);

			//	Gl.VertexAttribPointer((uint)_Program.LocationTexCoord, 2, VertexAttribType.Float, false, 0, vertexTexCoordLock.Address);
			//	Gl.EnableVertexAttribArray((uint)_Program.LocationTexCoord);

			//	Gl.UniformMatrix4(_Program.LocationMVP, false, mMVP);

			//	Gl.BindTexture(TextureTarget.Texture2d, texId);

			//	//if(pImageData != IntPtr.Zero) {
			//	//Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, imageWidth, imageHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pImageData);
			//	Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, imageWidth, imageHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pImageData);
			//	//Gl.BindTexture(TextureTarget.Texture2d, 0);

			//	//int val = 0;
			//	Gl.Uniform1(_Program.LocationTex, 0);
			//	//Gl.VertexAttribPointer((uint)_Program.LocationTex, 2, VertexAttribType.UnsignedByte, false, 0, 0);
			//	//Gl.EnableVertexAttribArray((uint)_Program.LocationTex);
			//	//}


			//	Gl.DrawArrays(PrimitiveType.Polygon, 0, _ArrayPosition.Length / 2);
			//}
		}

		private void glControl_Render(object sender, GlControlEventArgs e) {
			update();
		}

		//private static readonly float[] _ArrayPosition = new float[] {
		//	00, 64,
		//	00, 00,
		//	64, 00,
		//	64, 64,
		//};
		
		//private static readonly float[] _ArrayColor = new float[] {
		//	0.0f, 0.0f, 0.0f,
		//	1.0f, 0.0f, 0.0f,
		//	0.0f, 1.0f, 0.0f,
		//	0.0f, 0.0f, 1.0f,
		//};

		//private static readonly float[] _ArrayTexCoord = new float[] {
		//	0.0f, 1.0f,
		//	0.0f, 0.0f,
		//	1.0f, 0.0f,
		//	1.0f, 1.0f,
		//};

		//private static readonly string[] _VertexSourceGL = {
		//	//"#version 150 compatibility\n",
		//	//"#version 330 core\n",
		//	"uniform mat4 uMVP;\n",
		//	"in vec2 aPosition;\n",
		//	"in vec2 aTexCoord;\n",
		//	"varying vec2 vTexCoord;\n",
		//	//"in vec3 aColor;\n",
		//	//"out vec4 vColor;\n",
		//	"void main() {\n",
		//	"	gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);\n",
		//	//"	gl_Position = vec4(aPosition, 0.0, 1.0);\n",
		//	//"	vColor = aColor;\n",
		//	"	vTexCoord = aTexCoord;\n",
		//	"}\n"
		//};

		//private static readonly string[] _FragmentSourceGL = {
		//	//"#version 150 compatibility\n",
		//	//"#version 330 core\n",
		//	//"in vec4 vColor;\n",
		//	"varying vec2 vTexCoord;\n",
		//	"uniform sampler2D tex;\n",
		//	"void main() {\n",
		//	//"	gl_FragColor = vColor;\n",
		//	"	gl_FragColor = texture(tex, vTexCoord);\n",
		//	//"	discard;",
		//	"}\n"
		//};

		private void updateGlSize() {
			var w = glControl.Width;
			var h = glControl.Height;

			//Gl.Viewport(0, 0, w, h);
			//Gl.MatrixMode(MatrixMode.Projection);
			//Gl.LoadIdentity();
			//Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

			//Gl.MatrixMode(MatrixMode.Modelview);
			//Gl.LoadIdentity();

			OrthoProjectionMatrix projectionMatrix = new OrthoProjectionMatrix(0.0f, w, 0.0f, h, 0.0f, 1.0f);
			ModelMatrix modelMatrix = new ModelMatrix();
			mMVP = (projectionMatrix * modelMatrix).ToArray();

			//if(isEngineInited && _Program != null) {
			//	Gl.UniformMatrix4(_Program.LocationMVP, false, mMVP);
			//}
		}

		private void glControl_Resize(object sender, EventArgs e) {
			updateGlSize();
		}

	}
}
