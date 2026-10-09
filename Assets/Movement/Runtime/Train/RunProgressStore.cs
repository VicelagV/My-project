using System;
using System.IO;
using UnityEngine;
namespace AllAboard.Train
{
    [Serializable] public sealed class RunProgress
    {public int version=1,nextDay=1,blumt,repairDebt,seed=8008;public EconomyDifficulty difficulty;public bool failed;public int[] upgrades=new int[4];}
    public static class RunProgressStore
    {
        public static RunProgress SessionCheckpoint;
        public static string DefaultPath=>Path.Combine(Application.persistentDataPath,"all-aboard-run.json");
        public static RunProgress Load(string path)
        {
            foreach(string candidate in new[]{path,path+".backup"})
            {
                try{if(!File.Exists(candidate))continue;string json=File.ReadAllText(candidate);if(!json.Contains("\"version\"")||!json.Contains("\"nextDay\""))continue;var data=JsonUtility.FromJson<RunProgress>(json);if(data!=null&&data.version==1&&data.nextDay>=1&&data.blumt>=0&&data.repairDebt>=0)return data;}
                catch(Exception exception) when(exception is IOException||exception is ArgumentException){Debug.LogWarning("Run save could not be read: "+exception.Message);}
            }
            return null;
        }
        public static void Save(string path,RunProgress data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));string pending=path+".tmp";File.WriteAllText(pending,JsonUtility.ToJson(data,true));
            if(File.Exists(path))File.Replace(pending,path,path+".backup");else File.Move(pending,path);
        }
        public static void NewRun(string path){if(File.Exists(path))File.Delete(path);if(File.Exists(path+".backup"))File.Delete(path+".backup");}
    }
}
