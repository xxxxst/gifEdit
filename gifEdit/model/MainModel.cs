using csharpHelp.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.model {
	public class MainModel {
		public static MainModel ins = new MainModel();

		public ConfigModel configModel = new ConfigModel();

		public PointEditModel pointEditModel = null;
	}

	[XmlRoot("gifEdit")]
	public class ConfigModel {
		public XmlModelServer srv = new XmlModelServer();
		public BridgeServer brgSrv = new BridgeServer();

		[XmlAttr("win.x")]		[Bridge("Left")]	public int x = 100;
		[XmlAttr("win.y")]		[Bridge("Top")]		public int y = 100;
		[XmlAttr("win.width")]	[Bridge("Width")]	public int width = 800;
		[XmlAttr("win.height")]	[Bridge("Height")]	public int height = 450;
	}
	
}
