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
public class SSEMathClamp : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathClamp";
		SE.Name = "Maths/Clamp";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Min",0));
			SE.Inputs.Add(new ShaderVar("Max",1));
		} 
		SE.Inputs[0].NoSlider = true;
		SE.Inputs[1].NoSlider = true;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (SE.Inputs[0].Get()=="0"&&SE.Inputs[0].Get()=="1")
		return "saturate("+Line+")";
		return "clamp("+Line+","+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+")";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (SE.Inputs[0].Get()=="0"&&SE.Inputs[0].Get()=="1")
		return "saturate("+Line+")";
		return "clamp("+Line+","+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+")";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (SE.Inputs[0].Get()=="0"&&SE.Inputs[0].Get()=="1")
		return "saturate("+Line+")";
		return "clamp("+Line+","+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+")";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor NewColor = new ShaderColor(Mathf.Clamp(OldColor.r,SE.Inputs[0].Float,SE.Inputs[1].Float),Mathf.Clamp(OldColor.g,SE.Inputs[0].Float,SE.Inputs[1].Float),Mathf.Clamp(OldColor.b,SE.Inputs[0].Float,SE.Inputs[1].Float),Mathf.Clamp(OldColor.a,SE.Inputs[0].Float,SE.Inputs[1].Float));
		return NewColor;
	}
}

