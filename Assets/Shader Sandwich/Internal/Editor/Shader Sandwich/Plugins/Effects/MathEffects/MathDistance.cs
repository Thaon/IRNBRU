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
public class SSEMathDistance : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathDistance";
		SE.Name = "Maths/Distance";//+UnityEngine.Random.value.ToString();
		
		SE.Function = "";
		SE.LinePre = "";
		if (BInputs==true||SE.Inputs.Count!=5){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Dimensions",new string[]{"1D","2D","3D","4D"},new string[]{"","","",""},new string[]{"1D","2D","3D","4D"}));
			SE.Inputs.Add(new ShaderVar("X",0));
			SE.Inputs.Add(new ShaderVar("Y",0));
			SE.Inputs.Add(new ShaderVar("Z",0));
			SE.Inputs.Add(new ShaderVar("W",0));
		} 
		SE.Inputs[1].NoSlider = true;
		SE.Inputs[2].NoSlider = true;
		SE.Inputs[3].NoSlider = true;
		SE.Inputs[4].NoSlider = true;
		if (SE.Inputs[0].Type==3)
		SE.UseAlpha.Float=1f;
		if (SE.UseAlpha.Float==2f)
		SE.UseAlpha.Float=0f;
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string GenerateGeneric(ShaderEffect SE,string line){
		string Grey = line;
		if (SE.Inputs[0].Type==0)
		Grey = line+".r-"+SE.Inputs[1].Get();
		if (SE.Inputs[0].Type==1)
		Grey = "distance("+line+".xy,float2("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+"))";
		if (SE.Inputs[0].Type==2)
		Grey = "distance("+line+".xyz,float3("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+","+SE.Inputs[3].Get()+"))";
		if (SE.Inputs[0].Type==3)
		Grey = "distance("+line+",float4("+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+","+SE.Inputs[3].Get()+","+SE.Inputs[4].Get()+"))";
		return "(("+Grey+").rrrr)";	
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return GenerateGeneric(SE,Line);
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return GenerateGeneric(SE,Line);
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return GenerateGeneric(SE,Line);
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		//ShaderColor NewColor = OldColor*new ShaderColor(SE.Inputs[0].Float,SE.Inputs[1].Float,SE.Inputs[2].Float,SE.Inputs[3].Float);// = new ShaderColor(OldColor.r+SE.Inputs[0].Float,OldColor.g+SE.Inputs[0].Float,OldColor.b+SE.Inputs[0].Float,OldColor.a+SE.Inputs[0].Float);
		float Grey = 0;
		if (SE.Inputs[0].Type==0)
		Grey = OldColor.r-SE.Inputs[1].Float;
		if (SE.Inputs[0].Type==1)
		Grey = Vector2.Distance(new Vector2(OldColor.r,OldColor.g),new Vector2(SE.Inputs[1].Float,SE.Inputs[2].Float));
		if (SE.Inputs[0].Type==2)
		Grey = Vector3.Distance(new Vector3(OldColor.r,OldColor.g,OldColor.b),new Vector3(SE.Inputs[1].Float,SE.Inputs[2].Float,SE.Inputs[3].Float));
		if (SE.Inputs[0].Type==3)
		Grey = Vector4.Distance(new Vector4(OldColor.r,OldColor.g,OldColor.b,OldColor.a),new Vector4(SE.Inputs[1].Float,SE.Inputs[2].Float,SE.Inputs[3].Float,SE.Inputs[4].Float));
		
		
		ShaderColor NewColor = new ShaderColor(Grey,Grey,Grey,Grey);
		return NewColor;
	}
}

