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
public class SSEMathRound : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathRound";
		SE.Name = "Maths/Round";//+UnityEngine.Random.value.ToString();
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Split Number",1));
		}
		SE.Inputs[0].NoSlider = false;
		SE.Inputs[0].Range0 = 1f;
		SE.Inputs[0].Range1 = 40f;
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		
		SE.Function = "";
		SE.LinePre = "";
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "(round("+Line+"*"+SE.Inputs[0].Get()+")/"+SE.Inputs[0].Get()+")";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "(round("+Line+"*"+SE.Inputs[0].Get()+")/"+SE.Inputs[0].Get()+")";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "(round("+Line+"*"+SE.Inputs[0].Get()+")/"+SE.Inputs[0].Get()+")";
	}
	public static float Round(float f){
		return Mathf.Round(f);
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor NewColor = new ShaderColor(Round(OldColor.r*SE.Inputs[0].Float)/SE.Inputs[0].Float,Round(OldColor.g*SE.Inputs[0].Float)/SE.Inputs[0].Float,Round(OldColor.b*SE.Inputs[0].Float)/SE.Inputs[0].Float,Round(OldColor.a*SE.Inputs[0].Float)/SE.Inputs[0].Float);
		
		return NewColor;
	}
}

