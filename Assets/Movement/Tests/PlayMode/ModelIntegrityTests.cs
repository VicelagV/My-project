using System.Collections.Generic;
using AllAboard.Train;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace AllAboard.Movement.Tests
{
    public sealed class ModelIntegrityTests
    {
        private static string Point(Vector3 v)=>$"{Mathf.RoundToInt(v.x*100000f)},{Mathf.RoundToInt(v.y*100000f)},{Mathf.RoundToInt(v.z*100000f)}";
        private static void Closed(Mesh mesh)
        {
            var vertices=mesh.vertices;var triangles=mesh.triangles;var edges=new Dictionary<string,int>();
            for(int i=0;i<triangles.Length;i+=3)
            {
                Vector3 a=vertices[triangles[i]],b=vertices[triangles[i+1]],c=vertices[triangles[i+2]];
                Assert.That(Vector3.Cross(b-a,c-a).sqrMagnitude,Is.GreaterThan(1e-12f),mesh.name+" has a degenerate triangle");
                for(int j=0;j<3;j++)
                {
                    string first=Point(vertices[triangles[i+j]]),last=Point(vertices[triangles[i+(j+1)%3]]);
                    string key=string.CompareOrdinal(first,last)<0?first+" / "+last:last+" / "+first;
                    edges.TryGetValue(key,out int count);edges[key]=count+1;
                }
            }
            foreach(var edge in edges)Assert.That(edge.Value,Is.EqualTo(2),mesh.name+" has an open or overlapping seam at "+edge.Key);
        }
        [TestCase(5f,16f)][TestCase(8f,35f)][TestCase(16f,18f)]
        public void BuildingSlopesAndAtticsHaveClosedSeams(float width,float pitch)
        {
            var root=new GameObject("Roof Integrity Test");
            try
            {
                var roof=BuildingRoof.Build(root.transform,"Test Roof",Vector3.zero,width,14f,4f,pitch,null,null);
                foreach(var filter in roof.GetComponentsInChildren<MeshFilter>())Closed(filter.sharedMesh);
                var bounds=roof.GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds;
                Assert.That(bounds.max.y,Is.EqualTo(4f+BuildingRoof.Rise(width,pitch)).Within(.001f));
            }finally{Object.DestroyImmediate(root);}
        }
        [TestCase("TrainCoachRoof")][TestCase("TrainCabRoofHatch")]
        public void TrainRoofShellIsClosedIncludingHatchEdges(string name)
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Movement/Meshes/"+name+".asset");Assert.That(mesh,Is.Not.Null);Closed(mesh);
            var go=new GameObject("Roof Collision Audit");
            try
            {
                var collider=go.AddComponent<MeshCollider>();collider.sharedMesh=mesh;go.transform.localScale=new Vector3(4.1f,.6f,9f);Physics.SyncTransforms();
                Assert.That(collider.Raycast(new Ray(new Vector3(0f,2f,0f),Vector3.down),out _,3f),Is.True,"Roof centre must be solid.");
                if(name=="TrainCabRoofHatch")Assert.That(collider.Raycast(new Ray(new Vector3(1.1f,2f,-3f),Vector3.down),out _,3f),Is.False,"The intended ladder opening must remain clear.");
            }finally{Object.DestroyImmediate(go);}
        }
        [TestCase("TrainBevelledBox")][TestCase("TrainButtonZ")][TestCase("TrainCylinderY")]
        [TestCase("TrainPanelBox")][TestCase("TrainRoundRing")][TestCase("TrainThinRing")][TestCase("TrainCoalRock")]
        public void SharedTrainPartsHaveNoOpenMeshEdges(string name)
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Movement/Meshes/"+name+".asset");Assert.That(mesh,Is.Not.Null);Closed(mesh);
        }
        [Test] public void TunnelMountainFacesOutwardAndKeepsTheBoreOpen()
        {
            var parent=new GameObject("Mountain Integrity Test");var root=new GameObject("Tunnel Root").transform;root.SetParent(parent.transform,false);
            try
            {
                var paint=AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/RouteBallast.mat");Assert.That(paint,Is.Not.Null);
                var palette=new Material[10];for(int i=0;i<palette.Length;i++)palette[i]=paint;
                TunnelMountain.Build(root,140f,palette);Physics.SyncTransforms();var collider=root.GetComponentInChildren<MeshCollider>();
                Assert.That(collider.Raycast(new Ray(new Vector3(30f,10f,-80f),Vector3.forward),out _,40f),Is.True,"Approach face must face outward.");
                Assert.That(collider.Raycast(new Ray(new Vector3(30f,10f,80f),Vector3.back),out _,40f),Is.True,"Exit face must face outward.");
                Assert.That(collider.Raycast(new Ray(new Vector3(180f,3f,0f),Vector3.left),out _,40f),Is.True,"Mountain side must be closed.");
                Assert.That(collider.Raycast(new Ray(new Vector3(0f,2.5f,-80f),Vector3.forward),out _,160f),Is.False,"The intended train bore must remain open.");
            }finally{Object.DestroyImmediate(parent);}
        }
        [Test] public void PortalArchIsContinuousAndHasNoOpenMeshSeams()
        {
            var root=new GameObject("Portal Integrity Test");
            try{var arch=PortalArch.Build(root.transform,0f,null);Closed(arch.GetComponent<MeshFilter>().sharedMesh);}
            finally{Object.DestroyImmediate(root);}
        }
    }
}
