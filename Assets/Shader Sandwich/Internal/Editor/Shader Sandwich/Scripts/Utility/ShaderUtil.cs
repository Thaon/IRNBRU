using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;

public enum RectDir{Normal,Diag, Bottom,Right,Middle,MiddleTop};
public enum Frag{Surf, VertFrag};
public enum Vert{Surf, VertFrag};

static public class ShaderUtil{

	//static public int FragSurf = 0;
	//static public int FragVertFrag = 1;
	//static public int VertSurf = 2;
	//static public int VertVertFrag = 3;
	//public ShaderBase ShaderSandwich.Instance.OpenShader;
	static public string[] OldTooltip = new string[]{"",""};
	static public string[] Tooltip = new string[]{"",""};
	static public Vector2[] TooltipPos = new Vector2[]{new Vector2(0,0),new Vector2(0,0)};
	static public float[] TooltipAlpha = new float[]{0f,0f};
	static public double[] TooltipLastUpdate = new double[]{0f,0f};
	static public void MakeTooltip(int ID,Rect rect,string tool){
		if (rect.Contains(Event.current.mousePosition))
		Tooltip[ID] = tool;
	}
	static public void DrawTooltip(int ID,Vector2 WinSize){
		if (Event.current.type==EventType.Repaint){
			GUI.color = new Color(1,1,1,Mathf.Max(0,TooltipAlpha[ID]));
			GUI.backgroundColor = new Color(1,1,1,Mathf.Max(0,TooltipAlpha[ID]));
			if (OldTooltip[ID]!=Tooltip[ID]){
				TooltipAlpha[ID] = -5f;
			}
			if (ID==2){
				Debug.Log(ID);
				Debug.Log(Tooltip[ID]);
				Debug.Log(TooltipAlpha[ID]);
				//TooltipAlpha[ID] = 1;
			}
			OldTooltip[ID] = Tooltip[ID];
			
				TooltipPos[ID] = new Vector2(Event.current.mousePosition.x,Event.current.mousePosition.y);
				float delta = 300*((float)((EditorApplication.timeSinceStartup-TooltipLastUpdate[ID])));
//				Debug.Log(delta);
				TooltipAlpha[ID] += ((Tooltip[ID]=="")?-0.03f:0.03f)*delta;
				TooltipLastUpdate[ID] = EditorApplication.timeSinceStartup;
				TooltipAlpha[ID] = Mathf.Max(Mathf.Min(TooltipAlpha[ID],1f),-5f);
			
			Vector2 MinSize = GUI.skin.GetStyle("Tooltip").CalcSize(new GUIContent(Tooltip[ID]));
			Rect MinSize2 = new Rect(TooltipPos[ID].x,TooltipPos[ID].y-MinSize.y,MinSize.x,MinSize.y);
			//GUI.Box(MinSize2,"","Tooltip");
			if (WinSize.x<MinSize2.x+MinSize2.width&&(MinSize2.x-MinSize2.width+20>0)){
				MinSize2.x-=MinSize2.width;
			}
			GUI.Label(MinSize2,Tooltip[ID],"Tooltip");
			Tooltip[ID] = "";
		}
	}
	static public void TimerDebug(System.Diagnostics.Stopwatch sw){
		string ExecutionTimeTaken = string.Format("Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}",sw.Elapsed.Minutes,sw.Elapsed.Seconds,sw.Elapsed.TotalMilliseconds);
		UnityEngine.Debug.Log(ExecutionTimeTaken);	
	}
	static public void TimerDebug(System.Diagnostics.Stopwatch sw,string Add){
		string ExecutionTimeTaken = string.Format(Add+":\nMinutes :{0}\nSeconds :{1}\n Mili seconds :{2}",sw.Elapsed.Minutes,sw.Elapsed.Seconds,sw.Elapsed.TotalMilliseconds);
		UnityEngine.Debug.Log(ExecutionTimeTaken);	
		//sw.Reset();
	}
	static public int FlatArray(int X,int Y,int W,int H){
		return FlatArray(X,Y,W,H,null);
	}
	static public int FlatArray(int X,int Y,int W,int H,ShaderLayer SL){
		int ArrayPosX=0;
		int ArrayPosY=0;
		if (SL==null||SL.LayerType.Type!=(int)LayerTypes.Gradient){
			if (X!=0&&(W!=0))
			ArrayPosX = Mathf.Abs(X) % (W);//Mathf.Max(0,Mathf.Min(X,W-1));//Mathf.Floor(((float)X));
			if (Y!=0&&(H!=0))
			ArrayPosY = Mathf.Abs(Y) % (H);
		}
		else{
			ArrayPosX = Mathf.Max(0,Mathf.Min(X,W-1));
			ArrayPosY = Mathf.Max(0,Mathf.Min(Y,H-1));
		}

		
		//ArrayPosX = Mathf.Max(0,Mathf.Min(X,W-1));//Mathf.Floor(((float)Y));
		//ArrayPosY = Mathf.Max(0,Mathf.Min(Y,H-1));//Mathf.Floor(((float)Y));
		int ArrayPos = (ArrayPosY*(W))+ArrayPosX;
		return ArrayPos;
	}
	static public List<ShaderVar> GetAllShaderVars(){
		List<ShaderVar> SVs = new List<ShaderVar>();
		foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
			SL.UpdateShaderVars(true);
			SVs.AddRange(SL.ShaderVars);
		}
		SVs.AddRange(ShaderBase.Current.GetMyShaderVars());
		return SVs;
	}
	
	static public List<ShaderLayer> GetAllLayers(){
		List<ShaderLayer> tempList = new List<ShaderLayer>();
		foreach (List<ShaderLayer> SLL in ShaderSandwich.Instance.OpenShader.ShaderLayers())
		tempList.AddRange(SLL);
		
		return tempList;
	}
	
	static Material MixMaterial;
	static Material AddMaterial;
	static Material SubMaterial;
	static Material MulMaterial;
	static Material DivMaterial;
	static Material DarMaterial;
	static Material LigMaterial;	
	
	static Material NAMixMaterial;
	static Material NAAddMaterial;
	static Material NASubMaterial;
	static Material NAMulMaterial;
	static Material NADivMaterial;
	static Material NADarMaterial;
	static Material NALigMaterial;
	
	static public string something;
	
	static public Material GetMaterial(string S,Color col,bool UseAlpha){
		if (MixMaterial==null||AddMaterial==null||SubMaterial==null||NASubMaterial==null||NASubMaterial==null)
		{
			AddMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Additive") );
			LigMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Lighten") );
			DarMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Darken") );	
			SubMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Subtract") );	
			MulMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Multiply") );
			DivMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Divide") );
			MixMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Alpha Standard") );
			
			NAAddMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Additive") );
			NALigMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Lighten") );
			NADarMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Darken") );	
			NASubMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Subtract") );
			NAMulMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Multiply") );
			NADivMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Divide") );
			NAMixMaterial = new Material( Shader.Find("Hidden/ShaderSandwich/Standard") );
			/*string shaderText =
			"Shader \"Alpha Additive\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One ZWrite Off ColorMask RGBA Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha, texture}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			AddMaterial = new Material( shaderText );
			
			shaderText =
			"Shader \"Alpha Lighten\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp Max ZWrite Off ColorMask RGBA Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha, texture}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			LigMaterial = new Material( shaderText );
			shaderText =
			"Shader \"Alpha Darken\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp Min ZWrite Off ColorMask RGBA Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha, texture}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			DarMaterial = new Material( shaderText );			
			

			shaderText =
			"Shader \"Alpha Subtract\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp RevSub ZWrite Off ColorMask RGB Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			SubMaterial = new Material( shaderText );
			shaderText =
			"Shader \"Alpha Multiply\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,0,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend DstColor Zero ZWrite Off Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous+constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";	
			shaderText =
			"Shader \"Alpha Multiply\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend DstColor Zero ZWrite Off ColorMask RGB Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous+constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";			
			MulMaterial = new Material( shaderText );
			
			shaderText =
			"Shader \"Alpha Divide\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp RevSub ZWrite Off ColorMask RGB Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture * texture alpha}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			DivMaterial = new Material( shaderText );
			
			shaderText =
			"Shader \"Alpha Standard\" {" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\" {} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }" +
			"SubShader {" +
			"	Tags { \"Queue\" = \"Transparent\" }" +
			"	Pass {" +
			"		Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ColorMask RGBA Fog {Mode Off}" +
			"" +
			"		Lighting Off" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine texture*constant, texture*constant}" +
			"	}" +
			"}" +
			"}";
			
			MixMaterial = new Material( shaderText );
			
			
///////////////////////////////////////////////
///////////////////////////////////////////////
///////////////////////////////////////////////
			
			shaderText =
			"Shader \"Additive\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One ZWrite Off ColorMask RGBA Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine texture*constant alpha}\n"+
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			NAAddMaterial = new Material( shaderText );
			
			shaderText =
			"Shader \"Lighten\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp Max ZWrite Off ColorMask RGBA Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] {constantColor (0,0,0,1) combine texture, constant}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			NALigMaterial = new Material( shaderText );
			shaderText =
			"Shader \"Darken\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp Min ZWrite Off ColorMask RGBA Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] {constantColor (0,0,0,1) combine texture, constant}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			NADarMaterial = new Material( shaderText );			
			

			shaderText =
			"Shader \"Subtract\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp RevSub ZWrite Off ColorMask RGB Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			NASubMaterial = new Material( shaderText );
			shaderText =
			"Shader \"Multiply\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend DstColor Zero ZWrite Off ColorMask RGB Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous+constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			NAMulMaterial = new Material( shaderText );
			
			shaderText =
			"Shader \"Divide\" {\n" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\"{} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }\n" +
			"SubShader {\n" +
			"	Tags { \"Queue\" = \"Transparent\" }\n" +
			"	Pass {\n" +
			"		Blend One One BlendOp RevSub ZWrite Off ColorMask RGB Fog {Mode Off}\n" +
			"		Lighting Off\n" +
			"		SetTexture [_MainTex] { combine texture}\n" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}\n"+
			"	}\n" +
			"}\n" +
			"}\n";
			NADivMaterial = new Material( shaderText );
			
			shaderText =
			"Shader \"Standard\" {" +
			"Properties {_MainTex (\"Texture to blend\", 2D) = \"black\" {} \n"+
			"_Color (\"Main Color\", Color) = (1,1,1,1) }" +
			"SubShader {" +
			"	Tags { \"Queue\" = \"Transparent\" }" +
			"	Pass {" +
			"		Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ColorMask RGBA Fog {Mode Off}" +
			"" +
			"		Lighting Off" +
			"		SetTexture [_MainTex] {constantColor [_Color] combine texture*constant, constant}" +
			"	}" +
			"}" +
			"}";			
			
			
			
			NAMixMaterial = new Material( shaderText );*/
		}
		MixMaterial.SetColor("_Color",col);
		AddMaterial.SetColor("_Color",col);
		SubMaterial.SetColor("_Color",col);
		MulMaterial.SetColor("_Color",new Color(col.r,col.g,col.b,1f-col.a));
		DivMaterial.SetColor("_Color",col);
		DarMaterial.SetColor("_Color",col);
		LigMaterial.SetColor("_Color",col);
		
		NAMixMaterial.SetColor("_Color",col);
		NAAddMaterial.SetColor("_Color",col);
		NASubMaterial.SetColor("_Color",col);
		NAMulMaterial.SetColor("_Color",new Color(col.r,col.g,col.b,1f-col.a));
		NADivMaterial.SetColor("_Color",col);
		NADarMaterial.SetColor("_Color",col);
		NALigMaterial.SetColor("_Color",col);
		Material ReturnMat = null;
		if (UseAlpha){
			if (S=="Add")
				ReturnMat = AddMaterial;
			if (S=="Mix")
				ReturnMat = MixMaterial;
			if (S=="Subtract")
				ReturnMat = SubMaterial;
			if (S=="Multiply")
				ReturnMat = MulMaterial;
			if (S=="Divide")
				ReturnMat = DivMaterial;
			if (S=="Lighten")
				ReturnMat = LigMaterial;
			if (S=="Darken")
				ReturnMat = DarMaterial;
			if (S=="Normals Mix")
				ReturnMat = LigMaterial;
		}
		if (!UseAlpha){
			if (S=="Add")
				ReturnMat = NAAddMaterial;
			if (S=="Mix")
				ReturnMat = NAMixMaterial;
			if (S=="Subtract")
				ReturnMat = NASubMaterial;
			if (S=="Multiply")
				ReturnMat = NAMulMaterial;
			if (S=="Divide")
				ReturnMat = NADivMaterial;
			if (S=="Lighten")
				ReturnMat = NALigMaterial;
			if (S=="Darken")
				ReturnMat = NADarMaterial;
			if (S=="Normals Mix")
				ReturnMat = NALigMaterial;
		}
		
		return ReturnMat;
	}
	
	static public void Label(Rect r,string Text,int S){
	GUI.skin.label.fontSize = S;
	GUI.skin.label.wordWrap = true;
	GUI.Label(r,Text);
	}	
	static public Rect Rect2(RectDir dir,float x,float y,float width, float height)
	{	
		//if (dir==RectDir.Normal)
		if (dir==RectDir.Middle)
		return new Rect(x-width/2f,y-height/2f,width,height);	
		if (dir==RectDir.MiddleTop)
		return new Rect(x-width/2f,y,width,height);	
		if (dir==RectDir.Diag)
		return new Rect(x-width,y-height,width,height);		
		if (dir==RectDir.Bottom)
		return new Rect(x,y-height,width,height);		
		if (dir==RectDir.Right)
		return new Rect(x-width,y,width,height);
		
		return new Rect(x,y,width,height);	
	}
	static public ShaderInput GetShaderInput(ShaderInput SI){
	return new ShaderInput();//GetShaderInput(SI];
	}
	static public int InputSelection(int Type, int In)
	{
		int[] tempdropnumb = GenShaderInputArray(Type,true);
		string[] tempdropname= ShaderInputArrayToNames(tempdropnumb,true);

		return EditorGUILayout.IntPopup(In,tempdropname,tempdropnumb,GUILayout.Width(100));
	}
	static public bool IsTypeType(int Type, int Type2)
	{
		if (Type==Type2)
		return true;
		if (Type==3)
		{
			if (Type2 == 4||Type2 == 100||Type2 == 101||Type2 == 102||Type2 == 103||Type2 == 104||Type2 == 105||Type2 == 106||Type2 == 107||Type2 == 108||Type2 == 109||Type2 == 110)
			return true;
		}
		return false;
	}
	static public bool IsInputGood(int Type, ShaderInput InID)
	{
		if (
		(InID==null)||(!IsTypeType(Type,InID.Type))
		//((ShaderInputs[InID].Type!=Type&&Type!=3)||
		//(Type==3&&(ShaderInputs[InID].Type!=3&&ShaderInputs[InID].Type!=4)))
		)
		return false;
		else
		return true;
	}

	static public string GetColorInput(ref ShaderVar Value)
	{
		return GetColorInputReal(ref Value,true);
	}
	static public string GetColorInput(ref ShaderVar Value, bool OnOff)
	{
		return GetColorInputReal(ref Value,OnOff);
	}
	static public string GetColorInputReal(ref ShaderVar Value,bool OnOff)
	{
		string TempColor;
		if (IsInputGood(1,Value.Input))
		{TempColor = GetShaderInput(Value.Input).Name;}
		else
		{

		TempColor = "fixed"+(OnOff ? "4":"3")+"("+Value.Vector.r.ToString()+","+Value.Vector.g.ToString()+","+Value.Vector.b.ToString();
		if (OnOff==true)
		TempColor += ","+Value.Vector.a.ToString()+")";
		else
		TempColor += ")";
		}

		return TempColor;
	}
	static public Color GetColorInputValue(ref ShaderVar Value)
	{
		Color TempColor;
		if (IsInputGood(1,Value.Input))
		{TempColor = GetShaderInput(Value.Input).Color.ToColor();}
		else
		{TempColor = Value.Vector.ToColor();}

		return TempColor;
	}
	static public string GetNumbInput(ref ShaderVar Value)
	{
		string TempColor;
		if (IsInputGood(3,Value.Input))
		{TempColor = GetShaderInput(Value.Input).Name;}
		else
		{TempColor = Value.Float.ToString();}

		return TempColor;
	}
	
	static public int[] GenShaderInputArray( int Type)
	{
		return GenShaderInputArray_Real(ShaderSandwich.Instance.OpenShader, Type,false);
	}
	static public int[] GenShaderInputArray(int Type, bool NoOption)
	{
		return GenShaderInputArray_Real(ShaderSandwich.Instance.OpenShader,Type,NoOption);
	}
	static public int[] GenShaderInputArray_Real(ShaderBase OpenShader,int Type,bool NoOption)
	{
		List<int> RetArray =  new List<int>();
		//int InCount = 0;
		if (NoOption==true)
		RetArray.Add(-1);

		if (Type==3)
		{
			RetArray.Add(100);//Time
			RetArray.Add(101);//SinTime
			RetArray.Add(102);//ClampedSinTime
			RetArray.Add(103);//ClampedSinTime
			RetArray.Add(104);//ClampedSinTime
			RetArray.Add(105);//ClampedSinTime
			RetArray.Add(106);//ClampedSinTime
			RetArray.Add(107);//ClampedSinTime
			RetArray.Add(108);//ClampedSinTime
			if (ShaderSandwich.Instance.OpenShader.ParallaxOn.On)
			RetArray.Add(109);//ClampedSinTime
			if (ShaderSandwich.Instance.OpenShader.ShellsOn.On)
			RetArray.Add(110);//ClampedSinTime
		}

		for (int i = 0;i<=ShaderSandwich.Instance.OpenShader.ShaderInputCount;i++)
		{
			ShaderInput SI = ShaderSandwich.Instance.OpenShader.ShaderInputs[i];
			if (SI!=null)
			{
				if (SI.Type==Type||(Type==3&&(SI.Type==3||SI.Type==4)))
				{
					RetArray.Add(i);
					//InCount+=1;
				}
			}
		}



		return RetArray.ToArray();
	}
	static public string[] ShaderInputArrayToNames(int[] Arr)
	{
		return ShaderInputArrayToNames_Real(Arr,false);
	}
	static public string[] ShaderInputArrayToNames(int[] Arr,bool NoOption)
	{
		return ShaderInputArrayToNames_Real(Arr,NoOption);
	}
	static public string[] ShaderInputArrayToNames_Real(int[] Arr,bool NoOption)
	{
		string[] RetArray = new string[Arr.Length];
		int ia = 0;
		if (NoOption==true)
		{
			ia = 1;
			RetArray[0]="\n";
		}
		else
		{
			ia = 0;
		}

		for (int i = ia;i<Arr.Length;i++)
		{
			/*if (Arr[i]==-10)//Time
			RetArray[i] = ShaderInputs[100].VisName;
			else if (Arr[i]==-11)//SinTime
			RetArray[i] = ShaderInputs[101].VisName;
			else if (Arr[i]==-12)//ClampedSinTime
			RetArray[i] = ShaderInputs[102].VisName;
			else*/
			RetArray[i] = ShaderSandwich.Instance.OpenShader.ShaderInputs[Arr[i]].VisName;
		}
		return RetArray;
	}
	static public void MoveItem<T>(ref List<T> list,int OldIndex,int NewIndex){
	if (NewIndex<list.Count&&NewIndex>=0)
	{
		T item = list[OldIndex];
		list.RemoveAt(OldIndex);
		//if (NewIndex > OldIndex)
		//	NewIndex -= 1;
		
		list.Insert(NewIndex,item);
	}
	}
	static public string Ser<T>(T obj2){
	//Debug.Log(obj2);
		//XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
		
		
		DataContractSerializer serializer = new DataContractSerializer(typeof(T), null, 
		2000, //maxItemsInObjectGraph
		false, //ignoreExtensionDataObject
		true, //preserveObjectReferences : this is where the magic happens 
		null); //dataContractSurrogate
			
		
		
		/*StringWriter sww = new StringWriter();
		XmlWriter writer = new XmlTextWriter(sww);//.Create(sww);
		serializer.WriteObject(writer, obj2);	
		//xsSubmit.Serialize(sww, obj2);
		return sww.GetStringBuilder().ToString();//.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>",""); // Your xml	*/
		using (StringWriter output = new StringWriter())
		using (XmlTextWriter writer = new XmlTextWriter(output) {Formatting = Formatting.Indented})
		{
			serializer.WriteObject(writer, obj2);
			//Debug.Log(output.GetStringBuilder().ToString());
			return output.GetStringBuilder().ToString();
		}		
	}
	static public void SaveString(string fileName,string text){
		StreamWriter  sr = File.CreateText(fileName);
		sr.NewLine = "\n";
        sr.WriteLine (text);
        sr.Close();
	}
	static public List<Rect> Rects = new List<Rect>();
	static public void BeginGroup(Rect rect, GUIStyle GS){
		GUI.BeginGroup(rect,GS);
		Rects.Add(rect);
	}
	static public void BeginGroup(Rect rect){
		GUI.BeginGroup(rect);
		Rects.Add(rect);
	}
	static public void EndGroup(){
		if (Rects.Count>0){
			GUI.EndGroup();
			
			Rects.RemoveAt(Rects.Count-1);
		}
	}
	static public Vector2 BeginScrollView(Rect rect,Vector2 vec,Rect rect2,bool bo1,bool bo2){
		vec = GUI.BeginScrollView(rect,vec,rect2,bo1,bo2);
		Rects.Add(new Rect(-vec.x,-vec.y,100,100));
		return vec;
	}
	static public void EndScrollView(){
		if (Rects.Count>0){
			GUI.EndScrollView();
			
			Rects.RemoveAt(Rects.Count-1);
		}
	}	
	static public Rect GetGroupRect(){
		float x = 0;
		float y = 0;
		float w = 0;
		float h = 0;
		foreach(Rect rect in Rects){
			x+=rect.x;
			y+=rect.y;
			w=rect.width;
			h=rect.height;
		}
		return new Rect(x,y,w,h);
	}
	static public Vector2 GetGroupVector(){
		float x = 0;
		float y = 0;
		float w = 0;
		float h = 0;
		foreach(Rect rect in Rects){
			x+=rect.x;
			y+=rect.y;
			w+=rect.width;
			h+=rect.height;
		}
		return new Vector2(x,y);
	}
	static public Rect AddRect(Rect rect,Rect rect2){
		return new Rect(rect.x+rect2.x,rect.y+rect2.y,rect.width+rect2.width,rect.height+rect2.height);
	}
	static public Rect AddRectVector(Rect rect,Vector2 rect2){
		return new Rect(rect.x+rect2.x,rect.y+rect2.y,rect.width,rect.height);
	}
	static public string SaveDict(Dictionary<string,ShaderVar> D){
		string S = "";
		foreach(KeyValuePair<string, ShaderVar> entry in D){
			S += entry.Key+" #! "+entry.Value.Save();
		}
		return S;
	}
	static public void LoadLine(Dictionary<string,ShaderVar> D,string Line){
		string[] parts = Line.Replace("#^ CC0","").Split(new string[] { "#!" },StringSplitOptions.None);
		//if (parts.Length<2)
		//Debug.Log(Line);
		if (D.ContainsKey(parts[0].Trim()))
		D[parts[0].Trim()].Load(parts[1].Trim());	
	}
	static public string[] LoadLineExplode(string Line){
		return LoadLineExplode(Line,true);
	}
	static public string[] LoadLineExplode(string Line,bool RemoveCC){
		if (RemoveCC)
		return Line.Replace("#^ CC0","").Split(new string[] { "#!" },StringSplitOptions.None);
		else
		return Line.Split(new string[] { "#!" },StringSplitOptions.None);
	}
	static public string Sanitize(string S){
		if (S.IndexOf("#?")>=0)
		S = S.Substring(0,S.IndexOf("#?"));	
		
		return S;
	}
	static public bool MouseDownIn(Rect rect){
		if (Event.current.type == EventType.MouseDown&&(rect.Contains(Event.current.mousePosition))){
		Event.current.Use();
		return true;
		}
		return false;
	}
	static public bool MouseUpIn(Rect rect){
		if (Event.current.type == EventType.MouseUp&&(rect.Contains(Event.current.mousePosition))){
		Event.current.Use();
		return true;
		}
		return false;
	}
	static public bool MouseDownIn(Rect rect,bool Eat){
		if (Event.current.type == EventType.MouseDown&&(rect.Contains(Event.current.mousePosition))){
		if (Eat==true)
		Event.current.Use();
		return true;
		}
		return false;
	}
	static public void AddProSkin(Vector2 WinSize){
		AddProSkin_Real(true,WinSize);
	}
	static public GUISkin SSSkin;
	static public GUIStyle EditorFloatInput;
	static public GUIStyle EditorPopup;
	static public void Defocus(){
		GUI.FocusControl("");
	}
	static public void AddProSkin_Real(bool FI,Vector2 WinSize){
		if (SSSkin==null)
		SSSkin = (GUISkin)GUISkin.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector));//(GUISkin)GUISkin.Instantiate(GUI.skin);		
		GUI.skin = SSSkin;
		Color TextColor = new Color(0.8f,0.8f,0.8f,1f);
		Color TextColor2 = new Color(0.8f,0.8f,0.8f,1f);
		Color TextColorA = new Color(1f,1f,1f,1f);
		//Color TextColor = new Color(0f,0f,0f,1f);
		//Color TextColor2 = new Color(0f,0f,0f,1f);
		//Color TextColorA = new Color(0f,0f,0f,1f);
		Color BackgroundColor = new Color(0.18f,0.18f,0.18f,1);


			GUI.color = BackgroundColor;
			//if (!EditorGUIUtility.isProSkin)
			GUI.DrawTexture( new Rect(0,0,WinSize.x,WinSize.y), EditorGUIUtility.whiteTexture );		
			GUI.color = new Color(1f,1f,1f,1f);
			//if (!EditorGUIUtility.isProSkin)
			GUI.backgroundColor = new Color(0.25f,0.25f,0.25f,1f);
			//else
			//GUI.backgroundColor = new Color(0.5f,0.5f,0.5f,1f);
			
			GUI.skin.button.normal.textColor = TextColor;
			GUI.skin.button.active.textColor = TextColorA;
			GUI.skin.label.normal.textColor = TextColor;
			GUI.skin.box.normal.textColor = TextColorA;
			
			GUI.skin.textField.normal.textColor = TextColor2;
			GUI.skin.textField.active.textColor = TextColor2;
			GUI.skin.textField.focused.textColor = TextColor2;
			GUI.skin.textField.wordWrap = false;
			
			GUI.skin.textArea.normal.textColor = TextColor2;
			GUI.skin.textArea.active.textColor = TextColor2;
			GUI.skin.textArea.focused.textColor = TextColor2;
			
			GUI.skin.GetStyle("ButtonLeft").normal.textColor = TextColor2;
			GUI.skin.GetStyle("ButtonLeft").active.textColor = TextColor2;
			GUI.skin.GetStyle("ButtonLeft").focused.textColor = TextColor2;
			
			GUI.skin.GetStyle("ButtonRight").normal.textColor = TextColor2;
			GUI.skin.GetStyle("ButtonRight").active.textColor = TextColor2;
			GUI.skin.GetStyle("ButtonRight").focused.textColor = TextColor2;
			
			GUI.skin.GetStyle("ButtonMid").normal.textColor = TextColor2;
			GUI.skin.GetStyle("ButtonMid").active.textColor = TextColor2;
			GUI.skin.GetStyle("ButtonMid").focused.textColor = TextColor2;
			//Debug.Log(EditorStyles.popup.name);//MiniPopup
			//Debug.Log(EditorStyles.toolbar.name);//Toolbar
			//Debug.Log(EditorStyles.toolbarDropDown.name);//ToolbarDropDown
			GUI.skin.GetStyle("MiniPopup").normal.textColor = TextColor2;
			GUI.skin.GetStyle("MiniPopup").active.textColor = TextColor2;
			GUI.skin.GetStyle("MiniPopup").focused.textColor = TextColor2;
			
			if (EditorFloatInput==null&&FI==true){
				EditorFloatInput = new GUIStyle(EditorStyles.numberField);//new GUIStyle();
				EditorFloatInput.normal.textColor = TextColor2;
				EditorFloatInput.active.textColor = TextColor2;
				EditorFloatInput.focused.textColor = TextColor2;
				EditorFloatInput.hover.textColor = TextColor2;
				EditorFloatInput.onNormal.textColor = TextColor2;
				EditorFloatInput.onActive.textColor = TextColor2;
				EditorFloatInput.onFocused.textColor = TextColor2;
				EditorFloatInput.onHover.textColor = TextColor2;
				EditorFloatInput.wordWrap = false;
			}
			if (EditorPopup==null&&FI==true){
				EditorPopup = new GUIStyle(EditorStyles.popup);//new GUIStyle();
				EditorPopup.normal.textColor = TextColor2;
				EditorPopup.active.textColor = TextColor2;
				EditorPopup.focused.textColor = TextColor2;
				EditorPopup.hover.textColor = TextColor2;
			}
			GUI.skin.GetStyle("Toolbar").normal.textColor = TextColor2;
			GUI.skin.GetStyle("Toolbar").active.textColor = TextColor2;
			GUI.skin.GetStyle("Toolbar").focused.textColor = TextColor2;
			//GUI.skin.GetStyle("ToolbarDropDown").normal.textColor = TextColor2;
			//GUI.skin.GetStyle("ToolbarDropDown").active.textColor = TextColor2;
			//GUI.skin.GetStyle("ToolbarDropDown").focused.textColor = TextColor2;
			
	}
	static public void DrawEffects(Rect rect,ShaderLayer SL,List<ShaderEffect> LayerEffects,ref int SelectedEffect){//ref
		
		int SEBoxHeight = 60;
		foreach(ShaderEffect SE in LayerEffects){
			if (SEBoxHeight<(SE.Inputs.Count*20+20))
			SEBoxHeight = (SE.Inputs.Count*20+20);
		}
		SEBoxHeight+=(LayerEffects.Count*15);
		rect.height = SEBoxHeight;
		ShaderUtil.BeginGroup(rect);
		int YOffset = 0;
		GUI.Box(new Rect(0,YOffset,rect.width,rect.height),"","button");
		GUI.Box(new Rect(0,YOffset,rect.width,20),"Effects");
		YOffset+=20;
		
		GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
		ButtonStyle.padding = new RectOffset(2,2,2,2);
		ButtonStyle.margin = new RectOffset(2,2,2,2);
		
		ShaderEffect Delete = null;
		ShaderEffect MoveUp = null;
		ShaderEffect MoveDown = null;
		SelectedEffect = Mathf.Max(0,SelectedEffect);
		int y = -1;
		foreach(ShaderEffect SE in LayerEffects){
			SE.Update();
			y+=1;
			bool Selected = false;
			if (SelectedEffect<LayerEffects.Count&&LayerEffects[SelectedEffect]==SE)
			Selected = true;
			
			Selected = GUI.Toggle(new Rect(0,YOffset+y*15,rect.width-90,15),Selected,SE.Name,GUI.skin.button);
			if (Selected==true){
				if (SelectedEffect!=y)
					ShaderUtil.Defocus();
				SelectedEffect = y;
			}
			///////////////////////////////////////
			bool GUIEN = GUI.enabled;
			GUI.enabled = false;
			//UnityEngine.Debug.Log(SE.TypeS+":"+SE.HandleAlpha.ToString());
			if (SE.HandleAlpha==true&&SL.LayerType.Type!=(int)LayerTypes.Previous)
			GUI.enabled = GUIEN;
			if(Event.current.type==EventType.Repaint){
			GUI.Toggle(new Rect(rect.width-90,YOffset+y*15,15,15),SE.UseAlpha.Float==1||SE.UseAlpha.Float==2,(SE.UseAlpha.Float==1) ? ShaderSandwich.AlphaOn: ShaderSandwich.AlphaOff,ButtonStyle);
			GUI.Button(new Rect(rect.width-90+100,YOffset+y*15,15,15),"");
			}
			else {
				GUI.Toggle(new Rect(rect.width-90+100,YOffset+y*15,15,15),false,"");
				if (GUI.Button(new Rect(rect.width-90,YOffset+y*15,15,15),""))
				SE.UseAlpha.Float+=1;
				if (SE.UseAlpha.Float>2)
				SE.UseAlpha.Float = 0;
				
			}
			GUI.enabled = GUIEN;
			////////////////////////////////////////
			SE.Visible = GUI.Toggle(new Rect(rect.width-75,YOffset+y*15,30,15),SE.Visible,SE.Visible ? ShaderSandwich.EyeOpen: ShaderSandwich.EyeClose,ButtonStyle);
			if (GUI.Button(new Rect(rect.width-45,YOffset+y*15,15,15),ShaderSandwich.UPArrow,ButtonStyle))
			MoveUp = SE;
			if (GUI.Button(new Rect(rect.width-30,YOffset+y*15,15,15),ShaderSandwich.DOWNArrow,ButtonStyle))
			MoveDown = SE;
			if (GUI.Button(new Rect(rect.width-15,YOffset+y*15,15,15),ShaderSandwich.CrossRed,ButtonStyle))
			Delete = SE;
		}
		if (Delete!=null){
			if (LayerEffects.IndexOf(Delete)<SelectedEffect)
			SelectedEffect-=1;
			LayerEffects.Remove(Delete);
		}
		if (MoveUp!=null){
			if (LayerEffects.IndexOf(Delete)<SelectedEffect)
			SelectedEffect-=1;
			
			ShaderUtil.MoveItem(ref LayerEffects,LayerEffects.IndexOf(MoveUp),LayerEffects.IndexOf(MoveUp)-1);
		}
		if (MoveDown!=null){
			if (LayerEffects.IndexOf(Delete)<SelectedEffect)
			SelectedEffect+=1;
			
			ShaderUtil.MoveItem(ref LayerEffects,LayerEffects.IndexOf(MoveDown),LayerEffects.IndexOf(MoveDown)+1);
		}
		y+=1;
		if (GUI.Button(ShaderUtil.Rect2(RectDir.Right,rect.width,YOffset-20,20,20),ShaderSandwich.Plus,ButtonStyle)){
			GenericMenu toolsMenu = new GenericMenu();
			foreach(string Ty in ShaderSandwich.EffectsList){
				ShaderEffect SE = ShaderEffect.CreateInstance<ShaderEffect>();
				SE.ShaderEffectIn(Ty,SL);
				toolsMenu.AddItem(new GUIContent(SE.Name), false, SL.AddLayerEffect,Ty);
			}
			toolsMenu.DropDown(ShaderUtil.Rect2(RectDir.Right,rect.width,YOffset-20,20,20));
			//EditorGUIUtility.ExitGUI();
		}
		SelectedEffect = Mathf.Max(0,SelectedEffect);
		if (SelectedEffect<LayerEffects.Count&&LayerEffects[SelectedEffect]!=null)
		LayerEffects[SelectedEffect].Draw(new Rect(0,YOffset+y*15,rect.width,SEBoxHeight));
		ShaderUtil.EndGroup();
	}
	public static string CodeName(string NewName){
		NewName = NewName.Replace(" ","_");
		NewName = Regex.Replace(NewName, "[^a-zA-Z0-9_]","");
		NewName = NewName.Replace("__","_a");
		NewName = NewName.Replace("__","_a");
		NewName = NewName.Replace("__","_a");
		NewName = NewName.Replace("__","_a");
		return NewName;
	}
	
}