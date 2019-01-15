using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.model {
	public class ImageModel {
		public int width = 0;
		public int height = 0;
		public byte[] data = new byte[0];

		public int pxChannel = 4;
	}
}
