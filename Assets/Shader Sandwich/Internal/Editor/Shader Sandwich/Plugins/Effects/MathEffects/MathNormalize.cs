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
public class SSENormalize : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSENormalize";
		SE.Name = "Maths/Normalize";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "(normalize("+Line+"))";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "(normalize("+Line+"))";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "("+Line+")";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		//rsqrt(dot(v,v))*v
		ShaderColor NewColor = new ShaderColor(OldColor.r,OldColor.g,OldColor.b,OldColor.a);
		float dot;
		if (SE.UseAlpha.Float==2f)
		dot = OldColor.a*OldColor.a;
		else
		if (SE.UseAlpha.Float==1f)
		dot = OldColor.r*OldColor.r+OldColor.g*OldColor.g+OldColor.b*OldColor.b+OldColor.a*OldColor.a;
		else
		dot = OldColor.r*OldColor.r+OldColor.g*OldColor.g+OldColor.b*OldColor.b;
		
		dot = Mathf.Pow(dot,-0.5f);
		NewColor *= dot;
		return NewColor;
	}
}

