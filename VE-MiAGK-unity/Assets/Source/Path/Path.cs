using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Path : MonoBehaviour
{
    public List<PathElement> path = new List<PathElement>();


	void Start()
    {
        
    }

    void Update()
    {
        
    }

	public void GetPath()
	{
		path.Clear();
		Transform[] allChildren = GetComponentsInChildren<Transform>();
		foreach(Transform child in allChildren)
		{
			PathElement pe = child.GetComponent<PathElement>();
			if(pe != null) path.Add(pe);
		}
	}
}
