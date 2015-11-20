#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
#define PRE_UNITY_5
#else
#define UNITY_5
#endif
#pragma warning disable 0618
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using SU = ShaderUtil;
using UnityEngine.Rendering;
[System.Serializable]
public class ShaderPreview : EditorWindow {
	
	public GameObject previewCamPoint;
	public GameObject previewCamPointTarget;
	public GameObject previewCam;
	public GameObject previewCamW;
	public GameObject previewObject;
	public GameObject previewBack;
	public GameObject Light1;
	public GameObject Light2;
	public Mesh previewMesh;
	public Material previewMat;
	public Material previewMat2;
	public Material previewMatW;
	public Material backMat;
	
	public Cubemap defaultBackCube;
	
	public Mesh customObject;
	public Cubemap backCube;
	public Cubemap BlackCube;
	
	public int MeshType;
	public List<Mesh> Meshes;
	public bool AutoRotate = false;
	public bool InDrag = false;
	public bool InResize = false;
	public bool Wireframe = false;
	public bool SmoothDamp = true;
	public bool Expose = false;
	public int TopBarHeight = 66;
	static public ShaderPreview Instance;
	//[MenuItem ("Window/Shader Sandwich Preview")]
	public static void Init () {
		ShaderPreview windowG = (ShaderPreview)EditorWindow.GetWindow (typeof (ShaderPreview));
		Instance = windowG;
		windowG.previewCam = null;
		#if UNITY_5
		windowG.defaultBackCube = ShaderSandwich.DayCube;
		#else
		windowG.defaultBackCube = ShaderSandwich.KitchenCube;
			#if !UNITY_PRO_LICENCE
			EditorUtility.DisplayDialog("Unity 4 Free","Hi! Unity 4 Free does not support render textures, so the preview window will render using a technique called handles. However, this does not always look correct and can have z-writing issues. If you want, I'd recommend turning on Expose and opening the file ShaderSandwichPreviewScene.unity and using that as the preview window :).","Ok");
			#endif
		#endif
		windowG.Setup();
		windowG.wantsMouseMove = true;
		windowG.minSize = new Vector2(200,200);
		//windowG.maxSize = new Vector2(400,400);
		windowG.title = "Shader Preview";
//		Debug.Log(windowG.previewMat);

		//windowG.Meshes = null;
		
	}
	void Update(){
		if (ShaderSandwich.Instance==null)
		Close();

		#if UNITY_5
		defaultBackCube = ShaderSandwich.DayCube;
		#else
		defaultBackCube = ShaderSandwich.KitchenCube;
		#endif
	}
	float ToGreyscale(Color Col){
		return (Col.r+Col.g+Col.b)/3f;//Col.grayscale;//(Col.r+Col.g+Col.b)/3f;
		 //return ((Mathf.Max(Col.r,Mathf.Max(Col.b,Col.g))));
	}
	void CalcAmbientLight(Cubemap TempCube){
			//AMBIENT LIGHT CALCULATION
			string path = AssetDatabase.GetAssetPath(TempCube);
			TextureImporter ti =  TextureImporter.GetAtPath(path) as TextureImporter;
			bool OldIsReadable = false;
			int OldMaxSize = 1;
			if (ti!=null){
					OldIsReadable = ti.isReadable;
					ti.isReadable = true;
					OldMaxSize = ti.maxTextureSize;
					ti.maxTextureSize = 32;
					AssetDatabase.ImportAsset(path);
			}
			//CubeResize = new Texture2D(70,70,TextureFormat.ARGB32,false);
			//Color[] colors = new Color[(int)(CubeResize.width*CubeResize.height)];
			Cubemap UseCube = TempCube;
			int MipmapLevel = 0;
			try {
				TempCube.GetPixel(CubemapFace.PositiveX,0,0);
			}
			catch {
				UseCube = defaultBackCube;
			}
			int i = 1;
			if (UseCube.height>4){
				for (i=1;i<9;i++){
					bool Continue = false;
					try {
						Continue = false;
						Color[] colors2 = UseCube.GetPixels(CubemapFace.PositiveX,i);
						if (colors2!=null&&colors2.Length>1)
						Continue = true;
					}
					catch{
						Continue = false;
					}
	//				Debug.Log(Continue);
					if (!Continue)
					break;
				}
			}
			MipmapLevel = i-1;
//			Debug.Log(MipmapLevel);
			
			
			Color PosXo = UseCube.GetPixel(CubemapFace.PositiveX,0,0);
			Color NegXo = UseCube.GetPixel(CubemapFace.NegativeX,0,0);
			Color PosYo = UseCube.GetPixel(CubemapFace.PositiveY,0,0);
			Color NegYo = UseCube.GetPixel(CubemapFace.NegativeY,0,0);
			Color PosZo = UseCube.GetPixel(CubemapFace.PositiveZ,0,0);
			Color NegZo = UseCube.GetPixel(CubemapFace.NegativeZ,0,0);
			
			if (MipmapLevel!=0){
				PosXo = UseCube.GetPixels(CubemapFace.PositiveX,MipmapLevel)[0];
				NegXo = UseCube.GetPixels(CubemapFace.NegativeX,MipmapLevel)[0];
				PosYo = UseCube.GetPixels(CubemapFace.PositiveY,MipmapLevel)[0];
				NegYo = UseCube.GetPixels(CubemapFace.NegativeY,MipmapLevel)[0];
				PosZo = UseCube.GetPixels(CubemapFace.PositiveZ,MipmapLevel)[0];
				NegZo = UseCube.GetPixels(CubemapFace.NegativeZ,MipmapLevel)[0];
			}
			
			Color PosX = Color.Lerp(PosXo,NegYo,0.5f);
			Color NegX = Color.Lerp(NegXo,NegYo,0.5f);
			Color PosY = Color.Lerp(PosYo,PosXo,0.2f);
			Color NegY = Color.Lerp(NegYo,NegXo,0.2f);
			Color PosZ = Color.Lerp(PosZo,NegYo,0.5f);
			Color NegZ = Color.Lerp(NegZo,NegYo,0.5f);
			
			#if UNITY_5
			SphericalHarmonicsL2 L = new SphericalHarmonicsL2();
			L.AddDirectionalLight(new Vector3(1,0,0),PosX,ToGreyscale(PosX));
			L.AddDirectionalLight(new Vector3(0,1,0),PosY,ToGreyscale(PosY));
			L.AddDirectionalLight(new Vector3(0,0,1),PosZ,ToGreyscale(PosZ));
			
			L.AddDirectionalLight(new Vector3(-1,0,0),NegX,ToGreyscale(NegX));
			L.AddDirectionalLight(new Vector3(0,-1,0),NegY,ToGreyscale(NegY));
			L.AddDirectionalLight(new Vector3(0,0,-1),NegZ,ToGreyscale(NegZ));
			

			(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).NewAmbient = L;
			#endif
			Color C = (PosX/6f)+(NegX/6f)+(PosY/6f)+(NegY/6f)+(PosZ/6f)+(NegZ/6f);
			//Debug.Log(C);
			(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).NewAmbientColor = C;
			//L.AddDirectionalLight(new Vector3(0,0,1),new Color(0.5f,0,0),1f);
			//Debug.Log(ToGreyscale(new Color(0.5f,0,0)));
			//CubeResize.SetPixels(colors);
			//CubeResize.Apply(false,false);
			if (ti!=null){
				ti.isReadable = OldIsReadable;
				ti.maxTextureSize = OldMaxSize;
				AssetDatabase.ImportAsset(path);
			}
	}
	void Setup(){
		if (Meshes==null||Meshes.Count!=3){
			Meshes = new List<Mesh>();
			//GameObject Prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
			GameObject Prim = (GameObject)GameObject.Instantiate( EditorGUIUtility.Load("Shader Sandwich/Models/SimpleCube.fbx") as GameObject);
			Meshes.Add(((MeshFilter)(Prim.GetComponent("MeshFilter"))).sharedMesh);
			GameObject.DestroyImmediate(Prim,true);
			Prim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Meshes.Add(((MeshFilter)(Prim.GetComponent("MeshFilter"))).sharedMesh);
			GameObject.DestroyImmediate(Prim,true);
			Prim = (GameObject)GameObject.Instantiate( EditorGUIUtility.Load("Shader Sandwich/Models/Monkey.fbx") as GameObject);
			Meshes.Add(((MeshFilter)(Prim.GetComponent("MeshFilter"))).sharedMesh);
			GameObject.DestroyImmediate(Prim,true);
			Prim = (GameObject)GameObject.Instantiate( EditorGUIUtility.Load("Shader Sandwich/Models/TesselatedCube.fbx") as GameObject);
			Meshes.Add(((MeshFilter)(Prim.GetComponent("MeshFilter"))).sharedMesh);
			GameObject.DestroyImmediate(Prim,true);
			
		}
		if ((previewCam==null||previewObject==null||previewCamPoint==null||previewCamPointTarget==null||backMat==null||previewBack==null))
		{
			Transform[] obs = FindObjectsOfType(typeof(Transform)) as Transform[];

			//foreach(GameObject ob in obs)
			for(int i=0;i<obs.Length;i++)
			{
				if (obs[i]!=null){
					GameObject ob = obs[i].gameObject;

					if (ob!=null)
					{

						if (ob.name=="PreviewCamera"||ob.name=="PreviewObject"||ob.name=="SSPrevL"||ob.name=="SSPrevL1"||ob.name=="SSPrevL2"||ob.name=="PreviewBack")//||ob.hideFlags!=HideFlags.None)
						{	//Debug.Log(ob.name);
							GameObject.DestroyImmediate(ob,true);
						}
					}
				}
			}
			previewCamPoint = new GameObject();
			previewCamPoint.hideFlags = HideFlags.HideInHierarchy;//HideFlags.HideAndDontSave;
			previewCamPoint.name="PreviewCamPoint";
			previewCamPoint.transform.eulerAngles = new Vector3(-30f,0,0);
			
			previewCamPointTarget = new GameObject();
			previewCamPointTarget.hideFlags = HideFlags.HideInHierarchy;//HideFlags.HideAndDontSave;
			previewCamPointTarget.name="previewCamPointTarget";
			previewCamPointTarget.transform.eulerAngles = new Vector3(-30f,0,0);

			previewCam = new GameObject();
			previewCam.hideFlags = HideFlags.HideInHierarchy;
			previewCam.name = "PreviewCamera";
			Camera CamComp = previewCam.AddComponent<Camera>() as Camera;
			CamComp.depth=-20;
			#if !UNITY_5_0
			CamComp.clearFlags = CameraClearFlags.SolidColor;
			CamComp.backgroundColor = new Color(12f/255f,12f/255f,12f/255f,0);
			#endif
			CamComp.renderingPath = RenderingPath.Forward;
			//CamComp.transparencySortMode = TransparencySortMode.Perspective;
			previewCam.AddComponent<PreviewCameraSet>();
			previewCam.transform.position = new Vector3(0,0,-2.175257f);
			previewCam.transform.parent = previewCamPoint.transform;
			
			
			/*previewCamW = new GameObject();
			previewCamW.hideFlags = HideFlags.HideInHierarchy;
			previewCamW.name = "PreviewCamera";
			CamComp = previewCamW.AddComponent<Camera>() as Camera;
			CamComp.depth=-20;
			#if !UNITY_5_0
			CamComp.clearFlags = CameraClearFlags.SolidColor;
			CamComp.backgroundColor = new Color(12f/255f,12f/255f,12f/255f,0);
			#endif
			CamComp.renderingPath = RenderingPath.Forward;
			//CamComp.transparencySortMode = TransparencySortMode.Perspective;
			previewCamW.AddComponent<PreviewCameraSet>();
			previewCamW.transform.position = new Vector3(0,0,-2.175257f);
			previewCamW.transform.parent = previewCamPoint.transform;
			*/
			
			
			
			previewCamPoint.transform.Rotate(Vector3.up * 45, Space.World);
			previewCamPoint.transform.Rotate(Vector3.left * -30, Space.Self);
			
			previewCamPointTarget.transform.Rotate(Vector3.up * 45, Space.World);
			previewCamPointTarget.transform.Rotate(Vector3.left * -30, Space.Self);
			
//			Debug.Log("CreatedMat");
			
			#if !UNITY_5_0
			backMat = new Material (Shader.Find("Hidden/SSCubemapSkybox"));
			#endif
			#if UNITY_5_0
			backMat = new Material (Shader.Find("Hidden/SSCubemapSkybox2"));
			#endif
			backMat = new Material (Shader.Find("Hidden/SSCubemapSkybox2"));
			//AssetDatabase.CreateAsset(previewMat, "Assets/TempMaterial.mat");
			backMat.SetTexture("_Cube",defaultBackCube);
			
			
			
			CalcAmbientLight(defaultBackCube);
			(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).NewCubemap = defaultBackCube;
			
			
			previewObject = new GameObject();
			previewObject.hideFlags = HideFlags.HideInHierarchy;//HideFlags.HideAndDontSave;
			previewObject.name = "PreviewObject";
			previewObject.transform.eulerAngles = new Vector3(0f,0,0);

			previewBack = new GameObject();
			previewBack.hideFlags = HideFlags.HideInHierarchy;//HideFlags.HideAndDontSave;
			previewBack.name = "PreviewBack";
			previewBack.transform.eulerAngles = new Vector3(0f,0,0);
			previewBack.transform.localScale = new Vector3(30,30,30);

			MeshFilter MeshFilComp = previewObject.AddComponent<MeshFilter>() as MeshFilter;
			MeshFilComp.sharedMesh = Meshes[0];
			MeshRenderer MeshRendComp = previewObject.AddComponent<MeshRenderer>() as MeshRenderer;

			MeshFilComp = previewBack.AddComponent<MeshFilter>() as MeshFilter;
			MeshFilComp.sharedMesh = Meshes[1];
			MeshRendComp = previewBack.AddComponent<MeshRenderer>() as MeshRenderer;
			MeshRendComp.material = backMat;

			Light1 = new GameObject();
			Light1.hideFlags = HideFlags.HideInHierarchy;//HideFlags.HideAndDontSave;
			Light1.name="SSPrevL1";
			Light1.transform.position = new Vector3(-1.997899f,0.1365404f,-1.467583f);
			Light1.transform.eulerAngles = new Vector3(37.70282f,428.0752f,-47.84827f);
			Light Light1Light = Light1.AddComponent<Light>() as Light;
			Light1Light.intensity = 0.35f;
			Light1Light.color = new Color(1,1,1,1);
			Light1Light.type = LightType.Directional;
			Light1.transform.parent = previewObject.transform;

			Light2 = new GameObject();
			Light2.hideFlags = HideFlags.HideInHierarchy;//HideFlags.HideAndDontSave;
			Light2.name="SSPrevL2";
			Light2.transform.position = new Vector3(0.1437979f,0.1365404f,-2.396458f);
			Light2.transform.eulerAngles = new Vector3(-34.28003f,-121.2071f,88.64641f);
			Light Light2Light = Light2.AddComponent<Light>() as Light;
			Light2Light.intensity = 0.32f;
			//Light2Light.color = new Color(229f/255,116f/255f,179f/255f);
			Light2Light.color = new Color(229f/255,229f/255f,229f/255f);
			Light2Light.type = LightType.Directional;
			Light2.transform.parent = previewObject.transform;
		}
		if (previewMat==null){//||previewMat.shader==null){
			previewMat2 = (Material)AssetDatabase.LoadAssetAtPath("Assets/Shader Sandwich/Internal/Shader Sandwich/SSTemp.mat",typeof(Material));
			if (previewMat2==null){
				if (Shader.Find("Hidden/SSTemp")!=null){
					previewMat2 = new Material (Shader.Find("Hidden/SSTemp"));
					previewMat2.SetTexture("_Cube",defaultBackCube);
				}
				else
				if (Shader.Find("Shader Sandwich/SSTemp")!=null){
					previewMat2 = new Material (Shader.Find("Shader Sandwich/SSTemp"));
					previewMat2.SetTexture("_Cube",defaultBackCube);
				}
				if (previewMat2!=null){
					AssetDatabase.CreateAsset(previewMat2, "Assets/Shader Sandwich/Internal/Shader Sandwich/SSTemp.mat");
					AssetDatabase.ImportAsset("Assets/Shader Sandwich/Internal/Shader Sandwich/SSTemp.mat");
				}
			}
			if (previewMat2!=null){
				if (Shader.Find("Hidden/SSTemp")!=null)
				previewMat2.shader = Shader.Find("Hidden/SSTemp");
				else
				previewMat2.shader = Shader.Find("Shader Sandwich/SSTemp");
				previewMat2.SetTexture("_Cube",defaultBackCube);
			}
			
			if (Shader.Find("Hidden/SSTemp")!=null){
				previewMat = new Material (Shader.Find("Hidden/SSTemp"));
				previewMat.SetTexture("_Cube",defaultBackCube);
			}
			else
			if (Shader.Find("Shader Sandwich/SSTemp")!=null){
				previewMat = new Material (Shader.Find("Shader Sandwich/SSTemp"));
				previewMat.SetTexture("_Cube",defaultBackCube);
			}
			
					
		}
		if (previewMatW==null){//||previewMat.shader==null){
			if (Shader.Find("Hidden/SSTempWireframe")!=null)
			previewMatW = new Material (Shader.Find("Hidden/SSTempWireframe"));
		}
		if(previewMat2!=null){
			previewMat2.renderQueue = -1;
		}
		previewObject.GetComponent<MeshRenderer>().material = previewMat;	
		//previewMat.shader = Shader.Find("Hidden/SSTemp");
		/*if (previewCamW.GetComponent<BLACKIFY>()==null){
			previewCamW.AddComponent<BLACKIFY>();
			//previewCamW.GetComponent<BLACKIFY>().enabled = false;
		}*/
	}
	RenderTexture NormalRenderTexture;
	RenderTexture WireframeRenderTexture;
	
