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
public class SSEMathFrac : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathFrac";
		SE.Name = "Maths/Frac";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Split Number",1));
		} 
		SE.Inputs[0].NoSlider = false;
		SE.Inputs[0].Range0 = 1f;
		SE.Inputs[0].Range1 = 20f;
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		if (SE.Inputs[0].Get()=="0")
		return "fmod("+Line+",1)";

		return "fmod("+Line+"*"+SE.Inputs[0].Get()+",1)";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		if (SE.Inputs[0].Get()=="0")
		return "fmod("+Line+",1)";

		return "fmod("+Line+"*"+SE.Inputs[0].Get()+",1)";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		if (SE.Inputs[0].Get()=="0")
		return "fmod("+Line+",1)";

		return "fmod("+Line+"*"+SE.Inputs[0].Get()+",1)";
	}
	public static float Frac(float f){
		return f-Mathf.Floor(f);
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		ShaderColor NewColor = new ShaderColor(Frac(OldColor.r*SE.Inputs[0].Float),Frac(OldColor.g*SE.Inputs[0].Float),Frac(OldColor.b*SE.Inputs[0].Float),Frac(OldColor.a*SE.Inputs[0].Float));
		
		return NewColor;
	}
}

