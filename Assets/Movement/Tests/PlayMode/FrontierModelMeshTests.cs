using System.Collections.Generic;
using AllAboard.Train;
using NUnit.Framework;
using UnityEngine;

namespace AllAboard.Movement.Tests
{
    public sealed class FrontierModelMeshTests
    {
        [Test] public void NewSolidPartsAreClosedAndFaceOutward()
        {
            var meshes=new[]{FrontierModelMesh.Cylinder(),FrontierModelMesh.Cylinder(8,.31f,.5f),FrontierModelMesh.Ring(),FrontierModelMesh.ArchBack()};
            foreach(var mesh in meshes)
            {
                try
                {
                    var vertices=mesh.vertices;var triangles=mesh.triangles;var edges=new Dictionary<string,int>();double volume=0;
                    for(int i=0;i<triangles.Length;i+=3)
                    {
                        var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];
                        Assert.That(Vector3.Cross(b-a,c-a).sqrMagnitude,Is.GreaterThan(.0000001f),mesh.name+" has a collapsed face.");
                        volume+=Vector3.Dot(a,Vector3.Cross(b,c))/6.0;
                        Edge(edges,a,b);Edge(edges,b,c);Edge(edges,c,a);
                    }
                    foreach(var count in edges.Values)Assert.That(count,Is.EqualTo(2),mesh.name+" has an open or non-manifold edge.");
                    Assert.That(volume,Is.GreaterThan(0),mesh.name+" faces inward.");
                }
                finally{Object.DestroyImmediate(mesh);}
            }
        }
        private static string Point(Vector3 p)=>Mathf.RoundToInt(p.x*10000)+","+Mathf.RoundToInt(p.y*10000)+","+Mathf.RoundToInt(p.z*10000);
        private static void Edge(Dictionary<string,int> edges,Vector3 a,Vector3 b)
        {
            string first=Point(a),second=Point(b),key=string.CompareOrdinal(first,second)<0?first+"|"+second:second+"|"+first;
            edges.TryGetValue(key,out int count);edges[key]=count+1;
        }
    }
}
