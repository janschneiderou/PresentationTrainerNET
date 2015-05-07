using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationTrainer
{
    public class RulesAnalyzerFIFO
    {
        MainWindow parent;

        bool noMistake = true;
        bool freeStyle = true;
        bool interruptionNeeded = false;

        public bool interruption = true;
        public bool interrupted = false;

        BodyFramePreAnalysis bfpa;
        AudioPreAnalysis apa;
        Body oldBody;
        JudgementMaker myJudgementMaker;
        PresentationAction previousAction;

        public delegate void FeedBackEvent(object sender, PresentationAction x);
        public event FeedBackEvent feedBackEvent;

        public delegate void CorrectionEvent(object sender, PresentationAction x);
        public event CorrectionEvent correctionEvent;

        public delegate void InterruptionEvent(object sender, PresentationAction[] x);
        public event InterruptionEvent myInterruptionEvent;

        ArrayList mistakes;
        PresentationAction[] sentMistakes;
        PresentationAction[] crossedArms;
        PresentationAction[] handsUnderHips;
        PresentationAction[] handsBehindBack;
        PresentationAction[] hunchPosture;
        PresentationAction[] leaningPosture;
        PresentationAction[] highVolumes;
        PresentationAction[] lowVolumes;
        PresentationAction[] longPauses;
        PresentationAction[] longTalks;
        PresentationAction[] periodicMovements;
        PresentationAction[] hmmms;
        PresentationAction[] legsCrossed;
        PresentationAction[] handsNotMoving;
        PresentationAction[] handsMovingMuch;
        PresentationAction[] noModulation;

       int crossedArmsMistakes =0;
       int handsUnderHipsMistakes = 0;
       int handsBehindBackMistakes = 0;
       int hunchPostureMistakes = 0;
       int leaningPostureMistakes = 0;
       int highVolumesMistakes = 0;
       int lowVolumesMistakes = 0;
       int longPausesMistakes = 0;
       int longTalksMistakes = 0;
       int periodicMovementsMistakes = 0;
       int hmmmsMistakes = 0;
       int legsCrossedMistakes = 0;
       int handsNotMovingMistakes = 0;
       int handsMovingMuchMistakes = 0;
       int noModulationMistakes = 0;
       public double lastFeedbackTime = 0;
       public double timeBetweenFeedbacks = 3500;
       public bool noInterrupt = true;
 
        public RulesAnalyzerFIFO(MainWindow parent)
        {
            this.parent = parent;
            myJudgementMaker = new JudgementMaker(parent);
            mistakes = new ArrayList();
            crossedArms = new PresentationAction[4];
            handsUnderHips = new PresentationAction[4];
            handsBehindBack = new PresentationAction[4];
            hunchPosture = new PresentationAction[4];
            leaningPosture = new PresentationAction[4];
            highVolumes = new PresentationAction[4];
            lowVolumes = new PresentationAction[4];
            longPauses = new PresentationAction[4];
            longTalks = new PresentationAction[4];
            periodicMovements = new PresentationAction[4];
            hmmms = new PresentationAction[4];
            legsCrossed = new PresentationAction[4];
            handsNotMoving = new PresentationAction[4];
            handsMovingMuch = new PresentationAction[4];
            noModulation = new PresentationAction[4];
            previousAction = new PresentationAction();
            previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
        }

        public void setFreeStyle(bool fs)
        {
            freeStyle = fs;
        }

        #region analysisCycle

        public void AnalyseRules()
        {
            if (parent.freestyleMode != null)
            {
                if (parent.freestyleMode.myState == PresentationTrainer.FreestyleMode.currentState.play )
                {
                    if(checkTimeToStartAnalysing()==true)
                    {
                        myJudgementMaker.analyze();
                    }

                    if (checkTimeToGiveFeedback()==true)
                    {
                        bool didIGiveFeedback = false;

                        

                        //mistake was corrected?

                        //updateLists
                        if (myJudgementMaker.mistakeList.Count > 0)
                        {

                            updateMistakeList();
                        }
                        else
                        {
                            mistakes.Clear();
                        }

                        //give correction when no mistakes and previous mistake is not NOMISTAKE
                        if (mistakes.Count == 0 && previousAction.myMistake != PresentationAction.MistakeType.NOMISTAKE)
                        {
                            doGoodEventStuff();
                            
                            //put previous mistake to no Mistake
                            //start timer
                        }

                        if (mistakes.Count > 0)
                        {
                            if (((PresentationAction)mistakes[0]).myMistake != previousAction.myMistake &&
                                 previousAction.myMistake != PresentationAction.MistakeType.NOMISTAKE)
                            {
                                doGoodEventStuff();
                                
                                // mistakes.Clear();
                                didIGiveFeedback = true;
                            }
                            if (didIGiveFeedback == false)
                            {
                                doFeedbackEventStuff();
                            }
                        }

                       
                    }
                    }
                   
 

            }
        }

        public bool checkTimeToStartAnalysing()
        {
            bool startAnalysis = false;
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (currentTime - lastFeedbackTime + 600 > timeBetweenFeedbacks)
            {
                startAnalysis = true;
            }

            return startAnalysis;
        }

        public bool checkTimeToGiveFeedback()
        {
            bool giveFeedback = false;

             double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

             if (currentTime - lastFeedbackTime > timeBetweenFeedbacks)
             {
                 giveFeedback=true;
             }

            return giveFeedback;
        }

        #endregion

        // update, lists add mistakes, delete no longer mistakes
        #region manageLists 

        private void updateMistakeList()
        {
            if (mistakes.Count > 0)
            {
                deleteNoLongerMistakes();
            }

            addNewMistakes();
        }

        private void addNewMistakes()
        {
            foreach (PresentationAction ba in myJudgementMaker.mistakeList)
            {
                bool isAlreadyThere = false;
                foreach (PresentationAction a in mistakes)
                {
                    if (ba.myMistake == a.myMistake)
                    {
                        isAlreadyThere = true;
                        break;
                    }
                }
                if (isAlreadyThere == false)
                {
                    if(ba.interrupt)
                    {
                        mistakes.Insert(0, ba);
                    }
                    else
                    {
                        mistakes.Add(ba);
                    }
                    
                }
            }
        }

        private void deleteNoLongerMistakes()
        {
            int[] nolongerErrors = new int[mistakes.Count];
            int i = 0;

            foreach (PresentationAction a in mistakes)
            {

                foreach (PresentationAction ba in myJudgementMaker.mistakeList)//judgementMaker.mistakeList
                {
                    if (a.myMistake == ba.myMistake)
                    {
                        nolongerErrors[i] = 1;
                        break;
                    }
                }
                i++;
            }
            for (int j = nolongerErrors.Length; j > 0; j--)
            {
                if (nolongerErrors[j - 1] == 0)
                {
                    mistakes.RemoveAt(j - 1);
                }
            }
        }

        #endregion

        //sending feedbacks, corrections, and interruptions
        #region sendFeedbacks

        private void doGoodEventStuff()
        {

            correctionEvent(this, previousAction);

            lastFeedbackTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
            myJudgementMaker.clearLists();
            //if(previousAction!=null)
            //{
            //    double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            //    if (parent.freestyleMode.myState == PresentationTrainer.FreestyleMode.currentState.play)
            //    {
            //        correctionEvent(this, previousAction);

            //        lastFeedbackTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            //        previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
            //    }
                
            //}
            //else
            //{
                
            //   // correctionEvent(this, previousAction);
            //}
            


        }

        private void doFeedbackEventStuff()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            ((PresentationAction)mistakes[0]).timeFinished = currentTime;


            if (previousAction.myMistake != ((PresentationAction)mistakes[0]).myMistake && ((PresentationAction)mistakes[0]).interrupt == false)
            {
                if (((PresentationAction)mistakes[0]).myMistake == PresentationAction.MistakeType.HANDS_NOT_MOVING)
                {
                    ((PresentationAction)mistakes[0]).firstImage = myJudgementMaker.myVoiceAndMovementObject.firstImage;
                    ((PresentationAction)mistakes[0]).lastImage = myJudgementMaker.myVoiceAndMovementObject.lastImage;
                }
                feedBackEvent(this, (PresentationAction)mistakes[0]);
                checkWhereToPutMistake();
                if(noInterrupt==true)
                {
                    previousAction = (PresentationAction)mistakes[0];
                }
                
            }
            else if (((PresentationAction)mistakes[0]).interrupt == true)
            {
                checkWhereToPutMistake();
                bigMistakeInterruption();

            }

        }

        public void doInterruptionThing()
        {
            noInterrupt = false;
            myInterruptionEvent(this, sentMistakes);
            
           // doGoodEventStuff();
            
         //   myJudgementMaker.mistakeList = new ArrayList();
         //   myJudgementMaker.tempMistakeList = new ArrayList();
            // // myJudgementMaker.audioMovementMistakeTempList = new ArrayList();
        }
        public void resetAfterPause()
        {
            mistakes = new ArrayList();
            previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
            myJudgementMaker = new JudgementMaker(parent);
            noInterrupt = true;
        }
        public void resetAfterInterruption()
        {
            resetMistakeArray();
            mistakes = new ArrayList();
            previousAction.myMistake = PresentationAction.MistakeType.NOMISTAKE;
            myJudgementMaker = new JudgementMaker(parent);
            noInterrupt = true;
        }

        #endregion

        #region handleInterruptions

        private void bigMistakeInterruption()
        {
            switch (((PresentationAction)mistakes[0]).myMistake)
            {
                case PresentationAction.MistakeType.ARMSCROSSED:

                    sentMistakes = crossedArms;
                    break;
                case PresentationAction.MistakeType.LEFTHANDBEHINDBACK:
                case PresentationAction.MistakeType.RIGHTHANDBEHINDBACK:

                    sentMistakes = handsBehindBack;
                    break;
                case PresentationAction.MistakeType.LEGSCROSSED:

                    sentMistakes = legsCrossed;
                    break;

                case PresentationAction.MistakeType.RIGHTHANDUNDERHIP:
                case PresentationAction.MistakeType.LEFTHANDUNDERHIP:

                    sentMistakes = handsUnderHips;
                    break;
                case PresentationAction.MistakeType.HUNCHBACK:

                    sentMistakes = hunchPosture;
                    break;
                case PresentationAction.MistakeType.RIGHTLEAN:
                case PresentationAction.MistakeType.LEFTLEAN:

                    sentMistakes = leaningPosture;
                    break;
                case PresentationAction.MistakeType.DANCING:

                    sentMistakes = periodicMovements;
                    break;
                case PresentationAction.MistakeType.HANDS_NOT_MOVING:

                    sentMistakes = handsNotMoving;
                    break;
                case PresentationAction.MistakeType.HANDS_MOVING_MUCH:

                    sentMistakes = handsMovingMuch;
                    break;
                case PresentationAction.MistakeType.HIGH_VOLUME:

                    sentMistakes = highVolumes;
                    break;
                case PresentationAction.MistakeType.LOW_VOLUME:

                    sentMistakes = lowVolumes;
                    break;
                case PresentationAction.MistakeType.LOW_MODULATION:

                    sentMistakes = noModulation;
                    break;
                case PresentationAction.MistakeType.LONG_PAUSE:

                    sentMistakes = longPauses;
                    break;
                case PresentationAction.MistakeType.LONG_TALK:

                    sentMistakes = longTalks;
                    break;
                case PresentationAction.MistakeType.HMMMM:

                    sentMistakes = hmmms;
                    break;
            }
            doInterruptionThing();
        }



        private void resetMistakeArray()
        {
            switch (((PresentationAction)mistakes[0]).myMistake)
            {
                case PresentationAction.MistakeType.ARMSCROSSED:
                    crossedArms = new PresentationAction[4];
                    crossedArmsMistakes++;
                    break;
                case PresentationAction.MistakeType.LEFTHANDBEHINDBACK:
                case PresentationAction.MistakeType.RIGHTHANDBEHINDBACK:
                    handsBehindBack = new PresentationAction[4];
                    handsBehindBackMistakes++;

                    break;
                case PresentationAction.MistakeType.LEGSCROSSED:
                    legsCrossed = new PresentationAction[4];
                    legsCrossedMistakes++;
                    break;

                case PresentationAction.MistakeType.RIGHTHANDUNDERHIP:
                case PresentationAction.MistakeType.LEFTHANDUNDERHIP:
                    handsUnderHips = new PresentationAction[4];
                    handsUnderHipsMistakes++;
                    break;
                case PresentationAction.MistakeType.HUNCHBACK:
                    hunchPosture = new PresentationAction[4];
                    hunchPostureMistakes++;
                    break;
                case PresentationAction.MistakeType.RIGHTLEAN:
                case PresentationAction.MistakeType.LEFTLEAN:
                    leaningPosture = new PresentationAction[4];
                    leaningPostureMistakes++;
                    break;
                case PresentationAction.MistakeType.DANCING:
                    periodicMovements = new PresentationAction[4];
                    periodicMovementsMistakes++;
                    break;
                case PresentationAction.MistakeType.HANDS_NOT_MOVING:
                    handsNotMoving = new PresentationAction[4];
                    handsNotMovingMistakes++;
                    break;
                case PresentationAction.MistakeType.HANDS_MOVING_MUCH:
                    handsMovingMuch = new PresentationAction[4];
                    handsMovingMuchMistakes++;
                    break;
                case PresentationAction.MistakeType.HIGH_VOLUME:
                    highVolumes = new PresentationAction[4];
                    highVolumesMistakes++;
                    break;
                case PresentationAction.MistakeType.LOW_VOLUME:
                    lowVolumes = new PresentationAction[4];
                    lowVolumesMistakes++;
                    break;
                case PresentationAction.MistakeType.LOW_MODULATION:
                    noModulation = new PresentationAction[4];
                    noModulationMistakes++;
                    break;
                case PresentationAction.MistakeType.LONG_PAUSE:
                    longPauses = new PresentationAction[4];
                    longPausesMistakes++;
                    break;
                case PresentationAction.MistakeType.LONG_TALK:
                    longTalks = new PresentationAction[4];
                    longTalksMistakes++;
                    break;
                case PresentationAction.MistakeType.HMMMM:
                    hmmms = new PresentationAction[4];
                    hmmmsMistakes++;
                    break;
            }
        }


        private void checkWhereToPutMistake()
        {
            int i = 0;
            int x = 0;
            // sentMistakes;
            PresentationAction temp = new PresentationAction();
            temp = ((PresentationAction)mistakes[0]).Clone();

            switch (temp.myMistake)
            {
                case PresentationAction.MistakeType.ARMSCROSSED:
                    crossedArmsMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (crossedArms[i] == null)
                        {
                            crossedArms[i] = temp;
                            crossedArms[i].myMistake = PresentationAction.MistakeType.ARMSCROSSED;
                            break;
                        }
                    }
                    sentMistakes = crossedArms;
                    break;
                case PresentationAction.MistakeType.LEFTHANDBEHINDBACK:
                case PresentationAction.MistakeType.RIGHTHANDBEHINDBACK:
                    handsBehindBackMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (handsBehindBack[i] == null)
                        {
                            handsBehindBack[i] = temp;
                            handsBehindBack[i].myMistake = PresentationAction.MistakeType.RIGHTHANDBEHINDBACK;
                            break;
                        }
                    }
                    sentMistakes = handsBehindBack;
                    break;
                case PresentationAction.MistakeType.LEGSCROSSED:
                    legsCrossedMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (legsCrossed[i] == null)
                        {
                            legsCrossed[i] = temp;
                            legsCrossed[i].myMistake = PresentationAction.MistakeType.LEGSCROSSED;
                            break;
                        }
                    }
                    sentMistakes = legsCrossed;
                    break;

                case PresentationAction.MistakeType.RIGHTHANDUNDERHIP:
                case PresentationAction.MistakeType.LEFTHANDUNDERHIP:
                    handsUnderHipsMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (handsUnderHips[i] == null)
                        {
                            handsUnderHips[i] = temp;
                            handsUnderHips[i].myMistake = PresentationAction.MistakeType.RIGHTHANDUNDERHIP;
                            break;
                        }
                    }
                    sentMistakes = handsUnderHips;
                    break;
                case PresentationAction.MistakeType.HUNCHBACK:
                    hunchPostureMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (hunchPosture[i] == null)
                        {
                            hunchPosture[i] = temp;
                            hunchPosture[i].myMistake = PresentationAction.MistakeType.HUNCHBACK;
                            break;
                        }
                    }
                    sentMistakes = hunchPosture;
                    break;
                case PresentationAction.MistakeType.RIGHTLEAN:
                case PresentationAction.MistakeType.LEFTLEAN:
                    leaningPostureMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (leaningPosture[i] == null)
                        {
                            leaningPosture[i] = temp;
                            leaningPosture[i].myMistake = PresentationAction.MistakeType.RIGHTLEAN;
                            break;
                        }
                    }
                    sentMistakes = leaningPosture;
                    break;
                case PresentationAction.MistakeType.DANCING:
                    periodicMovementsMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (periodicMovements[i] == null)
                        {
                            periodicMovements[i] = temp;
                            periodicMovements[i].myMistake = PresentationAction.MistakeType.DANCING;
                            break;
                        }
                    }
                    sentMistakes = periodicMovements;
                    break;
                case PresentationAction.MistakeType.HANDS_NOT_MOVING:
                    handsNotMovingMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (handsNotMoving[i] == null)
                        {
                           // System.Windows.Media.ImageSource im = parent.videoHandler.kinectImage.Source;  
                            handsNotMoving[i] = temp;
                            handsNotMoving[i].myMistake = PresentationAction.MistakeType.HANDS_NOT_MOVING;
                           // handsNotMoving[i].lastImage = im.CloneCurrentValue();

                            break;
                        }
                    }
                    sentMistakes = handsNotMoving;
                    break;
                case PresentationAction.MistakeType.HANDS_MOVING_MUCH:
                    handsMovingMuchMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (handsMovingMuch[i] == null)
                        {
                            handsMovingMuch[i] = temp;
                            handsMovingMuch[i].myMistake = PresentationAction.MistakeType.HANDS_MOVING_MUCH;
                            
                            break;
                        }
                    }
                    sentMistakes = handsMovingMuch;
                    break;
                case PresentationAction.MistakeType.HIGH_VOLUME:
                    highVolumesMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (highVolumes[i] == null)
                        {
                            highVolumes[i] = temp;
                            highVolumes[i].myMistake = PresentationAction.MistakeType.HIGH_VOLUME;
                            break;
                        }
                    }
                    sentMistakes = highVolumes;
                    break;
                case PresentationAction.MistakeType.LOW_VOLUME:
                    lowVolumesMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (lowVolumes[i] == null)
                        {
                            lowVolumes[i] = temp;
                            lowVolumes[i].myMistake = PresentationAction.MistakeType.LOW_VOLUME;
                            break;
                        }
                    }
                    sentMistakes = lowVolumes;
                    break;
                case PresentationAction.MistakeType.LOW_MODULATION:
                    noModulationMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (noModulation[i] == null)
                        {
                            noModulation[i] = temp;
                            noModulation[i].myMistake = PresentationAction.MistakeType.LOW_MODULATION;
                            break;
                        }
                    }
                    sentMistakes = noModulation;
                    break;
                case PresentationAction.MistakeType.LONG_PAUSE:
                    longPausesMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (longPauses[i] == null)
                        {
                            longPauses[i] = temp;
                            longPauses[i].myMistake = PresentationAction.MistakeType.LONG_PAUSE;
                            break;
                        }
                    }
                    sentMistakes = longPauses;
                    break;
                case PresentationAction.MistakeType.LONG_TALK:
                    longTalksMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (longTalks[i] == null)
                        {
                            longTalks[i] = temp;
                            longTalks[i].myMistake = PresentationAction.MistakeType.LONG_TALK;
                            break;
                        }
                    }
                    sentMistakes = longTalks;
                    break;
                case PresentationAction.MistakeType.HMMMM:
                    hmmmsMistakes++;
                    for (i = 0; i < 4; i++)
                    {
                        if (hmmms[i] == null)
                        {
                            hmmms[i] = temp;
                            hmmms[i].myMistake = PresentationAction.MistakeType.HMMMM;
                            break;
                        }
                    }
                    sentMistakes = hmmms;
                    break;
            }
            if (i == 4)
            {
                doInterruptionThing();
            }
        }



    }

        #endregion

      
}
