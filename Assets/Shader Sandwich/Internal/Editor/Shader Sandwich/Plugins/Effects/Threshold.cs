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
public class SSEThreshold : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEThreshold";
		SE.Name = "Color/Threshold";//+UnityEngine.Random.value.ToString();
		
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Threshold",0.3f));
		}
		SE.Inputs[0].Range0 = 0;
		SE.Inputs[0].Range1 = 1;
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		float Grey = Vector3.Dot(new Vector3(0.3f,0.59f,0.11f),new Vector3(OldColor.r,OldColor.g,OldColor.b));
		ShaderColor NewColor = OldColor;
		if (Grey<SE.Inputs[0].Float)
		NewColor = new ShaderColor(0,0,0,OldColor.a);
		if (NewColor.a<SE.Inputs[0].Float)
		NewColor.a = 0;
		return NewColor;
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "lerp(float3(0,0,0),"+Line+",ceil(dot(float3(0.3,0.59,0.11),"+Line+")-"+SE.Inputs[0].Get()+"))";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "lerp(float4(0,0,0,0),"+Line+",ceil(dot(float3(0.3,0.59,0.11),"+Line+".rgb)-"+SE.Inputs[0].Get()+")*ceil("+Line+".a-"+SE.Inputs[0].Get()+"))";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "lerp(0,"+Line+",ceil("+Line+"-"+SE.Inputs[0].Get()+"))";
	}
}

