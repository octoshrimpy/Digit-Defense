using UnityEngine;
using System.Collections;
using Leap.Unity;
using Leap;
using UnityEngine.UI;

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
    public GameObject display;
    GameObject followSpell;
    Hand left_hand;
    Hand right_hand;
    handShape activeShape;
    UnityEngine.UI.Image i;
    handShape currentHandshape;

	// Use this for initialization
	void Start () {
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
        UnityEngine.UI.Image i = display.GetComponent<UnityEngine.UI.Image>();
        i.color = (new Color(255, 255, 255, 0));
        curState = state.noSpell;
        fireSpell = (GameObject) Resources.Load("fire" );
        waterSpell = (GameObject)Resources.Load("water");
        airSpell = (GameObject)Resources.Load("air");
        earthSpell = (GameObject)Resources.Load("earth");
	}
	
	// Update is called once per frame
	void Update () {
        i = display.GetComponent<UnityEngine.UI.Image>();
        Frame frame = provider.CurrentFrame;
        left_hand = null;
        right_hand = null;
        

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
            
        }

        if (!(right_hand == null))
        {
            if (isRightHandFist())
            {
                if (curState == state.spellHeld)
                {
                    stateTimer = .23F;
                    followSpell.transform.position = right_hand.PalmPosition.ToVector3();
                }
                else if (!(left_hand == null))
                {
                    //filler comment
                    //do stuff
                    //check right hand for fist


                    handShapeDecision();

                    //else noHandStates

                }
                else
                {
                    noHandStates();
                }
                
            }
            else
            {
                noHandStates();
            }
        }
        else
        {
            noHandStates();
        }
        
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1 - spellTimer / CHARGE_PERIOD);

	}

    void handShapeDecision()
    {
        //check left hand for shape
        currentHandshape = getLeftHandShape();
        if (currentHandshape == handShape.none)
        {
            noHandStates();
        }
        else
        {
            //correct hand shape
            if (currentHandshape != activeShape)
            {
                //Change spell and reset Timer PUT CHANGE IN COLOR I HERE
                curState = state.loadingSpell;
                activeShape = currentHandshape;
                spellTimer = CHARGE_PERIOD;

                switch (activeShape)
                {
                    case handShape.air: i.color = new Color(1.0F, 1.0F, 1.0F); break;
                    case handShape.water: i.color = new Color(0F, 0F, 1.0F); break;
                    case handShape.earth: i.color = new Color(0F, 1.0F, 0F); break;
                    case handShape.fire: i.color = new Color(1.0F, 0F, 0F); break;
                }


            }
            else
            {
                if (spellTimer > 0)
                {
                    spellTimer -= Time.deltaTime;
                }
                else
                {
                    //Cast Spell
                    curState = state.spellHeld;
                    GameObject cloneObject = getActiveSpell();
                    cloneObject.transform.position = right_hand.PalmPosition.ToVector3();
                    followSpell = Instantiate(cloneObject);
                    activeShape = handShape.none;
                    stateTimer = .23F;
                    spellTimer = CHARGE_PERIOD;
                }
            }
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
            case state.spellHeld:
                if(stateTimer > 0)
                {
                    stateTimer -= Time.deltaTime;
                    followSpell.transform.position = right_hand.PalmPosition.ToVector3();

                }
                else
                {
                    curState = state.noSpell;
                    followSpell = null;
                }
                break;
            case state.dischargingSpell: 
                //decrement counter
                if(spellTimer < CHARGE_PERIOD)
                {
                    spellTimer += Time.deltaTime;
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
