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
public class SSEPixelate : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEPixelate";
		SE.Name = "Blur/Pixelate";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Seperate",true));
			SE.Inputs.Add(new ShaderVar("X Size",0.00001f));
			SE.Inputs.Add(new ShaderVar("Y Size",0.00001f));
			SE.Inputs.Add(new ShaderVar("Z Size",0.00001f));
		}
		SE.Inputs[1].Range0 = 0.000001f;
		SE.Inputs[2].Range0 = 0.000001f;
		SE.Inputs[3].Range0 = 0.000001f;
		//SE.Inputs[1].NoSlider = true;
		//SE.Inputs[2].NoSlider = true;
		//SE.Inputs[3].NoSlider = true;
		
		
		if (!SE.Inputs[0].On){
			SE.Inputs[2].Hidden = true;
			SE.Inputs[2].Use = false;
			SE.Inputs[3].Hidden = true;
			SE.Inputs[3].Use = false;
			SE.Inputs[1].Name = "Size";
			SE.Inputs[2].Float = SE.Inputs[1].Float;
			SE.Inputs[2].UseInput = SE.Inputs[1].UseInput;
			SE.Inputs[3].Float = SE.Inputs[1].Float;
			SE.Inputs[3].UseInput = SE.Inputs[1].UseInput;
		}
		else{
			SE.Inputs[1].Name = "X Size";
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
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,int UVDimensions,int TypeDimensions){
		if (!SE.Inputs[0].On){
		SE.Inputs[2].Float = SE.Inputs[1].Float;
		SE.Inputs[3].Float = SE.Inputs[1].Float;
		SE.Inputs[2].Input = SE.Inputs[1].Input;
		SE.Inputs[3].Input = SE.Inputs[1].Input;
		}
		string Mapping="";
		if (UVDimensions==3)
		Mapping = "float3("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+","+SE.Inputs[3].Get()+")";
		if (UVDimensions==2)
		Mapping = "float2("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+")";
		if (UVDimensions==1)
		Mapping = "("+SE.Inputs[1].Get()+")";
		
		if (Mapping!="")
		return "(round("+Map+"/"+Mapping+")*"+Mapping+")";
		
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		Vector2 NormalUV = new Vector2(UV.x/(float)width,UV.y/(float)height);
		if (!SE.Inputs[0].On){
		SE.Inputs[2].Float = SE.Inputs[1].Float;
		SE.Inputs[3].Float = SE.Inputs[1].Float;
		}
		NormalUV.x=Mathf.Round(NormalUV.x/SE.Inputs[1].Float)*SE.Inputs[1].Float;
		NormalUV.y=Mathf.Round(NormalUV.y/SE.Inputs[2].Float)*SE.Inputs[2].Float;
		//NormalUV.y=SE.Inputs[2].Float;
		
		UV = new Vector2(NormalUV.x*(float)width,NormalUV.y*(float)height);
		return UV;
	}
	public static string GenerateBase(ShaderGenerate SG, ShaderEffect SE,ShaderLayer SL,string PixCol,string Map){
		if (SL.LayerType.Type == (int)LayerTypes.Texture){
			if (SG.InVertex)
			PixCol = "tex2Dlod("+SL.Image.Input.Get()+",float4("+Map+",0,0))";
			else
			PixCol = "tex2D("+SL.Image.Input.Get()+","+Map+",ddx("+SL.GCUVs(SG,false)+"), ddy("+SL.GCUVs(SG,false)+"))";
		}
		//if (SL.LayerType.Type == (int)LayerTypes.Cubemap){
		//	PixCol = "texCUBE("+SL.Cube.Input.Get()+",float4("+Map+","+SE.Inputs[0].Get()+"))";		
		//}	
		return PixCol;
	}
}

