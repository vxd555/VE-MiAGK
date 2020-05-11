using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathType
{
    walk = 0,
    jump = 1,
    dodge = 2
};

public class PathElement : MonoBehaviour
{
    public PathType pathType = PathType.walk;
}
