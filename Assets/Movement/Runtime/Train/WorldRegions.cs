using UnityEngine;
namespace AllAboard.Train
{
    public enum LandscapeRegion { Plains, Forest, Desert, Hills, Mountains }
    public static class WorldRegions
    {
        public static LandscapeRegion At(RouteNetwork graph,Vector3 point)
        {
            if(Vector3.Distance(point,graph.Node("Terminum").position)<650f||Vector3.Distance(point,graph.Node("Redfort").position)<220f)return LandscapeRegion.Plains;
            if(point.z<graph.Node("Supply").position.z+100f)return LandscapeRegion.Forest;
            if(Vector3.Distance(point,graph.Node("Bluefort").position)<350f)return LandscapeRegion.Hills;
            float value=Mathf.PerlinNoise(point.x*0.0014f+graph.seed%431,point.z*0.002f+graph.seed%719);
            return value<0.30f?LandscapeRegion.Plains:value<0.46f?LandscapeRegion.Forest:value<0.62f?LandscapeRegion.Desert:value<0.76f?LandscapeRegion.Hills:LandscapeRegion.Mountains;
        }
        public static float Height(RouteNetwork graph,Vector3 point)
        {
            float broad=Mathf.PerlinNoise(point.x*0.003f+graph.seed%997,point.z*0.003f+graph.seed%991);
            float detail=Mathf.PerlinNoise(point.x*0.016f+71f,point.z*0.016f+19f);
            float value=Mathf.PerlinNoise(point.x*.0014f+graph.seed%431,point.z*.002f+graph.seed%719);
            var region=At(graph,point);
            float low=region==LandscapeRegion.Forest?broad*9+detail*1.8f:region==LandscapeRegion.Desert?broad*5+detail:broad*2+detail*.5f;
            float hill=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.61f,.75f,value));
            float mountain=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.73f,.88f,value));
            float raised=2+detail*.8f+hill*(30+broad*70)+mountain*(110+broad*broad*100);
            float height=region==LandscapeRegion.Hills||region==LandscapeRegion.Mountains?raised:low;
            // Bluefort's hills taper into the surrounding region rather than ending in a vertical ring.
            float blue=Vector3.Distance(point,graph.Node("Bluefort").position);
            float influence=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(200,450,blue));
            return Mathf.Lerp(height,35+broad*100+detail*8,influence);
        }
        public static Color GroundColour(LandscapeRegion region)
        {
            switch(region)
            {
                case LandscapeRegion.Forest:return new Color(.32f,.43f,.25f);
                case LandscapeRegion.Desert:return new Color(.66f,.49f,.28f);
                case LandscapeRegion.Hills:return new Color(.37f,.42f,.24f);
                case LandscapeRegion.Mountains:return new Color(.42f,.40f,.34f);
                default:return new Color(.43f,.49f,.29f);
            }
        }
    }
}
