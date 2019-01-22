using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APNGLib {
	public static class APNGAssemblerEx {
		//public static APNG CreateEmpty() {
		//	return new APNG();
		//}

		//public static APNG Create(Stream s) {
		//	APNG apng = new APNG();
		//	apng.Load(s);
		//	return apng;
		//}

		public static void Add(this APNG apngBase, PNG pngFrame, int fps) {
			bool isFirst = apngBase.acTL == null;
			if(isFirst) {
				SetupAPNGChunks(apngBase, pngFrame);
			}

			if(pngFrame.IDATList.Count < 0) {
				return;
			}

			uint sequenceCount = 0;

			if(apngBase.FrameCount == 1) {
				sequenceCount = 1;
			} else if(apngBase.FrameCount > 1) {
				sequenceCount = apngBase.GetFrame(apngBase.FrameCount - 1).fdATs.Last().SequenceNumber + 1;
			}
			//Debug.WriteLine("sequenceCount:" + sequenceCount + "," + pngFrame.IDATList.Count);

			Frame frame = CreateFrame(fps, pngFrame.Height, pngFrame.Width, 0, 0, ref sequenceCount, isFirst, pngFrame.IDATList);
			apngBase.AddFrame(frame);

			apngBase.acTL.NumFrames = (uint)apngBase.FrameCount;
			//apngBase.Validate();
		}

		//public static void Add(this APNG apngBase, IList<PNG> lstPngFrame) {
		//	uint sequenceCount = 0;
			
		//	for(int i = 0; i < lstPngFrame.Count; ++i) {
		//		bool isFirst = apngBase.acTL == null;
		//		if(isFirst) {
		//			SetupAPNGChunks(apngBase, lstPngFrame[i]);
		//		}

		//		Frame frame = CreateFrame(lstPngFrame[i].Height, lstPngFrame[i].Width, 0, 0, ref sequenceCount, true, lstPngFrame[i].IDATList);
		//		apngBase.AddFrame(frame);
		//	}

		//	//apngBase.acTL.NumFrames += (uint)lstPngFrame.Count;
		//	apngBase.acTL.NumFrames = (uint)apngBase.FrameCount;
		//	apngBase.Validate();
		//}

		public static void Save(this APNG apng, string filePath) {
			using(Stream pngStr = apng.ToStream()) {
				pngStr.Position = 0;
				
				using(FileStream fs = File.Create(filePath)) {
					pngStr.CopyTo(fs);
				}
			}
		}

		//public static APNG AssembleAPNG(IList<string> files, bool optimize) {
		//	if(files.Count < 1) {
		//		return null;
		//	}
		//	uint sequenceCount = 0;
		//	APNG apng = new APNG();
		//	PNG first = new PNG();
		//	using(Stream s = File.OpenRead(files.First())) {
		//		first.Load(s);
		//	}
		//	SetupAPNGChunks(apng, first);
		//	Frame firstFrame = CreateFrame(first.Height, first.Width, 0, 0, ref sequenceCount, true, first.IDATList);
		//	apng.AddFrame(firstFrame);

		//	foreach(string file in files.Skip(1)) {
		//		Point p = new Point(0, 0);
		//		PNG png = new PNG();
		//		using(Stream fileStr = File.OpenRead(file)) {
		//			if(optimize) {
		//				using(Stream optStr = OptimizeBitmapStream(fileStr, out p)) {
		//					png.Load(optStr);
		//				}
		//			} else {
		//				png.Load(fileStr);
		//			}
		//		}
		//		Frame f = CreateFrame(png.Height, png.Width, (uint)p.X, (uint)p.Y, ref sequenceCount, false, png.IDATList);
		//		apng.AddFrame(f);
		//	}
		//	apng.acTL.NumFrames = (uint)apng.FrameCount;

		//	apng.Validate();
		//	return apng;
		//}

		//public static APNG AssembleAPNG(IList<FileInfo> files, bool optimize) {
		//	IList<string> filenames = new List<string>();
		//	foreach(FileInfo fi in files) {
		//		filenames.Add(fi.FullName);
		//	}
		//	return AssembleAPNG(filenames, optimize);
		//}

		private static Frame CreateFrame(int fps, uint h, uint w, uint xoff, uint yoff, ref uint seq, bool first, IList<IDATChunk> idats) {
			fcTLChunk fctl = new fcTLChunk() {
				DelayNumerator = 1,
				DelayDenominator = (ushort)fps,
				Height = h,
				Width = w,
				DisposeOperation = 1,
				BlendOperation = 0,
				XOffset = xoff,
				YOffset = yoff,
				SequenceNumber = seq++
			};
			Frame f = new Frame(first, fctl);
			foreach(IDATChunk idat in idats) {
				if(first) {
					f.AddChunk(idat);
				} else {
					fdATChunk fdat = new fdATChunk() {
						FrameData = idat.ImageData,
						SequenceNumber = seq++
					};
					f.AddChunk(fdat);
				}
			}
			return f;
		}

		private static void SetupAPNGChunks(APNG apng, PNG png) {
			apng.IHDR = png.IHDR;
			apng.acTL = new acTLChunk() {
				NumPlays = 0
			};
			foreach(IDATChunk chunk in png.IDATList) {
				apng.IDATList.Add(chunk);
			}
			apng.IEND = png.IEND;
			apng.PLTE = png.PLTE;
			apng.tRNS = png.tRNS;
			apng.cHRM = png.cHRM;
			apng.gAMA = png.gAMA;
			apng.iCCP = png.iCCP;
			apng.sBIT = png.sBIT;
			apng.sRGB = png.sRGB;
			foreach(tEXtChunk chunk in png.tEXtList) {
				apng.tEXtList.Add(chunk);
			}
			foreach(zTXtChunk chunk in png.zTXtList) {
				apng.zTXtList.Add(chunk);
			}
			foreach(iTXtChunk chunk in png.iTXtList) {
				apng.iTXtList.Add(chunk);
			}
			apng.bKGD = png.bKGD;
			apng.hIST = png.hIST;
			apng.pHYs = png.pHYs;
			apng.sPLT = png.sPLT;

			DateTime dt = DateTime.Now;
			apng.tIME = new tIMEChunk() {
				Day = (byte)dt.Day,
				Month = (byte)dt.Month,
				Year = (ushort)dt.Year,
				Hour = (byte)dt.Hour,
				Minute = (byte)dt.Minute,
				Second = (byte)dt.Second
			};
		}

		private static Stream OptimizeBitmapStream(Stream bmStr, out Point p) {
			Bitmap bm = new Bitmap(bmStr);
			Bitmap opt = TrimBitmap(bm, out p);
			Stream ret = new MemoryStream();
			opt.Save(ret, ImageFormat.Png);
			ret.Position = 0;
			return ret;
		}

		private static Bitmap TrimBitmap(Bitmap source, out Point p) {
			Rectangle srcRect = default(Rectangle);
			BitmapData data = null;
			try {
				data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				byte[] buffer = new byte[data.Height * data.Stride];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

				int xMin = int.MaxValue,
					xMax = int.MinValue,
					yMin = int.MaxValue,
					yMax = int.MinValue;

				bool foundPixel = false;

				// Find xMin
				for(int x = 0; x < data.Width; x++) {
					bool stop = false;
					for(int y = 0; y < data.Height; y++) {
						byte alpha = buffer[(y * data.Stride) + (4 * x) + 3];
						if(alpha != 0) {
							xMin = x;
							stop = true;
							foundPixel = true;
							break;
						}
					}
					if(stop) {
						break;
					}
				}
				if(!foundPixel) {
					// Image is empty...
					p = new Point(0, 0);
					return new Bitmap(1, 1);
				}
				// Find yMin
				for(int y = 0; y < data.Height; y++) {
					bool stop = false;
					for(int x = xMin; x < data.Width; x++) {
						byte alpha = buffer[(y * data.Stride) + (4 * x) + 3];
						if(alpha != 0) {
							yMin = y;
							stop = true;
							break;
						}
					}
					if(stop) {
						break;
					}
				}
				// Find xMax
				for(int x = data.Width - 1; x >= xMin; x--) {
					bool stop = false;
					for(int y = yMin; y < data.Height; y++) {
						byte alpha = buffer[(y * data.Stride) + (4 * x) + 3];
						if(alpha != 0) {
							xMax = x;
							stop = true;
							break;
						}
					}
					if(stop) {
						break;
					}
				}
				// Find yMax
				for(int y = data.Height - 1; y >= yMin; y--) {
					bool stop = false;
					for(int x = xMin; x <= xMax; x++) {
						byte alpha = buffer[(y * data.Stride) + (4 * x) + 3];
						if(alpha != 0) {
							yMax = y;
							stop = true;
							break;
						}
					}
					if(stop) {
						break;
					}
				}
				srcRect = Rectangle.FromLTRB(xMin, yMin, xMax, yMax);
				p = new Point(xMin, yMin);
			} finally {
				if(data != null) {
					source.UnlockBits(data);
				}
			}
			return source.Clone(srcRect, source.PixelFormat);
		}
	}
}
