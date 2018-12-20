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
		[XmlAttr("path")]					public string path = "";					//路径

		[XmlValue("x")]						public double x = 100;						//坐标x
		[XmlValue("xFloat")]				public double xFloat = 0;					//坐标x浮动

		[XmlValue("y")]						public double y = 100;						//坐标y
		[XmlValue("yFloat")]				public double yFloat = 0;					//坐标y浮动

		[XmlValue("gravityValue")]			public double gravityValue = 0;             //重力
		[XmlValue("gravityAngle")]			public double gravityAngle = 100;           //重力方向

		[XmlValue("startSpeed")]			public double startSpeed = 20;				//开始速度
		[XmlValue("startSpeedFloat")]		public double startSpeedFloat = 0;			//开始速度浮动

		[XmlValue("startSpeedAngle")]		public double startSpeedAngle = 0;			//开始速度方向
		[XmlValue("startSpeedAngleFloat")]	public double startSpeedAngleFloat = 0;		//开始速度方向浮动

		[XmlValue("rotateSpeed")]			public double rotateSpeed = 0;				//旋转速度
		[XmlValue("rotateSpeedFloat")]		public double rotateSpeedFloat = 0;			//旋转速度浮动

		[XmlValue("directionSpeed")]		public double directionSpeed = 0;			//分离速度
		[XmlValue("directionSpeedFloat")]	public double directionSpeedFloat = 0;		//分离速度方向

		[XmlValue("pointCount")]			public int pointCount = 100;				//粒子数
		[XmlValue("pointLife")]				public double pointLife = 5;				//粒子生命周期
		[XmlValue("pointLifeFloat")]		public double pointLifeFloat = 0;			//粒子生命周期浮动

		[XmlValue("pointStartSize")]		public double pointStartSize = 20;			//粒子开始大小
		[XmlValue("pointStartSizeFloat")]	public double pointStartSizeFloat = 0;		//粒子开始大小浮动

		[XmlValue("pointEndSize")]			public double pointEndSize = 10;			//粒子结束大小
		[XmlValue("pointEndSizeFloat")]		public double pointEndSizeFloat = 0;		//粒子结束大小浮动

		[XmlValue("pointStartAngle")]		public double pointStartAngle = 0;			//粒子开始角度
		[XmlValue("pointStartAngleFloat")]	public double pointStartAngleFloat = 0;		//粒子开始角度浮动

		[XmlValue("pointEndAngle")]			public double pointEndAngle = 0;			//粒子结束角度
		[XmlValue("pointEndAngleFloat")]	public double pointEndAngleFloat = 0;		//粒子结束角度浮动
	}																							   
}
