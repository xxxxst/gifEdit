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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gifEdit.view {
	/// <summary>粒子渲染区域</summary>
	public partial class ParticleRenderBox : UserControl {
		bool isEngineInited = false;

		int renderTime = 0;
		int maxRenderTime = 1;

		GlTime glTime = new GlTime();
		FpsCtl fpsCtl = new FpsCtl();
		
		private float[] mMVP = null;
		
		List<ParticleEmitter> lstEmitter = new List<ParticleEmitter>();
		
		int boxWidth = 0;
		int boxHeight = 0;

		int slbX = 0;
		int slbY = 0;

		uint glbufOutput = 0;

		public ParticleRenderBox() {
			InitializeComponent();
			
			if(DesignerProperties.GetIsInDesignMode(this)) {
				glControl.Animation = false;
				glControl.Visible = false;
			}
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
					update();
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
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
			Gl.Enable(EnableCap.PolygonSmooth);
			Gl.Enable(EnableCap.LineSmooth);
			//Gl.Enable(EnableCap.DepthTest);
			//Gl.Enable(EnableCap.Multisample);
			//Gl.Enable(EnableCap.PrimitiveRestart);
			Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			//Gl.DepthFunc(DepthFunction.Less);

			glbufOutput = Gl.GenFramebuffer();

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
		}

		int fpsUpdateIdx = 0;
		//int idx = 0;
		private void update() {
			//time
			float gapTime = glTime.getTime();
			renderTime = (int)(renderTime + gapTime);
			if(renderTime >= maxRenderTime) {
				renderTime = maxRenderTime + renderTime % maxRenderTime;
			} else {
				renderTime = renderTime % maxRenderTime;
			}

			//test
			//++idx;
			//if(idx > 20) {
			//	lblTest.Content = renderTime;
			//	idx = 0;
			//}

			//fps
			fpsCtl.update();
			++fpsUpdateIdx;
			if(fpsUpdateIdx > 60) {
				//lblFps.Content = fpsCtl.getFps();
				EventServer.ins.onUpdateFPSEvent(fpsCtl.getFps());
				fpsUpdateIdx = 0;
			}
			
			if(!isEngineInited) {
				return;
			}

			//var senderControl = glControl;

			//int vpx = 0;
			//int vpy = 0;
			//int vpw = senderControl.ClientSize.Width;
			//int vph = senderControl.ClientSize.Height;
			//Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			//ParticleEditModel md = MainModel.ins.particleEditModel;
			//if(md == null || boxWidth == 0 || boxHeight == 0) {
			//	return;
			//}

			bool isOk = isRenderToBuffer;
			isRenderToBuffer = false;
			if(isOk) {
				_renderToBuffer();
			} else {
				renderGl();
			}

			//var w = boxWidth;
			//var h = boxHeight;

			//float x = slbX + (boxWidth - md.width) / 2 + 0.5f;
			//float y = slbY + (boxHeight - md.height) / 2 + 0.5f;

			//Gl.Viewport(vpx, vpy, vpw, vph);
			//Gl.Clear(ClearBufferMask.ColorBufferBit);

			//Gl.MatrixMode(MatrixMode.Projection);
			//Gl.LoadIdentity();
			//Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

			//Gl.MatrixMode(MatrixMode.Modelview);
			//Gl.LoadIdentity();

			////line 1
			//float xl1 = x;
			//float yl1 = y;

			//Gl.LineWidth(1f);
			//Gl.Begin(PrimitiveType.LineLoop);
			//Gl.Color3(1.0f, 1.0f, 1.0f);
			//Gl.Vertex2(xl1, yl1);
			//Gl.Vertex2(xl1 + md.width, yl1);
			//Gl.Vertex2(xl1 + md.width, yl1 + md.height);
			//Gl.Vertex2(xl1, yl1 + md.height);
			//Gl.End();

			////line 2
			//float xl2 = x - 1;
			//float yl2 = y - 1;

			//Gl.Begin(PrimitiveType.LineLoop);
			//Gl.Color3(0.0f, 0.0f, 0.0f);
			//Gl.Vertex2(xl2, yl2);
			//Gl.Vertex2(xl2 + md.width + 2, yl2);
			//Gl.Vertex2(xl2 + md.width + 2, yl2 + md.height + 2);
			//Gl.Vertex2(xl2, yl2 + md.height + 2);
			//Gl.End();

			////mask
			//if(md.isMaskBox) {
			//	Gl.Enable(EnableCap.ScissorTest);
			//	Gl.Scissor((int)x, (int)y, md.width, md.height);
			//}

			////emitter
			//for(int i = 0; i < lstEmitter.Count; ++i) {
			//	lstEmitter[i].setStartPos((int)x, (int)y);
			//	lstEmitter[i].render(mMVP, renderTime);
			//}

			////mask
			//if(md.isMaskBox) {
			//	Gl.Disable(EnableCap.ScissorTest);
			//}
		}

		private void renderGl(bool isOutput = false) {
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.width == 0 || md.height == 0) {
				return;
			}

			float x = 0;
			float y = 0;
			if(!isOutput) {
				x = slbX + (boxWidth - md.width) / 2 + 0.5f;
				y = slbY + (boxHeight - md.height) / 2 + 0.5f;
			}

			var w = boxWidth;
			var h = boxHeight;

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

			if(!isOutput) {
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
			}

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

		bool isRenderToBuffer = false;

		public void renderToBuffer() {
			isRenderToBuffer = true;
			return;
		}

		public void _renderToBuffer() {
			if(!isEngineInited) {
				return;
			}
			ParticleEditModel md = MainModel.ins.particleEditModel;
			if(md == null || md.width == 0 || md.height == 0) {
				return;
			}
			//glControl.Animation = false;
			const int pxSize = 4;

			int w = md.width;
			int h = md.height;

			try {
				Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, glbufOutput);
				renderGl(true);

				int size = w * h * pxSize;
				IntPtr pBuffer = Marshal.AllocHGlobal(size);

				Gl.ReadPixels(0, 0, w, h, PixelFormat.Bgra, PixelType.UnsignedByte, pBuffer);
				IntPtr pData = Gl.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);

				byte[] data = new byte[size];
				Marshal.Copy(pBuffer, data, 0, data.Length);

				Marshal.Release(pBuffer);

				Gl.UnmapBuffer(BufferTarget.PixelPackBuffer);
				Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
				
				FIBITMAP dib = FreeImage.Allocate(w, h, 32, 8, 8, 8);
				IntPtr pFibData = FreeImage.GetBits(dib);
				Marshal.Copy(data, 0, pFibData, data.Length);

				FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, "bbb.png", FREE_IMAGE_SAVE_FLAGS.DEFAULT);
				FreeImage.Unload(dib);

			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}

		}

		private void glControl_Render(object sender, GlControlEventArgs e) {
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

			OrthoProjectionMatrix projectionMatrix = new OrthoProjectionMatrix(0.0f, w, 0.0f, h, 0.0f, 1000000.0f);
			ModelMatrix modelMatrix = new ModelMatrix();
			mMVP = (projectionMatrix * modelMatrix).ToArray();
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

		private void glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
			ScrollBar slb = isDownShift() ? slbHor : slbVer;

			double oldVal = slb.Value;
			double newVal = oldVal + slb.SmallChange * (e.Delta < 0 ? 1 : -1);
			if(oldVal * newVal < 0) {
				newVal = 0;
			}

			slb.Value = newVal;
		}

		private bool isDownShift() {
			return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
		}
	}
}
