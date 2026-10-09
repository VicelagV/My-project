using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailwayStockPlaque:MonoBehaviour
    {
        private RailwayInventory inventory;private TextMesh label;
        public void Configure(RailwayInventory owner,TextMesh text){inventory=owner;label=text;Refresh();}
        private void Update()=>Refresh();
        private void Refresh(){if(inventory!=null&&label!=null)label.text=inventory.Gold+" GOLD | CREDIT "+inventory.CreditDebt+"\nSTEEL: "+inventory.SteelMetres.ToString("0.0")+" m";}
    }
}
