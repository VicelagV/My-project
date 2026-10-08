#if UNITY_EDITOR
using AllAboard.Train;
using NUnit.Framework;
using UnityEngine;
namespace AllAboard.Movement.Tests
{
    public sealed class RailSmoothnessTests
    {
        [Test]
        public void TravelAndHeadingStayContinuousAtEveryArcCacheBoundary()
        {
            var obj=new GameObject("Curve continuity test");
            try
            {
                var path=obj.AddComponent<RailPath>();path.Configure(new[]{Vector3.zero,new Vector3(0,0,60),new Vector3(40,0,100),new Vector3(90,0,100)});
                var field=typeof(RailPath).GetField("distances",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                var distances=(float[])field.GetValue(path);
                foreach(float d in distances)
                {
                    path.Sample(d-0.0005f,out Vector3 a,out Quaternion qa);path.Sample(d+0.0005f,out Vector3 b,out Quaternion qb);
                    Assert.That(Vector3.Distance(a,b),Is.LessThan(0.002f));
                    Vector3 f=qa*Vector3.forward,g=qb*Vector3.forward;
                    Assert.That(Vector3.Distance(f,g),Is.LessThan(0.0002f),"Heading steps at cached distance "+d);
                }
            }
            finally{Object.DestroyImmediate(obj);}
        }
    }
}
#endif
