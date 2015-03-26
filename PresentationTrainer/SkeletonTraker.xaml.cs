﻿using Microsoft.Kinect;
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

namespace PresentationTrainer
{
    /// <summary>
    /// Interaction logic for SkeletonTraker.xaml
    /// </summary>
    public partial class SkeletonTraker : UserControl
    {
        MainWindow parent;

        public SkeletonTraker()
        {
            InitializeComponent();
        }
        public void initialize(MainWindow parent)
        {
            this.parent = parent;
            if (parent.bodyFrameHandler.bodyFrameReader != null)
            {
                parent.bodyFrameHandler.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }
         public void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            parent.bodyFrameHandler.Reader_FrameArrived(sender, e);
          //  parent.bodyFrameHandler.paintSkeleton = true;
            myImage.Source = parent.bodyFrameHandler.kinectImage.Source;
        }
    }
}
