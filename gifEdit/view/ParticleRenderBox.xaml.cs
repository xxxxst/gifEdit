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
	/// <summary>粒子渲染区域</summary>
	public partial class ParticleRenderBox : UserControl {
		bool isEngineInited = false;

		int renderTime = 0;
		int maxRenderTime = 1;

		GlTime glTime = new GlTime();
		FpsCtl fpsCtl = new FpsCtl();
		
		private float[] mMVP = null;
		
		List<ParticleEmitter> lstEmitter = new List<ParticleEmitter>();

		public ParticleRenderBox() {
			InitializeComponent();
			
			if(DesignerProperties.GetIsInDesignMode(this)) {
				glControl.Animation = false;
				glControl.Visible = false;
			}
		}

		public void init() {
			try {
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

		public void updateEmitterAttr(int idx) {
			if (idx < 0 || idx >= lstEmitter.Count) {
				return;
			}

			bool isUpdate = updateMaxRenderTime();

			if (!isUpdate) {
				lstEmitter[idx].updateAttr();
			}
		}

		private bool updateMaxRenderTime() {
			ParticleEditModel md = MainModel.ins.particleEditModel;

			var time = 1;
			for (int i = 0; i < md.lstResource.Count; ++i) {
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

			int width = md.width;
			int height = md.height;

			width = Math.Min(width, (int)grdRenderBox.ActualWidth - 2);
			height = Math.Min(height, (int)grdRenderBox.ActualHeight - 2);

			width = Math.Max(width, 0);
			height = Math.Max(height, 0);

			//win.Width = width;
			//win.Height = height;
			formHost.Width = width;
			formHost.Height = height;
		}

		private void grdRenderBox_SizeChanged(object sender, SizeChangedEventArgs e) {
			updateRenderBoxSize();
		}
		
		private void glControl_ContextCreated(object sender, GlControlEventArgs e) {
			Gl.Enable(EnableCap.AlphaTest);
			Gl.Enable(EnableCap.Blend);
			Gl.Enable(EnableCap.Texture2d);
			Gl.Enable(EnableCap.PolygonSmooth);
			//Gl.Enable(EnableCap.DepthTest);
			//Gl.Enable(EnableCap.Multisample);
			//Gl.Enable(EnableCap.PrimitiveRestart);
			Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			//Gl.DepthFunc(DepthFunction.Less);

			updateGlSize();

			//Gl.ClearColor(27 / 255f, 28 / 255f, 32 / 255f, 1.0f);
			//glClear("1e2027");
			glClear("24252c", 1);
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

		private void glControl_ContextDestroying(object sender, GlControlEventArgs e) {
			for(int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].Dispose();
			}
			lstEmitter.Clear();
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
				lblFps.Content = fpsCtl.getFps();
				fpsUpdateIdx = 0;
			}
			
			if(!isEngineInited) {
				return;
			}

			var senderControl = glControl;

			int vpx = 0;
			int vpy = 0;
			int vpw = senderControl.ClientSize.Width;
			int vph = senderControl.ClientSize.Height;

			Gl.Viewport(vpx, vpy, vpw, vph);
			Gl.Clear(ClearBufferMask.ColorBufferBit);
			//Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			for(int i = 0; i < lstEmitter.Count; ++i) {
				lstEmitter[i].render(mMVP, renderTime);
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

	}
}
