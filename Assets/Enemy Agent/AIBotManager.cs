using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBotManager : Singleton<AIBotManager>
{
    public List<Transform> WaypointLists = new List<Transform>();
    public List<List<AIBotController>> Bots = new List<List<AIBotController>>();
    public List<int> WaypointListBotCount = new List<int>();

    public List<Transform> globalEnemies = new List<Transform>();

    public GameObject AIBotPrefab;
    void Start()
    {
        InvokeRepeating("BotSpawn", 0f, 5f);
    }

    // Update is called once per frame
    void BotSpawn()
    {
        for (int i = 0;i<WaypointLists.Count;i++)
        {
            if(Bots.Count<i+1)
            {
                Bots.Add(new List<AIBotController>());
            }
            if (Bots[i] == null)
            {
                Bots[i] = new List<AIBotController>();
            }

            if (Bots[i].Count< WaypointListBotCount[i] && Random.value>0.5f)
            {
                AIBotController aIBot = Instantiate(AIBotPrefab).GetComponent<AIBotController>();
                aIBot.waypointsParent = WaypointLists[i];

                aIBot.enemies = globalEnemies;

                Bots[i].Add(aIBot);
            }
        }
    }
}
