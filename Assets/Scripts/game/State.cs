using UnityEngine;
using System.Collections;

public class State : MonoBehaviour
{
    /// <summary>
    /// Game states.
    /// </summary>
    public enum States
    {
        /// <summary>Initial state.</summary>
        Null,

        /// <summary>Scene loading.</summary>
        LoadScene,
        /// <summary>Level specific initialization.</summary>
        InitializeLevel,
        StartLevel,
        Play,
        EndLevel,

        StartCutscene,
        Cutscene,
        EndCutscene
    }

    /////////////////////////////
    //  Non-Serialized Fields  //
    /////////////////////////////

    static bool printDebug = true;
    
    static States currentState;

    //////////////////
    //  Properties  //
    //////////////////

    public static bool PrintDebug
    {
        set { printDebug = value; }
    }
    
    public static States Current
    {
        get { return currentState; }
    }

    /////////////////
    //  Delegates  //
    /////////////////

    public delegate void StateDelegate(States previousState, States newState);
    public static event StateDelegate OnStateChanged;

    //////////////////////////
    //  Public Functions  //
    //////////////////////////

    public static void Set(States newState, string debugMessage = null)
    {
        States oldState = currentState;

        currentState = newState;

        if (printDebug)
        {
            Debug.Log("Changed state from " + oldState + " to " + newState + (debugMessage == null ? "" : " -> " + debugMessage));
        }

        if (OnStateChanged != null)
            OnStateChanged(oldState, newState);
    }
}
