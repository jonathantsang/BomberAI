using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IBM.Watson.DeveloperCloud.Services.TradeoffAnalytics.v1;

public class watsonAICalculate : MonoBehaviour {

	[Header("Timer on Update Function")]
	private float timeElapsed = 0f;
	private float timeDuration = 2f;

	// The AI has 6 main options, 4 directions, place a bomb, or do nothing
	private int numOptions = 6;

	[Header("AI Movement")]
	private int move;
	private Rigidbody2D rb2d;

	[Header("AI")]
	public GameObject AI;
	private Problem problemToSolve = new Problem();
	private List<Option> listOption;

	TradeoffAnalytics m_TradeoffAnalytics = new TradeoffAnalytics();

	void Start () {
		rb2d = AI.transform.GetComponent<Rigidbody2D> ();
	}

	// API call to the Watson API
	private void calculateMovement(){
		// Define a new problem each time;
		problemToSolve = new Problem();
		problemToSolve.subject = "Beat player with AI";

		List<Column> listColumn = new List<Column>();
		Column columnMovement = new Column();
		columnMovement.description = "Movement";
		columnMovement.range = new ValueRange();
		((ValueRange)columnMovement.range).high = 270;
		((ValueRange)columnMovement.range).low = 0;
		columnMovement.type = "numeric";
		columnMovement.key = "direction";
		columnMovement.full_name = "Direction";
		columnMovement.goal = "min";
		columnMovement.is_objective = true;

		// Danger Status
		Column columnDanger = new Column();
		columnDanger.description = "Danger to minimize";
		columnDanger.range = new ValueRange();
		// Danger ranges from 5, where you can get hit from 5 lanes, or 0 where you are safe
		((ValueRange)columnDanger.range).high = 5;
		((ValueRange)columnDanger.range).low = 0;
		columnDanger.type = "numeric";
		columnDanger.key = "danger";
		columnDanger.full_name = "Danger Distance";
		columnDanger.goal = "min";
		columnDanger.is_objective = true;

		// Distance from the player in x
		Column columnDistanceX = new Column();
		columnDanger.description = "Distance horizontally from player";
		columnDanger.range = new ValueRange();
		// Want to get close to player, but not too close
		((ValueRange)columnDanger.range).high = 999;
		((ValueRange)columnDanger.range).low = 3;
		columnDanger.type = "numeric";
		columnDanger.key = "distanceX";
		columnDanger.full_name = "Distance Horizontal";
		columnDanger.goal = "min";
		columnDanger.is_objective = true;

		// Distance from the player in y
		Column columnDistanceY = new Column();
		columnDistanceY.description = "Distance vertically from player";
		columnDanger.range = new ValueRange();
		// Want to get close to player, but not too close
		((ValueRange)columnDanger.range).high = 999;
		((ValueRange)columnDanger.range).low = 3;
		columnDistanceY.type = "numeric";
		columnDistanceY.key = "distanceX";
		columnDistanceY.full_name = "Distance Horizontal";
		columnDistanceY.goal = "min";
		columnDistanceY.is_objective = true;

		// Priority of states, broken because of IBM Unity
		Column columnAction = new Column();
		columnAction.description = "All Actions";
		columnAction.type = "categorical";
		columnAction.key = "action";
		columnAction.full_name = "Action distance";
		columnAction.goal = "min";
		columnAction.is_objective = true;
		// Rank the different actions with moving, bomb placement, then not acting
		columnAction.preference = new string[] { "move", "bomb", "nomove" };
		columnAction.range = new CategoricalRange();
		((CategoricalRange)columnAction.range).keys = new string[]{"move", "bomb", "nomove"};

		// Add each column that is a parameter
		listColumn.Add(columnMovement);
		listColumn.Add(columnDanger);
		// Assuming the categorical data type is broken since it doesn't work in the example file either
		// listColumn.Add(columnAction);

		problemToSolve.columns = listColumn.ToArray();

		// Default options, edit the weights later
		listOption = new List<Option>();

		Option option1 = new Option();
		option1.key = "1";
		option1.name = "right";
		option1.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = 0;
		(option1.values as TestDataValue).distanceY = 0;
		(option1.values as TestDataValue).danger = 0;

		(option1.values as TestDataValue).action = "move";
		(option1.values as TestDataValue).direction = 0;
		listOption.Add(option1);

		Option option2 = new Option();
		option2.key = "2";
		option2.name = "up";
		option2.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = 0;
		(option1.values as TestDataValue).distanceY = 0;
		(option1.values as TestDataValue).danger = 0;

		(option2.values as TestDataValue).action = "move";
		(option2.values as TestDataValue).direction = 90;
		listOption.Add(option2);

		Option option3 = new Option();
		option3.key = "3";
		option3.name = "left";
		option3.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = 0;
		(option1.values as TestDataValue).distanceY = 0;
		(option1.values as TestDataValue).danger = 0;

		(option3.values as TestDataValue).action = "move";
		(option3.values as TestDataValue).direction = 180;
		listOption.Add(option3);

		Option option4 = new Option();
		option4.key = "4";
		option4.name = "down";
		option4.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = 0;
		(option1.values as TestDataValue).distanceY = 0;
		(option1.values as TestDataValue).danger = 0;

		(option4.values as TestDataValue).action = "move";
		(option4.values as TestDataValue).direction = 270;
		listOption.Add(option4);

		Option option5 = new Option();
		option5.key = "5";
		option5.name = "placebomb";
		option5.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = 0;
		(option1.values as TestDataValue).distanceY = 0;
		(option1.values as TestDataValue).danger = 0;

		(option5.values as TestDataValue).action = "bomb";
		// Technically no direction
		(option5.values as TestDataValue).direction = 90;
		listOption.Add(option5);

		Option option6 = new Option();
		option6.key = "6";
		option6.name = "waiting";
		option6.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = 0;
		(option1.values as TestDataValue).distanceY = 0;
		(option1.values as TestDataValue).danger = 0;

		(option6.values as TestDataValue).action = "nomove";
		// Technically no direction
		(option6.values as TestDataValue).direction = 90;
		listOption.Add(option6);

		problemToSolve.options = listOption.ToArray();
	}

