//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;
        
        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        
        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;
        static Socket server;
        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        
        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                   this.sensor = potentialSensor;                    break;                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }

 server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
 server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999));//绑定端口号和IP
            Console.WriteLine("服务端已经开启");

            



        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// 

        double rrget_roll(double p11, double p12, double p13, double p21, double p22, double p23, double p31, double p32, double p33)
        {
            double p11_p21 = p11 - p21;
            double p12_p22 = p12 - p22;
            double p13_p23 = p13 - p23;
            double p31_p21 = p31 - p21;
            double p32_p22 = p32 - p22;
            double p33_p23 = p33 - p23;

            double fenzi = (p11_p21 * p31_p21) + (p32_p22 * p12_p22) + (p13_p23 * p33_p23);
            double fenmu1 = Math.Sqrt( Math.Pow(p11_p21, 2) + Math.Pow(p12_p22, 2) + Math.Pow(p13_p23, 2));
            double fenmu2 = Math.Sqrt(Math.Pow(p31_p21, 2) + Math.Pow(p32_p22, 2) + Math.Pow(p33_p23, 2));
            double fenmu = fenmu1 * fenmu2;
            double roll = Math.Acos(fenzi / fenmu);
            return roll;
            
        }

        double rrpitch(double p11,double p12,double p13,double p21,double p22,double p23)
        {
            double p21_p11 = p21 - p11;
            double p22_p12 = p22 - p12;
            double p23_p13 = p23 - p13;
            double pe;
            pe = p22_p12 / Math.Sqrt(p21_p11 * p21_p11 + p22_p12 * p22_p12 + p23_p13 * p23_p13);

            return Math.Acos(pe);
        }

        double rrYaw(double p11, double p12, double p13, double p21, double p22, double p23, double p31, double p32, double p33,double p41,double p42,double p43)
        {
            //p2-p1
            double p21_p11 = p21 - p11;
            double p22_p12 = p22 - p12;
            double p23_p13 = p23 - p13;

            //p4-p3
            double p41_p31 = p41 - p31;
            double p42_p32 = p42 - p32;
            double p43_p33 = p43 - p33;

            //p3-p2
            double p31_p21 = p31 - p21;
            double p32_p22 = p32 - p22;
            double p33_p23 = p33 - p21;

            //(p4-p3)*(p3-p2)
            double i = p42_p32 * p33_p23 - p43_p33 * p32_p22;
            double j = p43_p33 * p31_p21 - p41_p31 * p33_p23;
            double k = p41_p31 * p32_p22 - p42_p32 * p31_p21;

            //(p4-p3)*(p3-p2)·（p2-p1)
            double x = i * p21_p11;
            double y = j * p22_p12;
            double z = k * p23_p13;
            double mole = x + y + z;

            //分母
            //|p2p1|
            double temp1 = p21_p11 * p21_p11 + p22_p12 * p22_p12 + p23_p13 * p23_p13;
            double len1 = Math.Sqrt(temp1);

            //|p3,4 p2,3|
            double temp2 = i * i + j * j + k * k;
            double len2 = Math.Sqrt(temp2);

            double deno = len1 * len2;

            double final = mole / deno;

            double angle = Math.Acos(final) + Math.PI / 2;

            return angle;
        }

        double Pitch(Vector4 quaternion)
        {
            double value1 = 2.0 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            double value2 = 1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);

          //  double roll = Math.Atan2(value1, value2);
            double roll = Math.Atan2(value1,value2);
            //Console.WriteLine("pitch:" + "W:" + quaternion.W + "   Z:" + quaternion.Z + "   X:" + quaternion.X + "   Y" + quaternion.Y + "   shuju:" + roll);

            return roll;
        }

        /// <summary>
        /// Rotates the specified quaternion around the Y axis.
        /// </summary>
        /// <param name="quaternion">The orientation quaternion.</param>
        /// <returns>The rotation in degrees.</returns>
        double Yaw(Vector4 quaternion)
        {
            double value = 2.0 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            
            value = value > 1.0 ? 1.0 : value;
            value = value < -1.0 ? -1.0 : value;

            double pitch = Math.Asin(value);
            //Console.WriteLine("yaw:" + "W:" + quaternion.W + "   Z:" + quaternion.Z + "   X:" + quaternion.X + "   Y" + quaternion.Y+"   ans:"+pitch);
            return -pitch;
        }

        /// <summary>
        /// Rotates the specified quaternion around the Z axis.
        /// </summary>
        /// <param name="quaternion">The orientation quaternion.</param>
        /// <returns>The rotation in degrees.</returns>
        double Roll(Vector4 quaternion)
        {
            double value1 = 2.0 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double value2 = 1.0 - 2.0 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
         //   double yaw = Math.Atan2(value1, value2);
            double yaw = Math.Atan2(value1,value2);
            yaw = -yaw;
           // Console.WriteLine("roll:" + "W:" + quaternion.W + "   Z:" + quaternion.Z + "   X:" + quaternion.X + "   Y" + quaternion.Y + "   ans:" + yaw);
            return yaw;
        }

        

        int times = 0;
        
       /* void get_sketetonpiont(Skeleton first)
        {
            times = times + 1;
            if (times == 10)
            {
                double ShoulderRight_roll=Roll(first.BoneOrientations[JointType.ShoulderRight].AbsoluteRotation.Quaternion);
                //double ShoulderRight_roll = Pitch(first.BoneOrientations[JointType.ShoulderRight].AbsoluteRotation.Quaternion);
                double ShoulderRight_pitch = Pitch(first.BoneOrientations[JointType.ShoulderRight].AbsoluteRotation.Quaternion);
                double ShoulderRight_yaw = Yaw(first.BoneOrientations[JointType.ShoulderRight].AbsoluteRotation.Quaternion);
              //  double ShoulderRight_pitch = Yaw(first.BoneOrientations[JointType.ElbowRight].AbsoluteRotation.Quaternion);
                //double ElbowRight_roll = Pitch(first.BoneOrientations[JointType.ElbowRight].AbsoluteRotation.Quaternion);
                //double ElbowRight_yaw = Yaw(first.BoneOrientations[JointType.ElbowRight].AbsoluteRotation.Quaternion);
                EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

                string ShoulderRight_roll_ = ShoulderRight_roll.ToString();
                string ShoulderRight_pitch_ = ShoulderRight_pitch.ToString();
                string ShoulderRight_yaw_ = ShoulderRight_yaw.ToString();
                //string ElbowRight_roll_ = ElbowRight_roll.ToString();
                //string ElbowRight_yaw_ = ElbowRight_yaw.ToString();
               // Console.WriteLine(ShoulderRight_roll_ + "   " + ShoulderRight_pitch + "    " + ElbowRight_roll + "      " + ElbowRight_yaw);
                Console.WriteLine(ShoulderRight_roll + "   " + ShoulderRight_pitch + "    " + "      " + ShoulderRight_yaw);
                //if ()
                //string all = ShoulderRight_roll_ + "," + ShoulderRight_pitch_ + "," + ElbowRight_roll_ + "," + ElbowRight_yaw_;
                string all = ShoulderRight_roll_ + "," + ShoulderRight_pitch_;
                server.SendTo(Encoding.UTF8.GetBytes(all), point);
                //server.SendTo(Encoding.UTF8.GetBytes(all), point);
                Console.WriteLine("succeed  ：");
                Console.WriteLine();
                times = 0;
            }
            */

 /*       void get_sketetonpiont(Skeleton first)
        {

            times = times + 1;
            // lefthand.Text = "lefthand = " + first.Joints[JointType.HandLeft].Position.X + "              " + first.Joints[JointType.HandLeft].Position.Y + "              " + first.Joints[JointType.HandLeft].Position.Z;
            // righthand.Text = "righthand = " + first.Joints[JointType.HandRight].Position.X + "              " + first.Joints[JointType.HandRight].Position.Y + "              " + first.Joints[JointType.HandRight].Position.Z;
            //  head.Text = "head = " + first.Joints[JointType.Head].Position.X + "              " + first.Joints[JointType.Head].Position.Y + "              " + first.Joints[JointType.Head].Position.Z;
            //  shouldercenter.Text = "shouldercenter = " + first.Joints[JointType.ShoulderCenter].Position.X + "              " + first.Joints[JointType.ShoulderCenter].Position.Y + "              " + first.Joints[JointType.ShoulderCenter].Position.Z;
            if (times == 10)
            {

                double roll = rrget_roll(first.Joints[JointType.WristLeft].Position.X, first.Joints[JointType.WristLeft].Position.Y, first.Joints[JointType.WristLeft].Position.Z,
                                       first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                                       first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z);
                roll = 3.14-roll ;
                double yaw = rrYaw(first.Joints[JointType.WristLeft].Position.X, first.Joints[JointType.WristLeft].Position.Y, first.Joints[JointType.WristLeft].Position.Z,
                                 first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                                 first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z,
                                 first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z);
                yaw =-( yaw - 3.14)+0.75;

                double roll2 = rrget_roll(first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                                       first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z,
                                       first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z);
                roll2 = 1.57 - roll2+0.2;
          //      double pitch2 = rrget_roll(
          //                       first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z,
           //                      first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z,
            //                     first.Joints[JointType.HipCenter].Position.X, first.Joints[JointType.HipCenter].Position.Y, first.Joints[JointType.HipCenter].Position.Z);
                double pitch2 = rrpitch(
                    first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                                 first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z
               );

                pitch2 = 1.8 - pitch2;



              //  pitch2 = pitch2 - 1.57;
                Console.WriteLine("头部  ：" + first.Joints[JointType.Head].Position.X + "            " + first.Joints[JointType.Head].Position.Y + "            " + first.Joints[JointType.Head].Position.Z);
                Console.WriteLine("肩部心  ：" + first.Joints[JointType.ShoulderCenter].Position.X + "            " + first.Joints[JointType.ShoulderCenter].Position.Y + "            " + first.Joints[JointType.ShoulderCenter].Position.Z);
                Console.WriteLine("左肩  ：" + first.Joints[JointType.ShoulderLeft].Position.X + "            " + first.Joints[JointType.ShoulderLeft].Position.Y + "            " + first.Joints[JointType.ShoulderLeft].Position.Z);
                Console.WriteLine("左肘  ：" + first.Joints[JointType.ElbowLeft].Position.X + "            " + first.Joints[JointType.ElbowLeft].Position.Y + "            " + first.Joints[JointType.ElbowLeft].Position.Z);
                Console.WriteLine("左腕  ：" + first.Joints[JointType.WristLeft].Position.X + "            " + first.Joints[JointType.WristLeft].Position.Y + "            " + first.Joints[JointType.WristLeft].Position.Z);

                EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

                string msg = roll.ToString();
                string msg2 = yaw.ToString();
                
                Console.WriteLine("roll:" + msg);
                Console.WriteLine("yaw:" + msg2);


                string msg3 = roll2.ToString();
                string msg4 = pitch2.ToString();
                string aa = msg3 + "," + msg4 + "," + msg + "," + msg2;
                Console.WriteLine("roll2:" + msg3);
                Console.WriteLine("pitch2:" + msg4);
                server.SendTo(Encoding.UTF8.GetBytes(aa), point);
                Console.WriteLine("succeed  ：");



                // Console.WriteLine("右肩  ：" + first.Joints[JointType.ShoulderRight].Position.X + "            " + first.Joints[JointType.ShoulderRight].Position.Y + "            " + first.Joints[JointType.ShoulderRight].Position.Z);
                //Console.WriteLine("右肘  ：" + first.Joints[JointType.ElbowRight].Position.X + "            " + first.Joints[JointType.ElbowRight].Position.Y + "            " + first.Joints[JointType.ElbowRight].Position.Z);
                //Console.WriteLine("右腕  ：" + first.Joints[JointType.WristRight].Position.X + "            " + first.Joints[JointType.WristRight].Position.Y + "            " + first.Joints[JointType.WristRight].Position.Z);
                //Console.WriteLine("右手  ：" + first.Joints[JointType.HandRight].Position.X + "            " + first.Joints[JointType.HandRight].Position.Y + "            " + first.Joints[JointType.HandRight].Position.Z);
                Console.WriteLine("\n");
                times = 0;
            
            }

        }
  */

        void get_R(Skeleton first,double[] R)
{ 
	double roll = rrget_roll(first.Joints[JointType.WristRight].Position.X, first.Joints[JointType.WristRight].Position.Y, first.Joints[JointType.WristRight].Position.Z,
                      		 first.Joints[JointType.ElbowRight].Position.X, first.Joints[JointType.ElbowRight].Position.Y, first.Joints[JointType.ElbowRight].Position.Z,
                      		 first.Joints[JointType.ShoulderRight].Position.X, first.Joints[JointType.ShoulderRight].Position.Y, first.Joints[JointType.ShoulderRight].Position.Z);
        roll = 3.14-roll ;
        double yaw = rrYaw(first.Joints[JointType.WristRight].Position.X, first.Joints[JointType.WristRight].Position.Y, first.Joints[JointType.WristRight].Position.Z,
                     	   first.Joints[JointType.ElbowRight].Position.X, first.Joints[JointType.ElbowRight].Position.Y, first.Joints[JointType.ElbowRight].Position.Z,
                     	   first.Joints[JointType.ShoulderRight].Position.X, first.Joints[JointType.ShoulderRight].Position.Y, first.Joints[JointType.ShoulderRight].Position.Z,
                     	   first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z);
        yaw =yaw - 3.14+0.75;

        double roll2 = rrget_roll(first.Joints[JointType.ElbowRight].Position.X, first.Joints[JointType.ElbowRight].Position.Y, first.Joints[JointType.ElbowRight].Position.Z,
                       		  first.Joints[JointType.ShoulderRight].Position.X, first.Joints[JointType.ShoulderRight].Position.Y, first.Joints[JointType.ShoulderRight].Position.Z,
                       		  first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z);
        roll2 = 1.57 - roll2+0.5;
        double pitch2 = rrpitch(first.Joints[JointType.ElbowRight].Position.X, first.Joints[JointType.ElbowRight].Position.Y, first.Joints[JointType.ElbowRight].Position.Z,
                        	first.Joints[JointType.ShoulderRight].Position.X, first.Joints[JointType.ShoulderRight].Position.Y, first.Joints[JointType.ShoulderRight].Position.Z);

        pitch2 = 1.8 - pitch2;
	
	R[0]=roll;
	R[1]=yaw;
	R[2]=roll2;
	R[3]=pitch2;
}


        void get_L(Skeleton first,double [] L)
{ 
	double roll = rrget_roll(first.Joints[JointType.WristLeft].Position.X, first.Joints[JointType.WristLeft].Position.Y, first.Joints[JointType.WristLeft].Position.Z,
                      		 first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                      		 first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z);
        roll =-( 3.14-roll );
        double yaw = rrYaw(first.Joints[JointType.WristLeft].Position.X, first.Joints[JointType.WristLeft].Position.Y, first.Joints[JointType.WristLeft].Position.Z,
                     	   first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                     	   first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z,
                     	   first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z);
        yaw =( yaw - 3.14)-0.75;

        double roll2 = rrget_roll(first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                       		  first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z,
                       		  first.Joints[JointType.ShoulderCenter].Position.X, first.Joints[JointType.ShoulderCenter].Position.Y, first.Joints[JointType.ShoulderCenter].Position.Z);
        roll2 =-( 1.57 - roll2+0.5);
        double pitch2 = rrpitch(first.Joints[JointType.ElbowLeft].Position.X, first.Joints[JointType.ElbowLeft].Position.Y, first.Joints[JointType.ElbowLeft].Position.Z,
                        	first.Joints[JointType.ShoulderLeft].Position.X, first.Joints[JointType.ShoulderLeft].Position.Y, first.Joints[JointType.ShoulderLeft].Position.Z);

        pitch2 = 1.8 - pitch2;
	
	L[0]=roll;
	L[1]=yaw;
	L[2]=roll2;
	L[3]=pitch2;
}


