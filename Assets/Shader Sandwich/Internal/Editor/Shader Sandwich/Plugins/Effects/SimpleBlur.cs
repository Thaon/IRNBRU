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
public class SSESimpleBlur : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSESimpleBlur";
		SE.Name = "Blur/Simple Blur";//+UnityEngine.Random.value.ToString();
		SE.ChangeBaseCol = true;
		
		SE.Function = "";

		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Blur",0));
		} 
		SE.Inputs[0].Range0 = 0;
		SE.Inputs[0].Range1 = 10;		
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;
		SE.IsUVEffect = true;
	}
	public static ShaderColor GetAddBlur(ShaderEffect SE,ShaderColor[] OldColors,int X,int Y,int W,int H,int XAdd,int YAdd){
		int XX = (int)(Mathf.Floor((float)X/(SE.Inputs[0].Float*3+1)+XAdd)*(SE.Inputs[0].Float*3+1));
		int YY = (int)(Mathf.Floor((float)Y/(SE.Inputs[0].Float*3+1)+YAdd)*(SE.Inputs[0].Float*3+1));
		return OldColors[ShaderUtil.FlatArray(XX,YY,W,H)];	
	}
	public static ShaderColor LerpBlur(float X, float Y, ShaderColor Col1,ShaderColor Col2,ShaderColor Col3,ShaderColor Col4){
		return Col1*(1f-X)*(1f-Y) + Col2*X*(1f-Y) + Col3*(1f-X)*Y + Col4*X*Y;
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor[] OldColors,int X, int Y, int W, int H){
		//Color OldColor = GetAddBlur(SE,OldColors,X,Y,W,H,0,0);
		float OldSE = SE.Inputs[0].Float;
		SE.Inputs[0].Float = Mathf.Max(0,(Mathf.Round(SE.Inputs[0].Float))-1);//Mathf.Pow(SE.Inputs[0].Float/5f,2f)*8f;
		float DXX = ((float)X/(SE.Inputs[0].Float*3+1))-Mathf.Floor((float)X/(SE.Inputs[0].Float*3+1));
		float DYY = (((float)Y/(SE.Inputs[0].Float*3+1))-Mathf.Floor((float)Y/(SE.Inputs[0].Float*3+1)));
		
		ShaderColor NewColor = LerpBlur(DXX,DYY,GetAddBlur(SE,OldColors,X,Y,W,H,0,0),GetAddBlur(SE,OldColors,X,Y,W,H,1,0),GetAddBlur(SE,OldColors,X,Y,W,H,0,1),GetAddBlur(SE,OldColors,X,Y,W,H,1,1));
		SE.Inputs[0].Float = OldSE;
		//OldColor = new Color(DXX,DYY,0,1);
		
		//Color NewColor = OldColor;
		//NewColor.a = OldColor.a;
		return NewColor;
	}
	
	public static string GenerateBase(ShaderEffect SE,ShaderLayer SL,string PixCol,string Map){
		if (SL.LayerType.Type == (int)LayerTypes.Texture){
			PixCol = "tex2Dlod("+SL.Image.Input.Get()+",float4("+Map+",0,"+SE.Inputs[0].Get()+"))";
		}
		if (SL.LayerType.Type == (int)LayerTypes.Cubemap){
			PixCol = "texCUBElod("+SL.Cube.Input.Get()+",float4("+Map+","+SE.Inputs[0].Get()+"))";		
		}
		if (SL.LayerType.Type == (int)LayerTypes.GrabDepth){
			if (SL.SpecialType.Type==0)
			PixCol = "tex2Dlod( _GrabTexture, float4("+Map+",0,"+SE.Inputs[0].Get()+"))";
			else{
				if (SL.LinearizeDepth.On)
				PixCol = "(LinearEyeDepth(tex2Dlod(_CameraDepthTexture, floatt4("+Map+",0,"+SE.Inputs[0].Get()+")).r).rrrr)";
				else
				PixCol = "(tex2Dlod(_CameraDepthTexture, float4("+Map+",0,"+SE.Inputs[0].Get()+")).rrrr)";
			}
		}
		return PixCol;
	}
}