	private void OnGetDilemma( DilemmasResponse resp )
	{
		Debug.Log(resp == null);
		Debug.Log("Response: " + resp);
		// iterates in reverse order to have priority for moving over not moving. Workaround for categorical data not working.
		for (int i = numOptions -1; i > -1; i--){
			Debug.Log (resp.resolution.solutions[i].solution_ref + " " + resp.resolution.solutions[i].status);
			// Find the FRONT option from the array of solutions
			if (resp.resolution.solutions [i].status == "FRONT") {
				move = System.Int32.Parse(resp.resolution.solutions [i].solution_ref);
			}
		}
		Debug.Log ("chosen: " + move);
	}

	private void callWatsonAPI(){
		m_TradeoffAnalytics.GetDilemma (OnGetDilemma, problemToSolve, false);
	}

	private void editProblem(){
		// Testing of getting the values from the testdatavalue of each option
		// Edit in the future based on player movement

		// Left
		(problemToSolve.options [0].values as TestDataValue).distanceX = 91;

		// Up
		//(problemToSolve.options [1].values as TestDataValue).distanceX = 91;

		// Right
		//(problemToSolve.options [2].values as TestDataValue).distanceX = 91;

		// Down
		// (problemToSolve.options [3].values as TestDataValue).distanceX = 91;
		// Bomb
	}

	// Update is called once per frame
	void Update () {
		timeElapsed += Time.deltaTime;

		// Move based on the Watson API Tradeoff
		if (timeElapsed >= timeDuration) {
			// Right now makes a fresh new problem at the time of running, meaning no past saved state/memory
			calculateMovement ();
			editProblem ();
			callWatsonAPI ();

			timeElapsed = 0f;
			int movex = 0;
			int movey = 0;
			// Right 
			if (move == 1) {
				movex = 1;
			} else if (move == 2) {
				movey = 1;
			} else if (move == 3) {
				movex = -1;
			} else if (move == 4) {
				movey = -1;
				// Placing bomb, no movement
			} else if (move == 5) {
				Debug.Log ("bomb placed");
				// Strategic wait
			} else if (move == 6) {
				Debug.Log ("wait");
			}

			rb2d.position = new Vector2 (rb2d.position.x + movex, rb2d.position.y + movey);
		}
	}

	/// <summary>
	/// Application data value.
	/// </summary>
	public class TestDataValue : IBM.Watson.DeveloperCloud.Services.TradeoffAnalytics.v1.ApplicationDataValue
	{
		public double direction { get; set; }
		public double distanceX { get; set; }
		public double distanceY { get; set; }
		public double danger { get; set; }
		public string action { get; set; }
	}

}
