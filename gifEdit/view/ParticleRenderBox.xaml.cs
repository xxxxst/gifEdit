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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>粒子渲染区域</summary>
	public partial class ParticleRenderBox : UserControl, System.Windows.Forms.IMessageFilter {
		bool isEngineInited = false;

		int renderTime = 0;
		int maxRenderTime = 1;

		GlTime glTime = new GlTime();
		FpsCtl fpsCtl = new FpsCtl();
		
		private float[] bufferMVP = null;
		private float[] mMVP = null;

		List<ParticleEmitter> lstEmitter = new List<ParticleEmitter>();
		
		int boxWidth = 0;
		int boxHeight = 0;

		int slbX = 0;
		int slbY = 0;

		int bufWidth = 0;
		int bufHeight = 0;
		uint glbufTexCache = 0;
		uint glbufOutput = 0;
		MemoryLock mlBufferCache = null;
		byte[] bufferCache = new byte[0];
		byte[] bufferOutput = new byte[0];
		MemoryLock mlBufferOutput = null;

		Brush coBorderDef = null;
		Brush coBorderAct = null;

		public ParticleRenderBox() {
			InitializeComponent();

			coBorderDef = FindResource("comBorderColor") as Brush;
			coBorderAct = FindResource("comBorderColorActivate") as Brush;
			
			if(DesignerProperties.GetIsInDesignMode(this)) {
				glControl.Animation = false;
				glControl.Visible = false;
			}

			System.Windows.Forms.Application.AddMessageFilter(this);
		}

		private bool isMouseOverGlControl = false;
		public bool PreFilterMessage(ref System.Windows.Forms.Message msg) {
			switch(msg.Msg) {
				case 0x20a: {
					if(!isMouseOverGlControl) {
						break;
					}
					int delta = msg.WParam.ToInt32();
					delta = delta >> 16;
					updateScroll(delta);
					break;
				}
			}

			return false;
		}

		public void init() {
			try {
				clear();

				ParticleEditModel md = MainModel.ins.particleEditModel;

				//maxRenderTime = 5 * 1000;
				maxRenderTime = 1;
				for(int i = 0; i < md.lstResource.Count; ++i) {
					int ms = (int)(md.lstResource[i].particleLife * 1000);
					maxRenderTime = Math.Max(maxRenderTime, ms);
				}
				
				for(int i = 0; i < md.lstResource.Count; ++i) {
					ParticleEmitter emt = new ParticleEmitter(md.lstResource[i], maxRenderTime);
					lstEmitter.Add(emt);
				}

				//udpateImage();
				updateRenderBoxSize();
				
				if(isEngineInited) {
					//update();
					initOutputBuffer();
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void startAnimation(bool isStart) {
			glControl.Animation = isStart;
		}

		public void createEmitter(int idx) {
			ParticleEditModel md = MainModel.ins.particleEditModel;

			ParticleEmitter emt = new ParticleEmitter(md.lstResource[idx], maxRenderTime);
			lstEmitter.Add(emt);

			bool isUpdate = updateMaxRenderTime();
			if(!isUpdate) {
				emt.updateAttr();
			}
		}

		public void removeEmitter(int idx) {
			ParticleEditModel md = MainModel.ins.particleEditModel;

			ParticleEmitter emt = lstEmitter[idx];
			lstEmitter.RemoveAt(idx);

			emt.Dispose();

			updateMaxRenderTime();
		}

		public void updateGlobalAttr() {
			if(!isEngineInited) {
				return;
			}

			try {
				ParticleEditModel md = MainModel.ins.particleEditModel;

				glClear(md.background);

				updateRenderBoxSize();

			} catch(Exception) {

			}
		}

		public void updateEmitterAttr(int idx) {
			if (idx < 0 || idx >= lstEmitter.Count) {
				return;
			}

			bool isUpdate = updateMaxRenderTime();

			if (!isUpdate) {
				lstEmitter[idx].updateAttr();
			}
		}

		public void updateEmitterImage(int idx) {
			if(idx < 0 || idx >= lstEmitter.Count) {
				return;
			}
			
			lstEmitter[idx].updateImage();
		}

		private bool updateMaxRenderTime() {
			ParticleEditModel md = MainModel.ins.particleEditModel;

			var time = 1;
			for (int i = 0; i < lstEmitter.Count; ++i) {
				int ms = (int)(md.lstResource[i].particleLife * 1000);
				time = Math.Max(time, ms);
			}

			if (time == maxRenderTime) {
				return false;
			}

			maxRenderTime = time;
			renderTime = 0;
			for (int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].updateAttr(maxRenderTime);
			}

			return true;
		}

		public void updateRenderBoxSize() {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				return;
			}

			boxWidth = (int)bdRenderBox.ActualWidth;
			boxHeight = (int)bdRenderBox.ActualHeight;

			int width = md.width;
			int height = md.height;

			//width = Math.Min(width, (int)bdRenderBox.ActualWidth - 2);
			//height = Math.Min(height, (int)bdRenderBox.ActualHeight - 2);
			width = boxWidth;
			height = boxHeight;
			
			width = Math.Max(width, 0);
			height = Math.Max(height, 0);

			//win.Width = width;
			//win.Height = height;
			formHost.Width = width;
			formHost.Height = height;

			updateScrollBarSize();
		}

		private void bdRenderBox_SizeChanged(object sender, SizeChangedEventArgs e) {
			updateRenderBoxSize();
		}
		
		private void glControl_ContextCreated(object sender, GlControlEventArgs e) {
			Gl.Enable(EnableCap.AlphaTest);
			Gl.Enable(EnableCap.Blend);
			Gl.Enable(EnableCap.Texture2d);
			//Gl.Enable(EnableCap.PolygonSmooth);
			//Gl.Enable(EnableCap.LineSmooth);
			//Gl.Disable(EnableCap.CullFace);
			//Gl.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
			//Gl.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
			//Gl.Enable(EnableCap.DepthTest);
			//Gl.Enable(EnableCap.Multisample);
			//Gl.Enable(EnableCap.PrimitiveRestart);
			Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			//Gl.DepthFunc(DepthFunction.Less);

			glbufTexCache = Gl.GenTexture();
			glbufOutput = Gl.GenFramebuffer();
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, glbufOutput);
			Gl.DrawBuffers(Gl.COLOR_ATTACHMENT0);
			Gl.ReadBuffer(ReadBufferMode.ColorAttachment0);
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			updateGlSize();

			//Gl.ClearColor(27 / 255f, 28 / 255f, 32 / 255f, 1.0f);
			//glClear("1e2027");
			//glClear("24252c", 1);
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null) {
				glClear("24252c", 1);
			} else {
				glClear(md.background, 1);
			}
			//glClear(0, 1);

			isEngineInited = true;
			EventServer.ins.onPointEngineInited();
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

		private void clear() {
			try {
				for(int i = 0; i < lstEmitter.Count; ++i) {
					lstEmitter[i].Dispose();
				}
				lstEmitter.Clear();
			} catch(Exception) { }
		}

		private void glControl_ContextDestroying(object sender, GlControlEventArgs e) {
			clear();
			if(mlBufferCache != null) {
				mlBufferCache.Dispose();
				mlBufferCache = null;
			}
			if(mlBufferOutput != null) {
				mlBufferOutput.Dispose();
				mlBufferOutput = null;
			}
			if(glbufTexCache > 0) {
				Gl.DeleteTextures(glbufTexCache);
				glbufTexCache = 0;
			}
			if(glbufOutput > 0) {
				Gl.DeleteFramebuffers(glbufOutput);
				glbufOutput = 0;
			}
		}

		private void updateTime() {
			float gapTime = glTime.getTime();
			renderTime = (int)(renderTime + gapTime);
			if(renderTime >= maxRenderTime) {
				renderTime = maxRenderTime + renderTime % maxRenderTime;
			} else {
				renderTime = renderTime % maxRenderTime;
			}
		}

		public void setRenderTime(int time) {
			renderTime = time;
		}

		public int getMaxRenderTime() {
			return maxRenderTime;
		}

		int fpsUpdateIdx = 0;
		private void updateFps() {
			fpsCtl.update();
			++fpsUpdateIdx;
			if(fpsUpdateIdx > 60) {
				//lblFps.Content = fpsCtl.getFps();
				EventServer.ins.onUpdateFPSEvent(fpsCtl.getFps());
				fpsUpdateIdx = 0;
			}
		}

		//int idx = 0;
		private void update() {
			//time
			//updateTime();

			//fps
			updateFps();
			
			if(!isEngineInited) {
				return;
			}
			renderGl();
		}

		private void renderInitMatrix(int w, int h) {
			int vpx = 0;
			int vpy = 0;
			int vpw = w;
			int vph = h;

			Gl.Viewport(vpx, vpy, vpw, vph);
			Gl.Clear(ClearBufferMask.ColorBufferBit);

			Gl.MatrixMode(MatrixMode.Projection);
			Gl.LoadIdentity();
			Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

			Gl.MatrixMode(MatrixMode.Modelview);
			Gl.LoadIdentity();
		}

		private void renderGl() {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.width == 0 || md.height == 0) {
				return;
			}

			float x = slbX + (boxWidth - md.width) / 2 + 0.5f;
			float y = slbY + (boxHeight - md.height) / 2 + 0.5f;

			var w = boxWidth;
			var h = boxHeight;

			//int vpx = 0;
			//int vpy = 0;
			//int vpw = w;
			//int vph = h;

			//Gl.Viewport(vpx, vpy, vpw, vph);
			//Gl.Clear(ClearBufferMask.ColorBufferBit);

			//Gl.MatrixMode(MatrixMode.Projection);
			//Gl.LoadIdentity();
			//Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

			//Gl.MatrixMode(MatrixMode.Modelview);
			//Gl.LoadIdentity();

			Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			renderInitMatrix(w, h);

			//line 1
			float xl1 = x;
			float yl1 = y;

			Gl.LineWidth(1f);
			Gl.Begin(PrimitiveType.LineLoop);
			Gl.Color3(1.0f, 1.0f, 1.0f);
			Gl.Vertex2(xl1, yl1);
			Gl.Vertex2(xl1 + md.width, yl1);
			Gl.Vertex2(xl1 + md.width, yl1 + md.height);
			Gl.Vertex2(xl1, yl1 + md.height);
			Gl.End();

			//line 2
			float xl2 = x - 1;
			float yl2 = y - 1;

			Gl.Begin(PrimitiveType.LineLoop);
			Gl.Color3(0.0f, 0.0f, 0.0f);
			Gl.Vertex2(xl2, yl2);
			Gl.Vertex2(xl2 + md.width + 2, yl2);
			Gl.Vertex2(xl2 + md.width + 2, yl2 + md.height + 2);
			Gl.Vertex2(xl2, yl2 + md.height + 2);
			Gl.End();

			//mask
			if(md.isMaskBox) {
				Gl.Enable(EnableCap.ScissorTest);
				Gl.Scissor((int)x, (int)y, md.width, md.height);
			}

			//emitter
			for(int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].setStartPos((int)x, (int)y);
				lstEmitter[i].render(mMVP, renderTime);
			}

			//mask
			if(md.isMaskBox) {
				Gl.Disable(EnableCap.ScissorTest);
			}
		}

		private void renderGlToBuffer(int frameIdx = -1) {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.width == 0 || md.height == 0) {
				return;
			}

			float x = 0;
			float y = 0;
			int w = md.width;
			int h = md.height;

			int time = renderTime;
			if(frameIdx >= 0) {
				time = (int)((float)frameIdx / md.fps * 1000);
			}

			//int vpx = 0;
			//int vpy = 0;
			//int vpw = w;
			//int vph = h;

			//Gl.Viewport(vpx, vpy, vpw, vph);
			//Gl.Clear(ClearBufferMask.ColorBufferBit);

			//Gl.MatrixMode(MatrixMode.Projection);
			//Gl.LoadIdentity();
			//Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

			//Gl.MatrixMode(MatrixMode.Modelview);
			//Gl.LoadIdentity();
			Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
			Gl.BlendEquation(BlendEquationMode.FuncAdd);
			renderInitMatrix(w, h);

			//mask
			if(md.isMaskBox) {
				Gl.Enable(EnableCap.ScissorTest);
				Gl.Scissor((int)x, (int)y, md.width, md.height);
			}

			//emitter
			for(int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].setStartPos((int)x, (int)y);
				lstEmitter[i].render(bufferMVP, time);
			}

			//mask
			if(md.isMaskBox) {
				Gl.Disable(EnableCap.ScissorTest);
			}
		}

		//bool isRenderToBuffer = false;

		//public void renderToBuffer() {
		//	isRenderToBuffer = true;
		//	return;
		//}

		const int pxChannel = 4;
		
		private void initOutputBuffer() {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			//if(md == null || md.width == 0 || md.height == 0) {
			//	return;
			//}

			if(bufWidth == md.width && bufHeight == md.height) {
				return;
			}
			
			int w = md.width;
			int h = md.height;

			bufWidth = w;
			bufHeight = h;

			bufferMVP = calcMVP(w, h);

			//Gl.Enable(EnableCap.FramebufferSrgb);

			if(mlBufferCache != null) {
				mlBufferCache.Dispose();
			}
			if(mlBufferOutput != null) {
				mlBufferOutput.Dispose();
			}

			int size = w * h * pxChannel;
			bufferCache = new byte[size];
			mlBufferCache = new MemoryLock(bufferCache);

			bufferOutput = new byte[size];
			mlBufferOutput = new MemoryLock(bufferOutput);

			//Gl.BufferData(BufferTarget.PixelPackBuffer, (uint)size, mlImageData.Address, BufferUsage.DynamicRead);

			Gl.BindTexture(TextureTarget.Texture2d, glbufTexCache);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
			//Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			//Gl.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)LightEnvModeSGIX.Replace);

			Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, w, h, 0, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mlBufferCache.Address);
			Gl.BindTexture(TextureTarget.Texture2d, 0);

			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, glbufOutput);
			Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, glbufTexCache, 0);
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public ImageModel renderToBuffer(int frameIdx = -1, bool isTransplate = false) {
			if(!isEngineInited) {
				return null;
			}
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.width == 0 || md.height == 0) {
				return null;
			}

			int w = md.width;
			int h = md.height;
			int size = w * h * pxChannel;

			byte[] data1 = new byte[size];
			_renderOneBuffer(isTransplate, frameIdx);
			Array.Copy(bufferOutput, 0, data1, 0, size);

			//byte[] data2 = new byte[size];
			//_renderOneBuffer(0x00ffffff);
			//Array.Copy(bufferOutput, 0, data2, 0, size);

			//for(int i = 0; i < data1.Length; ++i) {
			//	data1[i] = (byte)((data1[i] + data2[i]) / 2);
			//}
			const int imageChannel = 4;
			if(isTransplate) {
				for(int x = 0; x < w; ++x) {
					for(int y = 0; y < h; ++y) {
						int idx = (y * w + x) * imageChannel;
						if(data1[idx + 3] == 0) {
							continue;
						}
						data1[idx + 0] = (byte)(data1[idx + 0] / ((float)(data1[idx + 3]) / 255));
						data1[idx + 1] = (byte)(data1[idx + 1] / ((float)(data1[idx + 3]) / 255));
						data1[idx + 2] = (byte)(data1[idx + 2] / ((float)(data1[idx + 3]) / 255));
					}
				}
			}

			return new ImageModel() {
				width = w,
				height = h,
				data = data1,
			};
		}

		private void _renderOneBuffer(bool isTransplate, int frameIdx = -1) {
			ParticleEditModel md = MainModel.ins.particleEditModel;

			int w = md.width;
			int h = md.height;
			int size = w * h * pxChannel;

			try {
				initOutputBuffer();

				Gl.BindFramebuffer(FramebufferTarget.Framebuffer, glbufOutput);

				if(isTransplate) {
					glClear(0);
				}
				//glClear(0x00808080);
				renderGlToBuffer(frameIdx);
				glClear(md.background, 1);

				Gl.ReadPixels(0, 0, w, h, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mlBufferOutput.Address);

				Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		//public ImageModel _renderToBuffer() {
		//	if(!isEngineInited) {
		//		return null;
		//	}
		//	ParticleEditModel md = MainModel.ins.particleEditModel;
		//	if(md == null || md.width == 0 || md.height == 0) {
		//		return null;
		//	}
		//	//glControl.Animation = false;

		//	int w = md.width;
		//	int h = md.height;
		//	int size = w * h * pxChannel;

		//	try {
		//		//Gl.Enable(EnableCap.FramebufferSrgb);
				
		//		initOutputBuffer();

		//		Gl.BindFramebuffer(FramebufferTarget.Framebuffer, glbufOutput);
		//		//Gl.DrawBuffers(Gl.COLOR_ATTACHMENT0);

		//		glClear(0x0);
		//		//glClear(0x00808080);
		//		renderGlToBuffer();
		//		glClear(md.background);

		//		//IntPtr pBuffer = Marshal.AllocHGlobal(size);
		//		//Gl.ReadBuffer(ReadBufferMode.ColorAttachment0);
		//		//Gl.BindFramebuffer(FramebufferTarget.Framebuffer, glbufOutput);
		//		//Gl.ReadBuffer(ReadBufferMode.ColorAttachment0);
		//		Gl.ReadPixels(0, 0, w, h, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mlBufferOutput.Address);

		//		//byte[] data = new byte[size];
		//		//Marshal.Copy(mlBufferOutput.Address, data, 0, data.Length);
		//		//Marshal.Release(pBuffer);

		//		Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		//		//Gl.Disable(EnableCap.FramebufferSrgb);


		//		return new ImageModel() {
		//			width = w,
		//			height = h,
		//			data = bufferOutput,
		//		};

		//		//saveImage(bufferOutput, w, h, "bbb.png");

		//	} catch(Exception ex) {
		//		Debug.WriteLine(ex.ToString());
		//	}

		//	return null;
		//}

		private void glControl_Render(object sender, GlControlEventArgs e) {
			//needUpdate = true;
			update();
		}

		private void updateGlSize() {
			var w = glControl.Width;
			var h = glControl.Height;

			//Gl.Viewport(0, 0, w, h);
			//Gl.MatrixMode(MatrixMode.Projection);
			//Gl.LoadIdentity();
			//Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

			//Gl.MatrixMode(MatrixMode.Modelview);
			//Gl.LoadIdentity();

			//OrthoProjectionMatrix projectionMatrix = new OrthoProjectionMatrix(0.0f, w, 0.0f, h, 0.0f, 1000000.0f);
			//ModelMatrix modelMatrix = new ModelMatrix();
			//mMVP = (projectionMatrix * modelMatrix).ToArray();
			mMVP = calcMVP(w, h);
		}

		private float[] calcMVP(float w, float h) {
			OrthoProjectionMatrix projectionMatrix = new OrthoProjectionMatrix(0.0f, w, 0.0f, h, 0.0f, 1000000.0f);
			ModelMatrix modelMatrix = new ModelMatrix();
			return (projectionMatrix * modelMatrix).ToArray();
		}

		private void glControl_Resize(object sender, EventArgs e) {
			updateGlSize();
		}

		private void updateScrollBarSize() {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || boxWidth == 0 || boxHeight == 0) {
				return;
			}

			int gap = 50;

			int maxHor = (md.width + gap * 2 - boxWidth) / 2;
			maxHor = Math.Max(0, maxHor);
			slbHor.ViewportSize = boxWidth;
			slbHor.Minimum = -maxHor;
			slbHor.Maximum = maxHor;
			if(md.width < boxWidth) {
				slbHor.Value = 0;
				//slbHor.Minimum = 0;
				//slbHor.Maximum = 0;
			}

			int maxVer = (md.height + gap * 2 - boxHeight) / 2;
			maxVer = Math.Max(0, maxVer);
			slbVer.ViewportSize = boxHeight;
			slbVer.Minimum = -maxVer;
			slbVer.Maximum = maxVer;
			if(md.height < boxHeight) {
				slbVer.Value = 0;
				//slbVer.Minimum = 0;
				//slbVer.Maximum = 0;
			}
		}

		private void slbHor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			slbX = -(int)slbHor.Value;
		}

		private void slbVer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			slbY = (int)slbVer.Value;
		}

		private void updateScroll(int delta) {
			ScrollBar slb = isDownShift() ? slbHor : slbVer;

			double oldVal = slb.Value;
			double newVal = oldVal + slb.SmallChange * (delta < 0 ? 1 : -1);
			if(oldVal * newVal < 0) {
				newVal = 0;
			}

			slb.Value = newVal;
		}

		private void glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
			//ScrollBar slb = isDownShift() ? slbHor : slbVer;

			//double oldVal = slb.Value;
			//double newVal = oldVal + slb.SmallChange * (e.Delta < 0 ? 1 : -1);
			//if(oldVal * newVal < 0) {
			//	newVal = 0;
			//}

			//slb.Value = newVal;

			//updateScroll(e.Delta);
		}

		private bool isDownShift() {
			return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
		}

		private void ctlParticaleRenderBox_GotFocus(object sender, RoutedEventArgs e) {
			BorderBrush = coBorderAct;
		}

		private void ctlParticaleRenderBox_LostFocus(object sender, RoutedEventArgs e) {
			BorderBrush = coBorderDef;
		}

		private void ctlParticaleRenderBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if(IsVisible) {
				//glControl.Focus();
				//FocusManager.SetFocusedElement(this);
				Keyboard.Focus(this);
			}
		}

		private bool isDoDownEvent = false;

		private void ctlParticaleRenderBox_KeyDown(object sender, KeyEventArgs e) {
			if(e.Key != Key.C || !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) ) {
				return;
			}
			if(isDoDownEvent) {
				return;
			}
			isDoDownEvent = true;
			EventServer.ins.onCopyToClipboard();
		}

		private void ctlParticaleRenderBox_KeyUp(object sender, KeyEventArgs e) {
			isDoDownEvent = false;
		}

		private void GrdBox_MouseWheel(object sender, MouseWheelEventArgs e) {
			updateScroll(e.Delta);
		}

		private void GlControl_MouseHover(object sender, EventArgs e) {
			isMouseOverGlControl = true;
		}

		private void GlControl_MouseLeave(object sender, EventArgs e) {
			isMouseOverGlControl = false;
		}
	}
}
