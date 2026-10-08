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
            switch(At(graph,point))
            {
                case LandscapeRegion.Forest:return broad*9f+detail*1.8f;
                case LandscapeRegion.Desert:return broad*5f+detail;
                case LandscapeRegion.Hills:return 35f+broad*100f+detail*8f;
                case LandscapeRegion.Mountains:return 120f+Mathf.Pow(broad,2f)*160f+detail*10f;
                default:return broad*2f+detail*0.5f;
            }
        }
        public static Color GroundColour(LandscapeRegion region)
        {
            switch(region)
            {
                case LandscapeRegion.Forest:return new Color(.24f,.34f,.17f);
                case LandscapeRegion.Desert:return new Color(.66f,.49f,.28f);
                case LandscapeRegion.Hills:return new Color(.37f,.42f,.24f);
                case LandscapeRegion.Mountains:return new Color(.42f,.40f,.34f);
                default:return new Color(.48f,.48f,.25f);
            }
        }
    }
}
