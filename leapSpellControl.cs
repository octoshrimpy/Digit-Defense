using UnityEngine;
using System.Collections;
using Leap.Unity;
using Leap;

public class leapSpellControl : MonoBehaviour {

    LeapProvider provider;
    
   
    enum state
    {
        noSpell = 0, loadingSpell = 1, dischargingSpell = 2, spellHeld = 3
    }
    enum handShape
    {
        none = 0, earth = 1, water = 2, air = 3, fire = 4
    }

    GameObject fireSpell, waterSpell, earthSpell, airSpell;

    state curState;
    float stateTimer;
    float spellTimer;
    const float CHARGE_PERIOD = 3.0F;
    const float DISCHARGE_PERIOD = 0.5F;
    Hand left_hand;
    Hand right_hand;
    handShape activeShape;
    bool rightHandFist;

	// Use this for initialization
	void Start () {
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;

        curState = state.noSpell;
        fireSpell = (GameObject) Resources.Load("fire" );
        waterSpell = (GameObject)Resources.Load("water");
        airSpell = (GameObject)Resources.Load("air");
        earthSpell = (GameObject)Resources.Load("earth");
        rightHandFist = false;
	}
	
	// Update is called once per frame
	void Update () {
        Frame frame = provider.CurrentFrame;
        left_hand = null;
        right_hand = null;
        handShape currentHandshape;

        if (frame.Hands.Count > 0)
        {
            foreach (Hand hand in frame.Hands)
            {

                if (hand.IsLeft)
                {
                    left_hand = hand;
                }
                if (hand.IsRight)
                {
                    right_hand = hand;
                }

            }
            if(right_hand != null)
            rightHandFist = isRightHandFist();
        }
        

        if(!(left_hand == null) && !(right_hand == null))
        {
            //do stuff
            //check right hand for fist
            if (isRightHandFist())
            {
                //check left hand for shape
                currentHandshape = getLeftHandShape();
                if(currentHandshape == handShape.none)
                {
                    noHandStates();
                }
                else
                {
                    //correct hand shape
                    if(currentHandshape != activeShape)
                    {
                        activeShape = currentHandshape;
                        spellTimer = CHARGE_PERIOD;
                    }
                    else
                    {
                        if(spellTimer > 0)
                        {
                            spellTimer -= Time.deltaTime;
                        }
                        else
                        {
                            curState = state.spellHeld;
                            GameObject cloneObject = getActiveSpell();
                            Debug.Log(cloneObject.name);
                            activeShape = handShape.none;
                            spellTimer = CHARGE_PERIOD;
                        }
                    }
                }
            }
            else
            {
                noHandStates();
                //else noHandStates
            }
        }
        
        else
        {
            noHandStates();
        }


	}

    GameObject getActiveSpell()
    {
        switch (activeShape)
        {
            case handShape.air: return (airSpell);
            case handShape.earth: return(earthSpell);
            case handShape.water: return(waterSpell);
            case handShape.fire: return(fireSpell);
        }
        return airSpell;
    }

    bool isRightHandFist()
    {
        foreach(Finger fin in right_hand.Fingers)
        {
            if(fin.IsExtended == true)
            {
                return false;
            }
        }

        return true;
    }

    handShape getLeftHandShape()
    {
        bool fin1, fin2, fin3, fin4, fin5;

        fin1 = left_hand.Fingers[0].IsExtended;
        fin2 = left_hand.Fingers[1].IsExtended;
        fin3 = left_hand.Fingers[2].IsExtended;
        fin4 = left_hand.Fingers[3].IsExtended;
        fin5 = left_hand.Fingers[4].IsExtended;
        
        if(!fin1 && !fin2 && !fin3 && !fin4 && !fin5)
        {
            return handShape.earth;
        }
        else if(fin1 && fin2 && fin3 && !fin4 && !fin5)
        {
            return handShape.water;
        }
        else if(!fin1 && !fin2 && !fin3 && !fin4 && fin5)
        {
            return handShape.fire;
        }
        else if(!fin1 && fin2 && !fin3 && !fin4 && fin5)
        {
            return handShape.air;
        }


        return handShape.none;
    }

    void noHandStates()
    {
        switch(curState)
        {
           
            case state.dischargingSpell: 
                //decrement counter
                if(stateTimer > 0)
                {
                    stateTimer -= Time.deltaTime;
                }
                else
                {
                    curState = state.noSpell;
                    activeShape = handShape.none;
                }
                break;
            case state.loadingSpell:
                //set to discharging spell
                curState = state.dischargingSpell;
                stateTimer = DISCHARGE_PERIOD;
                break;
            default: break;
        }

    }
}
