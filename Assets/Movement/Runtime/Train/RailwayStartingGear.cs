using UnityEngine;
namespace AllAboard.Train
{
    public static class RailwayStartingGear
    {
        public static void Install(TrainController train,Material[] m)
        {
            var car=train.Cars[1];
            if(car.transform.Find("Rail Repair Locker")!=null)return;
            var rack=new GameObject("Rail Repair Locker").transform;rack.SetParent(car.transform,false);rack.localPosition=new Vector3(-1.55f,1.4f,-2.6f);
            RouteProps.Box(rack,"Tool Locker",new Vector3(0f,.45f,0f),new Vector3(.5f,.9f,1.5f),m[2],true);
            RouteProps.Board(rack,"RAIL REPAIRS",new Vector3(.27f,.75f,0f),new Vector3(1.3f,.25f,.05f),m,Quaternion.Euler(0f,-90f,0f));
            for(int i=0;i<2;i++)
            {
                var item=RailTool.Create((RailToolKind)i,m);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(item,train.gameObject.scene);
                item.transform.SetParent(rack,false);item.transform.localPosition=new Vector3(.30f,.32f,-.4f+i*.8f);item.transform.localRotation=Quaternion.Euler(0f,90f,0f);
                item.GetComponent<Rigidbody>().isKinematic=true;item.AddComponent<StoredRailTool>();
            }
            var stock=RouteProps.Board(rack,"",new Vector3(.28f,1.2f,0f),new Vector3(1.6f,.5f,.05f),m,Quaternion.Euler(0f,-90f,0f));
            stock.gameObject.AddComponent<RailwayStockPlaque>().Configure(train.GetComponent<RailwayInventory>(),stock);
        }
    }
}
