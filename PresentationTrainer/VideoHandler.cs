﻿using System;
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
    public class VideoHandler
    {
        private KinectSensor kinectSensor;

        #region bodyMapInitialization

        public Image kinectImage;

        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for depth/color/body index frames
        /// </summary>
        public MultiSourceFrameReader multiFrameSourceReader = null;

        /// <summary>
        /// Intermediate storage for receiving depth frame data from the sensor
        /// </summary>
        private ushort[] depthFrameData = null;

        /// <summary>
        /// Intermediate storage for receiving color frame data from the sensor
        /// </summary>
        private byte[] colorFrameData = null;

        /// <summary>
        /// Intermediate storage for receiving body index frame data from the sensor
        /// </summary>
        private byte[] bodyIndexFrameData = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] displayPixels = null;

        /// <summary>
        /// Intermediate storage for the color to depth mapping
        /// </summary>
        private DepthSpacePoint[] depthPoints = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap bitmapBody = null;

        #endregion

        public VideoHandler(KinectSensor kinectSensor)
        {
            kinectImage=new Image();
            this.kinectSensor = kinectSensor;
            // open multiframereader for depth, color, and bodyindex frames
            this.multiFrameSourceReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);

            // wire handler for frames arrival
      //      this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get FrameDescription from DepthFrameSource
            FrameDescription depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            int depthWidth = depthFrameDescription.Width;
            int depthHeight = depthFrameDescription.Height;

            // allocate space to put the pixels being received and converted
            this.depthFrameData = new ushort[depthWidth * depthHeight];
            this.bodyIndexFrameData = new byte[depthWidth * depthHeight];

            // get FrameDescription from ColorFrameSource
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;

            this.displayPixels = new byte[colorWidth * colorHeight * this.bytesPerPixel];
            this.depthPoints = new DepthSpacePoint[colorWidth * colorHeight];

            // create the bitmap to display
            this.bitmapBody = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);

            // allocate space to put the pixels being received
            this.colorFrameData = new byte[colorWidth * colorHeight * this.bytesPerPixel];
        }

        public void close()
        {
            if (this.multiFrameSourceReader != null)
            {
                // MultiSourceFrameReder is IDisposable
                this.multiFrameSourceReader.Dispose();
                this.multiFrameSourceReader = null;
            }
        }

        public void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            int depthWidth = 0;
            int depthHeight = 0;

            bool multiSourceFrameProcessed = false;
            bool colorFrameProcessed = false;
            bool depthFrameProcessed = false;
            bool bodyIndexFrameProcessed = false;

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            if (multiSourceFrame != null)
            {
                // Frame Acquisition should always occur first when using multiSourceFrameReader
                using (DepthFrame depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame())
                {
                    if (depthFrame != null)
                    {
                        FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                        depthWidth = depthFrameDescription.Width;
                        depthHeight = depthFrameDescription.Height;

                        if ((depthWidth * depthHeight) == this.depthFrameData.Length)
                        {
                            depthFrame.CopyFrameDataToArray(this.depthFrameData);
                            depthFrameProcessed = true;
                        }
                    }
                }

                using (ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
                {
                    if (colorFrame != null)
                    {
                        FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                        if ((colorFrameDescription.Width * colorFrameDescription.Height * this.bytesPerPixel) == this.colorFrameData.Length)
                        {
                            if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                            {
                                colorFrame.CopyRawFrameDataToArray(this.colorFrameData);
                            }
                            else
                            {
                                colorFrame.CopyConvertedFrameDataToArray(this.colorFrameData, ColorImageFormat.Bgra);
                            }

                            colorFrameProcessed = true;
                        }
                    }
                }

                using (BodyIndexFrame bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame())
                {
                    if (bodyIndexFrame != null)
                    {
                        FrameDescription bodyIndexFrameDescription = bodyIndexFrame.FrameDescription;

                        if ((bodyIndexFrameDescription.Width * bodyIndexFrameDescription.Height) == this.bodyIndexFrameData.Length)
                        {
                            bodyIndexFrame.CopyFrameDataToArray(this.bodyIndexFrameData);
                            bodyIndexFrameProcessed = true;
                        }
                    }

                    multiSourceFrameProcessed = true;
                }
            }

            // we got all frames
            if (multiSourceFrameProcessed && depthFrameProcessed && colorFrameProcessed && bodyIndexFrameProcessed)
            {
                this.coordinateMapper.MapColorFrameToDepthSpace(this.depthFrameData, this.depthPoints);

                Array.Clear(this.displayPixels, 0, this.displayPixels.Length);

                // loop over each row and column of the depth
                for (int colorIndex = 0; colorIndex < this.depthPoints.Length; ++colorIndex)
                {
                    DepthSpacePoint depthPoint = this.depthPoints[colorIndex];

                    if (!float.IsNegativeInfinity(depthPoint.X) && !float.IsNegativeInfinity(depthPoint.Y))
                    {
                        // make sure the depth pixel maps to a valid point in color space
                        int depthX = (int)(depthPoint.X + 0.5f);
                        int depthY = (int)(depthPoint.Y + 0.5f);

                        if ((depthX >= 0) && (depthX < depthWidth) && (depthY >= 0) && (depthY < depthHeight))
                        {
                            int depthIndex = (depthY * depthWidth) + depthX;
                            byte player = this.bodyIndexFrameData[depthIndex];

                            // if we're tracking a player for the current pixel, sets its color and alpha to full
                            if (player != 0xff)
                            {
                                // set source for copy to the color pixel
                                int sourceIndex = colorIndex * this.bytesPerPixel;

                                // write out blue byte
                                this.displayPixels[sourceIndex] = this.colorFrameData[sourceIndex++];

                                // write out green byte
                                this.displayPixels[sourceIndex] = this.colorFrameData[sourceIndex++];

                                // write out red byte
                                this.displayPixels[sourceIndex] = this.colorFrameData[sourceIndex++];

                                // write out alpha byte
                                this.displayPixels[sourceIndex] = 0xff;
                            }
                        }
                    }
                }

                this.RenderColorPixels();
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderColorPixels()
        {
            this.bitmapBody.WritePixels(
                new Int32Rect(0, 0, this.bitmapBody.PixelWidth, this.bitmapBody.PixelHeight),
                this.displayPixels,
                this.bitmapBody.PixelWidth * this.bytesPerPixel,
                0);

            kinectImage.Source = bitmapBody;
        }

    
    
    }
}
