using FreeImageAPI;
using gifEdit.control.glEngine;
using gifEdit.model;
using gifEdit.util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.control {
	/// <summary>粒子发射器</summary>
	public class PointEmitter : IDisposable {
		private PointResourceModel md = null;
		private int maxRenderTime = 0;
		private int zIndex = 0;

		private IntPtr pImageData = IntPtr.Zero;
		private int imageWidth = 0;
		private int imageHeight = 0;
		
		uint attrBufferId = 0;
		uint texId = 0;
		uint texFraBufferId = 0;

		private uint ProgramName;
		private int LocationMVP;
		private int LocationIndex;
		private int LocationCoord;
		private int LocationTex;
		private int LocationNowTime;
		//private int LocationZIndex;
		private int LocationTotalLifeTime;
		private int LocationPointCount;

		//private int LocationPointAttr;

		private int pointCount = 0;
		private MemoryLock lockBufferData = null;

		public PointEmitter(PointResourceModel _md, int _maxRenderTime) {
			md = _md;
			//zIndex = _zIndex;
			maxRenderTime = _maxRenderTime;

			attrBufferId = Gl.GenBuffer();
			//bufferId = Gl.GenVertexArray();
			texId = Gl.GenTexture();
			texFraBufferId = Gl.GenFramebuffer();

			string[] vetex = ComUtil.loadEmbedShader("vPointEmitter.glsl");
			string[] fragment = ComUtil.loadEmbedShader("fPointEmitter.glsl");

			using(GlObject vObject = new GlObject(ShaderType.VertexShader, vetex))
			using(GlObject fObject = new GlObject(ShaderType.FragmentShader, fragment)) {
				// Create program
				ProgramName = Gl.CreateProgram();
				// Attach shaders
				Gl.AttachShader(ProgramName, vObject.ShaderName);
				Gl.AttachShader(ProgramName, fObject.ShaderName);
				// Link program
				Gl.LinkProgram(ProgramName);

				// Check linkage status
				Gl.GetProgram(ProgramName, ProgramProperty.LinkStatus, out int linked);

				if(linked == 0) {
					const int logMaxLength = 1024;

					StringBuilder infolog = new StringBuilder(logMaxLength);
					int infologLength;

					Gl.GetProgramInfoLog(ProgramName, 1024, out infologLength, infolog);

					throw new InvalidOperationException($"unable to link program: {infolog}");
				}

				LocationMVP = getUniformId("uMVP");
				LocationIndex = getAttrId("index");
				LocationCoord = getAttrId("vCoord");
				LocationTex = getUniformId("tex");
				LocationNowTime = getUniformId("nowTime");
				//LocationZIndex = getUniformId("zIndex");
				LocationTotalLifeTime = getUniformId("totalLifeTime");
				LocationPointCount = getUniformId("pointCount");
			}

			udpateImage();
			updateAttr();
		}

		public void udpateImage() {
			_isTextureExist = false;
			var rootMd = MainModel.ins.pointEditModel;

			string path = md.path;
			if(path.Length < 2 || path[1] != ':') {
				path = rootMd.path + "/" + path;
			}

			//path = @"E:\00workself\csharp\project\desktopDate\desktopDate\resource\image\icon.png";
			//var fif = FreeImage FreeImage_GetFileType(FileName, 0);

			if(pImageData != null) {
				FreeImage.FreeHbitmap(pImageData);
				pImageData = IntPtr.Zero;
			}

			if(!File.Exists(path)) {
				return;
			}

			var type = FreeImage.GetFileType(path, 0);
			if(type == FREE_IMAGE_FORMAT.FIF_UNKNOWN) {
				return;
			}

			if(!FreeImage.FIFSupportsReading(type)) {
				return;
			}

			var dib = FreeImage.Load(type, path, FREE_IMAGE_LOAD_FLAGS.DEFAULT);

			imageWidth = (int)FreeImage.GetWidth(dib);
			imageHeight = (int)FreeImage.GetHeight(dib);

			//var dib2 = FreeImage.Rescale(dib, 16, 16, FREE_IMAGE_FILTER.FILTER_BILINEAR);
			//var bits = FreeImage.GetBits(dib2);
			//imageWidth = imageHeight = 16;
			var bits = FreeImage.GetBits(dib);
			pImageData = bits;

			initTexture();

			FreeImage.FreeHbitmap(pImageData);
			pImageData = IntPtr.Zero;

			_isTextureExist = true;
		}

		private bool _isTextureExist = false;
		public bool isTextureExist{
			get { return _isTextureExist; }
		}

		//const int GL_NEAREST = 0x2600;
		const int GL_LINEAR = 0x2601;
		const int GL_REPEAT = 0x2901;
		const int GL_REPLACE = 0x1E01;

		private void initTexture() {
			//Gl.ActiveTexture(TextureUnit.Texture0);
			Gl.BindTexture(TextureTarget.Texture2d, texId);
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, new int[] { GL_LINEAR });
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, new int[] { GL_LINEAR });
			Gl.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, GL_REPLACE);

			Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, imageWidth, imageHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pImageData);
			//Gl.TexImage2DMultisample(TextureTarget.Texture2dMultisample, 0, InternalFormat.Rgba, imageWidth, imageHeight, true);
			//Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2dMultisample, texId, 0);

			//Gl.GenerateMipmap(TextureTarget.Texture2d);
			//Gl.GetnTexImage(TextureTarget.Texture2d, 2, PixelFormat.Rgba, PixelType.UnsignedByte, 
			//Gl.BindFramebuffer(FramebufferTarget.Framebuffer, texFraBufferId);
			//Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, texFraBufferId, 1);
		}

		public void updateAttr(int _maxRenderTime = -1) {
			if(md.pointCount <= 0) {
				return;
			}
			if(_maxRenderTime > 0) {
				maxRenderTime = _maxRenderTime;
			}

			var rootMd = MainModel.ins.pointEditModel;
			//md.
			Random rand = null;
			if(rootMd.isSeedAuto) {
				rand = new Random();
			} else {
				rand = new Random(rootMd.seed);
			}
			
			int count = _ArrayIndex.Length;
			int idxCount = count / 2;

			const int attrCount = 14;
			
			float[] _arrPointAttr = new float[md.pointCount * attrCount];

			for(int i = 0; i < md.pointCount; ++i) {
				int x = i * 2;
				int y = x + 1;
				float ir0 = (float)rand.NextDouble();
				float ir1 = (float)rand.NextDouble();
				float ir2 = (float)rand.NextDouble();
				float ir3 = (float)rand.NextDouble();
				float ir4 = (float)rand.NextDouble();
				float ir5 = (float)rand.NextDouble();
				float ir6 = (float)rand.NextDouble();
				float ir7 = (float)rand.NextDouble();
				float ir8 = (float)rand.NextDouble();

				//pos
				float px = md.xFloat * (ir0 - 0.5f) * 2;
				float py = md.yFloat * (ir1 - 0.5f) * 2;
				px = md.x + px;
				py = md.y + py;

				//speed
				float speed = md.startSpeed + md.startSpeedFloat * (ir2 - 0.5f) * 2;
				float sAngle = md.startSpeedAngle - 90 + md.startSpeedAngleFloat * (ir3 - 0.5f) * 2;
				float speedx = (float)(speed * Math.Cos(sAngle / 180 * Math.PI));
				float speedy = (float)(speed * Math.Sin(sAngle / 180 * Math.PI));

				//aspeed
				float aSpeed = md.gravityValue;
				float aSAngle = md.gravityAngle - 90;
				float aSpeedx = (float)(aSpeed * Math.Cos(aSAngle / 180 * Math.PI));
				float aSpeedy = (float)(aSpeed * Math.Sin(aSAngle / 180 * Math.PI));

				//size
				float startSize = md.pointStartSize + md.pointStartSizeFloat * (ir4 - 0.5f) * 2;
				startSize = Math.Max(0, startSize);
				float endSize = md.pointEndSize + md.pointEndSizeFloat * (ir5 - 0.5f) * 2;
				endSize = Math.Max(0, endSize);
				endSize = (endSize - startSize) / md.pointLife;
				
				//point angle
				float startAngle = md.pointStartAngle + md.pointStartAngleFloat * (ir6 - 0.5f) * 2;
				float pointRotateSpeed = md.pointRotateSpeed + md.pointRotateSpeedFloat * (ir7 - 0.5f) * 2;
				startAngle = startAngle / 180 * (float)Math.PI;
				pointRotateSpeed = pointRotateSpeed / 180 * (float)Math.PI;

				//alpha
				float startAlpha = Math.Min(1, Math.Max(0, md.startAlpha));
				float endAlpha = Math.Min(1, Math.Max(0, md.endAlpha));
				endAlpha = (endAlpha - startAlpha) / md.pointLife;

				//lifeTime
				float lifeTime = md.pointLife - md.pointLifeFloat * ir8;
				lifeTime *= 1000;

				//startTime
				float startTime = (float)(md.pointCount - i) / md.pointCount * maxRenderTime;

				_arrPointAttr[i * attrCount +  0] = px;
				_arrPointAttr[i * attrCount +  1] = py;
				_arrPointAttr[i * attrCount +  2] = speedx;
				_arrPointAttr[i * attrCount +  3] = speedy;
				_arrPointAttr[i * attrCount +  4] = aSpeedx;
				_arrPointAttr[i * attrCount +  5] = aSpeedy;
				_arrPointAttr[i * attrCount +  6] = startSize;
				_arrPointAttr[i * attrCount +  7] = endSize;
				_arrPointAttr[i * attrCount +  8] = startAngle;
				_arrPointAttr[i * attrCount +  9] = pointRotateSpeed;
				_arrPointAttr[i * attrCount + 10] = startAlpha;
				_arrPointAttr[i * attrCount + 11] = endAlpha;
				_arrPointAttr[i * attrCount + 12] = lifeTime;
				_arrPointAttr[i * attrCount + 13] = startTime;
			}

			pointCount = md.pointCount;
			arrPointAttr = _arrPointAttr;

			if(lockBufferData != null) {
				lockBufferData.Dispose();
			}
			lockBufferData = new MemoryLock(arrPointAttr);

			Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, attrBufferId);
			Gl.BufferData(BufferTarget.ShaderStorageBuffer, (uint)arrPointAttr.Length * sizeof(float), lockBufferData.Address, BufferUsage.DynamicDraw);
		}

		public void render(float[] mMVP, int renderTime) {
			if(!isTextureExist) {
				return;
			}

			if(md.pointCount <= 0) {
				return;
			}

			if(pointCount <= 0) {
				return;
			}

			Gl.UseProgram(ProgramName);
			
			using(MemoryLock lockIndex = new MemoryLock(_ArrayIndex))
			using(MemoryLock lockCoord = new MemoryLock(_ArrayCoord)) {
				//index
				Gl.VertexAttribPointer((uint)LocationIndex, 2, VertexAttribType.Float, false, 0, lockIndex.Address);
				Gl.EnableVertexAttribArray((uint)LocationIndex);

				//cord
				Gl.VertexAttribPointer((uint)LocationCoord, 2, VertexAttribType.Float, false, 0, lockCoord.Address);
				Gl.EnableVertexAttribArray((uint)LocationCoord);

				//MVP
				Gl.UniformMatrix4(LocationMVP, false, mMVP);
				
				//attr
				Gl.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, attrBufferId);
				Gl.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
				
				Gl.Uniform1(LocationNowTime, (float)renderTime);
				Gl.Uniform1(LocationTotalLifeTime, (float)maxRenderTime);
				Gl.Uniform1(LocationPointCount, pointCount);
				//Gl.Uniform1(LocationZIndex, (float)zIndex);
				//Gl.Uniform1(LocationPointCount, pointCount);

				//texture
				Gl.BindTexture(TextureTarget.Texture2d, texId);
				//Gl.GenerateMipmap(TextureTarget.Texture2d);
				Gl.Uniform1(LocationTex, 0);
				
				Gl.DrawArraysInstanced(PrimitiveType.Polygon, 0, _ArrayIndex.Length / 2, pointCount);
			}

		}

		private int getAttrId(string name) {
			int val = 0;
			if((val = Gl.GetAttribLocation(ProgramName, name)) < 0) {
				throw new InvalidOperationException("no attribute " + name);
			}

			return val;
		}

		private int getUniformId(string name) {
			int val = 0;
			if((val = Gl.GetUniformLocation(ProgramName, name)) < 0) {
				throw new InvalidOperationException("no attribute " + name);
			}

			return val;
		}

		private float[] arrPointAttr = new float[0];

		private static readonly float[] _ArrayIndex = new float[] {
			-1,  1,
			-1, -1,
			 1, -1,
			 1,  1,
		};

		private static readonly float[] _ArrayCoord = new float[] {
			0, 1,
			0, 0,
			1, 0,
			1, 1,
		};

		public void Dispose() {
			if(ProgramName > 0) {
				Gl.DeleteProgram(ProgramName);
				ProgramName = 0;
			}

			if(texId > 0) {
				Gl.DeleteTextures(new uint[] { texId });
				texId = 0;
			}
		}

	}
}