	public void WinTestFunc(int win){
		if (GUI.Button(new Rect(10, 20, 100, 20), "Hello World"))
				Debug.Log("Got a click in window " + win);
			GUI.DragWindow(new Rect(0, 0, 10000, 20));
	}
	//Rect WinRectTest = new Rect(20,20,200,200);
	void OnGUI(){

		Vector2 WinSize = new Vector2(position.width,position.height);
		if (ShaderSandwich.Instance!=null){
		Instance = this;
		Repaint();
		Setup();
		previewCam.GetComponent<Camera>().farClipPlane = 20F;
		previewCam.GetComponent<Camera>().nearClipPlane = 0.05F;
		
		Vector2 WindowSize = new Vector2(position.width,position.height);
		GUISkin oldskin = GUI.skin;
		if(Event.current.type==EventType.Repaint)
		ShaderUtil.AddProSkin(WindowSize);
		
		Rect PreviewRect = new Rect(0,TopBarHeight,WindowSize.x,WindowSize.y-TopBarHeight);
		TopBarHeight = Mathf.Min(Math.Max(TopBarHeight,66),(int)WinSize.y-32);
		//GUI.Box(PreviewRect,"","GameViewBackground");
		//Texture2D texture = new Texture2D(1, 1);
		//texture.SetPixel(0,0,previewCam.GetComponent<Camera>().backgroundColor);
		//texture.Apply();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		Rect MeshTypeRect = EditorGUILayout.GetControlRect(false,40,GUILayout.Width(Mathf.Min(220,WinSize.x-5)));
		ShaderUtil.MakeTooltip(1,MeshTypeRect,"The object to preview.\nYou can select a custom one to the right.");
		//Debug.Log(MeshTypeRect);
		//Debug.Log(Event.current.mousePosition);
		//Debug.Log(ShaderUtil.Tooltip);
		MeshType = GUI.SelectionGrid(MeshTypeRect,MeshType,new string[]{"Cube","Sphere","Monkey","Tesselated Cube"},2);
		if (MeshType!=-1){
		((MeshFilter)previewObject.GetComponent("MeshFilter")).sharedMesh = Meshes[MeshType];
		customObject = null;
		}
		GUILayout.BeginHorizontal();
		GUI.backgroundColor = new Color(0.2f,0.2f,0.2f,1f);
				
		GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
		ButtonStyle.padding = new RectOffset(0,0,0,0);
		ButtonStyle.margin = new RectOffset(0,0,0,0);
		
		int Subtracter = (int)Mathf.Max(0,(225-(int)WinSize.x)/2f);
		GUILayout.Space(4);
		//Rect MeshTypeRect = EditorGUILayout.GetControlRect(GUILayout.Width(Mathf.Min(220,WinSize.x-5)));
		Rect WireframeRect = new Rect(4,MeshTypeRect.height,108-Subtracter,20);
		ShaderUtil.MakeTooltip(1,WireframeRect,"Enable wireframe mode. Doesn't work\nsometimes (I think on Windows 10?), sorry :(.");
		if (Wireframe)
		Wireframe = GUILayout.Toggle(Wireframe,new GUIContent("Wireframe",ShaderSandwich.Tick),ButtonStyle, GUILayout.Height(20),GUILayout.Width(108-Subtracter));
		else
		Wireframe = GUILayout.Toggle(Wireframe,new GUIContent("Wireframe",ShaderSandwich.Cross),ButtonStyle, GUILayout.Height(20),GUILayout.Width(108-Subtracter));
		//GUI.Label(new Rect(1,40,95,20),"Wireframe");
		GUILayout.Space(4);
		
		Rect SmoothDampRect = new Rect(WireframeRect.width+8,MeshTypeRect.height,108-Subtracter,20);
		ShaderUtil.MakeTooltip(1,SmoothDampRect,"Make rotating and zooming\nin the viewport smooth.");
		if (SmoothDamp)
		SmoothDamp = GUILayout.Toggle(SmoothDamp,new GUIContent("Smooth Cam",ShaderSandwich.Tick),ButtonStyle, GUILayout.Height(20),GUILayout.Width(108-Subtracter));
		else
		SmoothDamp = GUILayout.Toggle(SmoothDamp,new GUIContent("Smooth Cam",ShaderSandwich.Cross),ButtonStyle, GUILayout.Height(20),GUILayout.Width(108-Subtracter));	
		//GUI.Label(new Rect(101,40,95,20),"Smooth Cam");
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		//if (BlackCube=null){
		//	BlackCube = EditorGUIUtility.Load("Misc/BlackCube.png") as Cubemap;
		//}
		GUI.changed = false;
		Color OldCol = GUI.color;
		Color OldColb = GUI.backgroundColor;
		GUI.color = new Color(1,1,1,1);
		GUI.backgroundColor = new Color(1,1,1,1);
		Subtracter = (int)Mathf.Max(0,(343-(int)WinSize.x));
		if (WinSize.x<304){
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			Subtracter = 0;
			int PrevRectSub = Math.Min(66,(int)PreviewRect.height-32);
			PreviewRect.height-=PrevRectSub;
			PreviewRect.y+=PrevRectSub;
			//TopBarHeight+=66;
		}
			GUILayout.BeginVertical();
			GUILayout.Space(4);
			Rect CubemapRect = EditorGUILayout.GetControlRect(false,GUILayout.MaxWidth(108));
			ShaderUtil.MakeTooltip(1,CubemapRect,"Choose the background cubemap.");
			//backCube = (Cubemap)EditorGUILayout.ObjectField(backCube,typeof(Cubemap),false,GUILayout.MaxWidth(108));
			backCube = (Cubemap)EditorGUI.ObjectField(CubemapRect,backCube,typeof(Cubemap),false);
			if (GUI.changed){
				Cubemap TempCube = defaultBackCube;
				if (backCube!=null)
				TempCube = backCube;
				
				CalcAmbientLight(TempCube);
				
				
				
				
				backMat.SetTexture("_Cube",TempCube);
				previewMat.SetTexture("_Cube",TempCube);
				//RenderSettings.customReflection = TempCube;
				(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).NewCubemap = TempCube;
			}
			GUILayout.Space(2);
			Rect CustomRect = EditorGUILayout.GetControlRect(false,GUILayout.MaxWidth(108));
			ShaderUtil.MakeTooltip(1,CustomRect,"Choose a custom preview object.");
			customObject = (Mesh)EditorGUI.ObjectField(CustomRect,customObject,typeof(Mesh),false);
			//customObject = (Mesh)EditorGUILayout.ObjectField(customObject,typeof(Mesh),false,GUILayout.MaxWidth(108));
			if (customObject){
				MeshType = -1;
				((MeshFilter)previewObject.GetComponent("MeshFilter")).sharedMesh = customObject;
			}
		GUI.color = OldCol;
		GUI.backgroundColor = OldColb;
			GUILayout.Space(3);
			//Rect ExposeRect = new Rect(SmoothDampRect.width+WireframeRect.width+8,MeshTypeRect.height,108-Subtracter,20);
			Rect ExposeRect = EditorGUILayout.GetControlRect(true,GUILayout.Height(20),GUILayout.Width(108-Subtracter));
			ShaderUtil.MakeTooltip(1,ExposeRect,"Expose the temp shader as one you can\nuse in a material (Shader Sandwich/SSTemp).");
			/*if (Expose)
				Expose = GUILayout.Toggle(Expose,new GUIContent("Expose",ShaderSandwich.Tick),ButtonStyle, GUILayout.Height(20),GUILayout.Width(108-Subtracter));
			else
				Expose = GUILayout.Toggle(Expose,new GUIContent("Expose",ShaderSandwich.Cross),ButtonStyle, GUILayout.Height(20),GUILayout.Width(108-Subtracter));*/
			if (Expose)
				Expose = GUI.Toggle(ExposeRect,Expose,new GUIContent("Expose",ShaderSandwich.Tick),ButtonStyle);
			else
				Expose = GUI.Toggle(ExposeRect,Expose,new GUIContent("Expose",ShaderSandwich.Cross),ButtonStyle);
			GUILayout.EndVertical();
		//else{
			//PreviewRect.height-=58;
			//PreviewRect.y+=58;
			//backCube = (Cubemap)EditorGUI.ObjectField(new Rect(1,60,80,50), backCube,typeof(Cubemap),false);
		//}
		//GUILayout.EndHorizontal();
		if (ShaderUtil.MouseDownIn(new Rect(0,PreviewRect.y-3,WinSize.x,6),false))
		InResize = true;
		if (Event.current.type == EventType.MouseUp)
		InResize = false;
		bool InResize2 = Event.current.type == EventType.MouseDrag&&InResize;
		if (InResize2){
			TopBarHeight+=(int)Event.current.delta.y;
		}		
		

			//if (backCube==null)
			//previewBack.GetComponent<Transform>().localPosition = new Vector3(1000000,0,0);
			//else
			//previewBack.GetComponent<Transform>().localPosition = new Vector3(0,0,0);

			
		float time = (float)EditorApplication.timeSinceStartup;   
		Vector4 vTime = new Vector4(( time / 20f)%1000f, time%1000f, (time*2f)%1000f, (time*3f)%1000f);
		Vector4 vCosTime = new Vector4( Mathf.Cos(time / 8f), Mathf.Cos(time/4f), Mathf.Cos(time/2f), Mathf.Cos(time));
		Vector4 vSinTime = new Vector4( Mathf.Sin(time / 8f), Mathf.Sin(time/4f), Mathf.Sin(time/2f), Mathf.Sin(time));
		//(t/8, t/4, t/2, t)
		Shader.SetGlobalVector( "_SSTime", vTime );
		Shader.SetGlobalVector( "_SSCosTime", vCosTime );
		Shader.SetGlobalVector( "_SSSinTime", vSinTime );
		
		
		if (Event.current.type==EventType.Repaint){
			Camera PC = previewCam.GetComponent<Camera>();
			//Camera PCW = previewCamW.GetComponent<Camera>();
			
			PC.backgroundColor = new Color(0,0,0,0);
			//PCW.backgroundColor = new Color(0,0,0,0);
			#if UNITY_5||UNITY_PRO_LICENSE
				if (PC.targetTexture==null||PC.targetTexture.width != (int)PreviewRect.width||PC.targetTexture.height != (int)PreviewRect.height){
				
					if (NormalRenderTexture!=null)
					NormalRenderTexture.Release();
					if (WireframeRenderTexture!=null)
					WireframeRenderTexture.Release();
					
					
					NormalRenderTexture = new RenderTexture((int)PreviewRect.width,(int)PreviewRect.height,16);
					WireframeRenderTexture = new RenderTexture((int)PreviewRect.width,(int)PreviewRect.height,16);
					PC.targetTexture = NormalRenderTexture;
					//PCW.targetTexture = NormalRenderTexture;
					NormalRenderTexture.antiAliasing = 8;
					NormalRenderTexture.depth = 24;
					WireframeRenderTexture.antiAliasing = 4;
					NormalRenderTexture.Create();
					WireframeRenderTexture.Create();
				}
				PC.enabled = false;
				PC.depthTextureMode = DepthTextureMode.Depth;
				//PCW.enabled = false;
				(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).CamStart();
//previewBack.GetComponent<MeshRenderer>().enabled = false;
				if (!Wireframe){
					PC.clearFlags = CameraClearFlags.SolidColor;
					PC.targetTexture = NormalRenderTexture;
					PC.Render();
				}
				else{
					PC.clearFlags = CameraClearFlags.SolidColor;
					PC.targetTexture = NormalRenderTexture;
					PC.Render();
					previewBack.GetComponent<MeshRenderer>().enabled = false;
					PC.clearFlags = CameraClearFlags.SolidColor;
					PC.targetTexture = WireframeRenderTexture;
					GL.wireframe = true;
					if (ShaderSandwich.Instance.OpenShader.TransparencyType.Type==1&&ShaderSandwich.Instance.OpenShader.TransparencyOn.On)
					previewObject.GetComponent<MeshRenderer>().material = previewMatW;
					PC.Render();
					previewObject.GetComponent<MeshRenderer>().material = previewMat;
					GL.wireframe = false;
					previewBack.GetComponent<MeshRenderer>().enabled = true;
					
				}
				(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).CamEnd();
				GUI.DrawTexture(PreviewRect,NormalRenderTexture,ScaleMode.StretchToFill,true);
				if (Wireframe){
				Graphics.DrawTexture(PreviewRect,WireframeRenderTexture,ShaderUtil.GetMaterial("Mix",new Color(0,0,0,1),true));
				}
			#else
			Handles.BeginGUI();
				(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).CamStart();
				Handles.DrawCamera(PreviewRect,previewCam.GetComponent<Camera>());
				(previewCam.GetComponent("PreviewCameraSet") as PreviewCameraSet).CamEnd();
			Handles.EndGUI();
			#endif
			//
		}
		
		
		if (!ShaderSandwich.Instance.RealtimePreviewUpdates){
			OldCol = GUI.color;
			GUI.color = new Color(1,1,1,1);
			GUI.skin.label.fontStyle = FontStyle.Bold;
			ShaderUtil.Label(new Rect(PreviewRect.x+1,PreviewRect.y+1+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			ShaderUtil.Label(new Rect(PreviewRect.x-1,PreviewRect.y-1+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			ShaderUtil.Label(new Rect(PreviewRect.x,PreviewRect.y-1+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			ShaderUtil.Label(new Rect(PreviewRect.x,PreviewRect.y+1+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			ShaderUtil.Label(new Rect(PreviewRect.x-1,PreviewRect.y+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			ShaderUtil.Label(new Rect(PreviewRect.x+1,PreviewRect.y+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			GUI.color = new Color(1,0,0,1);
			ShaderUtil.Label(new Rect(PreviewRect.x,PreviewRect.y+PreviewRect.height-20,PreviewRect.width,PreviewRect.height),"Realtime Updates are turned off...",15);
			GUI.color = OldCol;
			GUI.skin.label.fontStyle = FontStyle.Normal;
			ShaderUtil.Label(new Rect(0,0,0,0),"Realtime Updates are turned off...",12);
		}
		EditorGUIUtility.AddCursorRect(new Rect(0,PreviewRect.y-3,WinSize.x,6),MouseCursor.ResizeVertical);
		
		bool InDrag2 = InDrag;
		if (InDrag2&&Event.current.button == 0)
		EditorGUIUtility.AddCursorRect(PreviewRect,MouseCursor.Pan);
		else
		if (InDrag2&&Event.current.button == 1)
		EditorGUIUtility.AddCursorRect(PreviewRect,MouseCursor.ResizeVertical);

		GUI.Box(new Rect(0,PreviewRect.y-3,WinSize.x,6),"","Button");
		WindowPreviewUpdate(PreviewRect);
		GUI.skin = oldskin;
		ShaderUtil.DrawTooltip(1,WinSize);
//		Debug.Log("FMYlAG");
		}
	}

	float previewCamPointScale = 1f;
	void WindowPreviewUpdate(Rect PreviewRect){
		if (AutoRotate==true)
		previewCamPoint.transform.Rotate(Vector3.up * -45f*Time.deltaTime*0.5f, Space.World);
		//PreviewRectSmaller = PreviewRect.Instatiate();
		PreviewRect.height-=3;
		PreviewRect.y+=3;
		if (ShaderUtil.MouseDownIn(PreviewRect,false))
		InDrag = true;
		if (Event.current.type == EventType.MouseUp)
		InDrag = false;
		
		bool InDrag2 = Event.current.type == EventType.MouseDrag&&InDrag;
		//if (Event.current.type == EventType.MouseDrag&&Event.current.button == 0)
		if (InDrag2&&Event.current.button == 0)
		{
			previewCamPointTarget.transform.Rotate(Vector3.up * Event.current.delta.x, Space.World);//World x/World Up
			previewCamPointTarget.transform.Rotate(Vector3.left * -Event.current.delta.y, Space.Self);//Local z/
		}
		if (InDrag2&&Event.current.button == 1)
		{
			previewCamPointScale += Event.current.delta.y*-0.02f;
			previewCamPointScale = Mathf.Min(Mathf.Max(previewCamPointScale,0.5f),2f);
		}
		if (Event.current.type == EventType.ScrollWheel)
		{
			previewCamPointScale += Event.current.delta.y*0.04f;
			previewCamPointScale = Mathf.Min(Mathf.Max(previewCamPointScale,0.5f),2f);
		}
		if (Event.current.type==EventType.Repaint){
		if (SmoothDamp){
		float NewScale = Mathf.Lerp(previewCamPoint.transform.localScale.x,previewCamPointScale,Time.deltaTime*15f);
		previewCamPoint.transform.localScale = new Vector3(NewScale,NewScale,NewScale);
		
		previewCamPoint.transform.rotation = Quaternion.Slerp(previewCamPoint.transform.rotation,previewCamPointTarget.transform.rotation,Time.deltaTime*15f);
		}
		else
		{
		float NewScale = Mathf.Lerp(previewCamPoint.transform.localScale.x,previewCamPointScale,1);
		previewCamPoint.transform.localScale = new Vector3(NewScale,NewScale,NewScale);
		
		previewCamPoint.transform.rotation = Quaternion.Slerp(previewCamPoint.transform.rotation,previewCamPointTarget.transform.rotation,1);
		}
		}
	}	
	void Nothing(){}
}