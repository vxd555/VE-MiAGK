using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Path))]
public class GetPathInspector : Editor
{
    public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Path path = (Path)target;
		if(GUILayout.Button("Get Path"))
		{
			path.GetPath();
		}
	}
}
