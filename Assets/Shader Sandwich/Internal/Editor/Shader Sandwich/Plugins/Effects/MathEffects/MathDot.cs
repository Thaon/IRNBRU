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
public class SSEMathDot : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathDot";
		SE.Name = "Maths/Dot";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("R",0.333f));
			SE.Inputs.Add(new ShaderVar("G",0.333f));
			SE.Inputs.Add(new ShaderVar("B",0.333f));
			SE.Inputs.Add(new ShaderVar("A",0.333f));
		} 
		SE.Inputs[0].NoSlider = true;
		SE.Inputs[1].NoSlider = true;
		SE.Inputs[2].NoSlider = true;
		SE.Inputs[3].NoSlider = true;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		string a= "dot("+Line+",float3("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+"))";
		return "float4("+a+","+a+","+a+","+a+")";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		string a= "dot("+Line+","+SE.Inputs[0].Get()+")";
		return "float4("+a+","+a+","+a+","+a+")";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		string a= "dot("+Line+",float3("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+","+SE.Inputs[3].Get()+"))";
		return "float4("+a+","+a+","+a+","+a+")";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		ShaderColor NewColor = OldColor*new ShaderColor(SE.Inputs[0].Float,SE.Inputs[1].Float,SE.Inputs[2].Float,SE.Inputs[3].Float);// = new ShaderColor(OldColor.r+SE.Inputs[0].Float,OldColor.g+SE.Inputs[0].Float,OldColor.b+SE.Inputs[0].Float,OldColor.a+SE.Inputs[0].Float);
		float Grey = NewColor.r+NewColor.g+NewColor.b;
		if (SE.UseAlpha.Float==2f)
		Grey = NewColor.a;
		if (SE.UseAlpha.Float==1f)
		Grey = NewColor.r+NewColor.g+NewColor.b+NewColor.a;
		NewColor = new ShaderColor(Grey,Grey,Grey,Grey);
		return NewColor;
	}
}

