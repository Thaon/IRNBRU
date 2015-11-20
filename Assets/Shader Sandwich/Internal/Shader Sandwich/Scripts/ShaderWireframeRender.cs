/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]
public class ShaderWireframeRender : MonoBehaviour {
public Color lineColor;
public Color backgroundColor;
public Mesh OldMesh;
public bool ZWrite = true;
public bool AWrite = true;
public bool blend = true;
 
private List<Vector3> lines;
private List<Vector3> linesArray;
//private Material lineMaterial;
private MeshRenderer meshRenderer;

void Start ()
{
	//Debug.Log("I Work! hafuc");
	meshRenderer = GetComponent<MeshRenderer>();
	//if(!meshRenderer) meshRenderer = gameObject.AddComponent<MeshRenderer>();
	//meshRenderer.material = new Material("Shader \"Lines/Background\" { Properties { _Color (\"Main Color\", Color) = (1,1,1,1) } SubShader { Pass {" + (ZWrite ? " ZWrite on " : " ZWrite off ") + (blend ? " Blend SrcAlpha OneMinusSrcAlpha" : " ") + (AWrite ? " Colormask RGBA " : " ") + "Lighting Off Offset 1, 1 Color[_Color] }}}");

	// Old Syntax without Bind :    
	//   lineMaterial = new Material("Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Front Fog { Mode Off } } } }");

	// New Syntax with Bind :
//	lineMaterial = new Material("Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha BindChannels { Bind \"Color\",color } ZWrite On Cull Front Fog { Mode Off } } } }");

	lineMaterial.hideFlags = HideFlags.HideAndDontSave;
	lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;

	
	MeshFilter filter = GetComponent<MeshFilter>();
	Mesh mesh = filter.sharedMesh;
	if (mesh!=OldMesh)
	UpdateLines(mesh);
	OldMesh = mesh;
}
 void UpdateLines(Mesh mesh){
   Vector3[] vertices = mesh.vertices;
   int[] triangles = mesh.triangles;
   linesArray = new List<Vector3>();
   for (int i = 0; i < triangles.Length / 3; i++)
   {
      linesArray.Add(vertices[triangles[i * 3]]);
      linesArray.Add(vertices[triangles[i * 3 + 1]]);
      linesArray.Add(vertices[triangles[i * 3 + 2]]);
   }
   
   lines = linesArray;//.ToBuiltin(Vector3);
 }
 
void OnRenderObject()
{    
	MeshFilter filter = GetComponent<MeshFilter>();
	Mesh mesh = filter.sharedMesh;
	if (mesh!=OldMesh||lines==null||lines.Count==0)
	UpdateLines(mesh);
	OldMesh = mesh;
   meshRenderer.sharedMaterial.color = backgroundColor;
   lineMaterial.SetPass(0);
   
   GL.PushMatrix();
   GL.MultMatrix(transform.localToWorldMatrix);
   GL.Begin(GL.LINES);
   GL.Color(lineColor);
   
   for (int i = 0; i < lines.Count / 3; i++)
   {
      GL.Vertex(lines[i * 3]);
      GL.Vertex(lines[i * 3 + 1]);
       
      GL.Vertex(lines[i * 3 + 1]);
      GL.Vertex(lines[i * 3 + 2]);
       
      GL.Vertex(lines[i * 3 + 2]);
      GL.Vertex(lines[i * 3]);
   }
         
   GL.End();
   GL.PopMatrix();
} 
}
*/