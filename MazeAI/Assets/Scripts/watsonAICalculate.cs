using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IBM.Watson.DeveloperCloud.Services.TradeoffAnalytics.v1;

public class watsonAICalculate : MonoBehaviour {

	[Header("Timer on Update Function")]
	private float timeElapsed = 0f;
	private float timeDuration = 0.1f;

	public float maxSpeed = 2f;

	// The AI has 2 choices
	private int numOptions = 2;

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
		columnWeight.goal = "max";
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
		option1.name = "movement";
		option1.values = new TestDataValue();
		// These three will be edited at runtime
		(option1.values as TestDataValue).distanceX = -100;
		(option1.values as TestDataValue).distanceY = -100;
		(option1.values as TestDataValue).danger = 0;
		(option1.values as TestDataValue).weight = 1;
		listOption.Add(option1);

		Option option2 = new Option();
		option2.key = "2";
		option2.name = "bomb";
		option2.values = new TestDataValue();
		// These three will be edited at runtime
		(option2.values as TestDataValue).distanceX = -100;
		(option2.values as TestDataValue).distanceY = -100;
		(option2.values as TestDataValue).danger = 0;
		(option2.values as TestDataValue).weight = 0;
		listOption.Add(option2);

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

		// movement and bomb
		(problemToSolve.options [0].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x - player.transform.position.x);
		(problemToSolve.options [0].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - player.transform.position.y);
		(problemToSolve.options [1].values as TestDataValue).distanceX = Mathf.Abs(currTransform.position.x - player.transform.position.x);
		(problemToSolve.options [1].values as TestDataValue).distanceY = Mathf.Abs(currTransform.position.y - player.transform.position.y);
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
	void FixedUpdate () {
		timeElapsed += Time.deltaTime;

		// Move based on the Watson API Tradeoff
		if (timeElapsed >= timeDuration) {
			// Right now makes a fresh new problem at the time of running, meaning no past saved state/memory
			calculateMovement ();
			editProblem ();
			callWatsonAPI();

			timeElapsed = 0f;

			// Right 
			if (move == 1) {
				Debug.Log ("moved");
				AI.transform.position = Vector2.MoveTowards (AI.transform.position, player.transform.position, Time.deltaTime * maxSpeed);
			} else if (move == 2) {
				Debug.Log ("placed bomb");
			}
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
