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
public class SSEDesaturate : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEDesaturate";
		SE.Name = "Color/Desaturate";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.Line = "float3(dot(float3(0.3,0.59,0.11),@Line))";
		SE.LinePre = "";
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "float3(dot(float3(0.3,0.59,0.11),"+Line+".rgb),dot(float3(0.3,0.59,0.11),"+Line+".rgb),dot(float3(0.3,0.59,0.11),"+Line+".rgb))";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		float Grey = Vector3.Dot(new Vector3(0.3f,0.59f,0.11f),new Vector3(OldColor.r,OldColor.g,OldColor.b));
		ShaderColor NewColor = new ShaderColor(Grey,Grey,Grey,OldColor.a);
		//NewColor.a = OldColor.a;
		return NewColor;
	}
}

