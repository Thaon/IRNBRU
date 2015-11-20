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
using System.Xml;


[System.Serializable]
public class ShaderLayerList : IEnumerable<ShaderLayer>{
	public ShaderVar Name = new ShaderVar("LayerListName","");
	public ShaderVar NameUnique = new ShaderVar("LayerListUniqueName","");
	public string Description = "";
	public List<ShaderLayer> SLs = new List<ShaderLayer>();
	public string CodeName = "";
	public string InputName = "";
	public ShaderVar EndTag = new ShaderVar("EndTag","rgb");
	public string Function = "";
	public int GCUses = 0;
	public ShaderVar IsMask = new ShaderVar("Is Mask",false);
	public ShaderVar IsLighting = new ShaderVar("Is Lighting",false);
	public bool Parallax = false;
	public int Inputs = 0;
	public string LayerCatagory = "";
	public Color BaseColor;
	public Vector2 Scroll = new Vector2(0,0);
	public int Count{
		get{
			return SLs.Count;
		}
		set{
			
		}
	}

	public bool UsedShell = false;
	public bool UsedBase = false;
	public string GCVariable(){
		string ShaderCode = "";
		
		if (EndTag.Text.Length==1)
		ShaderCode+="float "+CodeName+" = 1";
		if (EndTag.Text.Length==2)
		ShaderCode+="float2 "+CodeName+" = float2(1,1)";
		if (EndTag.Text.Length==3)
		ShaderCode+="float3 "+CodeName+" = float3(1,1,1)";
		if (EndTag.Text.Length==4)
		ShaderCode+="float4 "+CodeName+" = float4(1,1,1,1)";

		ShaderCode+=";\n";
		return "	//Set default mask color\n		"+ShaderCode;
	}
	public void FixParents(){
		foreach(ShaderLayer SL in SLs)
		SL.Parent = this;
	}
    public IEnumerator<ShaderLayer> GetEnumerator()
    {
		//Debug.Log(EndTag.Text+Name.Text);
        return SLs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
		//Debug.Log(EndTag.Text+Name.Text);
        return GetEnumerator();
    }
	public ShaderLayerList(string UN,string N,string IN,string CN,string ET,string F,Color BS){
		Name.Text = N;
		NameUnique.Text = UN;
		InputName = IN;
		CodeName = CN;
		EndTag.Text = ET;
		Function = F;
		BaseColor = BS;
		//Debug.Log(ET+N);
	}
	public ShaderLayerList(string UN,string N,string D,string IN,string CN,string ET,string F,Color BS){
	//Debug.Log(ET+N);
		Name.Text = N;
		NameUnique.Text = UN;
		Description = D;
		InputName = IN;
		CodeName = CN;
		EndTag.Text = ET;
		Function = F;
		BaseColor = BS;
	}
	public ShaderLayerList(){
	}
	public void Add(ShaderLayer SL){
		SLs.Add(SL);
		if (Name.Text!="Diffuse"&&Name.Text!="Emission")
		SL.Color.Vector = new ShaderColor(BaseColor);
		else
		if (Name.Text=="Emission")
		SL.Color.Vector = new ShaderColor(0f,0.2f,0.6f,1f);
		SL.Parent = this;
	}
	public void AddC(ShaderLayer SL){
		SLs.Add(SL);
		SL.Parent = this;
	}
	public void RemoveAt(int Pos){
		SLs.RemoveAt(Pos);
	}
	public void MoveItem(int OldIndex,int NewIndex){
	if (NewIndex<SLs.Count&&NewIndex>=0)
	{
		ShaderLayer item = SLs[OldIndex];
		SLs.RemoveAt(OldIndex);
		//if (NewIndex > OldIndex)
		//	NewIndex -= 1;
		
		SLs.Insert(NewIndex,item);
	}
	}
	Texture2D Icon;
	public void UpdateIcon(Vector2 rect){
		//Debug.Log("Update!");
		Color[] colors = new Color[(int)(rect.x*rect.y)];
		for(int i = 0;i<colors.Length;i++){
			colors[i] = BaseColor;//new Color(0.8f,0.8f,0.8f,1f);
			if (EndTag.Text=="r")
				colors[i] = new Color(colors[i].r,colors[i].r,colors[i].r,colors[i].a);
			if (EndTag.Text=="g")
				colors[i] = new Color(colors[i].g,colors[i].g,colors[i].g,colors[i].a);
			if (EndTag.Text=="b")
				colors[i] = new Color(colors[i].b,colors[i].b,colors[i].b,colors[i].a);
			if (EndTag.Text=="a")
				colors[i] = new Color(colors[i].a,colors[i].a,colors[i].a,1);
		}
		if (Icon==null||(Icon.width!=(int)rect.x||Icon.height!=(int)rect.y))
		Icon = new Texture2D((int)rect.x,(int)rect.y,TextureFormat.ARGB32,false);
		
		List<System.Reflection.MethodInfo> Meths = new List<System.Reflection.MethodInfo>();
		List<int> MethTypes = new List<int>();

		foreach(ShaderLayer SL in SLs)
		{
			Meths.Clear();
			MethTypes.Clear();
			foreach(ShaderEffect SE in SL.LayerEffects){
							Meths.Add(ShaderEffect.GetMethod(SE.TypeS,"Preview"));
							int MS = 0;
							if (Meths[Meths.Count-1]!=null){
							if (Meths[Meths.Count-1].GetParameters().Length==6)
							MS = 1;
							else
							if (Meths[Meths.Count-1].ReturnType==typeof(Vector2))
							MS = 2;
							}
							MethTypes.Add(MS);
			}		
			Texture2D LayerTex = SL.GetTexture();
			
			if (LayerTex!=null||(SL.LayerType.Type==(int)LayerTypes.Texture&&SL.Image.Image==null)||SL.LayerType.Type == (int)LayerTypes.Previous){
			int mipWidth = 0;
			int mipHeight = 0;
			Color[] OrigLayerPixels = null;
			ShaderColor[] LayerPixels = null;
			if (SL.LayerType.Type != (int)LayerTypes.Previous){
				if (!SL.GetSample()){
					Color[] TempLayerPixels = LayerTex.GetPixels(0);
					OrigLayerPixels = new Color[TempLayerPixels.Length];
					Array.Copy(TempLayerPixels,OrigLayerPixels,TempLayerPixels.Length);
					LayerPixels = new ShaderColor[TempLayerPixels.Length];
					mipWidth=LayerTex.width;//Mathf.Max(1,LayerTex.width>>0);
					mipHeight=LayerTex.height;//Mathf.Max(1,LayerTex.height>>0);
				}
				else{
					LayerPixels = new ShaderColor[(int)(rect.x*rect.y)];
					mipWidth = (int)rect.x;
					mipHeight = (int)rect.y;
				}
//				int i = -1;
				/*foreach(Color TC in TempLayerPixels){
					i++;
					LayerPixels[i] = (ShaderColor)TC;
				}*/
				
				
				
				int XX = 0;
				int YY = 0;	
				int SEI = -1;
				for(int x = 0;x<(int)mipWidth;x++){
					for(int y = 0;y<(int)mipHeight;y++){
						SEI = -1;
						XX = x;
						YY = y;
						SL.LayerEffects.Reverse();
						Meths.Reverse();
						MethTypes.Reverse();
						foreach(ShaderEffect SE in SL.LayerEffects){
							SEI+=1;
							
							var Meth = Meths[SEI];
							int MethType = MethTypes[SEI];	
							if (SE.Visible){
								if (Meth!=null){
									if (MethType==2&&SL.LayerType.Type != (int)LayerTypes.Previous){
										Vector2 XY = (Vector2)Meth.Invoke(null, new object[]{SE,new Vector2(XX,YY),mipWidth,mipHeight});
										XX = (int)(XY.x);
										YY = (int)(XY.y);
									}
								}
							}
						}
						SL.LayerEffects.Reverse();
						Meths.Reverse();
						MethTypes.Reverse();
						if (!SL.GetSample())
							LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight,SL)] = (ShaderColor)OrigLayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight,SL)];
						else
							LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight,SL)] = SL.GetSample((float)XX/((float)mipWidth),(float)YY/((float)mipHeight));
					}
				}

					
				//if (EndTag.Text!="a"){
					XX = 0;
					YY = 0;
					SEI = -1;
						
					foreach(ShaderEffect SE in SL.LayerEffects){
						SEI+=1;
						if (SE.Visible){
							var Meth = Meths[SEI];
							int MethType = MethTypes[SEI];					
							for(int x = 0;x<(int)mipWidth;x++){
								for(int y = 0;y<(int)mipHeight;y++){
									XX = x;
									YY = y;
									ShaderColor OldCol = new ShaderColor(LayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight)].r,LayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight)].g,LayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight)].b,LayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight)].a);
									if (Meth!=null){
										if (MethType==1&&SL.LayerType.Type != (int)LayerTypes.Previous)
										LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight)] = (ShaderColor)Meth.Invoke(null, new object[]{SE, LayerPixels,XX,YY,mipWidth,mipHeight});
										
										//if (MethType==0)
										//Debug.Log((Color)Meth.Invoke(null, new object[]{SE, (Color)LayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight)]}));
										if (MethType==0)
										LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight)] = (ShaderColor)Meth.Invoke(null, new object[]{SE, LayerPixels[ShaderUtil.FlatArray(XX,YY,mipWidth,mipHeight)]});
										
										if (SE.UseAlpha.Float==0)
										LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight)].a = OldCol.a;
										if (SE.UseAlpha.Float==2){
											float A = LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight)].a;
											LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight)] = OldCol;
											LayerPixels[ShaderUtil.FlatArray(x,y,mipWidth,mipHeight)].a = A;
										}
									}
								}
							}
						}
					}
				}
				//}
				//SL.LayerType.Type != (int)LayerTypes.Previous
				//Debug.Log(LayerPixels.Length);
				for(int x = 0;x<(int)rect.x;x++){
					for(int y = 0;y<(int)rect.y;y++){
					
						//float Progress = 0.5f;
						//EditorUtility.DisplayProgressBar("Updating Preview", "", Progress);
					
						ShaderColor OldColor = (ShaderColor)colors[x+(y*(int)rect.x)];
						ShaderColor LayerPixel = OldColor;
						ShaderColor LayerPixelOrig = OldColor;
						if (SL.LayerType.Type != (int)LayerTypes.Previous){
							float ArrayPosX = Mathf.Floor(((float)x/rect.x)*((float)mipWidth));
							float ArrayPosY = Mathf.Floor(((float)y/rect.y)*((float)mipHeight));
						
							float ArrayPos = ShaderUtil.FlatArray((int)ArrayPosX,(int)ArrayPosY,mipWidth,mipHeight);
							LayerPixel = LayerPixels[(int)ArrayPos];
						}
						else{
							//LayerPixel = new Color(1,0,0,1);
							int SEI = -1;
							foreach(ShaderEffect SE in SL.LayerEffects){
							SEI+=1;
							if (SE.Visible){
								int MethType = MethTypes[SEI];
								var Meth = Meths[SEI];
								
								if (MethType == 0){
								LayerPixel = (ShaderColor)Meth.Invoke(null, new object[]{SE, LayerPixel});}
							}
							}
						}
						
						
						
						
						
							LayerPixelOrig =LayerPixel;
							if (SL.MixType.Names[SL.MixType.Type]=="Add")
							LayerPixel += OldColor;
							if (SL.MixType.Names[SL.MixType.Type]=="Subtract")
							LayerPixel = OldColor-LayerPixel;
							if (SL.MixType.Names[SL.MixType.Type]=="Divide")
							//LayerPixel = OldColor-LayerPixel;
							LayerPixel = new ShaderColor(OldColor.r/LayerPixel.r,OldColor.g/LayerPixel.g,OldColor.b/LayerPixel.b,OldColor.a/LayerPixel.a);
							if (SL.MixType.Names[SL.MixType.Type]=="Multiply")
							LayerPixel *= OldColor;
							if (SL.MixType.Names[SL.MixType.Type]=="Lighten")
							LayerPixel = new ShaderColor(Mathf.Max(OldColor.r,LayerPixel.r),Mathf.Max(OldColor.g,LayerPixel.g),Mathf.Max(OldColor.b,LayerPixel.b),Mathf.Max(OldColor.a,LayerPixel.a));
							if (SL.MixType.Names[SL.MixType.Type]=="Darken")
							LayerPixel = new ShaderColor(Mathf.Min(OldColor.r,LayerPixel.r),Mathf.Min(OldColor.g,LayerPixel.g),Mathf.Min(OldColor.b,LayerPixel.b),Mathf.Min(OldColor.a,LayerPixel.a));
							if (SL.MixType.Names[SL.MixType.Type]=="Normals Mix"){
								ShaderColor BOldColor = (OldColor);//-new ShaderColor(0.5f,0.5f,0.5f,0.5f))*2f;
								ShaderColor BLayerPixel = (LayerPixel);//-new ShaderColor(0.5f,0.5f,0.5f,0.5f))*2f;
								Vector3 NormalBlend = new Vector3(BOldColor.r+BLayerPixel.r,BOldColor.g+BLayerPixel.g,OldColor.b);
								//LayerPixel /=  Mathf.Max(LayerPixel.b,Mathf.Max(LayerPixel.r,LayerPixel.g));//(Color)(((Vector4)LayerPixel).normalized);
								NormalBlend.Normalize();
								//NormalBlend = NormalBlend/2f+new Vector3(0.5f,0.5f,0.5f);
								LayerPixel = new ShaderColor(NormalBlend.x,NormalBlend.y,NormalBlend.z,LayerPixel.a);
							}
							if (SL.MixType.Names[SL.MixType.Type]=="Dot")
							LayerPixel = new ShaderColor(Vector3.Dot(new Vector3(OldColor.r,OldColor.g,OldColor.b),new Vector3(LayerPixelOrig.r,LayerPixelOrig.g,LayerPixelOrig.b)));
							
							if (SL.UseAlpha.On&&SL.Stencil.Obj!=null){
							LayerPixel = ShaderColor.Lerp(OldColor,LayerPixel,
							LayerPixelOrig.a*
							SL.MixAmount.Float*
							(((ShaderLayerList)(SL.Stencil.Obj)).GetIcon().GetPixel(x,y).r));
							}
							else
							if (SL.UseAlpha.On)
							LayerPixel = ShaderColor.Lerp(OldColor,LayerPixel,LayerPixelOrig.a*SL.MixAmount.Float);
							else
							if (SL.Stencil.Obj!=null)
							LayerPixel = ShaderColor.Lerp(OldColor,LayerPixel,
							SL.MixAmount.Float*
							(((ShaderLayerList)(SL.Stencil.Obj)).GetIcon().GetPixel(x,y).r));
							else
							LayerPixel = ShaderColor.Lerp(OldColor,LayerPixel,SL.MixAmount.Float);
						//}
						//if (!SL.UseAlpha.On){
						
						if (EndTag.Text=="r")
							LayerPixel = new ShaderColor(LayerPixel.r,LayerPixel.r,LayerPixel.r,1);
						if (EndTag.Text=="g")
							LayerPixel = new ShaderColor(LayerPixel.g,LayerPixel.g,LayerPixel.g,1);
						if (EndTag.Text=="b")
							LayerPixel = new ShaderColor(LayerPixel.b,LayerPixel.b,LayerPixel.b,1);
						if (EndTag.Text=="a")
							LayerPixel = new ShaderColor(LayerPixel.a,LayerPixel.a,LayerPixel.a,1);
						
						//if (EndTag.Text.Length!=1)
						LayerPixel.a = 1;//OldColor.a;
						//}

						colors[x+(y*(int)rect.x)] = (Color)LayerPixel;
					}
				}
				
			}
		}
		if (CodeName=="o.Normal"){
			for(int x = 0;x<(int)rect.x;x++){
				for(int y = 0;y<(int)rect.y;y++){
					ShaderColor LayerPixel2 = (ShaderColor)colors[ShaderUtil.FlatArray(x,y,(int)rect.x,(int)rect.y)];
					LayerPixel2/=2f;
					LayerPixel2+= new ShaderColor(0.5f,0.5f,0.5f,0f);	
					LayerPixel2.a = 1f;
					colors[ShaderUtil.FlatArray(x,y,(int)rect.x,(int)rect.y)] = (Color)LayerPixel2;
				}
			}
		}
		/*if (Function!=""){
			ShaderEffect SE = new ShaderEffect(Function);
			Meth = ShaderEffect.GetMethod(Function,"Preview");
			for(int x = 0;x<(int)rect.x;x++){
				for(int y = 0;y<(int)rect.y;y++){
					Color LayerPixel2 = colors[ShaderUtil.FlatArray(x,y,(int)rect.x,(int)rect.y)];
					LayerPixel2 = (Color)Meth.Invoke(null, new object[]{SE, LayerPixel2});
					LayerPixel2.a = 1;
					colors[ShaderUtil.FlatArray(x,y,(int)rect.x,(int)rect.y)] = LayerPixel2;
				}
			}
		}		*/
		Icon.SetPixels(colors);
		Icon.Apply(false);	
		//EditorUtility.ClearProgressBar();
	}
	public void DrawIcon(Rect rect, bool Update){
		if (Icon==null){
			UpdateIcon(new Vector2(rect.width,rect.height));
		}
		GUI.DrawTexture(rect,Icon);
	}
	public Texture2D GetIcon(){
		if (Icon==null){
			UpdateIcon(new Vector2(70,70));
		}
		return Icon;
	}
	public Dictionary<string,ShaderVar> GetSaveLoadDict(){
		Dictionary<string,ShaderVar> D = new Dictionary<string,ShaderVar>();
		
		D.Add(NameUnique.Name,NameUnique);
		D.Add(Name.Name,Name);
		D.Add(IsMask.Name,IsMask);
		D.Add(IsLighting.Name,IsLighting);
		D.Add(EndTag.Name,EndTag);
		return D;
	}	
	public string Save(){
		string S = "BeginShaderLayerList\n";
		S+=ShaderUtil.SaveDict(GetSaveLoadDict());

		foreach (ShaderLayer SL in SLs){
			S+=SL.Save();
		}
		S += "EndShaderLayerList\n";
		//public List<ShaderLayer> SLs = new List<ShaderLayer>();
		return S;
	}
	static public void Load(StringReader S,List<ShaderLayerList> SLLs,List<ShaderLayerList> SLMs){
		ShaderLayerList SLL = null;
		string ShaderLayerUniqueName = ShaderUtil.LoadLineExplode(S.ReadLine())[1].Trim();
		string ShaderLayerName = ShaderUtil.LoadLineExplode(S.ReadLine())[1].Trim();
		string ShaderLayerIsMaskb = ShaderUtil.LoadLineExplode(S.ReadLine())[1].Trim();
		bool ShaderLayerIsMask = bool.Parse(ShaderLayerIsMaskb);
		bool ShaderLayerIsLighting = false;
		if (float.Parse(ShaderSandwich.Instance.OpenShader.FileVersion)>0.9f){
		string ShaderLayerIsLightingb = ShaderUtil.LoadLineExplode(S.ReadLine())[1].Trim();
		ShaderLayerIsLighting = bool.Parse(ShaderLayerIsLightingb);
		}
		foreach(ShaderLayerList SLL2 in SLLs){
			if (SLL2.NameUnique.Text==ShaderLayerUniqueName)
			SLL = SLL2;
		}
		if (SLL==null){
			SLL = new ShaderLayerList("Mask"+SLMs.Count.ToString(),"Mask"+SLMs.Count.ToString(),"Mask"+SLMs.Count.ToString(),"Mask"+SLMs.Count.ToString(),"rgb","",new Color(1f,1f,1f,1f));
			SLL.IsMask.On=true;
			SLMs.Add(SLL);
		}
		if (SLL!=null){
			SLL.Name.Text = ShaderLayerName;
			var D = SLL.GetSaveLoadDict();
			while(1==1){
				string Line =  S.ReadLine();
				if (Line!=null){
					if(Line=="EndShaderLayerList")break;
					
					if (Line.Contains("#!")){
					if (ShaderUtil.LoadLineExplode(Line)[0]!="EndTag"||(ShaderUtil.LoadLineExplode(Line)[0]=="EndTag"&&ShaderLayerIsMask==true))
					ShaderUtil.LoadLine(D,Line);
					}
					else
					if (Line=="BeginShaderLayer")
					SLL.SLs.Add(ShaderLayer.Load(S));
				}
				else
				break;
			}
			SLL.IsLighting.On = ShaderLayerIsLighting;
			SLL.UpdateIcon(new Vector2(70,70));
		}
		
	}
	public override string ToString(){
		return Name.Text;
	}
}