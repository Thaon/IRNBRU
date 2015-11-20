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
public class SSEMathMul : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathMul";
		SE.Name = "Maths/Multiply";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Multiply",1));
		} 
		SE.Inputs[0].NoSlider = true;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "("+Line+"*"+SE.Inputs[0].Get()+")";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "("+Line+"*"+SE.Inputs[0].Get()+")";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "("+Line+"*"+SE.Inputs[0].Get()+")";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor NewColor = new ShaderColor(OldColor.r*SE.Inputs[0].Float,OldColor.g*SE.Inputs[0].Float,OldColor.b*SE.Inputs[0].Float,OldColor.a*SE.Inputs[0].Float);
		return NewColor;
	}
}

