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
public class SSEUVOffset : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEUVOffset";
		SE.Name = "Mapping/Offset";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true||SE.Inputs.Count!=3){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("X Offset",0));
			SE.Inputs.Add(new ShaderVar("Y Offset",0));
			SE.Inputs.Add(new ShaderVar("Z Offset",0));
		} 
		SE.Inputs[0].NoSlider = true;
		SE.Inputs[1].NoSlider = true;
		SE.Inputs[2].NoSlider = true;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,ref int UVDimensions,ref int TypeDimensions){
		if (TypeDimensions==3){
			if (UVDimensions == 1)
				Map = "float3("+Map+",0,0)";
			if (UVDimensions == 2)
				Map = "float3("+Map+",0)";
			UVDimensions = 3;
			if (SL.LayerType.Type==(int)LayerTypes.Noise)
				return "("+Map+"+float3("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+","+SE.Inputs[2].Get(1f/3f)+"))";
			else
				return "("+Map+"+float3("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+","+SE.Inputs[2].Get(1f)+"))";
		}
		if (TypeDimensions==2){
			if (UVDimensions == 1)
				Map = "float2("+Map+",0)";
			
			if (UVDimensions<3){
				UVDimensions = 2;
				return "("+Map+"+float2("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+"))";
			}
			else{
				return "("+Map+"+float3("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+",0))";
			}
		}
		if (TypeDimensions==1)
		return "("+Map+"+("+SE.Inputs[0].Get()+"))";
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		UV.x+=Mathf.Round(SE.Inputs[0].Float*width);
		UV.y+=Mathf.Round(SE.Inputs[1].Float*height);
		return UV;
	}
}

