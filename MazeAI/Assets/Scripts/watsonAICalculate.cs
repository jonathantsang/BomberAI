using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IBM.Watson.DeveloperCloud.Services.TradeoffAnalytics.v1;

public class watsonAICalculate : MonoBehaviour {

	[Header("Timer on Update Function")]
	private float timeElapsed = 0f;
	private float timeDuration = 2f;

	public float maxSpeed = 1.5f;

	// The AI has 6 main options, 4 directions, place a bomb, or do nothing
	private int numOptions = 6;

	[Header("AI Movement")]
	private int move;
	private Rigidbody2D rb2d;

	[Header("AI")]
	public GameObject AI;
	public GameObject player;
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
		columnDistanceX.description = "Distance horizontally from player";
		columnDistanceX.type = "numeric";
		columnDistanceX.key = "distanceX";
		columnDistanceX.full_name = "Distance Horizontal";
		// Want max, to close the most distance
		columnDistanceX.goal = "max";
		columnDistanceX.is_objective = true;

		// Distance from the player in y
		Column columnDistanceY = new Column();
		columnDistanceY.description = "Distance vertically from player";
		columnDistanceY.type = "numeric";
		columnDistanceY.key = "distanceY";
		columnDistanceY.full_name = "Distance Vertical";
		// Want max, to close the most distance
		columnDistanceY.goal = "max";
		columnDistanceY.is_objective = true;

		// Weight ways more on some elements to make it so AI doesn't get stuck
		Column columnWeight = new Column();
		columnWeight.description = "Weight Column to minimize";
		columnWeight.type = "numeric";
		columnWeight.key = "weight";
		columnWeight.full_name = "Weight";
		columnWeight.goal = "min";
		columnWeight.is_objective = true;

		// Add each column that is a parameter
		listColumn.Add(columnDanger);
		listColumn.Add(columnDistanceX);
		listColumn.Add(columnDistanceY);
		listColumn.Add(columnWeight);
		// Assuming the categorical data type is broken since it doesn't work in the example file either

		problemToSolve.columns = listColumn.ToArray();

		// Default options, edit the weights later
		listOption = new List<Option>();

		Option option1 = new Option();
		option1.key = "1";
		option1.name = "right";
		option1.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = -100;
		(option1.values as TestDataValue).distanceY = -100;
		(option1.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 1;
		listOption.Add(option1);

		Option option2 = new Option();
		option2.key = "2";
		option2.name = "up";
		option2.values = new TestDataValue();
		// These three will be edited at runtime
		(option2.values as TestDataValue).distanceX = -100;
		(option2.values as TestDataValue).distanceY = -100;
		(option2.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 0;
		listOption.Add(option2);

		Option option3 = new Option();
		option3.key = "3";
		option3.name = "left";
		option3.values = new TestDataValue();
		// These three will be edited at runtime
		(option3.values as TestDataValue).distanceX = -100;
		(option3.values as TestDataValue).distanceY = -100;
		(option3.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 1;
		(option1.values as TestDataValue).weight = 1;
		listOption.Add(option3);

		Option option4 = new Option();
		option4.key = "4";
		option4.name = "down";
		option4.values = new TestDataValue();
		// These three will be edited at runtime
		(option4.values as TestDataValue).distanceX = -100;
		(option4.values as TestDataValue).distanceY = -100;
		(option4.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 0;
		listOption.Add(option4);

		Option option5 = new Option();
		option5.key = "5";
		option5.name = "placebomb";
		option5.values = new TestDataValue();
		// These three will be edited at runtime
		(option5.values as TestDataValue).distanceX = -100;
		(option5.values as TestDataValue).distanceY = -100;
		(option5.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 1;
		listOption.Add(option5);

		Option option6 = new Option();
		option6.key = "6";
		option6.name = "waiting";
		option6.values = new TestDataValue();
		// These three will be edited at runtime
		(option6.values as TestDataValue).distanceX = -100;
		(option6.values as TestDataValue).distanceY = -100;
		(option6.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 1;
		listOption.Add(option6);

		problemToSolve.options = listOption.ToArray();
	}

	private void OnGetDilemma( DilemmasResponse resp )
	{
		if (resp == null) {
			Debug.Log ("error");
			return;
		}
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

		Transform currTransform = gameObject.transform;

		// right 1
		if(canMove(Vector2.right)){
			Debug.Log ("can move right");
			(problemToSolve.options [0].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x + 1 - player.transform.position.x);
			(problemToSolve.options [0].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - player.transform.position.y);
		}

		// up 2
		if(canMove(Vector2.up)){
			Debug.Log ("can move up");
			(problemToSolve.options [1].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x - player.transform.position.x);
			(problemToSolve.options [1].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y + 1 - player.transform.position.y);
		}

		// left 3
		if(canMove(Vector2.left)){
			Debug.Log ("can move left");
			(problemToSolve.options [2].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x -1 - player.transform.position.x);
			(problemToSolve.options [2].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - player.transform.position.y);
		}

		// down 4 
		if(canMove(Vector2.down)){
			Debug.Log ("can move down");
			(problemToSolve.options [3].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x - player.transform.position.x);
			(problemToSolve.options [3].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - 1 - player.transform.position.y);
		}
		// bomb and wait 5 and 6
		//(problemToSolve.options [4].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x - player.transform.position.x);
		//(problemToSolve.options [4].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - player.transform.position.y);
		//(problemToSolve.options [5].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x - player.transform.position.x);
		//(problemToSolve.options [5].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - player.transform.position.y);
	}

	private bool canMove(Vector2 dirVector){
		CircleCollider2D cc2d = AI.transform.GetChild (1).GetComponent<CircleCollider2D> ();
		cc2d.enabled = false;
		Debug.DrawRay (AI.transform.position, dirVector, Color.red, 1f);
		RaycastHit2D hit = Physics2D.Raycast (AI.transform.position, dirVector, 1f);
		cc2d.enabled = true;
		if (hit.collider == null) {
			return true;
		}
		Debug.Log (hit.collider.gameObject.name);
		return false;
	}

	// Update is called once per frame
	void Update () {
		timeElapsed += Time.deltaTime;

		// Move based on the Watson API Tradeoff
		if (timeElapsed >= timeDuration) {
			// Right now makes a fresh new problem at the time of running, meaning no past saved state/memory
			calculateMovement ();
			editProblem ();
			callWatsonAPI();

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
			rb2d.velocity = new Vector2 (movex , movey);
		}
	}

	/// <summary>
	/// Application data value.
	/// </summary>
	public class TestDataValue : IBM.Watson.DeveloperCloud.Services.TradeoffAnalytics.v1.ApplicationDataValue
	{
		public float distanceX { get; set; }
		public float distanceY { get; set; }
		public int danger { get; set; }
		public double weight { get; set; }
	}

}
