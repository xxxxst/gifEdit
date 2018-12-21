using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.control.glEngine {
	public class GlTime {
		private long oldTime = 0;

		//private int idx = 0;
		public float getTime() {
			//++idx;
			//if(idx < 10) {
			//	return 0;
			//}
			//idx = 0;

			long time = DateTime.Now.ToFileTimeUtc();

			float rst = 0;
			if(oldTime != 0) {
				rst = (float)(time - oldTime) / (1000 * 10);
			}
			oldTime = time;

			return rst;
		}

		public void clear() {
			oldTime = 0;
		}

	}
}
