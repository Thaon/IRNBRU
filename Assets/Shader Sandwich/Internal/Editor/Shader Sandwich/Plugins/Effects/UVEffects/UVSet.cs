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
public class SSEUVSet : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs,ShaderLayer SL){
		SE.TypeS = "SSEUVSet";
		SE.Name = "Mapping/Set";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true||SE.Inputs.Count!=2){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Values (RGBA)","ListOfObjects"));
			SE.Inputs.Add(new ShaderVar("Scale",1));
		} 
//if (SL!=null)
//Debug.Log(SL.IsLighting);
		if (SL!=null)
		SE.Inputs[0].LightingMasks = SL.IsLighting;
		
		SE.Inputs[0].SetToMasks(null,0);
		SE.Inputs[0].NoInputs = true;
		SE.Inputs[0].RGBAMasks = true;		
		SE.Inputs[1].NoSlider = true;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static void SetUsed(ShaderGenerate SG,ShaderEffect SE){
		if (SE.Inputs[0].Obj!=null&&SG.UsedMasks.ContainsKey((ShaderLayerList)SE.Inputs[0].Obj)){
			SG.UsedMasks[(ShaderLayerList)SE.Inputs[0].Obj]++;
		}
	}
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,ref int UVDimensions,ref int TypeDimensions){
		if (TypeDimensions==3){
			if (UVDimensions == 1)
				Map = "float3("+Map+",0,0)";
			if (UVDimensions == 2)
				Map = "float3("+Map+",0)";
			UVDimensions = 3;
			return "(((float3("+SE.Inputs[0].GetMaskName()+".r,"+SE.Inputs[0].GetMaskName()+".g,"+SE.Inputs[0].GetMaskName()+".b))*"+SE.Inputs[1].Get()+"))";
		}
		if (TypeDimensions==2){
			if (UVDimensions == 1)
				Map = "float2("+Map+",0)";
			
			if (UVDimensions<3){
				UVDimensions = 2;
				return "(((float2("+SE.Inputs[0].GetMaskName()+".r,"+SE.Inputs[0].GetMaskName()+".g))*"+SE.Inputs[1].Get()+"))";
			}
			else{
				return "(((float3("+SE.Inputs[0].GetMaskName()+".r,"+SE.Inputs[0].GetMaskName()+".g,0))*"+SE.Inputs[1].Get()+"))";
			}
		}
		if (TypeDimensions==1){
			//if (UVDimensions==1)
			return "((("+SE.Inputs[0].GetMaskName()+".r)*"+SE.Inputs[1].Get()+"))";
		}
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		Color Disp = new Color(0.5f,0.5f,0.5f,0.5f);
		if (SE.Inputs[0].Obj!=null)
		Disp = ((ShaderLayerList)(SE.Inputs[0].Obj)).GetIcon().GetPixel((int)UV.x,(int)UV.y);
		
		UV.x=(Disp.r)*width*SE.Inputs[1].Float;//Mathf.Round(SE.Inputs[0].Float*width);
		UV.y=(Disp.g)*height*SE.Inputs[1].Float;//Mathf.Round(SE.Inputs[1].Float*height);
		return UV;
	}
}