int MotionType(double[] L,double[] R)//Motion Type for HeadYaw
{
	if (L[0]<0.3&&L[2]<0.5&&L[3]>0&&L[3]<1.57&&R[0]<0.3&&R[2]<0.3&&R[3]>1.4&&R[3]<1.6)//左手135
		return 0;
	if (L[0]<0.3&&L[2]<0.3&&L[3]<-1.3&&L[3]>-1.8&&R[0]<0.3&&R[2]>-0.5&&R[3]>1.3&&R[3]<1.8)//立正
		return 0;
	if (L[0]<0.3&&L[3]<0.6&&L[2]>0.5&&L[2]<1.3&&R[0]<0.4&&R[2]>-0.5&&R[3]>1.3&&R[3]<1.8)//左手平举，右手垂下 error:R[3]=1.7
		return 1;
	if (L[0]<0.3&&L[3]<0.6&&L[2]>0.5&&L[2]<1.3&&R[0]<0.3&&R[3]<0.5&&R[2]<-0.5&&R[2]>-1.3)//平举，ok
		return -1;
	if (L[0]<0.3&&L[3]<0.6&&L[2]>0.5&&L[2]<1.3&&R[0]<0.6&&R[2]>-0.5&&R[3]<0.7&&R[3]>-0.3)//左手平举，右手直举
		return 1;
	if (L[0]<0.3&&L[3]<0.8&&L[2]>0.5&&L[2]<1.3&&R[0]>1.3&&R[0]<2.3&&R[1]>-0.4&&R[1]<0.4&&R[2]<0.4&&R[3]<0.7&&R[3]>-0.3)//左手平举，右手折举
		return 1;
	return -2;
}

        void outway(double[] La,double[] Ra,double[]Ll,double[]Rl,double[]feet,double T)
{
	

        EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

        string msg  = La[0].ToString();
        string msg2 = La[1].ToString();
        string msg3 = La[2].ToString();
        string msg4 = La[3].ToString();
        string msg5 = Ra[0].ToString();
        string msg6 = Ra[1].ToString();
        string msg7 = Ra[2].ToString();
        string msg8 = Ra[3].ToString();
        string msg9 = Ll[0].ToString();
        string msg10= Ll[1].ToString();
        string msg11= Ll[2].ToString();
        string msg12= Rl[0].ToString();
        string msg13= Rl[1].ToString();
        string msg14= Rl[2].ToString();
        string msg15 = feet[0].ToString();
        string msg16 = feet[1].ToString();
        string msg17 = T.ToString();
        Console.WriteLine("左臂:" +"Elbowroll : "+ msg   +"   ElbowYaw: "+msg2+    "   ShoulderRoll: "+msg3+"   ShoulderPitch: "+msg4);
        Console.WriteLine("右臂:" + "Elbowroll: " + msg5 +"   ElbowYaw: " + msg6 + "   ShoulderRoll: " + msg7 + "   ShoulderPitch: " + msg8);
        Console.WriteLine("左腿:" + "Hiproll  : " + msg9  +"   HipPitch: " + msg10+"   KneePitch   : " + msg11);
        Console.WriteLine("右腿:" + "Hiproll  : " + msg12 + "   HipPitch: "+msg13 +"   KneePitch   : " + msg14);
        Console.WriteLine("脚踝: " + "RightAnkle: " + msg15 + "   LeftAnkle: " + msg16);
        Console.WriteLine("头：  " + msg17);
        string message = msg + "," + msg2 + "," + msg3 + "," + msg4 + "," + msg5 + "," + msg6 + "," + msg7 + "," + msg8 + "," + msg9 + "," + msg10 + "," + msg11 + "," +
            msg12 + "," + msg13+","+msg14+","+msg15+"," +msg16+","+msg17;
        server.SendTo(Encoding.UTF8.GetBytes(message), point);
        Console.WriteLine("完成：");
}

        void get_RH(Skeleton first,double [] R)
{ 
	double roll = rrget_roll(first.Joints[JointType.KneeRight].Position.X, first.Joints[JointType.KneeRight].Position.Y, first.Joints[JointType.KneeRight].Position.Z,
                      		 first.Joints[JointType.HipRight].Position.X, first.Joints[JointType.HipRight].Position.Y, first.Joints[JointType.HipRight].Position.Z,
                      		 first.Joints[JointType.HipCenter].Position.X, first.Joints[JointType.HipCenter].Position.Y, first.Joints[JointType.HipCenter].Position.Z);
    roll = -(roll-3.14+0.8) ;
 
        double pitch = rrpitch(first.Joints[JointType.HipRight].Position.X, first.Joints[JointType.HipRight].Position.Y, first.Joints[JointType.HipRight].Position.Z,
                        	first.Joints[JointType.HipCenter].Position.X, first.Joints[JointType.HipCenter].Position.Y, first.Joints[JointType.HipCenter].Position.Z);

    pitch= 1.1 - pitch;
	
	double pitch2=rrget_roll(first.Joints[JointType.AnkleRight].Position.X, first.Joints[JointType.AnkleRight].Position.Y, first.Joints[JointType.AnkleRight].Position.Z,
                      		 first.Joints[JointType.KneeRight].Position.X, first.Joints[JointType.KneeRight].Position.Y, first.Joints[JointType.KneeRight].Position.Z,
                      		 first.Joints[JointType.HipRight].Position.X, first.Joints[JointType.HipRight].Position.Y, first.Joints[JointType.HipRight].Position.Z);
    pitch2 = 3.14 - pitch2+0.4;
	
	R[0]=roll;
	R[1]=pitch;
	R[2]=pitch2;
}

        void get_lL(Skeleton first,double [] R)
{ 
	double roll = rrget_roll(first.Joints[JointType.KneeLeft].Position.X, first.Joints[JointType.KneeLeft].Position.Y, first.Joints[JointType.KneeLeft].Position.Z,
                      		 first.Joints[JointType.HipLeft].Position.X, first.Joints[JointType.HipLeft].Position.Y, first.Joints[JointType.HipLeft].Position.Z,
                      		 first.Joints[JointType.HipCenter].Position.X, first.Joints[JointType.HipCenter].Position.Y, first.Joints[JointType.HipCenter].Position.Z);
        roll =roll-3.14+0.8 ;
 
        double pitch = rrpitch(first.Joints[JointType.HipLeft].Position.X, first.Joints[JointType.HipLeft].Position.Y, first.Joints[JointType.HipLeft].Position.Z,
                        	first.Joints[JointType.HipCenter].Position.X, first.Joints[JointType.HipCenter].Position.Y, first.Joints[JointType.HipCenter].Position.Z);

        pitch= 1.1 - pitch;
	
	double pitch2=rrget_roll(first.Joints[JointType.AnkleLeft].Position.X, first.Joints[JointType.AnkleLeft].Position.Y, first.Joints[JointType.AnkleLeft].Position.Z,
                      		 first.Joints[JointType.KneeLeft].Position.X, first.Joints[JointType.KneeLeft].Position.Y, first.Joints[JointType.KneeLeft].Position.Z,
                      		 first.Joints[JointType.HipLeft].Position.X, first.Joints[JointType.HipLeft].Position.Y, first.Joints[JointType.HipLeft].Position.Z);
    pitch2 = 3.14 - pitch2+0.4;
	R[0]=roll;
	R[1]=pitch;
	R[2]=pitch2;
}

        void get_Ankle(Skeleton first,double [] R)
        {
            double RightAnkle = rrget_roll(first.Joints[JointType.KneeRight].Position.X, first.Joints[JointType.KneeRight].Position.Y, first.Joints[JointType.KneeRight].Position.Z,
                             first.Joints[JointType.AnkleRight].Position.X, first.Joints[JointType.AnkleRight].Position.Y, first.Joints[JointType.AnkleRight].Position.Z,
                             first.Joints[JointType.FootRight].Position.X, first.Joints[JointType.FootRight].Position.Y, first.Joints[JointType.FootRight].Position.Z);
            double LeftAnkle = rrget_roll(first.Joints[JointType.KneeLeft].Position.X, first.Joints[JointType.KneeLeft].Position.Y, first.Joints[JointType.KneeLeft].Position.Z,
                             first.Joints[JointType.AnkleLeft].Position.X, first.Joints[JointType.AnkleLeft].Position.Y, first.Joints[JointType.AnkleRight].Position.Z,
                             first.Joints[JointType.FootLeft].Position.X, first.Joints[JointType.FootLeft].Position.Y, first.Joints[JointType.FootLeft].Position.Z);
            R[0] = RightAnkle - Math.PI / 2-0.3;
            R[1] = LeftAnkle - Math.PI / 2-0.3;

        }


        void get_sketetonpiont(Skeleton first)
{

 	times = times + 1;
    double HeadYaw = 0;
     // lefthand.Text = "lefthand = " + first.Joints[JointType.HandLeft].Position.X + "              " + first.Joints[JointType.HandLeft].Position.Y + "              " + first.Joints[JointType.HandLeft].Position.Z;
     // righthand.Text = "righthand = " + first.Joints[JointType.HandRight].Position.X + "              " + first.Joints[JointType.HandRight].Position.Y + "              " + first.Joints[JointType.HandRight].Position.Z;
    //  head.Text = "head = " + first.Joints[JointType.Head].Position.X + "              " + first.Joints[JointType.Head].Position.Y + "              " + first.Joints[JointType.Head].Position.Z;
    //  shouldercenter.Text = "shouldercenter = " + first.Joints[JointType.ShoulderCenter].Position.X + "              " + first.Joints[JointType.ShoulderCenter].Position.Y + "              " + first.Joints[JointType.ShoulderCenter].Position.Z;
	if (times == 10)
 	{

        double[] L = new double[4] 
          { 0,      //ElbowRoll
            0,      //ElbowYaw
            0,      //ShoulderRoll
            0 };    //ShoulderPitch
        double[] R = new double[4] { 0, 0, 0, 0 };
		double[] L1= new double[3] {0,0,0};
        double[] R1= new double[3] {0,0,0};
        double[] F = new double[2] { 0, 0 };
		get_L(first,L);
		get_R(first,R);
        get_lL(first,L1 );
        get_RH(first, R1);
        get_Ankle(first, F);
        
        int t=MotionType(L,R);
        if (t!=-2)
	        HeadYaw=0.7*t;      


        outway(L,R,L1,R1,F,HeadYaw);
        Console.WriteLine(t.ToString()+"\n");
        times = 0;
            
	}
}



  
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }
            //*********************************************************************
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    int i = 0;//******************************************
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);
                        get_sketetonpiont(skel);//*****************************
                        
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                        i = i + 1;
                        if (i == 1)
                        {
                            i = 0;
                            break; }
                            

                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
 
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }
    }
}