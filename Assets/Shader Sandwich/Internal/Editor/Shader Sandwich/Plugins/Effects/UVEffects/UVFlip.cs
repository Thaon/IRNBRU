using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SU = ShaderUtil;
using System.Xml.Serialization;
using System.Runtime.Serialization;
//using System.Xml;
[System.Serializable]
public class SSEUVFlip : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEUVFlip";
		SE.Name = "Mapping/Flip";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("X Flip",true));
			SE.Inputs.Add(new ShaderVar("Y Flip",false));
			SE.Inputs.Add(new ShaderVar("Z Flip",false));
		}
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,ref int UVDimensions,ref int TypeDimensions){
		//return "float3("+((SE.Inputs[0].On)?"1-":"")+")";
		if (UVDimensions==3)
		return "float3("+((SE.Inputs[0].On)?"1-":"")+Map+".x,"+((SE.Inputs[1].On)?"1-":"")+Map+".y,"+((SE.Inputs[2].On)?"1-":"")+Map+".z)";
		if (UVDimensions==2)
		return "float2("+((SE.Inputs[0].On)?"1-":"")+Map+".x,"+((SE.Inputs[1].On)?"1-":"")+Map+".y)";
		if (UVDimensions==1)
		return ""+(((SE.Inputs[0].On)?"1-":"")+Map);
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		if (SE.Inputs[0].On)
		UV.x=width-UV.x;
		if (SE.Inputs[1].On)
		UV.y=height-UV.y;
		return UV;
	}
}

