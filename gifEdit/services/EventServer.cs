﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.services {

	public class EventServer {
		public static EventServer ins = new EventServer();

		public event Action mainWinExitedEvent;
		public void onMainWinExited() {
			mainWinExitedEvent?.Invoke();
		}
	}
}