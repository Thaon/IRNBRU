#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
#define PRE_UNITY_5
#else
#define UNITY_5
#endif
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
[ExecuteInEditMode]
public class PreviewCameraSet : MonoBehaviour {
    public Light[] lights;
	public bool[] LightIntensities;
    public MeshRenderer[] meshRenderers;
    public SkinnedMeshRenderer[] skinnedMeshRenderers;
	public bool[] renderers;	
	public bool[] skinnedrenderers;	
	public Color AmbientLight;
	public Cubemap OldCubemap;
	public Cubemap NewCubemap;
	#if UNITY_5
	public SphericalHarmonicsL2 OldAmbient;
	public SphericalHarmonicsL2 NewAmbient;
	#endif
	public Color NewAmbientColor;
	public Color OldAmbientColor;
	public float AmbientIntensity = 0f;
	#if UNITY_5
	public DefaultReflectionMode OldCubemapSettings;
	public AmbientMode OldAmbientMode;
	#endif
	
	public void CamStart() {
	#if UNITY_5
		AmbientIntensity = RenderSettings.ambientIntensity;
		RenderSettings.ambientIntensity = 1f;
		#else
		OldAmbientColor = RenderSettings.ambientLight;
		RenderSettings.ambientLight = NewAmbientColor;
	#endif
		//
		
		
		lights = FindObjectsOfType(typeof(Light)) as Light[];
		LightIntensities = new bool[lights.Length];
		int i = 0;
		foreach(Light light in lights){
			if (light.gameObject.name!="SSPrevL1"&&light.gameObject.name!="SSPrevL2"){
				LightIntensities[i] = light.enabled;
				light.enabled = false;
			}
			else
			light.enabled = true;
			i+=1;
		}
		meshRenderers = FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
		skinnedMeshRenderers = FindObjectsOfType(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer[];
		renderers = new bool[meshRenderers.Length];
		skinnedrenderers = new bool[skinnedMeshRenderers.Length];
		i = 0;
		foreach(MeshRenderer meshRenderer in meshRenderers){
			if (meshRenderer.gameObject.name!="PreviewObject"&&meshRenderer.gameObject.name!="PreviewBack"){
				renderers[i] = meshRenderer.enabled;
				meshRenderer.enabled = false;
			}
			else{
				renderers[i] = false;
				meshRenderer.enabled = true;
			}
			i+=1;
		}
		i = 0;
		foreach(SkinnedMeshRenderer meshRenderer in skinnedMeshRenderers){
			if (meshRenderer.gameObject.name!="PreviewObject"&&meshRenderer.gameObject.name!="PreviewBack"){
				skinnedrenderers[i] = meshRenderer.enabled;
				meshRenderer.enabled = false;
			}
			else{
				skinnedrenderers[i] = false;
				meshRenderer.enabled = true;
			}
			i+=1;
		}
		#if UNITY_5
		OldCubemap = RenderSettings.customReflection;
		OldCubemapSettings = RenderSettings.defaultReflectionMode;
		RenderSettings.customReflection = NewCubemap;
		RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
		
		OldAmbient = RenderSettings.ambientProbe;
		RenderSettings.ambientProbe = NewAmbient;
		OldAmbientMode = RenderSettings.ambientMode;
		RenderSettings.ambientMode = AmbientMode.Skybox;
		//#else
		//OldAmbientColor = RenderSettings.ambientLight;
		//RenderSettings.ambientLight = NewAmbientColor;
		#endif

		//RenderSettings.ambientProbe = new SphericalHarmonicsL2();
    }
    public void CamEnd() {
		
		#if UNITY_5
		RenderSettings.ambientIntensity = AmbientIntensity;
		RenderSettings.ambientMode = OldAmbientMode;
		#else
		RenderSettings.ambientLight = OldAmbientColor;
		#endif
		//RenderSettings.ambientLight = AmbientLight;
		int i = 0;
		foreach(Light light in lights){
			if (light.gameObject.name!="SSPrevL")
			light.enabled = LightIntensities[i];
			else
			light.enabled = false;
			
			i+=1;
		}
		i=0;
		foreach(MeshRenderer meshRenderer in meshRenderers){
			meshRenderer.enabled = renderers[i];
			i+=1;
		}
		i=0;
		foreach(SkinnedMeshRenderer meshRenderer in skinnedMeshRenderers){
			meshRenderer.enabled = skinnedrenderers[i];
			i+=1;
		}
		#if UNITY_5
		RenderSettings.customReflection = OldCubemap;
		RenderSettings.ambientProbe = OldAmbient;
		RenderSettings.defaultReflectionMode = OldCubemapSettings;
		#endif
    }
}