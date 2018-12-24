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

		private IntPtr pImageData = IntPtr.Zero;
		private int imageWidth = 0;
		private int imageHeight = 0;

		//private float[] mMVP = null;
		uint bufferId = 0;
		uint texId = 0;

		private uint ProgramName;
		private int LocationMVP;
		private int LocationTime;
		//private int LocationPointIndex;
		private int LocationIndex;
		//private int LocationColor;
		private int LocationCoord;
		private int LocationTex;

		//private int LocationPointAttr;
		private int LocationPos;
		private int LocationPointSpeed;
		private int LocationASpeed;

		private int LocationStartSize;
		private int LocationSizeSpeed;

		private int LocationStartAngle;
		private int LocationPointRotateSpeed;

		private int LocationStartAlpha;
		private int LocationAlphaSpeed;

		//private MemoryLock lockBufferData = null;

		public PointEmitter(PointResourceModel _md) {
			md = _md;

			bufferId = Gl.GenBuffer();
			texId = Gl.GenTexture();

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
				LocationTime = getUniformId("aTime");
				//LocationPointIndex = getAttrId("pointIndex");
				LocationIndex = getAttrId("aIndex");
				LocationCoord = getAttrId("aCoord");
				LocationTex = getUniformId("tex");

				//LocationPointAttr = getUniformId("pointAttrs");
				//LocationPointAttr = getAttrId("pointAttrs");
				LocationPos = getAttrId("pos");
				LocationPointSpeed = getAttrId("speed");
				LocationASpeed = getAttrId("aSpeed");
				LocationStartSize = getAttrId("startSize");
				LocationSizeSpeed = getAttrId("sizeSpeed");
				LocationStartAngle = getAttrId("startAngle");
				LocationPointRotateSpeed = getAttrId("pointRotateSpeed");
				LocationStartAlpha = getAttrId("startAlpha");
				LocationAlphaSpeed = getAttrId("alphaSpeed");
			}

			udpateImage();
			updateAttr();
		}

		public void udpateImage() {
			var rootMd = MainModel.ins.pointEditModel;

			string path = md.path;
			if(path.Length < 2 || path[1] != ':') {
				path = rootMd.path + "/" + path;
			}
			//Debug.WriteLine(path);

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

			//var dib2 = FreeImage.Rescale(dib, 64, 64, FREE_IMAGE_FILTER.FILTER_BILINEAR);
			var bits = FreeImage.GetBits(dib);
			pImageData = bits;

			initTexture();
		}

		public bool isTextureExist{
			get { return pImageData != IntPtr.Zero; }
		}

		private void initTexture() {
			//const int GL_NEAREST = 0x2600;
			const int GL_LINEAR = 0x2601;
			const int GL_REPEAT = 0x2901;
			const int GL_REPLACE = 0x1E01;

			//Gl.ActiveTexture(TextureUnit.Texture0);
			Gl.BindTexture(TextureTarget.Texture2d, texId);
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, new int[] { GL_REPEAT });
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, new int[] { GL_LINEAR });
			Gl.TexParameterI(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, new int[] { GL_LINEAR });
			Gl.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, GL_REPLACE);

			Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, imageWidth, imageHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pImageData);
		}

		public void updateAttr() {
			if(md.pointCount <= 0) {
				return;
			}

			var rootMd = MainModel.ins.pointEditModel;
			//md.
			Random rand = null;
			if(rootMd.isSeedAuto) {
				rand = new Random();
			} else {
				rand = new Random(rootMd.seed);
			}

			//x, y, gravity, gravityAngle, startSpeed
			float[] arrAttr = new float[] {
				0
			};
			int count = _ArrayIndex.Length;
			int idxCount = count / 2;
			//int[] _arrPointIndex = new int[md.pointCount * idxCount];	//(idx1, idx2, idx3, idx4)
			//float[] _arrIndex = new float[md.pointCount * count];		//(x1,y1, x2,y2, x3,y3, x4,y4)
			//float[] _arrCoord = new float[md.pointCount * count];		//(x1,y1, x2,y2, x3,y3, x4,y4)

			float[] _arrPos = new float[md.pointCount * 2];				//(x,y)
			float[] _arrPointSpeed = new float[md.pointCount * 2];		//(x,y)
			float[] _arrASpeed = new float[md.pointCount * 2];			//(ax,ay)
			float[] _arrStartSize = new float[md.pointCount];			//size
			float[] _arrSizeSpeed = new float[md.pointCount];			//size speed
			float[] _arrStartAngle = new float[md.pointCount];			//angle
			float[] _arrPointRotateSpeed = new float[md.pointCount];    //point rotate speed
			float[] _arrStartAlpha = new float[md.pointCount];          //alpha
			float[] _arrAlphaSpeed = new float[md.pointCount];          //alpha speed

			//float[] _arrPointAttr = new float[md.pointCount * 2 * 3];		//

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

				//for(int j = 0; j < idxCount; ++j) {
				//	_arrPointIndex[i * idxCount + j] = i;
				//}
				//_ArrayIndex.CopyTo(_arrIndex, i * count);
				//_ArrayCoord.CopyTo(_arrCoord, i * count);

				//pos
				float px = md.xFloat * (ir0 - 0.5f) * 2;
				float py = md.yFloat * (ir1 - 0.5f) * 2;
				_arrPos[x] = md.x + px;
				_arrPos[y] = md.y + py;
				//Debug.WriteLine("aa:" + _arrPos[x] + "," + _arrPos[y]);

				//speed
				float speed = md.startSpeed + md.startSpeedFloat * (ir2 - 0.5f) * 2;
				float sAngle = md.startSpeedAngle - 90 + md.startSpeedAngleFloat * (ir3 - 0.5f) * 2;
				_arrPointSpeed[x] = (float)(speed * Math.Cos(sAngle / 180 * Math.PI));
				_arrPointSpeed[y] = (float)(speed * Math.Sin(sAngle / 180 * Math.PI));

				//aspeed
				float aSpeed = md.gravityValue;
				float aSAngle = md.gravityAngle - 90;
				_arrASpeed[x] = (float)(aSpeed * Math.Cos(aSAngle / 180 * Math.PI));
				_arrASpeed[y] = (float)(aSpeed * Math.Sin(aSAngle / 180 * Math.PI));

				//size
				_arrStartSize[i] = md.pointStartSize + md.pointStartSizeFloat * (ir4 - 0.5f) * 2;
				_arrStartSize[i] = Math.Max(0, _arrStartSize[i]);
				float endSize = md.pointEndSize + md.pointEndSizeFloat * (ir5 - 0.5f) * 2;
				endSize = Math.Max(0, endSize);
				_arrSizeSpeed[i] = (endSize - _arrStartSize[i]) / md.pointLife;
				
				//point angle
				float startAngle = md.pointStartAngle + md.pointStartAngleFloat * (ir6 - 0.5f) * 2;
				float pointRotateSpeed = md.pointRotateSpeed + md.pointRotateSpeedFloat * (ir7 - 0.5f) * 2;
				_arrStartAngle[i] = startAngle / 180 * (float)Math.PI;
				_arrPointRotateSpeed[i] = pointRotateSpeed / 180 * (float)Math.PI;
				//Debug.WriteLine("aa:" + _arrPointRotateSpeed[i]);

				//alpha
				_arrStartAlpha[i] = Math.Min(1, Math.Max(0, md.startAlpha));
				float endAlpha = Math.Min(1, Math.Max(0, md.endAlpha));
				_arrAlphaSpeed[i] = (endAlpha - _arrStartAlpha[i]) / md.pointLife;
			}

			//arrPointIndex = _arrPointIndex;
			//arrIndex = _arrIndex;
			//arrCoord = _arrCoord;

			//for(int i = 0; i < md.pointCount; ++i) {
			//	_arrPointAttr[i * 2 * 3 + 0] = _arrPos[i * 2 + 0];
			//	_arrPointAttr[i * 2 * 3 + 1] = _arrPos[i * 2 + 1];
			//	_arrPointAttr[i * 2 * 3 + 2] = _arrPointSpeed[i * 2 + 1];
			//	_arrPointAttr[i * 2 * 3 + 3] = _arrPointSpeed[i * 2 + 1];
			//	_arrPointAttr[i * 2 * 3 + 4] = _arrASpeed[i * 2 + 1];
			//	_arrPointAttr[i * 2 * 3 + 5] = _arrASpeed[i * 2 + 1];
			//}

			arrPos = _arrPos;
			arrPointSpeed = _arrPointSpeed;
			arrASpeed = _arrASpeed;
			//arrPointAttr = _arrPointAttr;
			arrStartSize = _arrStartSize;
			arrSizeSpeed = _arrSizeSpeed;
			arrStartAngle = _arrStartAngle;
			arrPointRotateSpeed = _arrPointRotateSpeed;
			arrStartAlpha = _arrStartAlpha;
			arrAlphaSpeed = _arrAlphaSpeed;

			//if(lockBufferData != null) {
			//	lockBufferData.Dispose();
			//}
			//lockBufferData = new MemoryLock(arrPointAttr);

			//Debug.WriteLine(Gl.CurrentVersion);
			//Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
			//Gl.BufferData(BufferTarget.ShaderStorageBuffer, (uint)arrPointAttr.Length, lockBufferData.Address, BufferUsage.DynamicDraw);
			//Gl.BufferData(BufferTarget.ArrayBuffer, (uint)arrPointAttr.Length, lockBufferData.Address, BufferUsage.DynamicDraw);
			//Gl.BindVertexArray(0);

			//var lst = rootMd.lstResource;
			//for(int i = 0; i < lst.Count; ++i) {

			//}
			//rand.NextDouble();
		}

		public void render(float[] mMVP, int renderTime) {
			if(!isTextureExist) {
				return;
			}

			if(md.pointCount <= 0) {
				return;
			}

			if(arrPos.Length <= 0) {
				return;
			}

			//if(arrPointAttr.Length <= 0) {
			//	return;
			//}

			Gl.UseProgram(ProgramName);

			//for(int i = 0; i < _ArrayIndex.Length; i += 2) {
			//	_ArrayIndex[i] += 1f;
			//}

			//Gl.PrimitiveRestartIndex(4);

			//using(MemoryLock lockPointIndex = new MemoryLock(arrPointIndex))
			using(MemoryLock lockIndex = new MemoryLock(_ArrayIndex))
			//using(MemoryLock vertexColorLock = new MemoryLock(_ArrayColor))
			using(MemoryLock lockCoord = new MemoryLock(_ArrayCoord))
			using(MemoryLock lockPos = new MemoryLock(arrPos))
			using(MemoryLock lockPointSpeed = new MemoryLock(arrPointSpeed))
			using(MemoryLock lockASpeed = new MemoryLock(arrASpeed))
			using(MemoryLock lockStartSize = new MemoryLock(arrStartSize))
			using(MemoryLock lockSizeSpeed = new MemoryLock(arrSizeSpeed))
			using(MemoryLock lockStartAngle = new MemoryLock(arrStartAngle))
			using(MemoryLock lockPointRotateSpeed = new MemoryLock(arrPointRotateSpeed))
			using(MemoryLock lockStartAlpha = new MemoryLock(arrStartAlpha))
			using(MemoryLock lockAlphaSpeed = new MemoryLock(arrAlphaSpeed))
			//using(MemoryLock lockBufferData = new MemoryLock(arrPos))
			{
				//Gl.VertexPointer(2, VertexPointerType.Float, 0, vertexArrayLock.Address);
				//Gl.EnableClientState(EnableCap.VertexArray);

				//Gl.VertexAttribPointer((uint)LocationPointIndex, 1, VertexAttribType.Int, false, 0, lockPointIndex.Address);
				//Gl.EnableVertexAttribArray((uint)LocationPointIndex);
				//Gl.VertexAttribDivisor((uint)LocationPointIndex, 1);

				Gl.VertexAttribPointer((uint)LocationIndex, 2, VertexAttribType.Float, false, 0, lockIndex.Address);
				Gl.EnableVertexAttribArray((uint)LocationIndex);
				//Gl.VertexAttribDivisor((uint)LocationIndex, 1);

				Gl.VertexAttribPointer((uint)LocationCoord, 2, VertexAttribType.Float, false, 0, lockCoord.Address);
				Gl.EnableVertexAttribArray((uint)LocationCoord);
				//Gl.VertexAttribDivisor((uint)LocationCoord, 1);

				Gl.UniformMatrix4(LocationMVP, false, mMVP);

				//Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
				//Gl.Uniform1(LocationPointAttr, 0);
				//Gl.VertexAttribPointer((uint)LocationPointAttr, 2, VertexAttribType.Float, false, 0, 0);
				//Gl.EnableVertexAttribArray((uint)LocationPointAttr);

				//Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
				//Gl.BufferData(BufferTarget.ArrayBuffer, (uint)arrPointAttr.Length, lockBufferData.Address, BufferUsage.DynamicDraw);
				////Gl.BufferData(BufferTarget.ShaderStorageBuffer, (uint)arrPointAttr.Length, lockBufferData.Address, BufferUsage.DynamicDraw);
				//Gl.BindBufferRange(BufferTarget.ShaderStorageBuffer, (uint)0, bufferId, lockBufferData.Address, (uint)arrPointAttr.Length);
				//Gl.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
				//Gl.VertexAttribDivisor((uint)LocationPointAttr, 1);
				//Gl.VertexAttribPointer((uint)LocationPointAttr, 6, VertexAttribType.Float, false, 0, lockBufferData.Address);
				//Gl.EnableVertexAttribArray((uint)LocationPointAttr);

				//pos
				Gl.VertexAttribDivisor((uint)LocationPos, 1);
				Gl.VertexAttribPointer((uint)LocationPos, 2, VertexAttribType.Float, false, 0, lockPos.Address);
				Gl.EnableVertexAttribArray((uint)LocationPos);

				//speed aspeed
				Gl.VertexAttribDivisor((uint)LocationPointSpeed, 1);
				Gl.VertexAttribPointer((uint)LocationPointSpeed, 2, VertexAttribType.Float, false, 0, lockPointSpeed.Address);
				Gl.EnableVertexAttribArray((uint)LocationPointSpeed);

				Gl.VertexAttribDivisor((uint)LocationASpeed, 1);
				Gl.VertexAttribPointer((uint)LocationASpeed, 2, VertexAttribType.Float, false, 0, lockASpeed.Address);
				Gl.EnableVertexAttribArray((uint)LocationASpeed);

				//size
				Gl.VertexAttribDivisor((uint)LocationStartSize, 1);
				Gl.VertexAttribPointer((uint)LocationStartSize, 1, VertexAttribType.Float, false, 0, lockStartSize.Address);
				Gl.EnableVertexAttribArray((uint)LocationStartSize);

				Gl.VertexAttribDivisor((uint)LocationSizeSpeed, 1);
				Gl.VertexAttribPointer((uint)LocationSizeSpeed, 1, VertexAttribType.Float, false, 0, lockSizeSpeed.Address);
				Gl.EnableVertexAttribArray((uint)LocationSizeSpeed);

				//point angle
				Gl.VertexAttribDivisor((uint)LocationStartAngle, 1);
				Gl.VertexAttribPointer((uint)LocationStartAngle, 1, VertexAttribType.Float, false, 0, lockStartAngle.Address);
				Gl.EnableVertexAttribArray((uint)LocationStartAngle);

				Gl.VertexAttribDivisor((uint)LocationPointRotateSpeed, 1);
				Gl.VertexAttribPointer((uint)LocationPointRotateSpeed, 1, VertexAttribType.Float, false, 0, lockPointRotateSpeed.Address);
				Gl.EnableVertexAttribArray((uint)LocationPointRotateSpeed);

				//alpha
				Gl.VertexAttribDivisor((uint)LocationStartAlpha, 1);
				Gl.VertexAttribPointer((uint)LocationStartAlpha, 1, VertexAttribType.Float, false, 0, lockStartAlpha.Address);
				Gl.EnableVertexAttribArray((uint)LocationStartAlpha);

				Gl.VertexAttribDivisor((uint)LocationAlphaSpeed, 1);
				Gl.VertexAttribPointer((uint)LocationAlphaSpeed, 1, VertexAttribType.Float, false, 0, lockAlphaSpeed.Address);
				Gl.EnableVertexAttribArray((uint)LocationAlphaSpeed);

				//float val = (float)renderIdx;
				//Gl.Uniform1f(LocationRenderIdx, 1, ref val);
				Gl.Uniform1(LocationTime, (float)renderTime);

				Gl.BindTexture(TextureTarget.Texture2d, texId);
				Gl.Uniform1(LocationTex, 0);

				//Gl.DrawArrays(PrimitiveType.Polygon, 0, _ArrayIndex.Length / 2);
				//Gl.DrawArrays(PrimitiveType.Polygon, 0, arrIndex.Length / 2);
				Gl.DrawArraysInstanced(PrimitiveType.Polygon, 0, _ArrayIndex.Length / 2, arrPos.Length / 2);
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

		//private static readonly string[] _VertexSourceGL = {
		//	//"#version 150 compatibility\n",
		//	"uniform mat4 uMVP;\n",
		//	"in vec2 aIndex;\n",
		//	"in vec2 aCoord;\n",
		//	"uniform float aTime;\n",

		//	//"in vec2 pos;\n",
		//	//"in float startSpeed;\n",
		//	//"in float startAngle;\n",

		//	"varying vec2 vTexCoord;\n",
		//	"void main() {\n",
		//	"	vec4 pos = vec4(aIndex, 0.0, 1.0);\n",
		//	//"	gl_Position.x += aTime * 0.1;\n",
		//	"	pos.x += aTime * 0.04;\n",
		//	"	pos = uMVP * pos;\n",
		//	"	gl_Position = pos;\n",
		//	"	vTexCoord = aCoord;\n",
		//	"}\n"
		//};

		//private static readonly string[] _FragmentSourceGL = {
		//	//"#version 150 compatibility\n",
		//	"varying vec2 vTexCoord;\n",
		//	"uniform sampler2D tex;\n",
		//	"void main() {\n",
		//	//"	gl_FragColor = vColor;\n",
		//	"	gl_FragColor = texture(tex, vTexCoord);\n",
		//	//"	discard;",
		//	"}\n"
		//};

		//private int[] arrPointIndex = new int[0];
		//private float[] arrIndex = new float[0];
		//private float[] arrCoord = new float[0];

		//private float[] arrPointAttr = new float[0];
		private float[] arrPos = new float[0];
		private float[] arrPointSpeed = new float[0];
		private float[] arrASpeed = new float[0];
		private float[] arrStartSize = new float[0];
		private float[] arrSizeSpeed = new float[0];
		private float[] arrStartAngle = new float[0];
		private float[] arrPointRotateSpeed = new float[0];
		private float[] arrStartAlpha = new float[0];
		private float[] arrAlphaSpeed = new float[0];

		private static readonly float[] _ArrayIndex = new float[] {
			-1,  1,
			-1, -1,
			 1, -1,
			 1,  1,
		};

		//private static readonly float[] _ArrayColor = new float[] {
		//	0, 0, 0,
		//	1, 0, 0,
		//	0, 1, 0,
		//	0, 0, 1,
		//};

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
