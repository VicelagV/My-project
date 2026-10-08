using UnityEngine;
namespace AllAboard.Train
{
    public enum RailDamageKind { Brittle, Strained, Missing }
    public static class RailDamagePlan
    {
        public static bool HasDamage(RouteEdge edge)
        {
            if(edge.id=="Opening")return true;
            if(edge.crossover)return false;
            if(edge.family==RouteFamily.Hard)return edge.id.StartsWith("Along")||edge.id.StartsWith("To");
            return edge.family==RouteFamily.Mixed&&(uint)WorldArtAssets.Hash(edge.id)%3==0;
        }
        public static RailDamageKind Kind(RouteEdge edge)
        {
            if(edge.id=="Opening"||edge.id=="ToHard0")return RailDamageKind.Brittle;
            if(edge.id=="AlongHard1")return RailDamageKind.Strained;
            if(edge.id=="AlongHard2")return RailDamageKind.Missing;
            return (RailDamageKind)((uint)WorldArtAssets.Hash(edge.id)%3);
        }
        public static float Start(RouteEdge edge,float length)=>Mathf.Floor((edge.id=="Opening"?length-45f:length*.57f)/2f)*2f;
        public const float Length=8f;
        public static float DerailChance(float speed)=>Mathf.Clamp01((Mathf.Abs(speed)-3f)/9f);
    }
}
