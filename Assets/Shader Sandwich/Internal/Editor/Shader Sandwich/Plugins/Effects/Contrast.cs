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
public class SSEContrast : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEContrast";
		SE.Name = "Color/Contrast";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.Line = "lerp(float3(0.5),@Line,@Arg1)";
		SE.LinePre = "";
		
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Contrast",1));
		}
		SE.Inputs[0].Range0 = 0;
		SE.Inputs[0].Range1 = 3;
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static float Lerp(float a, float b, float t){
		return a+((b-a)*t);
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "lerp(float3(0.5,0.5,0.5),"+Line+","+SE.Inputs[0].Get()+")";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "lerp(float4(0.5,0.5,0.5,0.5),"+Line+","+SE.Inputs[0].Get()+")";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "lerp(0.5,"+Line+","+SE.Inputs[0].Get()+")";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor Grey = new ShaderColor(0.5f,0.5f,0.5f,0.5f);
		ShaderColor NewColor = new ShaderColor(Lerp(Grey.r,OldColor.r,SE.Inputs[0].Float),Lerp(Grey.g,OldColor.g,SE.Inputs[0].Float),Lerp(Grey.b,OldColor.b,SE.Inputs[0].Float),Lerp(Grey.a,OldColor.a,SE.Inputs[0].Float));
		return NewColor;
	}
}

