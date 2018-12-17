using csharpHelp.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.model {
	[XmlRoot("gifEdit")]
	public class PointEditModel {
		public XmlModelServer srv = null;

		public string path = "";
		public string name = "";

		[XmlAttr("dataVersion")]			public string dataVersion = "1.0.0";

		[XmlValue("config.background")]		public string background = "#4d4d4d";
		[XmlValue("config.seed")]			public int seed = 0;
		[XmlValue("config.isSeedAuto")]		public bool isSeedAuto = true;
		[XmlValue("config.fps")]			public int fps = 60;
		[XmlValue("config.exportPath")]		public string exportPath = "";
		[XmlValue("config.width")]			public int width = 512;
		[XmlValue("config.height")]			public int height = 512;

		[XmlListChild("resourceBox.res")]	public List<PointResourceModel> lstResource = new List<PointResourceModel>();
	}

	public class PointResourceModel {
		[XmlAttr("path")]					public string path = "";
											
		[XmlValue("x")]						public double x = 0;
		[XmlValue("xFloat")]				public double xFloat = 0;
											
		[XmlValue("y")]						public double y = 0;
		[XmlValue("yFloat")]				public double yFloat = 0;
											
		[XmlValue("gravityAngle")]			public double gravityAngle = 0;
		[XmlValue("gravityValue")]			public double gravityValue = 0;

		[XmlValue("startSpeed")]			public double startSpeed = 0;
		[XmlValue("startSpeedFloat")]		public double startSpeedFloat = 0;

		[XmlValue("startSpeedAngle")]		public double startSpeedAngle = 0;
		[XmlValue("startSpeedAngleFloat")]	public double startSpeedAngleFloat = 0;

		[XmlValue("rotateSpeed")]			public double rotateSpeed = 0;
		[XmlValue("rotateSpeedFloat")]		public double rotateSpeedFloat = 0;

		[XmlValue("directionSpeed")]		public double directionSpeed = 0;
		[XmlValue("directionSpeedFloat")]	public double directionSpeedFloat = 0;

		[XmlValue("pointCount")]			public int pointCount = 0;
		[XmlValue("pointLife")]				public double pointLife = 0;
		[XmlValue("pointLifeFloat")]		public double pointLifeFloat = 0;

		[XmlValue("pointStartSize")]		public double pointStartSize = 0;
		[XmlValue("pointStartSizeFloat")]	public double pointStartSizeFloat = 0;

		[XmlValue("pointEndSize")]			public double pointEndSize = 0;
		[XmlValue("pointEndSizeFloat")]		public double pointEndSizeFloat = 0;

		[XmlValue("pointStartAngle")]		public double pointStartAngle = 0;
		[XmlValue("pointStartAngleFloat")]	public double pointStartAngleFloat = 0;

		[XmlValue("pointEndAngle")]			public double pointEndAngle = 0;
		[XmlValue("pointEndAngleFloat")]	public double pointEndAngleFloat = 0;
	}
}
