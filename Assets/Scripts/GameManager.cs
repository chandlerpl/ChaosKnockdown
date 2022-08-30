using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public CameraTracker cameraTracker;
    public bool IsFinished { get; set; }

    public Transform cyclists;
    public Transform downedCyclists;
    public Transform finishLine;

    // We may create a new level manager to handle game levels
    // Or something like a "Level Setting" so that we could add unique features in different scenes without overwhelming the game manager

    // Temp UI related
    private int _totalCyclist;
    private List<string> _damageInfo = new List<string>();
    [SerializeField] private Text _scoreText;
    private float _currentScore = 0;
    private float _targetScore = 0;
    private int _finalScore = 0;
    private int _chainReaction =  0;
    private float _bonus = 1;
    [SerializeField] private bool _useGUIDebug = true; // For enabling/disabling GUI debug info
    private GameObject closestCyclist;

    void Awake()
    {
        instance = this;
        _totalCyclist = cyclists.childCount;
        StartCoroutine(FinishCheck());
    }

    IEnumerator FinishCheck() {
        while(!IsFinished) {
            if(cyclists.childCount == 0) {
                IsFinished = true;
                break;
            }
            else
            {
                int cyclistRemaining = cyclists.childCount;
                int cyclistCompelted = 0;
                foreach (Transform t in cyclists)
                {
                    if (t.GetComponent<CyclistMovement>().completed) cyclistCompelted ++;
                }
                if (cyclistCompelted == cyclistRemaining)
                {
                    IsFinished = true;
                } 
            }

            float clostestDist = float.MaxValue;
            foreach(Transform trans in cyclists) {
                if(!trans.gameObject.activeInHierarchy)
                    continue;
                    
                CyclistMovement cyclist = trans.GetComponent<CyclistMovement>();
                float dist = cyclist.GetRemainingDistance();
                if(dist < clostestDist && !cyclist.completed) {
                    clostestDist = dist;
                    closestCyclist = trans.gameObject;
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        cameraTracker.StopCamera();
        // Open menu
    }

    void Update()
    {
        if (_currentScore != _targetScore)
        {
            float lerpSpeed = Mathf.Clamp(Mathf.Abs(_targetScore - _currentScore) * 4f + 16f, 64f, Mathf.Infinity);
            _currentScore = Mathf.MoveTowards(_currentScore, _targetScore, lerpSpeed * Time.deltaTime);
            _finalScore = Mathf.RoundToInt(_currentScore);
            if (_scoreText) _scoreText.text = _finalScore.ToString();
        }
    }

    public GameObject GetRandomCyclist() {
        if(cyclists.childCount == 0) {
            return null;
        }

        return cyclists.GetChild(Random.Range(0, cyclists.childCount)).gameObject;
    }

    public GameObject GetClosestCyclist() {
        return closestCyclist;
    }

    #region TEMP_STATS_UI
    // A temp UI showing game stats
    // This may give us some ideas about how the collision damage can be tweaked in order to make it more chaotic
    // And also help us consider how the score (if implemented) can be calculated .e.g a chain reaction
    void OnGUI()
    {
        if (_useGUIDebug)
        {
            GUI.Label(new Rect(8, 8, 256, 32), "Cyclist Remaining: " + cyclists.childCount + "/" + _totalCyclist);
            GUI.Label(new Rect(8, 24, 256, 32), IsFinished ? "Finished" : "In Progress");
            GUI.Label(new Rect(8, 40, 256, 32), "Chain Reaction: " + _chainReaction.ToString() + " (Bonus x " + _bonus.ToString() + ")");
            for(int i = 0; i < _damageInfo.Count; i ++)
            {
                GUI.Label(new Rect(8, 56 + 16 * i, 256, 32), _damageInfo[i]);
            }
        }
    }

    public void AddDamageInfo(string info)
    {
        _damageInfo.Add(info);
        StartCoroutine(ClearEntry());
        if (info.Equals("Environmental Collision")) AddScore(50); else AddScore((float)25 * _bonus);
    }

    private IEnumerator ClearEntry()
    {
        yield return new WaitForSeconds(2f);
        _damageInfo.RemoveAt(0);
    }

    public void IncreaseChainReactionCount()
    {
        _chainReaction ++;
        _bonus = ((float)_chainReaction)/(100f) + 1f;
        StartCoroutine(ResetBonus());
    }

    private IEnumerator ResetBonus()
    {
        int oldCount = _chainReaction;
        yield return new WaitForSeconds(5f);
        int newCount = _chainReaction;
        if (oldCount == newCount)
        {
            _chainReaction = 0;
            _bonus = 1;
        }
    }

    public void AddScore(float n)
    {
        _targetScore += n;
    }
    #endregion
}
