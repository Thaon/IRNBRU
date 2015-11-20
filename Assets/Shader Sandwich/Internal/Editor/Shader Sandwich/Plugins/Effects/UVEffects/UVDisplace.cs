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
public class SSEUVDisplace : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEUVDisplace";
		SE.Name = "Mapping/Displace";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true||SE.Inputs.Count!=3){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Displace Mask (RGBA)","ListOfObjects"));
			SE.Inputs.Add(new ShaderVar("Displace Amount",0));
			SE.Inputs.Add(new ShaderVar("Displace Middle",0.5f));
		} 
		SE.Inputs[0].SetToMasks(null,0);
		SE.Inputs[0].NoInputs = true;
		SE.Inputs[0].RGBAMasks = true;
		SE.Inputs[1].NoSlider = true;
		SE.Inputs[2].Range0 = 0f;
		SE.Inputs[2].Range1 = 1f;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static void SetUsed(ShaderGenerate SG,ShaderEffect SE){
	//Debug.Log(SG.UsedMasks.ContainsKey((ShaderLayerList)SE.Inputs[0].Obj));
		if (SE.Inputs[0].Obj!=null&&SG.UsedMasks.ContainsKey((ShaderLayerList)SE.Inputs[0].Obj)){
		SG.UsedMasks[(ShaderLayerList)SE.Inputs[0].Obj]++;
//		Debug.Log(SG.UsedMasks[(ShaderLayerList)SE.Inputs[0].Obj]);
		}
	}
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,ref int UVDimensions,ref int TypeDimensions){
		if (TypeDimensions==3){
			if (UVDimensions == 1)
				Map = "float3("+Map+",0,0)";
			if (UVDimensions == 2)
				Map = "float3("+Map+",0)";
			UVDimensions = 3;
			return "("+Map+"+((float3("+SE.Inputs[0].GetMaskName()+".r,"+SE.Inputs[0].GetMaskName()+".g,"+SE.Inputs[0].GetMaskName()+".b)-"+SE.Inputs[2].Get()+")*"+SE.Inputs[1].Get()+"))";
		}
		if (TypeDimensions==2){
			if (UVDimensions == 1)
				Map = "float2("+Map+",0)";
			
			if (UVDimensions<3){
				UVDimensions = 2;
				return "("+Map+"+((float2("+SE.Inputs[0].GetMaskName()+".r,"+SE.Inputs[0].GetMaskName()+".g)-"+SE.Inputs[2].Get()+")*"+SE.Inputs[1].Get()+"))";
			}
			else{
				return "("+Map+"+((float3("+SE.Inputs[0].GetMaskName()+".r,"+SE.Inputs[0].GetMaskName()+".g,0)-"+SE.Inputs[2].Get()+")*"+SE.Inputs[1].Get()+"))";
			}
		}
		if (TypeDimensions==1){
			//if (UVDimensions==1)
			return "("+Map+"+(("+SE.Inputs[0].GetMaskName()+".r-"+SE.Inputs[2].Get()+")*"+SE.Inputs[1].Get()+"))";
		}
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		Color Disp = new Color(0.5f,0.5f,0.5f,0.5f);
		if (SE.Inputs[0].Obj!=null)
		Disp = ((ShaderLayerList)(SE.Inputs[0].Obj)).GetIcon().GetPixel((int)UV.x,(int)UV.y);
		
		UV.x+=(Disp.r-SE.Inputs[2].Float)*width*SE.Inputs[1].Float;//Mathf.Round(SE.Inputs[0].Float*width);
		UV.y+=(Disp.g-SE.Inputs[2].Float)*height*SE.Inputs[1].Float;//Mathf.Round(SE.Inputs[1].Float*height);
		return UV;
	}
}

