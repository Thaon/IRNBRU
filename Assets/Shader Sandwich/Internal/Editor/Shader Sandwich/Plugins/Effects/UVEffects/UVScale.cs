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
public class SSEUVScale : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEUVScale";
		SE.Name = "Mapping/Scale";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Seperate",true));
			SE.Inputs.Add(new ShaderVar("X Scale",1));
			SE.Inputs.Add(new ShaderVar("Y Scale",1));
			SE.Inputs.Add(new ShaderVar("Z Scale",1));
		} 
		SE.Inputs[1].NoSlider = true;
		SE.Inputs[2].NoSlider = true;
		SE.Inputs[3].NoSlider = true;
		
		
		if (!SE.Inputs[0].On){
			SE.Inputs[2].Hidden = true;
			SE.Inputs[2].Use = false;
			SE.Inputs[3].Hidden = true;
			SE.Inputs[3].Use = false;
			SE.Inputs[1].Name = "Scale";
			SE.Inputs[2].Float = SE.Inputs[1].Float;
			SE.Inputs[2].Input = SE.Inputs[1].Input;
			SE.Inputs[3].Float = SE.Inputs[1].Float;
			SE.Inputs[3].Input = SE.Inputs[1].Input;
		}
		else{
			SE.Inputs[1].Name = "X Scale";
			SE.Inputs[2].Hidden = false;
			SE.Inputs[2].Use = true;
			SE.Inputs[3].Hidden = false;
			SE.Inputs[3].Use = true;
		}
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,ref int UVDimensions,ref int TypeDimensions){
		if (!SE.Inputs[0].On){
			if (UVDimensions==3)
			return "("+Map+"*float3("+SE.Inputs[1].Get()+","+SE.Inputs[1].Get()+","+SE.Inputs[1].Get()+"))";
			if (UVDimensions==2)
			return "("+Map+"*float2("+SE.Inputs[1].Get()+","+SE.Inputs[1].Get()+"))";
			if (UVDimensions==1)
			return "("+Map+"*("+SE.Inputs[1].Get()+"))";
		}
		else{
			if (UVDimensions==3)
			return "("+Map+"*float3("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+","+SE.Inputs[3].Get()+"))";
			if (UVDimensions==2)
			return "("+Map+"*float2("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+"))";
			if (UVDimensions==1)
			return "("+Map+"*("+SE.Inputs[1].Get()+"))";
		}
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		if (!SE.Inputs[0].On)
		SE.Inputs[2].Float = SE.Inputs[1].Float;
		if (!SE.Inputs[0].On)
		SE.Inputs[3].Float = SE.Inputs[1].Float;
		UV.x*=SE.Inputs[1].Float;
		UV.y*=SE.Inputs[2].Float;
		return UV;
	}
}

