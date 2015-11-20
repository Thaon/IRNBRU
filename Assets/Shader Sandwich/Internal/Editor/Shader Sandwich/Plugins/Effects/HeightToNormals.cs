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
public class SSENormalMap : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSENormalMap";
		SE.Name = "Conversion/Normal Map";//+UnityEngine.Random.value.ToString();

		if (BInputs==true||SE.Inputs.Count!=3){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Size",0.02f));
			SE.Inputs.Add(new ShaderVar("Height",1f));
			SE.Inputs.Add(new ShaderVar( "Channel",new string[] {"R", "G", "B","A"},new string[] {"","","",""},4));
			SE.Inputs[2].Type = 3;
		} 
		SE.Inputs[0].Range0 = 0;
		SE.Inputs[0].Range1 = 0.1f;
		SE.Inputs[1].Range0 = 0;
		SE.Inputs[1].Range1 = 3;
		if (SE.Inputs[2].Type==3)
		SE.UseAlpha.Float = 1;
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor[] OldColors,int X, int Y, int W, int H){
	
		//ShaderColor OldColor = OldColors[ShaderUtil.FlatArray(X,Y,W,H)];
		
		Vector2 NewColor = new Vector2(0,0);
		
		if (SE.Inputs[2].Type==0)
		NewColor = new Vector2(OldColors[ShaderUtil.FlatArray(X,Y,W,H)].r-OldColors[ShaderUtil.FlatArray(X+((int)Mathf.Round(SE.Inputs[0].Float*W)),Y,W,H)].r,
		OldColors[ShaderUtil.FlatArray(X,Y,W,H)].r-OldColors[ShaderUtil.FlatArray(X,Y+((int)Mathf.Round(SE.Inputs[0].Float*H)),W,H)].r);
		if (SE.Inputs[2].Type==1)
		NewColor = new Vector2(OldColors[ShaderUtil.FlatArray(X,Y,W,H)].g-OldColors[ShaderUtil.FlatArray(X+((int)Mathf.Round(SE.Inputs[0].Float*W)),Y,W,H)].g,
		OldColors[ShaderUtil.FlatArray(X,Y,W,H)].g-OldColors[ShaderUtil.FlatArray(X,Y+((int)Mathf.Round(SE.Inputs[0].Float*H)),W,H)].g);
		if (SE.Inputs[2].Type==2)
		NewColor = new Vector2(OldColors[ShaderUtil.FlatArray(X,Y,W,H)].b-OldColors[ShaderUtil.FlatArray(X+((int)Mathf.Round(SE.Inputs[0].Float*W)),Y,W,H)].b,
		OldColors[ShaderUtil.FlatArray(X,Y,W,H)].b-OldColors[ShaderUtil.FlatArray(X,Y+((int)Mathf.Round(SE.Inputs[0].Float*H)),W,H)].b);
		if (SE.Inputs[2].Type==3)
		NewColor = new Vector2(OldColors[ShaderUtil.FlatArray(X,Y,W,H)].a-OldColors[ShaderUtil.FlatArray(X+((int)Mathf.Round(SE.Inputs[0].Float*W)),Y,W,H)].a,
		OldColors[ShaderUtil.FlatArray(X,Y,W,H)].a-OldColors[ShaderUtil.FlatArray(X,Y+((int)Mathf.Round(SE.Inputs[0].Float*H)),W,H)].a);
		
		
		
		NewColor *= SE.Inputs[1].Float;
		return new ShaderColor(NewColor.x,NewColor.y,1,1);//Mathf.Sqrt(1-(NewColor.x*NewColor.x)-(NewColor.y*NewColor.y)),1);
		//NewColor/=2f;
		//NewColor+= new Color(0.5f,0.5f,0.5f,0f);
		//else
		//NewColor/=(SE.Inputs[3].Float*4f)+1;

		//return NewColor;
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		string[] Colors = new string[]{"r","g","b","a"};
		string Line1 = "(("+Line+"."+Colors[SE.Inputs[2].Type]+"-"+SL.StartNewBranch(SG,SL.GCUVs(SG,SE.Inputs[0].Get(),"0","0"),Effect)+"."+Colors[SE.Inputs[2].Type]+")*"+SE.Inputs[1].Get()+")";
		string Line2 = "(("+Line+"."+Colors[SE.Inputs[2].Type]+"-"+SL.StartNewBranch(SG,SL.GCUVs(SG,"0",SE.Inputs[0].Get(),"0"),Effect)+"."+Colors[SE.Inputs[2].Type]+")*"+SE.Inputs[1].Get()+")";
		//string retVal = "(float3("+Line1+","+Line2+","+"sqrt((1-pow("+Line1+",2)-pow("+Line2+",2)))))";///2+0.5)";
		string retVal = "(float3("+Line1+","+Line2+",1))";///2+0.5)";
		return retVal;
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		string[] Colors = new string[]{"r","g","b","a"};
		string Line1 = "(("+Line+"."+Colors[SE.Inputs[2].Type]+"-"+SL.StartNewBranch(SG,SL.GCUVs(SG,SE.Inputs[0].Get(),"0","0"),Effect)+"."+Colors[SE.Inputs[2].Type]+")*"+SE.Inputs[1].Get()+")";
		string Line2 = "(("+Line+"."+Colors[SE.Inputs[2].Type]+"-"+SL.StartNewBranch(SG,SL.GCUVs(SG,"0",SE.Inputs[0].Get(),"0"),Effect)+"."+Colors[SE.Inputs[2].Type]+")*"+SE.Inputs[1].Get()+")";
		//string retVal = "(float4("+Line1+","+Line2+","+"sqrt((1-pow("+Line1+",2)-pow("+Line2+",2))),"+Line+".a))";///2+0.5";
		string retVal = "(float4("+Line1+","+Line2+",1,"+Line+".a))";///2+0.5";
		return retVal;
	}
}