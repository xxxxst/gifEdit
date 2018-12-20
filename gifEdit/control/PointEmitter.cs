using FreeImageAPI;
using gifEdit.control.glEngine;
using gifEdit.model;
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
		uint texId = 0;

		private uint ProgramName;
		private int LocationMVP;
		private int LocationRenderIdx;
		private int LocationIndex;
		//private int LocationColor;
		private int LocationCoord;
		private int LocationTex;

		public PointEmitter(PointResourceModel _md) {
			md = _md;

			texId = Gl.GenTexture();

			using(GlObject vObject = new GlObject(ShaderType.VertexShader, _VertexSourceGL))
			using(GlObject fObject = new GlObject(ShaderType.FragmentShader, _FragmentSourceGL)) {
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
				LocationRenderIdx = getUniformId("aRenderIdx");
				LocationIndex = getAttrId("aIndex");
				LocationCoord = getAttrId("aCoord");
				LocationTex = getUniformId("tex");
			}

			udpateImage();
		}

		public void udpateImage() {
			var rootMd = MainModel.ins.pointEditModel;

			string path = md.path;
			if(path.Length < 2 || path[1] != ':') {
				path = rootMd.path + "/" + path;
			}
			//Debug.WriteLine(path);

			//string path = @"E:\00workself\csharp\project\gifEdit\gifEdit\resource\image\snow.png";
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
			var rootMd = MainModel.ins.pointEditModel;
			//md.
			var rand = new Random(rootMd.seed);

			//rand.NextDouble();
		}

		public void render(float[] mMVP, int renderIdx) {
			if(!isTextureExist) {
				return;
			}

			Gl.UseProgram(ProgramName);

			//for(int i = 0; i < _ArrayIndex.Length; i += 2) {
			//	_ArrayIndex[i] += 1f;
			//}

			using(MemoryLock vertexIndexLock = new MemoryLock(_ArrayIndex))
			//using(MemoryLock vertexColorLock = new MemoryLock(_ArrayColor))
			using(MemoryLock vertexCoordLock = new MemoryLock(_ArrayCoord)) {
				//Gl.VertexPointer(2, VertexPointerType.Float, 0, vertexArrayLock.Address);
				//Gl.EnableClientState(EnableCap.VertexArray);

				Gl.VertexAttribPointer((uint)LocationIndex, 2, VertexAttribType.Float, false, 0, vertexIndexLock.Address);
				Gl.EnableVertexAttribArray((uint)LocationIndex);

				Gl.VertexAttribPointer((uint)LocationCoord, 2, VertexAttribType.Float, false, 0, vertexCoordLock.Address);
				Gl.EnableVertexAttribArray((uint)LocationCoord);

				Gl.UniformMatrix4(LocationMVP, false, mMVP);

				//float val = (float)renderIdx;
				//Gl.Uniform1f(LocationRenderIdx, 1, ref val);
				Gl.Uniform1(LocationRenderIdx, (float)renderIdx);

				Gl.BindTexture(TextureTarget.Texture2d, texId);
				Gl.Uniform1(LocationTex, 0);

				Gl.DrawArrays(PrimitiveType.Polygon, 0, _ArrayIndex.Length / 2);
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

		private static readonly string[] _VertexSourceGL = {
			//"#version 150 compatibility\n",
			"uniform mat4 uMVP;\n",
			"uniform float aRenderIdx;\n",
			"in vec2 aIndex;\n",
			"in vec2 aCoord;\n",
			"varying vec2 vTexCoord;\n",
			"void main() {\n",
			"	vec4 pos = vec4(aIndex, 0.0, 1.0);\n",
			//"	gl_Position.x += aRenderIdx * 0.1;\n",
			"	pos.x += aRenderIdx * 0.8;\n",
			"	pos = uMVP * pos;\n",
			"	gl_Position = pos;\n",
			"	vTexCoord = aCoord;\n",
			"}\n"
		};

		private static readonly string[] _FragmentSourceGL = {
			//"#version 150 compatibility\n",
			"varying vec2 vTexCoord;\n",
			"uniform sampler2D tex;\n",
			"void main() {\n",
			//"	gl_FragColor = vColor;\n",
			"	gl_FragColor = texture(tex, vTexCoord);\n",
			//"	discard;",
			"}\n"
		};

		private static float[] _ArrayIndex = new float[] {
			0, 64,
			0, 0,
			64, 0,
			64, 64,
		};

		private static float[] _ArrayColor = new float[] {
			0, 0, 0,
			1, 0, 0,
			0, 1, 0,
			0, 0, 1,
		};

		private static float[] _ArrayCoord = new float[] {
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
