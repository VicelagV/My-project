using System.Collections;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AllAboard.Movement.Tests
{
    public sealed class RefinedTerrainTests
    {
        private Scene scene;private ProceduralRailway world;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;scene=SceneManager.GetSceneByPath("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity");
            world=scene.GetRootGameObjects().Select(g=>g.GetComponent<ProceduralRailway>()).First(w=>w!=null);
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return SceneManager.UnloadSceneAsync(scene);}
        [Test] public void GroundCoverExistsButCannotOccupyRailsOrSettlementPads()
        {
            var geometry=world.Geometry;Assert.That(geometry.GrassClusters,Is.GreaterThan(100));Assert.That(geometry.RockClusters,Is.GreaterThan(25));Assert.That(geometry.ShrubClusters,Is.GreaterThan(10));
            foreach(var node in world.Network.nodes.Where(n=>RouteGeometry.IsStop(n.place)))Assert.That(geometry.CanScatter(node.position),Is.False);
            foreach(var edge in world.Network.edges)foreach(var point in edge.points)Assert.That(geometry.CanScatter(point),Is.False);
            foreach(var draw in geometry.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Ground Cover")))Assert.That(draw.GetComponent<Collider>(),Is.Null);
        }
        [Test] public void SurfaceSamplingMatchesTheWalkableTerrainMesh()
        {
            Physics.SyncTransforms();int checkedPoints=0;
            foreach(var edge in world.Network.edges)
            {
                var point=edge.points[edge.points.Length/2]+new Vector3(101.7f,0,17.3f);float expected=world.Geometry.SurfaceHeight(point);
                var hits=Physics.RaycastAll(new Vector3(point.x,expected+500,point.z),Vector3.down,1000).Where(h=>h.collider.name.StartsWith("Terrain"));
                if(!hits.Any())continue;Assert.That(hits.First().point.y,Is.EqualTo(expected).Within(.02f));checkedPoints++;
            }
            Assert.That(checkedPoints,Is.GreaterThan(5));
        }
        [Test] public void FrontierStyleUsesImportedGrainAndReusesItsSceneProfile()
        {
            var style=world.GetComponent<FrontierVisualStyle>();Assert.That(style,Is.Not.Null);Assert.That(style.Ready,Is.True);
            foreach(string texture in new[]{"Earth","Gravel","Wood","Foliage","Metal"})Assert.That(Resources.Load<Texture2D>("FrontierTextures/"+texture),Is.Not.Null);
            Assert.That(RenderSettings.fog,Is.True);Assert.That(RenderSettings.fogStartDistance,Is.LessThan(150));
            var paints=world.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<Renderer>()).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&m.name.Contains("TrainNavy Frontier")).Distinct().ToArray();
            Assert.That(paints.Length,Is.GreaterThan(0));foreach(var paint in paints)Assert.That(paint.GetTexture("_BaseMap"),Is.Not.Null);
            var source=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainNavy.mat");var originals=new[]{source};
            var styled=style.Prepare(originals);Assert.That(originals[0],Is.SameAs(source));Assert.That(styled[0],Is.Not.SameAs(source));
            world.BuildInitialWorld();Assert.That(world.GetComponents<FrontierVisualStyle>().Length,Is.EqualTo(1));
            foreach(var paint in paints)Assert.That(paint,Is.Not.Null,"Rebuild must preserve the reusable material copies.");
        }
        [Test] public void TrainDecorationsAreBatchedAndHaveNoCollisionShapes()
        {
            foreach(var car in world.Train.Cars)
            {
                var decoration=car.transform.Find("Detailed Train Craftsmanship");Assert.That(decoration,Is.Not.Null);
                Assert.That(decoration.GetComponentsInChildren<Collider>(),Is.Empty);
                Assert.That(decoration.GetComponentsInChildren<MeshRenderer>().Length,Is.InRange(1,7));
                var count=decoration.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount);Assert.That(count,Is.GreaterThan(1000));
                foreach(var mesh in decoration.GetComponentsInChildren<MeshFilter>())Assert.That(float.IsNaN(mesh.sharedMesh.bounds.size.sqrMagnitude),Is.False);
            }
        }
    }
}
