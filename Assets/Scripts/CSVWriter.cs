using UnityEngine;
using System.IO;

public class CSVWriter : MonoBehaviour
{
    string filename = "";

    [System.Serializable]
    public class Scenario
    {
        public string title;
        public string whoserved;
        public string need;
        public string market;
        public string whyscen;
        public string stage;
    }

    [System.Serializable]
    public class ScenarioList
    {
        public Scenario[] scenario;
    }

    public ScenarioList firstScenarioList = new ScenarioList();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        filename = Application.dataPath + "/test.csv";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            print("Space pressed");
            WriteCSV();
    }

    public void WriteCSV()
    {
        if(firstScenarioList.scenario.Length > 0)
        {
            TextWriter tw = new StreamWriter(filename, false);
            tw.WriteLine("Scenario title, Customer or end-user being served, Customer need being addressed, Market or domain, Why this scenario why now, Stage of decision");
            tw.Close();


            tw = new StreamWriter(filename, true);

            for(int i = 0; i < firstScenarioList.scenario.Length; i++)
            {
                tw.WriteLine(firstScenarioList.scenario[i].title + "," + firstScenarioList.scenario[i].whoserved + "," + firstScenarioList.scenario[i].need + "," + firstScenarioList.scenario[i].market + "," + firstScenarioList.scenario[i].whyscen + "," + firstScenarioList.scenario[i].stage);
            }
            tw.Close();
        }
    }


}
