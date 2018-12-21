using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.control.glEngine {
	public class FpsCtl {
		private int fps = 0;
		private long lastTime = 0;
		private int count = 0;

		private int waitIdx = 0;
		public void update() {
			const long maxTime = 5 * 1000 * 1000 * 10;
			const int second = 1000 * 1000 * 10;

			++count;

			++waitIdx;
			if(waitIdx < 20) {
				return;
			}
			waitIdx = 0;

			long time = DateTime.Now.ToFileTimeUtc();

			if(lastTime == 0) {
				lastTime = time;
				return;
			}

			long gap = time - lastTime;
			if(gap == 0) {
				return;
			}

			//++count;
			//fps = (int)(count / ((float)gap / second));
			//lastTime = time;
			//count = 0;

			if(gap >= maxTime) {
				//long gap = time - lastTime - maxTime;
				//fps = (int)Math.Round((float)maxTime / gap * fps);
				count = (int)((float)(maxTime - second) / gap * count);
				fps = (int)(count / ((float)(maxTime - second) / second));

				lastTime = time - (maxTime - second);
			} else {
				fps = (int)(count / ((float)gap / second));
			}
		}

		public int getFps() {
			return fps;
		}
	}
}
