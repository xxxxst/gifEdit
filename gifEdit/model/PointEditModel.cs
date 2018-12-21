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

		[XmlValue("x")]						public float x = 100;						//坐标x
		[XmlValue("xFloat")]				public float xFloat = 0;					//坐标x浮动

		[XmlValue("y")]						public float y = 100;						//坐标y
		[XmlValue("yFloat")]				public float yFloat = 0;					//坐标y浮动

		[XmlValue("gravityValue")]			public float gravityValue = 0;             //重力
		[XmlValue("gravityAngle")]			public float gravityAngle = 100;           //重力方向

		[XmlValue("startSpeed")]			public float startSpeed = 20;				//开始速度
		[XmlValue("startSpeedFloat")]		public float startSpeedFloat = 0;			//开始速度浮动

		[XmlValue("startSpeedAngle")]		public float startSpeedAngle = 0;			//开始速度方向
		[XmlValue("startSpeedAngleFloat")]	public float startSpeedAngleFloat = 0;		//开始速度方向浮动

		[XmlValue("rotateSpeed")]			public float rotateSpeed = 0;				//旋转速度
		[XmlValue("rotateSpeedFloat")]		public float rotateSpeedFloat = 0;			//旋转速度浮动

		[XmlValue("directionSpeed")]		public float directionSpeed = 0;			//分离速度
		[XmlValue("directionSpeedFloat")]	public float directionSpeedFloat = 0;		//分离速度方向

		[XmlValue("pointCount")]			public int pointCount = 100;				//粒子数
		[XmlValue("pointLife")]				public float pointLife = 5;				//粒子生命周期
		[XmlValue("pointLifeFloat")]		public float pointLifeFloat = 0;			//粒子生命周期浮动

		[XmlValue("pointStartSize")]		public float pointStartSize = 20;			//粒子开始大小
		[XmlValue("pointStartSizeFloat")]	public float pointStartSizeFloat = 0;		//粒子开始大小浮动

		[XmlValue("pointEndSize")]			public float pointEndSize = 10;			//粒子结束大小
		[XmlValue("pointEndSizeFloat")]		public float pointEndSizeFloat = 0;		//粒子结束大小浮动

		[XmlValue("pointStartAngle")]		public float pointStartAngle = 0;			//粒子开始角度
		[XmlValue("pointStartAngleFloat")]	public float pointStartAngleFloat = 0;		//粒子开始角度浮动

		[XmlValue("pointRotateSpeed")]		public float pointRotateSpeed = 0;			//粒子旋转速度
		[XmlValue("pointRotateSpeedFloat")]	public float pointRotateSpeedFloat = 0;	//粒子旋转速度浮动
	}																							   
}
