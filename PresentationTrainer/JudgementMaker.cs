﻿using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PresentationTrainer
{
    public  class JudgementMaker
    {
        public ArrayList tempMistakeList;
        public ArrayList mistakeList;
        public ArrayList bodyMistakes;
        public ArrayList tempGoodiesList;
        public ArrayList GoodiesList;
        public ArrayList voiceAndMovementsList;
        public ArrayList audioMovementMistakeTempList;

        public double TIME_TO_CONSIDER_ACTION = 300;
        public double TIME_TO_CONSIDER_CORRECTION = 100;
        public double TIME_TO_CONSIDER_INTERRUPTION = 6000;

        public double ThresholdDefaultHandMovement = 1000;
        public double HandMovementFactor = 3770;
        public double bufferTime = 400;

     //   public bool periodicMov = false;

        public int[] nolongerBodyErrors;

        public PresentationAction myVoiceAndMovementObject;

        

        BodyFramePreAnalysis bfpa;
        AudioPreAnalysis apa;
        Body oldBody;
        MainWindow parent;
        PeriodicMovements periodicMovements;
        PresentationAction highVolume=null;
        PresentationAction LowVolume=null;

        public JudgementMaker(MainWindow parent)
        {
           this.parent = parent;
           myVoiceAndMovementObject = new PresentationAction();
           periodicMovements = new PeriodicMovements();
           
           tempMistakeList = new ArrayList();
           mistakeList = new ArrayList();
           voiceAndMovementsList = new ArrayList();
           audioMovementMistakeTempList = new ArrayList();
           bodyMistakes = new ArrayList();
            
        }

        public void clearLists()
        {
            tempMistakeList.Clear(); 
            mistakeList.Clear();
            voiceAndMovementsList.Clear(); 
            audioMovementMistakeTempList.Clear(); 
            bodyMistakes.Clear(); 
        }

        #region analyzeCycle

        public void analyze()
        {

            bfpa = this.parent.bodyFrameHandler.bodyFramePreAnalysis;
            apa = this.parent.audioHandler.audioPreAnalysis;

            searchMistakes();

            saveCurrentBodyAsOldBody();

            
        }

        private void searchMistakes()
        {

            addBodyMistakes();
            deleteBodyMistakes();
            findMistakesInVoiceAndMovement();
            deleteVoiceAndMovementsMistakes();
            mistakeList = new ArrayList(bodyMistakes);
            mistakeList.AddRange(audioMovementMistakeTempList);
            
            if(bfpa.body!=null)
            {
                if (periodicMovements.checPeriodicMovements(bfpa.body))
                {
                    PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.DANCING);
                    pa.interrupt = true;
                   // periodicMov = true;
                    mistakeList.Insert(0, pa);
                }

            }
           
            

           //todo add mistakes together
        }

        #endregion


        
        #region addingMistakes

        #region addingBodyMistakes

        private void addBodyMistakes()
        {
            addNewMistakesToTemp();
            findMistakesInTempList();

        }

        private void addNewMistakesToTemp()
        {
            
            foreach (PresentationAction fb in bfpa.currentMistakes)
            {
                int x = 0;
                foreach (PresentationAction fa in tempMistakeList)
                {
                    if (fa.myMistake == fb.myMistake)
                    {
                        x = 1;
                        break;
                    }
                }
                if (x == 0)
                {
                    fb.timeStarted = DateTime.Now.TimeOfDay.TotalMilliseconds;
                    tempMistakeList.Add(fb);
                }
            }
        }

        private void findMistakesInTempList()
        {
            foreach (PresentationAction fa in tempMistakeList)
            {
                foreach (PresentationAction fb in bfpa.currentMistakes)
                {
                    if (fa.myMistake == fb.myMistake)
                    {
                        if (findMistakeInMistakeList(fb) == false)
                        {
                            if (checkTimeToPutMistake(fa))
                            {

                                bodyMistakes.Add(fa);
                                
                                
                                fa.firstImage = parent.videoHandler.kinectImage.Source.CloneCurrentValue();
                            }
                        }
                        else
                        {
                            if (checkTimeToPutInterruption(fa))
                            {
                                fa.interrupt = true;
                            }
                        }
                    }
                }
            }
        }

        

        private bool findMistakeInMistakeList(PresentationAction fb)
        {
            bool found=false;
            foreach(PresentationAction fa in mistakeList)
            {
                if(fb.myMistake==fa.myMistake)
                {
                    found = true;
                    break;
                }   
            }
            return found;
        }

        private bool checkTimeToPutMistake(PresentationAction fa)
        {
            bool result = false;
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (currentTime - fa.timeStarted > TIME_TO_CONSIDER_ACTION)
            {
                result = true;
            }
            return result;
        }

        private bool checkTimeToPutInterruption(PresentationAction fa)
        {
            bool result = false;
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (currentTime - fa.timeStarted > TIME_TO_CONSIDER_INTERRUPTION)
            {
                result = true;
            }
            return result;
        }

        #endregion

        #region voiceAndMovementStuff

        #region handleLists

        public void findMistakesInVoiceAndMovement()
        {
            audioMovementMistakeTempList = new ArrayList();
            handleHmmm();
           

            if (myVoiceAndMovementObject.timeStarted == 0)
            {
                resetMyVoiceAndMovement();
            }
            if (myVoiceAndMovementObject.isSpeaking == true && apa.isSpeaking == true)
            {
                handleSpeakingTime();
            }
            else if (myVoiceAndMovementObject.isSpeaking == false && apa.isSpeaking == true)
            {
                resetMyVoiceAndMovement();
                handleSpeakingTime();

            }
            else if (myVoiceAndMovementObject.isSpeaking == false && apa.isSpeaking == false)
            {
                handlePauses();
            }
            else if (myVoiceAndMovementObject.isSpeaking == true && apa.isSpeaking == false)
            {
                resetMyVoiceAndMovement();
                handlePauses();
            }

        }

      

        #endregion

      
       

        private void handleHmmm()
        {
            if(apa.foundHmmm==true)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.HMMMM);
                pa.timeStarted = myVoiceAndMovementObject.timeStarted;
                pa.isVoiceAndMovementMistake = true;
                audioMovementMistakeTempList.Add(pa);
            }
        }

        private void handleSpeakingTime()
        {
            handleVolume();
            handleTimeSpeaking();
            if(bfpa.body!=null)
            {
                handleHandMovements();
            }
            
            
        }

        private void handleTimeSpeaking()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (currentTime - myVoiceAndMovementObject.timeStarted > apa.ThresholdIsSpeakingLongTime)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LONG_TALK);
                pa.timeStarted = myVoiceAndMovementObject.timeStarted;
                pa.isVoiceAndMovementMistake = true;
                if (currentTime - myVoiceAndMovementObject.timeStarted > apa.ThresholdIsSpeakingVeryLongTime)
                {
                    pa.interrupt = true;
                }

                audioMovementMistakeTempList.Add(pa);
            }
        }

        private void handleHandMovements()
        {
            
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            CameraSpacePoint handLeft = bfpa.body.Joints[JointType.HandLeft].Position;
            CameraSpacePoint handRight = bfpa.body.Joints[JointType.HandRight].Position;
            CameraSpacePoint hipLeft = bfpa.body.Joints[JointType.HipLeft].Position;
            CameraSpacePoint hipRight = bfpa.body.Joints[JointType.HipRight].Position;

            double leftDistance = Math.Sqrt(
                (handLeft.X - hipLeft.X) * (handLeft.X - hipLeft.X) +
                (handLeft.Y - hipLeft.Y) * (handLeft.Y - hipLeft.Y) +
                (handLeft.Z - hipLeft.Z) * (handLeft.Z - hipLeft.Z));

            double rightDistance = Math.Sqrt(
                (handRight.X - hipRight.X) * (handRight.X - hipRight.X) +
                (handRight.Y - hipRight.Y) * (handRight.Y - hipRight.Y) +
                (handRight.Z - hipRight.Z) * (handRight.Z - hipRight.Z));

            if(myVoiceAndMovementObject.leftHandHipDistance==0)
            {
                myVoiceAndMovementObject.leftHandHipDistance = leftDistance;
                myVoiceAndMovementObject.rightHandHipDistance = rightDistance;
            }
            
            double rightMovement = Math.Abs(myVoiceAndMovementObject.rightHandHipDistance - rightDistance);
            double leftMovement = Math.Abs(myVoiceAndMovementObject.leftHandHipDistance - leftDistance);

            myVoiceAndMovementObject.leftHandHipDistance = leftDistance;
            myVoiceAndMovementObject.rightHandHipDistance = rightDistance;

            double currentHandMovement = rightMovement;

            if(rightMovement>leftMovement)
            {
                myVoiceAndMovementObject.totalHandMovement = myVoiceAndMovementObject.totalHandMovement + rightMovement * HandMovementFactor;
            }
            else
            {
                currentHandMovement = leftMovement;
                myVoiceAndMovementObject.totalHandMovement = myVoiceAndMovementObject.totalHandMovement + leftMovement * HandMovementFactor;
            }
            
            double difTime= (currentTime - myVoiceAndMovementObject.timeStarted);
            if(difTime>0 )
            {
                myVoiceAndMovementObject.averageHandMovement = myVoiceAndMovementObject.totalHandMovement / difTime ;
            }

            if (myVoiceAndMovementObject.averageHandMovement < 1 )//&& currentHandMovement < 0.006)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.HANDS_NOT_MOVING);
                pa.timeStarted = myVoiceAndMovementObject.timeStarted;
                pa.isVoiceAndMovementMistake = true;
                if(myVoiceAndMovementObject.firstImage!=null)
                {
                    System.Windows.Media.ImageSource im = parent.videoHandler.kinectImage.Source;  
                  //  pa.firstImage = myVoiceAndMovementObject.firstImage.CloneCurrentValue();
                    myVoiceAndMovementObject.lastImage = im.CloneCurrentValue();
                  //  pa.lastImage = im.CloneCurrentValue();
                }
                
                audioMovementMistakeTempList.Add(pa);
            }
            else if (difTime > 2000) //use for debugging
            {
                int aa = 0;
                aa++;
          //     myVoiceAndMovementObject.totalHandMovement = difTime + 1000;
            }
            

        }
        private void handleVolume()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if(myVoiceAndMovementObject.minVolume==0)
            {
                myVoiceAndMovementObject.minVolume = apa.averageVolume;
                myVoiceAndMovementObject.maxVolume = apa.averageVolume;
            }
            if(apa.averageVolume<myVoiceAndMovementObject.minVolume)
            {
                myVoiceAndMovementObject.minVolume = apa.averageVolume;
            }
            if(apa.averageVolume>myVoiceAndMovementObject.maxVolume)
            {
                myVoiceAndMovementObject.maxVolume = apa.averageVolume;
            }
            if(apa.averageVolume>apa.ThresholdIsSpeakingLoud)
            {
                LowVolume = null;
                if(highVolume==null)
                {
                    highVolume = new PresentationAction(PresentationAction.MistakeType.HIGH_VOLUME);
                    highVolume.isVoiceAndMovementMistake = true;
                }
                if (currentTime - highVolume.timeStarted  > TIME_TO_CONSIDER_ACTION)
                {
                    
                    audioMovementMistakeTempList.Add(highVolume);
                }
            }
            else if(apa.averageVolume<apa.ThresholdIsSpeakingSoft)
            {
                highVolume = null;

                if (LowVolume == null)
                {
                    LowVolume = new PresentationAction(PresentationAction.MistakeType.LOW_VOLUME);
                    LowVolume.isVoiceAndMovementMistake = true;
                }
                if (currentTime - LowVolume.timeStarted > TIME_TO_CONSIDER_ACTION)
                {
                    audioMovementMistakeTempList.Add(LowVolume);
                }
            }
            else
            {
                highVolume = null;
                LowVolume = null;
            }
            if(currentTime - myVoiceAndMovementObject.timeStarted>3000 && 
                myVoiceAndMovementObject.maxVolume- myVoiceAndMovementObject.minVolume < 0.001)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LOW_MODULATION);
                {
                    pa.timeStarted = myVoiceAndMovementObject.timeStarted;
                    pa.isVoiceAndMovementMistake = true;
                    audioMovementMistakeTempList.Add(pa);
                }
            }


        }
        public void handlePauses()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            if (currentTime - myVoiceAndMovementObject.timeStarted > apa.ThresholdIsLongPauseTime)
            {
                PresentationAction pa = new PresentationAction(PresentationAction.MistakeType.LONG_PAUSE);
                pa.timeStarted = myVoiceAndMovementObject.timeStarted;
                pa.isVoiceAndMovementMistake = true;
                if (currentTime - myVoiceAndMovementObject.timeStarted > apa.ThresholdIsVeryLongPauseTime)
                {
                    pa.interrupt = true;
                }

                audioMovementMistakeTempList.Add(pa);
            }
        }

        public void resetMyVoiceAndMovement()
        {
            myVoiceAndMovementObject.isSpeaking = apa.isSpeaking;
            myVoiceAndMovementObject.timeStarted = DateTime.Now.TimeOfDay.TotalMilliseconds;
            myVoiceAndMovementObject.interrupt = false;
            myVoiceAndMovementObject.totalHandMovement = ThresholdDefaultHandMovement;
            myVoiceAndMovementObject.minVolume = 0;
            myVoiceAndMovementObject.maxVolume = 0;
            myVoiceAndMovementObject.rightHandHipDistance = 0;
            myVoiceAndMovementObject.leftHandHipDistance = 0;
            myVoiceAndMovementObject.averageHandMovement = 1;
            audioMovementMistakeTempList = new ArrayList();
            if (parent.videoHandler.kinectImage.Source!=null)
            {
                myVoiceAndMovementObject.firstImage = null;
                myVoiceAndMovementObject.firstImage = parent.videoHandler.kinectImage.Source.CloneCurrentValue();
            }
            
        }

        #endregion

        #endregion

        #region deletingMistakes

        #region deletingBodyMistakes
        private void deleteBodyMistakes()
        {
            findNoMistakesInBodyTempList();
            removeMistakesBodyTemp();
            removeBodyMistakes();
        }

        private void findNoMistakesInBodyTempList()
        {
            nolongerBodyErrors = new int[tempMistakeList.Count];
            int i = 0;
            foreach (PresentationAction fa in tempMistakeList)
            {
                int x = 0;
                foreach (PresentationAction fb in bfpa.currentMistakes)
                {
                    if (fa.myMistake == fb.myMistake)
                    {
                        x = 1;
                        fa.timeFinished = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        break;
                    }
                }
                if (x == 0)
                {
                    double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
              //      if (currentTime - fa.timeFinished > TIME_TO_CONSIDER_CORRECTION)
              //      {
                        nolongerBodyErrors[i] = 1;
               //     }
                }
                i++;
            }
        }

        private void removeMistakesBodyTemp()
        {
            for (int i = tempMistakeList.Count; i > 0; i--)
            {
                if (nolongerBodyErrors[i - 1] == 1)
                {
                    tempMistakeList.RemoveAt(i - 1);
                }
            }
        }

        private void removeBodyMistakes()
        {
            int[] nolongerMistakes = new int[bodyMistakes.Count];
            int i = 0;
            foreach (PresentationAction fa in bodyMistakes)
            {
                for (int j = tempMistakeList.Count; j > 0; j--)
                {
                    if (((PresentationAction)tempMistakeList[j - 1]).myMistake == fa.myMistake)
                    {
                        nolongerMistakes[i] = 1;
                    }
                }
                i++;
            }
            for (int ii = bodyMistakes.Count; ii > 0; ii--)
            {
                if (nolongerMistakes[ii - 1] == 0)
                {
                    bodyMistakes.RemoveAt(ii - 1);
                }
            }
        }

        #endregion

        #region deletingVoiceandMovements

        private void deleteVoiceAndMovementsMistakes()
        {
            int x= 0;
            int[] mistakesToDelete= new int [mistakeList.Count];
            foreach (PresentationAction pa in  mistakeList)
            {
                foreach(PresentationAction pb in audioMovementMistakeTempList)
                {
                    if(pb.myMistake==pa.myMistake)
                    {
                        mistakesToDelete[x] = 1;
                        break;
                    }
                }
                x++;
            }
            for (int j= mistakeList.Count; j>0;j--)
            {
                if(mistakesToDelete[j-1]==1 && ((PresentationAction)mistakeList[j-1]).isVoiceAndMovementMistake)
                {
                    mistakeList.RemoveAt(j - 1);
                }
            }
        }
        #endregion
        #endregion

        private void saveCurrentBodyAsOldBody()
        {
            bfpa.setOldBody();
        }

    }
}
