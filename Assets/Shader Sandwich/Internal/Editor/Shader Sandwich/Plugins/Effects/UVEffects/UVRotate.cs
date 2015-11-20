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
public class SSEUVRotate : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEUVRotate";
		SE.Name = "Mapping/Simple Rotate";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("X Rotate",new string[]{"0","90","180","270"},new string[]{"0","90","180","270"}));
		}
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static string GenerateMap(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Map,ref int UVDimensions,ref int TypeDimensions){
		//return "float3("+((SE.Inputs[0].On)?"1-":"")+")";
		string MapX = "0";
		string MapY = "0";
		//string MapZ = Map+".z";
		//Debug.Log(UVDimensions);
		bool UpSize1 = false;
		if (UVDimensions==1&&TypeDimensions>1)
		UpSize1 = true;
		if (SE.Inputs[0].Type==0){
			MapX = Map+".x";
			if (!UpSize1)
			MapY = Map+".y";
		}
		if (SE.Inputs[0].Type==1){
			if (!UpSize1){
				MapX = "1-"+Map+".y";
				MapY = Map+".x";
			}
			else{
				MapY = "1-"+Map+".x";
			}
		}
		if (SE.Inputs[0].Type==2){
			MapX = "1-"+Map+".x";
			if (!UpSize1)
			MapY = "1-"+Map+".y";
		}
		if (SE.Inputs[0].Type==3){
			if (!UpSize1){
				MapX = Map+".y";
				MapY = "1-"+Map+".x";
			}
			else{
				MapY = Map+".x";
			}
		}
		if (UVDimensions==3)
			return "float3("+MapX+","+MapY+","+Map+".z)";
		if (UVDimensions==2)
			return "float2("+MapX+","+MapY+")";
		if (UVDimensions==1)
			return "("+MapX+")";
		return Map;
	}
	public static Vector2 Preview(ShaderEffect SE,Vector2 UV,int width, int height){
		Vector2 OldUV = new Vector2(UV.x,UV.y);
		if (SE.Inputs[0].Type==1){
			UV.x=height-OldUV.y;
			UV.y=OldUV.x;
		}
		if (SE.Inputs[0].Type==2){
			UV.x=width-OldUV.x;
			UV.y=height-OldUV.y;
		}
		if (SE.Inputs[0].Type==3){
			UV.x=OldUV.y;
			UV.y=width-OldUV.x;
		}
		return UV;
	}
}

