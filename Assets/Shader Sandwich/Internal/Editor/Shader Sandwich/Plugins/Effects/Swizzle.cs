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
public class SSESwizzle : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSESwizzle";
		SE.Name = "Color/Swizzle";//+UnityEngine.Random.value.ToString();
		
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar( "Channel R",new string[] {"R", "G", "B","A"},new string[] {"","","",""},4));
			SE.Inputs[0].Type = 0;
			SE.Inputs.Add(new ShaderVar( "Channel G",new string[] {"R", "G", "B","A"},new string[] {"","","",""},4));
			SE.Inputs[1].Type = 1;
			SE.Inputs.Add(new ShaderVar( "Channel B",new string[] {"R", "G", "B","A"},new string[] {"","","",""},4));
			SE.Inputs[2].Type = 2;
			SE.Inputs.Add(new ShaderVar( "Channel A",new string[] {"R", "G", "B","A"},new string[] {"","","",""},4));
			SE.Inputs[3].Type = 3;
		}
		//if (SE.Inputs[0].Type==3||SE.Inputs[1].Type==3||SE.Inputs[2].Type==3||SE.Inputs[3].Type==3)
		SE.UseAlpha.Float = 1;
		
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		float R = 0;
		if (SE.Inputs[0].Type == 0)
		R = OldColor.r;
		if (SE.Inputs[0].Type == 1)
		R = OldColor.g;
		if (SE.Inputs[0].Type == 2)
		R = OldColor.b;
		if (SE.Inputs[0].Type == 3)
		R = OldColor.a;
		
		float G = 0;
		if (SE.Inputs[1].Type == 0)
		G = OldColor.r;
		if (SE.Inputs[1].Type == 1)
		G = OldColor.g;
		if (SE.Inputs[1].Type == 2)
		G = OldColor.b;
		if (SE.Inputs[1].Type == 3)
		G = OldColor.a;	
		
		float B = 0;
		if (SE.Inputs[2].Type == 0)
		B = OldColor.r;
		if (SE.Inputs[2].Type == 1)
		B = OldColor.g;
		if (SE.Inputs[2].Type == 2)
		B = OldColor.b;
		if (SE.Inputs[2].Type == 3)
		B = OldColor.a;
		
		float A = OldColor.a;
		if (SE.Inputs[3].Type == 0)
		A = OldColor.r;
		if (SE.Inputs[3].Type == 1)
		A = OldColor.g;
		if (SE.Inputs[3].Type == 2)
		A = OldColor.b;
		if (SE.Inputs[3].Type == 3)
		A = OldColor.a;
		
		ShaderColor NewColor = new ShaderColor(R,G,B,A);
		return NewColor;
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return Line+"."+((SE.Inputs[0].Names[SE.Inputs[0].Type]+SE.Inputs[1].Names[SE.Inputs[1].Type]+SE.Inputs[2].Names[SE.Inputs[2].Type]).ToLower());//+SE.Inputs[3].Names[SE.Inputs[3].Type];
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return Line+"."+(SE.Inputs[0].Names[SE.Inputs[0].Type]+SE.Inputs[1].Names[SE.Inputs[1].Type]+SE.Inputs[2].Names[SE.Inputs[2].Type]+SE.Inputs[3].Names[SE.Inputs[3].Type]).ToLower();//+SE.Inputs[3].Names[SE.Inputs[3].Type];
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return Line+"."+SE.Inputs[3].Names[SE.Inputs[3].Type].ToLower();//+SE.Inputs[3].Names[SE.Inputs[3].Type];
	}
}

