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
public class SSEUnpackNormal : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEUnpackNormal";
		SE.Name = "Conversion/Unpack Normal";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		SE.UseAlpha.Float = 1;
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "(UnpackNormal("+Line+"))";///2+0.5)";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "float4(UnpackNormal("+Line+"),"+Line+".a)";///2+0.5),"+Line+".a);";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor NewColor = new ShaderColor(OldColor.a,OldColor.g,0,0);
		NewColor*=2;
		NewColor -= new ShaderColor(1,1,0,0);
		//NewColor = new Color(NewColor.r,NewColor.g,0,0);
		
		NewColor = new ShaderColor(NewColor.r,NewColor.g,Mathf.Sqrt(1-(NewColor.r*NewColor.r)-(NewColor.g*NewColor.g)),1);
		//NewColor/=2f;
		//NewColor+= new Color(0.5f,0.5f,0.5f,0f);	
		//NewColor.a = OldColor.a;
		return NewColor;
	}
}

