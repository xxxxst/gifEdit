using FreeImageAPI;
using gifEdit.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.control {
	public class ImageDataCtl {
		//public Func<ImageModel> getImageModel = null;
		public Func<int, bool, ImageModel> getImageModelByFrame = null;

		public Bitmap toBitMap(ImageModel md) {
			Bitmap pic = new Bitmap(md.width, md.height, PixelFormat.Format32bppArgb);

			for(int x = 0; x < md.width; x++) {
				for(int y = 0; y < md.height; y++) {
					int idx = (y * md.width + x) * md.pxChannel;
					Color c = Color.FromArgb(
						md.data[idx + 3],
						md.data[idx + 2],
						md.data[idx + 1],
						md.data[idx + 0]
					);
					pic.SetPixel(x, md.height - 1 - y, c);
				}
			}

			return pic;
		}

		public void saveToClipboard() {
			ImageModel md = getImageModelByFrame(-1, true);
			if(md == null) {
				return;
			}

			Bitmap pic = toBitMap(md);
			ClipboardImageCtl.SetClipboardImage(pic, null, null);
		}

		public void saveAsPng(string savePath, int frameIdx) {
			ImageModel md = getImageModelByFrame(frameIdx, true);
			if(md == null) {
				return;
			}

			FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
			IntPtr pFibData = FreeImage.GetBits(dib);
			Marshal.Copy(md.data, 0, pFibData, md.data.Length);

			FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dib, savePath, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
			//FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, ms, FREE_IMAGE_SAVE_FLAGS.DEFAULT);

			//int size = FreeImage.TellMemory(ms);
			//byte[] buffer = new byte[size];
			//FreeImage.SeekMemory(ms, 0, SeekOrigin.Begin);
			//FreeImage.ReadMemory(buffer, (uint)size, (uint)size, ms);

			FreeImage.Unload(dib);
		}

		public void saveAsGif(string savePath, int startFrameIdx, int frameCount, int frameTime) {
			//ImageModel md = getImageModelByFrame(startFrameIdx);
			//if(md == null) {
			//	return;
			//}

			//FIMEMORY ms = new FIMEMORY();
			//FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
			//IntPtr pFibData = FreeImage.GetBits(dib);
			//Marshal.Copy(md.data, 0, pFibData, md.data.Length);
			//FreeImage.SaveToMemory(FREE_IMAGE_FORMAT.FIF_PNG, dib, ms, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
			//FreeImage.SeekMemory(ms, 0, SeekOrigin.Begin);

			//FreeImage.LoadMultiBitmapFromMemory(FREE_IMAGE_FORMAT.FIF_GIF, ms, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
			//FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);

			try {
				//FIMULTIBITMAP mdib = FreeImage.OpenMultiBitmap(FREE_IMAGE_FORMAT.FIF_GIF, savePath, true, false, false, FREE_IMAGE_LOAD_FLAGS.DEFAULT);

				//for(int i = startFrameIdx; i < frameCount; ++i) {
				//	ImageModel md = getImageModelByFrame(i);
				//	if(md == null) {
				//		continue;
				//	}
				//	FIBITMAP dib = FreeImage.Allocate(md.width, md.height, 32, 8, 8, 8);
				//	IntPtr pFibData = FreeImage.GetBits(dib);
				//	Marshal.Copy(md.data, 0, pFibData, md.data.Length);

				//	FITAG tag = FreeImage.CreateTag();
				//	//Debug.WriteLine("aaa:" + FreeImage.GetTagKey(tag));
				//	FreeImage.SetMetadata(FREE_IMAGE_MDMODEL.FIMD_ANIMATION, dib, FreeImage.GetTagKey(tag), tag);

				//	if(tag != null) {
				//		FreeImage.SetTagKey(tag, "FrameTime");
				//		FreeImage.SetTagType(tag, FREE_IMAGE_MDTYPE.FIDT_LONG);
				//		FreeImage.SetTagCount(tag, 1);
				//		FreeImage.SetTagLength(tag, 4);
				//		FreeImage.SetTagValue(tag, BitConverter.GetBytes(frameTime));
				//		FreeImage.SetMetadata(FREE_IMAGE_MDMODEL.FIMD_ANIMATION, dib, FreeImage.GetTagKey(tag), tag);
				//		FreeImage.DeleteTag(tag);
				//	}

				//	FreeImage.AppendPage(mdib, dib);

				//	//FreeImage.Unload(dib);
				//}

				//Debug.WriteLine("bbb:" + FreeImage.GetPageCount(mdib));
				//FreeImage.CloseMultiBitmap(mdib, FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION);

				using(var gif = AnimatedGif.AnimatedGif.Create(savePath, frameTime)) {
					for(int i = startFrameIdx; i < frameCount; ++i) {
						ImageModel md = getImageModelByFrame(i, false);
						if(md == null) {
							continue;
						}

						Bitmap pic = toBitMap(md);
						gif.AddFrame(pic, delay: -1, quality: AnimatedGif.GifQuality.Bit8);
					}
				}

			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

	}
}
