using csharpHelp.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gifEdit.model {
	[XmlRoot("gifEdit")]
	public class ParticleEditModel {
		public XmlModelServer srv = null;

		public string path = "";
		public string name = "";

		[XmlAttr("dataVersion")]			public string dataVersion = "1.0.0";

		[XmlValue("config.background")]		public string background = "24252c";
		[XmlValue("config.seed")]			public int seed = 0;
		[XmlValue("config.isSeedAuto")]		public bool isSeedAuto = true;
		[XmlValue("config.fps")]			public int fps = 30;
		[XmlValue("config.exportPath")]		public string exportPath = "";
		[XmlValue("config.width")]			public int width = 400;
		[XmlValue("config.height")]			public int height = 400;
		[XmlValue("config.isMaskBox")]		public bool isMaskBox = true;

		[XmlListChild("resourceBox.res")]	public List<ParticleResourceModel> lstResource = new List<ParticleResourceModel>();

		public int frameCount = 0;
	}

	public class ParticleResourceModel {
		[XmlAttr("path")]						public string path = "";					//路径

		[XmlValue("x")]							public float x = 100;						//坐标x
		[XmlValue("xFloat")]					public float xFloat = 50;					//坐标x浮动

		[XmlValue("y")]							public float y = 100;						//坐标y
		[XmlValue("yFloat")]					public float yFloat = 0;					//坐标y浮动

		[XmlValue("startAlpha")]				public float startAlpha = 1;				//开始透明度
		[XmlValue("endAlpha")]					public float endAlpha = 1;                  //结束透明度

		[XmlValue("gravityValue")]				public float gravityValue = 0;				//重力
		[XmlValue("gravityAngle")]				public float gravityAngle = 0;				//重力方向

		[XmlValue("startSpeed")]				public float startSpeed = 20;				//开始速度
		[XmlValue("startSpeedFloat")]			public float startSpeedFloat = 0;			//开始速度浮动

		[XmlValue("startSpeedAngle")]			public float startSpeedAngle = 0;			//开始速度方向
		[XmlValue("startSpeedAngleFloat")]		public float startSpeedAngleFloat = 0;		//开始速度方向浮动

		[XmlValue("rotateSpeed")]				public float rotateSpeed = 0;				//旋转速度
		[XmlValue("rotateSpeedFloat")]			public float rotateSpeedFloat = 0;			//旋转速度浮动

		[XmlValue("directionSpeed")]			public float directionSpeed = 0;			//分离速度
		[XmlValue("directionSpeedFloat")]		public float directionSpeedFloat = 0;		//分离速度方向

		[XmlValue("particleCount")]				public int   particleCount = 100;			//粒子数
		[XmlValue("particleLife")]				public float particleLife = 5;				//粒子生命周期
		[XmlValue("particleLifeFloat")]			public float particleLifeFloat = 0;			//粒子生命周期浮动

		[XmlValue("particleStartSize")]			public float particleStartSize = 20;		//粒子开始大小
		[XmlValue("particleStartSizeFloat")]	public float particleStartSizeFloat = 0;	//粒子开始大小浮动

		[XmlValue("particleEndSize")]			public float particleEndSize = 10;			//粒子结束大小
		[XmlValue("particleEndSizeFloat")]		public float particleEndSizeFloat = 0;		//粒子结束大小浮动

		[XmlValue("particleStartAngle")]		public float particleStartAngle = 0;		//粒子开始角度
		[XmlValue("particleStartAngleFloat")]	public float particleStartAngleFloat = 0;	//粒子开始角度浮动

		[XmlValue("particleRotateSpeed")]		public float particleRotateSpeed = 0;		//粒子旋转速度
		[XmlValue("particleRotateSpeedFloat")]	public float particleRotateSpeedFloat = 0;	//粒子旋转速度浮动
	}																							   
}
