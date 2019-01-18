using gifEdit.model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.control {
	public class ImageDataCtl {
		public Func<ImageModel> getImageModel = null;
		public Func<int, ImageModel> getImageModelByFrame = null;

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
			ImageModel md = getImageModel();
			Bitmap pic = toBitMap(md);
			ClipboardImageCtl.SetClipboardImage(pic, null, null);
		}

		public void saveAsPng(string savePath, int frameIdx) {

		}

		public void saveAsGif(string savePath, int startFrameIdx, int frameCount) {

		}

	}
}
