using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gifEdit.control {
	public class ClipboardImageCtl {

		/// <summary>
		/// Copies the given image to the clipboard as PNG, DIB and standard Bitmap format.
		/// </summary>
		/// <param name="image">Image to put on the clipboard.</param>
		/// <param name="imageNoTr">Optional specifically nontransparent version of the image to put on the clipboard.</param>
		/// <param name="data">Clipboard data object to put the image into. Might already contain other stuff. Leave null to create a new one.</param>
		public static void SetClipboardImage(Bitmap image, Bitmap imageNoTr = null, DataObject data = null) {
			Clipboard.Clear();
			if(data == null) {
				data = new DataObject();
			}
			if(imageNoTr == null) {
				imageNoTr = image;
			}
			using(MemoryStream pngMemStream = new MemoryStream())
			using(MemoryStream dibMemStream = new MemoryStream()) {
				// As standard bitmap, without transparency support
				data.SetData(DataFormats.Bitmap, imageNoTr, true);
				// As PNG. Gimp will prefer this over the other two.
				image.Save(pngMemStream, ImageFormat.Png);
				data.SetData("PNG", pngMemStream, false);
				// As DIB. This is (wrongly) accepted as ARGB by many applications.
				Byte[] dibData = ConvertToDib(image);
				dibMemStream.Write(dibData, 0, dibData.Length);
				data.SetData(DataFormats.Dib, dibMemStream, false);
				// The 'copy=true' argument means the MemoryStreams can be safely disposed after the operation.
				Clipboard.SetDataObject(data, true);
			}
		}

		/// <summary>
		/// Converts the image to Device Independent Bitmap format of type BITFIELDS.
		/// This is (wrongly) accepted by many applications as containing transparency,
		/// so I'm abusing it for that.
		/// </summary>
		/// <param name="image">Image to convert to DIB</param>
		/// <returns>The image converted to DIB, in bytes.</returns>
		public static Byte[] ConvertToDib(Image image) {
			Byte[] bm32bData;
			Int32 width = image.Width;
			Int32 height = image.Height;
			// Ensure image is 32bppARGB by painting it on a new 32bppARGB image.
			using(Bitmap bm32b = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb)) {
				using(Graphics gr = Graphics.FromImage(bm32b))
					gr.DrawImage(image, new Rectangle(0, 0, bm32b.Width, bm32b.Height));
				// Bitmap format has its lines reversed.
				bm32b.RotateFlip(RotateFlipType.Rotate180FlipX);
				Int32 stride;
				bm32bData = GetImageData(bm32b, out stride);
			}
			// BITMAPINFOHEADER struct for DIB.
			Int32 hdrSize = 0x28;
			Byte[] fullImage = new Byte[hdrSize + 12 + bm32bData.Length];
			//Int32 biSize;
			WriteIntToByteArray(fullImage, 0x00, 4, true, (UInt32)hdrSize);
			//Int32 biWidth;
			WriteIntToByteArray(fullImage, 0x04, 4, true, (UInt32)width);
			//Int32 biHeight;
			WriteIntToByteArray(fullImage, 0x08, 4, true, (UInt32)height);
			//Int16 biPlanes;
			WriteIntToByteArray(fullImage, 0x0C, 2, true, 1);
			//Int16 biBitCount;
			WriteIntToByteArray(fullImage, 0x0E, 2, true, 32);
			//BITMAPCOMPRESSION biCompression = BITMAPCOMPRESSION.BITFIELDS;
			WriteIntToByteArray(fullImage, 0x10, 4, true, 3);
			//Int32 biSizeImage;
			WriteIntToByteArray(fullImage, 0x14, 4, true, (UInt32)bm32bData.Length);
			// These are all 0. Since .net clears new arrays, don't bother writing them.
			//Int32 biXPelsPerMeter = 0;
			//Int32 biYPelsPerMeter = 0;
			//Int32 biClrUsed = 0;
			//Int32 biClrImportant = 0;

			// The aforementioned "BITFIELDS": colour masks applied to the Int32 pixel value to get the R, G and B values.
			WriteIntToByteArray(fullImage, hdrSize + 0, 4, true, 0x00FF0000);
			WriteIntToByteArray(fullImage, hdrSize + 4, 4, true, 0x0000FF00);
			WriteIntToByteArray(fullImage, hdrSize + 8, 4, true, 0x000000FF);
			Array.Copy(bm32bData, 0, fullImage, hdrSize + 12, bm32bData.Length);
			return fullImage;
		}

		/// <summary>
		/// Gets the raw bytes from an image.
		/// </summary>
		/// <param name="sourceImage">The image to get the bytes from.</param>
		/// <param name="stride">Stride of the retrieved image data.</param>
		/// <returns>The raw bytes of the image</returns>
		public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride) {
			BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, sourceImage.PixelFormat);
			stride = sourceData.Stride;
			Byte[] data = new Byte[stride * sourceImage.Height];
			Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
			sourceImage.UnlockBits(sourceData);
			return data;
		}

		public static void WriteIntToByteArray(Byte[] data, Int32 startIndex, Int32 bytes, Boolean littleEndian, UInt32 value) {
			Int32 lastByte = bytes - 1;
			if(data.Length < startIndex + bytes)
				throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to write a " + bytes + "-byte value at offset " + startIndex + ".");
			for(Int32 index = 0; index < bytes; index++) {
				Int32 offs = startIndex + (littleEndian ? index : lastByte - index);
				data[offs] = (Byte)(value >> (8 * index) & 0xFF);
			}
		}

		public static UInt32 ReadIntFromByteArray(Byte[] data, Int32 startIndex, Int32 bytes, Boolean littleEndian) {
			Int32 lastByte = bytes - 1;
			if(data.Length < startIndex + bytes)
				throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to read a " + bytes + "-byte value at offset " + startIndex + ".");
			UInt32 value = 0;
			for(Int32 index = 0; index < bytes; index++) {
				Int32 offs = startIndex + (littleEndian ? index : lastByte - index);
				value += (UInt32)(data[offs] << (8 * index));
			}
			return value;
		}
	}
}
