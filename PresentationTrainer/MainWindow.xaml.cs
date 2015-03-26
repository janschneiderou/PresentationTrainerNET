using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace PresentationTrainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        public FreestyleMode freestyleMode;
        public VolumeCalibration volumeCalibrationMode;
        public MainMenu mainMenu;
        public enum States { menu, freestyle, volumeCalibration, individual};
        public static States myState;

        private KinectSensor kinectSensor;
        public InfraredFrameReader frameReader = null;
        public RulesAnalyzer rulesAnalyzer;
        public RulesAnalyzerFIFO rulesAnalyzerFIFO;

        public VideoHandler videoHandler;
        public AudioHandler audioHandler;
        public BodyFrameHandler bodyFrameHandler;
        public IndividualSkills individualSkills;

        public MainWindow()
        {
            InitializeComponent();
            myState = new States();
            myState = States.menu;
            loadMode();

            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.frameReader = this.kinectSensor.InfraredFrameSource.OpenReader();

            rulesAnalyzer = new RulesAnalyzer(this);
            rulesAnalyzerFIFO = new RulesAnalyzerFIFO(this);
            videoHandler = new VideoHandler(this.kinectSensor);
            audioHandler = new AudioHandler(this.kinectSensor);
            bodyFrameHandler = new BodyFrameHandler(this.kinectSensor);
        }

        #region controlStuff
        public void loadMode()
        {
            switch (myState)
            {
                case States.menu:
                    loadMainMenu();
                    break;
                case States.freestyle:
                    loadFreestyle();
                    break;
                case States.volumeCalibration:
                    loadVolumeCalibration();
                    break;
               
            }
        }

        public void loadVolumeCalibration()
        {
            volumeCalibrationMode = new VolumeCalibration();
            volumeCalibrationMode.Height = this.ActualHeight;
            volumeCalibrationMode.Width = this.ActualWidth;
            MainCanvas.Children.Add(volumeCalibrationMode);
            Canvas.SetTop(volumeCalibrationMode, 0);
            Canvas.SetLeft(volumeCalibrationMode, 0);
            volumeCalibrationMode.Loaded += volumeCalibrationMode_Loaded;
        }

        void volumeCalibrationMode_Loaded(object sender, RoutedEventArgs e)
        {
            volumeCalibrationMode.parent = this;
            volumeCalibrationMode.loaded();
        }    

        public void loadMainMenu()
        {
            mainMenu = new MainMenu();
            MainCanvas.Children.Add(mainMenu);
            Canvas.SetTop(mainMenu, 0);
            Canvas.SetLeft(mainMenu, 0);
            mainMenu.Loaded += mainMenu_Loaded;
        }

        void mainMenu_Loaded(object sender, RoutedEventArgs e)
        {
            mainMenu.FreestyleButton.Click += mainMenu_FreestyleClicked;
            mainMenu.volumeCalibrationButton.Click += volumeCalibrationButton_Click;
        }

        public void loadFreestyle()
        {
            if(freestyleMode==null)
            {
                freestyleMode = new FreestyleMode();
            }
            
            freestyleMode.Height = this.ActualHeight;
            freestyleMode.Width = this.ActualWidth;
            MainCanvas.Children.Add(freestyleMode);
            Canvas.SetTop(freestyleMode, 0);
            Canvas.SetLeft(freestyleMode, 0);
            freestyleMode.Loaded += freeStyle_Loaded;
        }

        public void loadIndividualSkills( PresentationAction pa)
        {
            
        
            individualSkills = new IndividualSkills(pa);
            individualSkills.parent = this;
            MainCanvas.Children.Add(individualSkills);
            individualSkills.Height = ActualHeight;
            individualSkills.Width = ActualWidth;
            Canvas.SetTop(individualSkills, 0);
            Canvas.SetLeft(individualSkills, 0);
            myState = States.individual;
            individualSkills.Loaded += individualSkills_Loaded;
            
        }

        void individualSkills_Loaded(object sender, RoutedEventArgs e)
        {
            individualSkills.finish.GoBackButton.Click += GoBackButton_Click;
        }

        void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            myState = States.freestyle;
            individualSkills.finish.GoBackButton.Click -= GoBackButton_Click;
            individualSkills.Visibility = Visibility.Collapsed;
            MainCanvas.Children.Remove(individualSkills);

            if(freestyleMode!=null)
            {
                freestyleMode.OrdinaryReturn_Click(null, null);
            }
            else
            {
                loadMode();
            }
            
            
        }

        void freeStyle_Loaded(object sender, RoutedEventArgs e)
        {
            freestyleMode.parent = this;
            freestyleMode.loaded();
        }

        void volumeCalibrationButton_Click(object sender, RoutedEventArgs e)
        {
            closeMainMenu();
            myState = States.volumeCalibration;
            loadMode();
            volumeCalibrationMode.backButton.Click += volumeCalibrationMode_backButton_Click;
        }

        private void volumeCalibrationMode_backButton_Click(object sender, RoutedEventArgs e)
        {
            closeVolumeCalibration();
            myState = States.menu;
            loadMode();
            // we might have to unsubscribe (-=) to the click event of the pressed button
        }

        private void closeVolumeCalibration()
        {
            MainCanvas.Children.Remove(volumeCalibrationMode);
        }

        private void mainMenu_FreestyleClicked(object sender, RoutedEventArgs e)
        {
            closeMainMenu();
            myState = States.freestyle;
            loadMode();
            freestyleMode.stopButton.Click += freeStyleMode_stopButton_Click;
            //freestyleMode.reportStopButton.Click += freeStyleMode_reportStopButton_Click;
            //freestyleMode.logStopButton.Click += freeStyleMode_logStopButton_Click;
        }
        
        public void freeStyleMode_reportStopButton_Click(object sender, RoutedEventArgs e)
        {
            //Stop everything, then report.
            //after these two things, close freeStyleMode
            closeFreeStyleMode();
            myState = States.menu;
            loadMode();
            // we might have to unsubscribe (-=) to the click event of the pressed button
        }

        public void freeStyleMode_logStopButton_Click(object sender, RoutedEventArgs e)
        {
            //Stop everything, then log it, then report it.
            //after these three things, close freeStyleMode
            closeFreeStyleMode();
            myState = States.menu;
            loadMode();
            // we might have to unsubscribe (-=) to the click event of the pressed button
        }
        
        public void freeStyleMode_stopButton_Click(object sender, RoutedEventArgs e)
        {
            closeFreeStyleMode();
            myState = States.menu;
            loadMode();
            // we might have to unsubscribe (-=) to the click event of the pressed button
        }

        private void closeFreeStyleMode()
        {
            MainCanvas.Children.Remove(freestyleMode);
        }

        public void closeMainMenu()
        {
            MainCanvas.Children.Remove(mainMenu);
        }
        
        

        #endregion


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.frameReader != null)
            {
                this.frameReader.FrameArrived += frameReader_FrameArrived;
            }
        }
     

        void frameReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
          //  rulesAnalyzer.AnalyseRules();
            switch(myState)
            {
                case States.freestyle:
                    rulesAnalyzerFIFO.AnalyseRules();
                    break;
                case States.individual:
                    if(individualSkills.ready)
                    {
                        individualSkills.analyze();
                    }
                    break;
            }
            
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
          //  audioHandlerControl.close();
           // bodyFrameHandlerControl.close();
            videoHandler.close();

        }
    }
}
