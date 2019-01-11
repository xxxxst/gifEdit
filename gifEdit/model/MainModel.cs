using csharpHelp.services;
using gifEdit.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.model {
	public class MainModel {
		public static MainModel ins = new MainModel();

		public ConfigModel configModel = new ConfigModel();

		public ParticleEditModel particleEditModel = null;

		public MainWindow mainWin = null;
	}

	[XmlRoot("gifEdit")]
	public class ConfigModel {
		public XmlModelServer srv = new XmlModelServer();
		public BridgeServer brgSrv = new BridgeServer();

		[XmlAttr("win.x")]		[Bridge("Left")]	public int x = 100;
		[XmlAttr("win.y")]		[Bridge("Top")]		public int y = 100;
		[XmlAttr("win.width")]	[Bridge("Width")]	public int width = 1000;
		[XmlAttr("win.height")]	[Bridge("Height")]	public int height = 670;

		public int maxParticleCount = 100 * 1000;
		//public int maxParticleCount = 10000 * 1000;

		[XmlListChild("lastProjectBox.project")]	public List<LastProject> lstLastProject = new List<LastProject>();
	}

	public class LastProject {
		[XmlAttr("name")]			public string name = "";
		[XmlAttr("type")]			public PrjoectType type = PrjoectType.Spirit;
		[XmlAttr("lastOpenTime")]	public string lastOpenTime = "";
		[XmlAttr("path")]			public string path = "";
	}

	public enum PrjoectType {
		Spirit, Particle
	}

}
