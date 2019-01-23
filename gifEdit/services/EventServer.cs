using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.services {

	public class EventServer {
		public static EventServer ins = new EventServer();

		public event Action mainWinExited;
		public void onMainWinExited() {
			mainWinExited?.Invoke();
		}

		public event Action pointEngineInited;
		public void onPointEngineInited() {
			pointEngineInited?.Invoke();
		}

		public event Action<double> updateFPS;
		public void onUpdateFPSEvent(double fps) {
			updateFPS?.Invoke(fps);
		}

		public event Action copyToClipboard;
		public void onCopyToClipboard() {
			copyToClipboard?.Invoke();
		}

		public event Action preOpenExportWin;
		public void onPreOpenExportWin() {
			preOpenExportWin?.Invoke();
		}

		public event Action CloseExportWin;
		public void onCloseExportWin() {
			CloseExportWin?.Invoke();
		}
	}
}
