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
public class SSESwitchNormalScale : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSESwitchNormalScale";
		SE.Name = "Conversion/Descale Normal Map";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "("+Line+"/2+0.5)";///2+0.5)";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "("+Line+"/2+0.5)";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor NewColor = OldColor;
		//NewColor*=2;
		//NewColor -= new Color(1,1,0,0);
		//NewColor = new Color(NewColor.r,NewColor.g,0,0);
		
		//NewColor = new Color(NewColor.r,NewColor.g,Mathf.Sqrt(1-(NewColor.r*NewColor.r)-(NewColor.g*NewColor.g)),1);
		NewColor/=2f;
		NewColor+= new ShaderColor(0.5f,0.5f,0.5f,0f);	
		NewColor.a = OldColor.a;
		//NewColor.a = OldColor.a;
		return NewColor;
	}
}

