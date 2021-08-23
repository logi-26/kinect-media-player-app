
namespace MediaApplication {

    using System;
    using System.IO;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Collections.Generic;
    using System.Windows.Shapes;
    using System.Linq;
    using System.Windows.Threading;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Microsoft.Win32;
    using System.Diagnostics;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    

    // Interaction logic for MainWindow
    public partial class MainWindow {

        // Strings for the directory paths (pre-defined paths to save having to add code for dynamic directory paths)
        private String mainDirectory = Environment.CurrentDirectory;
        private String videoDirectory, audioDirectory, imageDirectory;
        private String videoDocumentaryDirectory, videoMovieDirectory, videoTVDirectory;
        private String videoMActionDirectory, videoMComedyDirectory, videoMHorrorDirectory, videoMRomanceDirectory, videoMSciFiDirectory;
        private String audioAudioBookDirectory, audioMusicDirectory, audioPodCastDirectory;
        private String audioMClassicalDirectory, audioMCountryDirectory, audioMDanceDirectory, audioMHipHopDirectory, audioMJazzDirectory, audioMPopDirectory, audioMRockDirectory;
        private String currentDirectory;

        // Enum to store the current media type being used
        private enum MediaType {
            Video,
            Audio
        };

        // Enum to store the current media state
        private enum MediaState {
            Unloaded,
            Stopped,
            Paused,
            Playing
        };

        private MediaType selectedMediaType;
        private MediaState currentMediaState;
        private Boolean userIsDraggingSlider = false;

        // For scrolling left
        public static readonly DependencyProperty PageLeftEnabledProperty = DependencyProperty.Register(
            "PageLeftEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        // For scrolling right
        public static readonly DependencyProperty PageRightEnabledProperty = DependencyProperty.Register(
            "PageRightEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        // Kinect and skelton variables
        private const double ScrollErrorMargin = 0.001;
        private readonly KinectSensorChooser sensorChooser;
        private Boolean SHOW_VIDEO = false;
        private Boolean DRAW_SKELETON = false; 
        private const int SKELETON_COUNT = 6;
        private Skeleton[] allSkeletons = new Skeleton[SKELETON_COUNT];
        private static int SKELETONS_TRACKED = 1;
        private Person[] People = new Person[SKELETONS_TRACKED];

        // Variables for the arm locations
        private int RightLowerArm = 0;
        private int LeftLowerArm = 0;

        // Booleans to determine if a command gesture has been perfomred
        private Boolean volumeCommandGesture = false;
        private Boolean skipCommandGesture = false;

        // Playlist
        private Boolean playListActive = false;
        List<String> playListArray = new List<String>();
        List<String> playListDisplayArray = new List<String>();

        // Stop watches used at the end of a gesture to prevent repeated calls in quick sucsession
        Stopwatch mediaControlStopWatch = new Stopwatch();
        Stopwatch mediaScrollSkipStopWatch = new Stopwatch();
        Stopwatch mediaScrollVolumeStopWatch = new Stopwatch();

        Boolean debugMode = false;



        // Initialises the main window 
        public MainWindow() {

            // Set the initial media type as video
            selectedMediaType = MediaType.Video;

            // Set the initial media state as "Unloaded"
            currentMediaState = MediaState.Unloaded;

            // Set the directory paths for the files
            setDirectoryPaths();

            this.InitializeComponent();

            // Initialise the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
            if (this.sensorChooser != null) this.sensorChooser.Kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            // Updates the tile buttons in each of the wrap panels
            updateMainPanel();
            updateBrowserPanel(videoDirectory);
            updateControlPanel();
            updateDebugDisplay();
            
            // Add event handlers for when the user scrolls on the 3 wrap panles
            scrollViewerBrowserPanel.ScrollChanged += (o, e) => this.BrowserPanelScrolled();
            scrollViewerMediaSkip.ScrollChanged += (o, e) => this.MediaWindowSkipScrolled();
            scrollViewerMediaVolume.ScrollChanged += (o, e) => this.MediaWindowVolumeScrolled();

            // Sets the GUI controls invisible when the application is launched and no media is loaded
            mediaVolumeBar.Visibility = Visibility.Hidden;
            lblMediaFileName.Visibility = Visibility.Hidden;
            mediaSliderProgress.Visibility = Visibility.Hidden;
            listBoxPlayList.Visibility = Visibility.Hidden;
            PlayListBackgroundImage.Visibility = Visibility.Hidden;

            // set the text in the GUI
            lblCommandDisplay.Content = "Not performing a command!";
            lblCommandDisplay.Foreground = System.Windows.Media.Brushes.Red;

            // This creates a dispatch timer which is used to keep track of the playing media file position
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += mediaTimer;
            timer.Start();
        }




        // Timer for monitoring the media file position (called every second)
        void mediaTimer(object sender, EventArgs e) {

            // If there is currently a media file loaded
            if (VideoControl.Source != null) {

                // Gets the media files current position and overal length formatted as Hour/Minute,Minute/Second,Second
                if (VideoControl.NaturalDuration.HasTimeSpan) {

                    // If the media file has not reached the end
                    if (VideoControl.Position < VideoControl.NaturalDuration.TimeSpan) {

                        lblMediaDuration.Content = String.Format("{0} / {1}", VideoControl.Position.ToString(@"h\:mm\:ss"), VideoControl.NaturalDuration.TimeSpan.ToString(@"h\:mm\:ss"));

                        // Update the media slider progress bar
                        if (!userIsDraggingSlider) {
                            mediaSliderProgress.Minimum = 0;
                            mediaSliderProgress.Maximum = VideoControl.NaturalDuration.TimeSpan.TotalSeconds;
                            mediaSliderProgress.Value = VideoControl.Position.TotalSeconds;
                        }
                    } else {

                        // When the media file reaches the end
                        lblMediaDuration.Content = "File ended..";

                        // If a file is playing and the playlist contains more than 1 file, this skips to the next file in the playlist
                        if (playListActive && playListArray.Count > 1) playlistNextFile();
                    }
                }
            }
            else lblMediaDuration.Content = "No file selected..";
        }





        // This function sets the strings with the directory paths
        private void setDirectoryPaths() {

            // These are all of the directory paths for the different categories
            videoDirectory = mainDirectory + "\\Video\\";
            audioDirectory = mainDirectory + "\\Audio\\";
            imageDirectory = mainDirectory + "\\Images\\";

            videoDocumentaryDirectory = videoDirectory + "Documentary\\";
            videoMovieDirectory = videoDirectory + "Movie\\";
            videoTVDirectory = videoDirectory + "TV\\";

            videoMActionDirectory = videoMovieDirectory + "Action\\";
            videoMComedyDirectory = videoMovieDirectory + "Comedy\\";
            videoMHorrorDirectory = videoMovieDirectory + "Horror\\";
            videoMRomanceDirectory = videoMovieDirectory + "Romance\\";
            videoMSciFiDirectory = videoMovieDirectory + "SciFi\\";

            audioAudioBookDirectory = audioDirectory + "AudioBook\\";
            audioMusicDirectory = audioDirectory + "Music\\";
            audioPodCastDirectory = audioDirectory + "PodCast\\";

            audioMClassicalDirectory = audioMusicDirectory + "Classical\\";
            audioMCountryDirectory = audioMusicDirectory + "Country\\";
            audioMDanceDirectory = audioMusicDirectory + "Dance\\";
            audioMHipHopDirectory = audioMusicDirectory + "Hip Hop\\";
            audioMJazzDirectory = audioMusicDirectory + "Jazz\\";
            audioMPopDirectory = audioMusicDirectory + "Pop\\";
            audioMRockDirectory = audioMusicDirectory + "Rock\\";

            // Set the current directory
            currentDirectory = videoDirectory;
        }





        // This function creates a tile button from an image file
        private KinectTileButton createTileButton(String imagePath, String buttonTag, int tileHeight, int tileWidth) {

            // Load image from file
            BitmapImage buttonImage = new BitmapImage();
            buttonImage.BeginInit();
            buttonImage.UriSource = new Uri(imagePath, UriKind.Relative);
            buttonImage.EndInit();

            // Create the tile button
            var tileButton = new KinectTileButton { Background = new ImageBrush(buttonImage), Height = tileHeight, Width = tileWidth, Tag = buttonTag };

            // Return the tile button
            return tileButton;
        }




        // This function adds the tile button to the debug panel
        private void updateDebugDisplay() {

            // Debug button
            int tileSize = 75;
            this.wrapPanelDebugPanel.Children.Clear();

            // Switch the debug tile button image
            if (debugMode) {
                KinectTileButton button_debug = createTileButton("Images/Button_Debug_2.jpg", "button_debug", tileSize, tileSize); ;
                this.wrapPanelDebugPanel.Children.Add(button_debug);
            } else {
                KinectTileButton button_debug = createTileButton("Images/Button_Debug_1.jpg", "button_debug", tileSize, tileSize); ;
                this.wrapPanelDebugPanel.Children.Add(button_debug);
            }
        }




        // This function adds the tile buttons to the main panel
        private void updateMainPanel() {

            int tileSize = 140;

            // Clear out placeholder content from the main panel
            this.wrapPanelMainPanel.Children.Clear();
            
            // Video button
            KinectTileButton button_video;
            if (selectedMediaType != MediaType.Video) button_video = createTileButton("Images/Button_Video_1.jpg", "button_video", tileSize, tileSize);
            else button_video = createTileButton("Images/Button_Video_2.jpg", "button_video", tileSize, tileSize);
            this.wrapPanelMainPanel.Children.Add(button_video);

            // Add file button (switch the button image depending on the folder type, catagory or content)
            if (!isCategoryFolder()) {
                KinectTileButton button_add = createTileButton("Images/Button_AddFile_1.jpg", "button_add", tileSize, tileSize); ;
                this.wrapPanelMainPanel.Children.Add(button_add);
            } else {
                KinectTileButton button_add = createTileButton("Images/Button_AddFile_2.jpg", "button_add", tileSize, tileSize); ;
                this.wrapPanelMainPanel.Children.Add(button_add);
            }

            // Audio button
            KinectTileButton button_audio;
            if (selectedMediaType != MediaType.Audio) button_audio = createTileButton("Images/Button_Audio_1.jpg", "button_audio", tileSize, tileSize);
            else button_audio = createTileButton("Images/Button_Audio_2.jpg", "button_audio", tileSize, tileSize);
            this.wrapPanelMainPanel.Children.Add(button_audio);

            // Play list button (switch the button image depending on the folder type, catagory or content)
            if (!isCategoryFolder()) {

                // If the play list feature is active
                if (playListActive) {
                    KinectTileButton button_playList = createTileButton("Images/Button_PlayList_3.jpg", "button_playlist", tileSize, tileSize); ;
                    this.wrapPanelMainPanel.Children.Add(button_playList);
                } else {
                    KinectTileButton button_playList = createTileButton("Images/Button_PlayList_1.jpg", "button_playlist", tileSize, tileSize); ;
                    this.wrapPanelMainPanel.Children.Add(button_playList);
                }

            } else {
                KinectTileButton button_playList = createTileButton("Images/Button_PlayList_2.jpg", "button_playlist", tileSize, tileSize); ;
                this.wrapPanelMainPanel.Children.Add(button_playList);
            }
        }




        // This function adds the tile buttons to the browser panel
        private void updateBrowserPanel(String chosenDirectory) {

            int tileSize = 160;

            // If the selected folder is a content folder, loop through each item in the folder and add the tile buttons dynamically
            if (!isCategoryFolder()) {

                // Clear out placeholder content from the browser panel
                this.wrapPanelBrowserPanel.Children.Clear();

                // Store all jpg file names from the selected folder to an array
                string[] filePaths = Directory.GetFiles(chosenDirectory, "*.jpg");

                // Loop through the array
                foreach (string s in filePaths) {

                    // This gets the file name with the extension from the full directory path
                    int stringSplitter1 = s.LastIndexOf('\\');
                    String imageFullName = s.Substring(stringSplitter1 + 1);

                    // This removes the file extension so we are left with just the file name
                    int stringSplitter2 = imageFullName.LastIndexOf('.');
                    String imageName = imageFullName.Substring(0, stringSplitter2);

                    String path = chosenDirectory + imageFullName;

                    // This creates the button image from the image in the selected folder
                    BitmapImage buttonImage1 = new BitmapImage();
                    buttonImage1.BeginInit();
                    buttonImage1.UriSource = new Uri(path, UriKind.Relative);
                    buttonImage1.EndInit();

                    // Creates the tile button and adds it to the browser wrap panel
                    var button = new KinectTileButton { Background = new ImageBrush(buttonImage1), Label = (imageName), Tag = imageName, Height = tileSize, Width = tileSize, FontSize = 14 };
                    this.wrapPanelBrowserPanel.Children.Add(button);
                }
            } 
            else {

                // If the selected folder is a catagory folder, tile buttons are added manually
                this.wrapPanelBrowserPanel.Children.Clear();

                if (chosenDirectory == videoDirectory) {
                    // Video catagory tile buttons
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Category_Documentary.jpg", "button_documentary", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Category_Movie.jpg", "button_movie", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Category_TV.jpg", "button_tv", tileSize, tileSize));
                }
                else if (chosenDirectory == audioDirectory) {
                    // Audio catagory tile buttons
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Category_AudioBook.jpg", "button_audioBook", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Category_Music.jpg", "button_music", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Category_PodCast.jpg", "button_podCast", tileSize, tileSize));
                }
                else if (chosenDirectory == videoMovieDirectory) {
                    // Movie catagory tile buttons
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Movie_Action.jpg", "button_movie_action", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Movie_Comedy.jpg", "button_movie_comedy", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Movie_Horror.jpg", "button_movie_horror", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Movie_Romance.jpg", "button_movie_romance", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Movie_SciFi.jpg", "button_movie_scifi", tileSize, tileSize));
                }
                else if (chosenDirectory == audioMusicDirectory) {
                    // Music catagory tile buttons
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_Classical.jpg", "button_music_classical", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_Country.jpg", "button_music_country", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_Dance.jpg", "button_music_dance", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_HipHop.jpg", "button_music_hiphop", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_Jazz.jpg", "button_music_jazz", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_Pop.jpg", "button_music_pop", tileSize, tileSize));
                    this.wrapPanelBrowserPanel.Children.Add(createTileButton("Images/Button_Music_Rock.jpg", "button_music_rock", tileSize, tileSize));
                }
            }
        }



        // This function adds the tile buttons to the control wrap panel
        private void updateControlPanel() {

            int tileHeight = 100;
            int tileWidth = 160;
            
            // Clear out placeholder content from the control wrap panel
            this.wrapPanelControlPanel.Children.Clear();

            // Creates the tile buttons and adds them to the wrap panel
            this.wrapPanelControlPanel.Children.Add(createTileButton("Images/Button_Control_Backward.jpg", "button_control_backward", tileHeight, tileWidth));
            
            if (currentMediaState == MediaState.Playing) this.wrapPanelControlPanel.Children.Add(createTileButton("Images/Button_Control_Pause.jpg", "button_control_pause", tileHeight, tileWidth));
            else this.wrapPanelControlPanel.Children.Add(createTileButton("Images/Button_Control_Play.jpg", "button_control_play", tileHeight, tileWidth));
            
            this.wrapPanelControlPanel.Children.Add(createTileButton("Images/Button_Control_Stop.jpg", "button_control_stop", tileHeight, tileWidth));
            this.wrapPanelControlPanel.Children.Add(createTileButton("Images/Button_Control_Forward.jpg", "button_control_forward", tileHeight, tileWidth));
        }




        // This function is used to check the user's arm positions
        private void checkArmPositions(Person person) {

            // Lower arm right ***********************************************************************************
            double math, rightElbow, rightHand, rightShoulder;
     
            // Right elbow, right hand, right shoulder
            rightElbow = Math.Sqrt(Math.Pow((person.jElbowRight.X - person.jHandRight.X), 2) + Math.Pow((person.jElbowRight.Y - person.jHandRight.Y), 2));                                 
            rightHand = Math.Sqrt(Math.Pow((person.jElbowRight.X - person.jShoulderRight.X), 2) + Math.Pow((person.jElbowRight.Y - person.jShoulderRight.Y), 2));                             
            rightShoulder = Math.Sqrt(Math.Pow((person.jHandRight.X - person.jShoulderRight.X), 2) + Math.Pow((person.jHandRight.Y - person.jShoulderRight.Y), 2));                            

            math = Math.Acos((Math.Pow(rightElbow, 2) + Math.Pow(rightHand, 2) - Math.Pow(rightShoulder, 2)) / (2 * rightElbow * rightHand));

            if (math <= 1.4 && RightLowerArm != 2) RightLowerArm = 2;                               // Right arm bent fully
            if (math < 2.6 && math > 1.4 && person.RightForeArm.X1 != 1) RightLowerArm = 1;         // Right arm bent half
            if (math >= 2.6 && person.RightForeArm.X1 != 0) RightLowerArm = 0;                      // Right arm straightened

            // Lower arm left ***********************************************************************************
            double leftElbow, leftHand, leftShoulder;

            // Left elbow, left hand, left shoulder
            leftElbow = Math.Sqrt(Math.Pow((person.jElbowLeft.X - person.jHandLeft.X), 2) +Math.Pow((person.jElbowLeft.Y - person.jHandLeft.Y), 2));                                   
            leftHand = Math.Sqrt(Math.Pow((person.jElbowLeft.X - person.jShoulderLeft.X), 2) + Math.Pow((person.jElbowLeft.Y - person.jShoulderLeft.Y), 2));                               
            leftShoulder = Math.Sqrt(Math.Pow((person.jHandLeft.X - person.jShoulderLeft.X), 2) +Math.Pow((person.jHandLeft.Y - person.jShoulderLeft.Y), 2));                              

            math = Math.Acos((Math.Pow(leftElbow, 2) + Math.Pow(leftHand, 2) - Math.Pow(leftShoulder, 2)) / (2 * leftElbow * leftHand));

            if (math <= 1.4 && person.LeftForeArm.X1 != 2) LeftLowerArm = 2;                        // Left arm bent fully
            if (math < 2.6 && math > 1.4 && person.LeftForeArm.X1 != 1) LeftLowerArm = 1;           // Left arm bent half
            if (math >= 2.6 && person.LeftForeArm.X1 != 0) LeftLowerArm = 0;                        // Left arm straightened

            //***************************************************************************************************

            // Left hand above left shoulder
            if (person.jHandLeft.Y + 70 < person.jShoulderLeft.Y) volumeCommandGesture = true;
            else volumeCommandGesture = false;
            
            // Left hand roughly equal to left shoulder
            if (person.jHandLeft.Y >= person.jShoulderLeft.Y - 40 && person.jHandLeft.Y <= person.jShoulderLeft.Y + 20) skipCommandGesture = true;
            else skipCommandGesture = false;

            // Check for a gesture
            checkGesture(person);
        }





        // This function is used to check for specific gestures
        private void checkGesture(Person person) {

            Boolean gesturePerformed = false;

            // Check if stopwatch is currently running
            if (mediaControlStopWatch.IsRunning) {

                // Once 1 second has passed, stop the stopwatch
                if (mediaControlStopWatch.ElapsedMilliseconds > 1000) {
                    mediaControlStopWatch.Stop();
                    mediaControlStopWatch.Reset();
                }
            }

            // If the stopwatch is not currently running
            if (!mediaControlStopWatch.IsRunning) {

                // If performing a volume command
                if (performingVolumeCommand()) {

                    // If right arm staight and above head (Increase volume)
                    if (RightLowerArm == 0 && person.jHandRight.Y + 50 < person.jHead.Y) {
                        mediaPlayerVolumeUp();
                        gesturePerformed = true;
                    }

                    // If right arm staight and below hip (Decrease volume)
                    if (RightLowerArm == 0 && person.jHandRight.Y > person.jHipRight.Y) {
                        mediaPlayerVolumeDown();
                        gesturePerformed = true;
                    }
                }
                // If performing a skip command
                else if (performingSkipCommand()) {

                    // If right arm staight and above head (Skip media forward)
                    if (RightLowerArm == 0 && person.jHandRight.Y + 50 < person.jHead.Y) {
                        mediaPlayerForward();
                        gesturePerformed = true;
                    }

                    // If right arm staight and below hip (Skip media backward)
                    if (RightLowerArm == 0 && person.jHandRight.Y > person.jHipRight.Y) {
                        mediaPlayerRewind();
                        gesturePerformed = true;
                    }

                    // If a push gesture is performed pause/play the media file
                    if (pushGesturePerformed(person)) {

                        if (currentMediaState == MediaState.Playing) mediaPlayerPause();
                        else if (currentMediaState == MediaState.Paused) mediaPlayerPlay();

                        gesturePerformed = true;

                        // Update the control wrap panel
                        updateControlPanel();
                    }
                }
            }

            // If a gesture is performed, start the stop watch to prevent gestures from repeatly triggering
            if (gesturePerformed) mediaControlStopWatch.Start();
        }






        // This function gets the skeleton data from the Kinect sensor
        private void GetSkeleton(AllFramesReadyEventArgs e, ref Skeleton me, int person) {

            // Retrieves skelital data from kinect
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame()) {

                if (skeletonFrameData == null) return;

                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                // Query to retrive list from skeleton frame data
                List<Skeleton> tmpSkel = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).Distinct().ToList();
                if (tmpSkel.Count < person + 1) return;
             
                me = tmpSkel[person];
            }
        }






        // This function gets the co-ordinates and the depth value for each of the joints
        private void GetXYD(Skeleton theSkeleton, Person person) {

            GetJoint(theSkeleton, person, JointType.Head);              // Head
            GetJoint(theSkeleton, person, JointType.AnkleRight);        // Ankle right
            GetJoint(theSkeleton, person, JointType.AnkleLeft);         // Ankle left
            GetJoint(theSkeleton, person, JointType.WristRight);        // Wrist right
            GetJoint(theSkeleton, person, JointType.WristLeft);         // Wrist left
            GetJoint(theSkeleton, person, JointType.ShoulderRight);     // Shoulder right
            GetJoint(theSkeleton, person, JointType.ShoulderLeft);      // Shoulder left
            GetJoint(theSkeleton, person, JointType.ShoulderCenter);    // Shoulder center
            GetJoint(theSkeleton, person, JointType.KneeLeft);          // Knee left
            GetJoint(theSkeleton, person, JointType.KneeRight);         // Knee right
            GetJoint(theSkeleton, person, JointType.HipCenter);         // Hip center
            GetJoint(theSkeleton, person, JointType.HipLeft);           // Hip left
            GetJoint(theSkeleton, person, JointType.HipRight);          // Hip right
            GetJoint(theSkeleton, person, JointType.Spine);             // Spine
            GetJoint(theSkeleton, person, JointType.ElbowLeft);         // Elbow left
            GetJoint(theSkeleton, person, JointType.ElbowRight);        // Elbow right
            GetJoint(theSkeleton, person, JointType.FootLeft);          // Foot left
            GetJoint(theSkeleton, person, JointType.FootRight);         // Foot right
            GetJoint(theSkeleton, person, JointType.HandLeft);          // Hand left
            GetJoint(theSkeleton, person, JointType.HandRight);         // Hand Right
        }





        // This function gets the co-ordinates from the person
        private void GetJoint(Skeleton theSkeleton, Person person, JointType joint) {

            DepthImagePoint depthPoint = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(theSkeleton.Joints[joint].Position, DepthImageFormat.Resolution640x480Fps30);
            ColorImagePoint colourPoint = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(theSkeleton.Joints[joint].Position, ColorImageFormat.RgbResolution640x480Fps30);

            if (joint == JointType.Head) {
                person.jHead.X = colourPoint.X;
                person.jHead.Y = colourPoint.Y;
                person.jHead.D = depthPoint.Depth;
            }
            else if (joint == JointType.AnkleRight) {
                person.jAnkleRight.X = colourPoint.X;
                person.jAnkleRight.Y = colourPoint.Y;
                person.jAnkleRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.AnkleLeft) {
                person.jAnkleLeft.X = colourPoint.X;
                person.jAnkleLeft.Y = colourPoint.Y;
                person.jAnkleLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.WristRight) {
                person.jWristRight.X = colourPoint.X;
                person.jWristRight.Y = colourPoint.Y;
                person.jWristRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.WristLeft) {
                person.jWristLeft.X = colourPoint.X;
                person.jWristLeft.Y = colourPoint.Y;
                person.jWristLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.ShoulderRight) {
                person.jShoulderRight.X = colourPoint.X;
                person.jShoulderRight.Y = colourPoint.Y;
                person.jShoulderRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.ShoulderLeft) {
                person.jShoulderLeft.X = colourPoint.X;
                person.jShoulderLeft.Y = colourPoint.Y;
                person.jShoulderLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.ShoulderCenter) {
                person.jShoulderCenter.X = colourPoint.X;
                person.jShoulderCenter.Y = colourPoint.Y;
                person.jShoulderCenter.D = depthPoint.Depth;
            }
            else if (joint == JointType.KneeLeft) {
                person.jKneeLeft.X = colourPoint.X;
                person.jKneeLeft.Y = colourPoint.Y;
                person.jKneeLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.KneeRight) {
                person.jKneeRight.X = colourPoint.X;
                person.jKneeRight.Y = colourPoint.Y;
                person.jKneeRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.HipCenter) {
                person.jHipCenter.X = colourPoint.X;
                person.jHipCenter.Y = colourPoint.Y;
                person.jHipCenter.D = depthPoint.Depth;
            }
            else if (joint == JointType.HipLeft) {
                person.jHipLeft.X = colourPoint.X;
                person.jHipLeft.Y = colourPoint.Y;
                person.jHipLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.HipRight) {
                person.jHipRight.X = colourPoint.X;
                person.jHipRight.Y = colourPoint.Y;
                person.jHipRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.Spine) {
                person.jSpine.X = colourPoint.X;
                person.jSpine.Y = colourPoint.Y;
                person.jSpine.D = depthPoint.Depth;
            }
            else if (joint == JointType.ElbowLeft) {
                person.jElbowLeft.X = colourPoint.X;
                person.jElbowLeft.Y = colourPoint.Y;
                person.jElbowLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.ElbowRight) {
                person.jElbowRight.X = colourPoint.X;
                person.jElbowRight.Y = colourPoint.Y;
                person.jElbowRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.FootLeft) {
                person.jFootLeft.X = colourPoint.X;
                person.jFootLeft.Y = colourPoint.Y;
                person.jFootLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.FootRight) {
                person.jFootRight.X = colourPoint.X;
                person.jFootRight.Y = colourPoint.Y;
                person.jFootRight.D = depthPoint.Depth;
            }
            else if (joint == JointType.HandLeft) {
                person.jHandLeft.X = colourPoint.X;
                person.jHandLeft.Y = colourPoint.Y;
                person.jHandLeft.D = depthPoint.Depth;
            }
            else if (joint == JointType.HandRight) {
                person.jHandRight.X = colourPoint.X;
                person.jHandRight.Y = colourPoint.Y;
                person.jHandRight.D = depthPoint.Depth;
            }
        }



        // Function to determine if a push gesture has been performed
        private Boolean pushGesturePerformed(Person person) {

            Boolean push = false;
            double difference;

            // Tracking right or left hand
            if (person.TRACK_RIGHT_HAND) difference = person.jSpine.D - person.jHandRight.D;
            else difference = person.jSpine.D - person.jHandLeft.D;

            // Comparison of spine and hand depth
            if (difference > person.PUSH_DIFFERANCE) push = true;
  
            return push;
        }





        // Called when the KinectSensorChooser gets a new sensor
        private  void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args) {
            
            if (args.OldSensor != null) {
                try {
                    args.OldSensor.ColorStream.Disable();
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException){}
            }

            if (args.NewSensor != null) {
                try {
                    args.NewSensor.ColorStream.Enable();
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();
                    // Load People
                    initalizePeople();

                    try {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        //args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException) {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        //args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                    }
                }
                catch (InvalidOperationException) {}
            }
        }




        // Skeleton tracking methods
        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e) {

            // Retrives source from kinect sensor
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {

                if (colorFrame == null) return;

                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);

                int stride = colorFrame.Width * 4;

                // Displays video
                if (SHOW_VIDEO) this.imgVideo.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }

            // Tracks multiple people in loop
            for (int i = 0; i < SKELETONS_TRACKED; i++) {

                Skeleton me = null;
                GetSkeleton(e, ref me, i);

                if (me == null) return;

                // Sets values to people array then draws the person
                GetXYD(me, People[i]);
                People[i].DrawPerson();

                // Check for gestures
                checkArmPositions(People[i]);
            }
        }



        // This function initialises the people from the array
        private void initalizePeople() {

            for (int i = 0; i < SKELETONS_TRACKED; i++) {

                // Adds joints to display
                People[i] = new Person(i);
                List<Ellipse> Joints = People[i].getJoints();
                if (DRAW_SKELETON) foreach (Ellipse Joint in Joints) canDraw.Children.Add(Joint);

                // Adds bones to display
                List<Line> Bones = People[i].getBones();
                if (DRAW_SKELETON) foreach (Line Bone in Bones) canDraw.Children.Add(Bone);
            }
        }





        //***************************************************************************************************************************************************
        // Tile button click functions

        // Handle a button click from the browser wrap panel
        private void KinectBrowserButtonClick(object sender, RoutedEventArgs e) {

            // Gets the clicked tile button
            var button = (KinectTileButton)e.OriginalSource;

            // Stores the buttons tag into a string
            String buttonTag = "";
            if (button.Tag != null) buttonTag = button.Tag.ToString();

            String chosenDirectory = "";

            // Set the directory based on the button tag
            if (buttonTag == "button_video") chosenDirectory = videoDirectory;
            else if (buttonTag == "button_documentary") chosenDirectory = videoDocumentaryDirectory;
            else if (buttonTag == "button_movie") chosenDirectory = videoMovieDirectory;
            else if (buttonTag == "button_movie_action") chosenDirectory = videoMActionDirectory;
            else if (buttonTag == "button_movie_comedy") chosenDirectory = videoMComedyDirectory;
            else if (buttonTag == "button_movie_horror") chosenDirectory = videoMHorrorDirectory;
            else if (buttonTag == "button_movie_romance") chosenDirectory = videoMRomanceDirectory;
            else if (buttonTag == "button_movie_scifi") chosenDirectory = videoMSciFiDirectory;
            else if (buttonTag == "button_tv") chosenDirectory = videoTVDirectory;
            else if (buttonTag == "button_audio") chosenDirectory = audioDirectory;
            else if (buttonTag == "button_audioBook") chosenDirectory = audioAudioBookDirectory;
            else if (buttonTag == "button_music") chosenDirectory = audioMusicDirectory;
            else if (buttonTag == "button_podCast") chosenDirectory = audioPodCastDirectory;
            else if (buttonTag == "button_music_classical") chosenDirectory = audioMClassicalDirectory;
            else if (buttonTag == "button_music_country") chosenDirectory = audioMCountryDirectory;
            else if (buttonTag == "button_music_dance") chosenDirectory = audioMDanceDirectory;
            else if (buttonTag == "button_music_hiphop") chosenDirectory = audioMHipHopDirectory;
            else if (buttonTag == "button_music_jazz") chosenDirectory = audioMJazzDirectory;
            else if (buttonTag == "button_music_pop") chosenDirectory = audioMPopDirectory;
            else if (buttonTag == "button_music_rock") chosenDirectory = audioMRockDirectory;

            if (chosenDirectory != "") currentDirectory = chosenDirectory;
            if (button.Label == null) updateBrowserPanel(chosenDirectory); else handleMediaFile(currentDirectory, buttonTag);

            //var selectionDisplay = new SelectionDisplay(button.Label as string);
            //this.kinectRegionGrid.Children.Add(selectionDisplay);
            e.Handled = true;

            updateMainPanel();
        }


        // Handle a button click from the control wrap panel
        private void KinectControlButtonClick(object sender, RoutedEventArgs e) {

            // Gets the clicked tile button
            var button = (KinectTileButton)e.OriginalSource;

            // Stores the buttons tag into a string
            String buttonTag = "";
            if (button.Tag != null) buttonTag = button.Tag.ToString();

            // If there is currently a media file loaded
            if (currentMediaState != MediaState.Unloaded) {

                // Media controls
                if (buttonTag == "button_control_backward") mediaPlayerRewind();            // Backward button click/push
                else if (buttonTag == "button_control_forward") mediaPlayerForward();       // Forward button click/push
                else if (buttonTag == "button_control_stop") mediaPlayerStop();             // Stop button click/push
                else if (buttonTag == "button_control_play") mediaPlayerPlay();             // Play button click/push
                else if (buttonTag == "button_control_pause") mediaPlayerPause();           // Pause button click/push
            }

            // Update the control wrap panel
            updateControlPanel();
        }


        // Handle a button click from the main wrap panel
        private void KinectMainButtonClick(object sender, RoutedEventArgs e) {

            // Gets the clicked tile button
            var button = (KinectTileButton)e.OriginalSource;

            // Stores the buttons tag into a string
            String buttonTag = "";
            if (button.Tag != null) buttonTag = button.Tag.ToString();

            String chosenDirectory = "";

            // Video button click/push
            if (buttonTag == "button_video") {
                selectedMediaType = MediaType.Video;
                chosenDirectory = videoDirectory;
                playListRemove();
            }
            // Audio button click/push
            else if (buttonTag == "button_audio") {
                selectedMediaType = MediaType.Audio;
                chosenDirectory = audioDirectory;
                playListRemove();
            }
            // Add file button click/push
            else if (buttonTag == "button_add") AddFile();
            
            // Play list button click/push
            else if (buttonTag == "button_playlist") if (!isCategoryFolder()) playList();
            
            // Set the current directory
            if (chosenDirectory != "") currentDirectory = chosenDirectory;
            if (button.Label == null && buttonTag != "button_add" && buttonTag != "button_playlist") updateBrowserPanel(chosenDirectory);

            updateMainPanel();
        }


        // Debug button click/press event
        private void KinectDebugButtonClick(object sender, RoutedEventArgs e) {

            // Enable/disable debug mode (show/hide skeleton data)
            if (!debugMode) {
                //SHOW_VIDEO = true;
                DRAW_SKELETON = true;
                debugMode = !debugMode;
                print("Debug mode = " + debugMode.ToString());
                canDraw.Visibility = Visibility.Visible;
                imgVideo.Visibility = Visibility.Visible;
                showSkeletonDataDisplay();
            }
            else {
                //SHOW_VIDEO = false;
                DRAW_SKELETON = false;
                debugMode = !debugMode;
                print("Debug mode = " + debugMode.ToString());
                canDraw.Visibility = Visibility.Hidden;
                imgVideo.Visibility = Visibility.Hidden;
                removeSkeletonDataDisplay();
            }

            updateDebugDisplay();
        }

        //***************************************************************************************************************************************************


        // This function draws the skeleton in the GUI (Called when debug mode is enabled)
        private void showSkeletonDataDisplay() {

            print("Number of people = " + People.Count().ToString());

            if (debugMode && People.Count() > 0) {

                // Adds joints to display
                List<Ellipse> Joints = People[0].getJoints();
                if (DRAW_SKELETON) foreach (Ellipse Joint in Joints) canDraw.Children.Add(Joint);

                // Adds bones to display
                List<Line> Bones = People[0].getBones();
                if (DRAW_SKELETON) foreach (Line Bone in Bones) canDraw.Children.Add(Bone);
            }
        }



        // This function removes the skeleton from the GUI (Called when debug mode is disabled)
        private void removeSkeletonDataDisplay() {

            if (People.Count() > 0) {

                // Removes joints from display
                List<Ellipse> Joints = People[0].getJoints();
                foreach (Ellipse Joint in Joints) canDraw.Children.Remove(Joint);

                // Removes bones from display
                List<Line> Bones = People[0].getBones();
                foreach (Line Bone in Bones) canDraw.Children.Remove(Bone);
            }
        }



        // This function displays or hides the playlist
        private void playList() {

            // If the playlist is not displayed, this displays it
            if (!playListActive) {

                playListActive = true;
                listBoxPlayList.DataContext = playListDisplayArray;
                listBoxPlayList.Visibility = Visibility.Visible;
                PlayListBackgroundImage.Visibility = Visibility.Visible;
            } else {
                playListRemove();  // If playlist is displayed, this hides it
            }
        }


        // This function updates the playlist display (Called when aitems are added or removed form playlist)
        private void playListUpdate() {

            // Update the listbox data with the data from the array list
            listBoxPlayList.DataContext = null;
            listBoxPlayList.DataContext = playListDisplayArray;
        }


        // This functions removes all items from the playlist and hides the display in the GUI
        private void playListRemove() {

            // Clear the playlist
            playListActive = false;
            playListDisplayArray.Clear();
            playListArray.Clear();

            // Sets the listbox and background not visible
            listBoxPlayList.Visibility = Visibility.Hidden;
            PlayListBackgroundImage.Visibility = Visibility.Hidden;
        }






        // This function loads the selected media file
        private void handleMediaFile(String selectedDirectory, String fileName) {

            String fileExtension = "";

            // Sets the file extension as avi for video files and mp3 for audio files
            if (selectedMediaType == MediaType.Audio) fileExtension = ".mp3"; else fileExtension = ".avi";

            // Composes the media file full path
            String fullPath = selectedDirectory + fileName + fileExtension;

            // If the playlist active boolean is set to false, this plays the file immediately
            if (!playListActive) {

                // Loads the selected media file into the video control and starts playing it
                VideoControl.Source = new Uri(fullPath);
                VideoControl.Play();

                // Sets the current media state
                currentMediaState = MediaState.Playing;

                // Display the media file name in the GUI
                lblMediaFileName.Content = fileName;

                // Make the volume bar and file name display visible in the GUI
                mediaVolumeBar.Visibility = Visibility.Visible;
                lblMediaFileName.Visibility = Visibility.Visible;
                mediaSliderProgress.Visibility = Visibility.Visible;

                // Update the control wrap panel
                updateControlPanel();
            } else {

                // Add the file to the playlist
                playListArray.Add(fullPath);
                playListDisplayArray.Add(fileName);

                // Update the listbox data with the data from the array list
                playListUpdate();

                // If this is the first file in the playlist, this plays it immediately
                if (playListDisplayArray.Count == 1) {

                    // Loads the selected media file into the video control and starts playing it
                    VideoControl.Source = new Uri(fullPath);
                    VideoControl.Play();

                    // Sets the current media state
                    currentMediaState = MediaState.Playing;

                    // Display the media file name in the GUI
                    lblMediaFileName.Content = fileName;

                    // Make the volume bar and file name display visible in the GUI
                    mediaVolumeBar.Visibility = Visibility.Visible;
                    lblMediaFileName.Visibility = Visibility.Visible;
                    mediaSliderProgress.Visibility = Visibility.Visible;

                    // Update the control wrap panel
                    updateControlPanel();
                }
            }
        }




        // This function creates an open file dialog and enables the user to add a new file
        private void AddFile() {

            // Only allow files to be added to the content folders
            if (!isCategoryFolder()) {

                // Create the open file dialog box
                Microsoft.Win32.OpenFileDialog addFileDialog = new Microsoft.Win32.OpenFileDialog();

                // Filter the file types based on the current selected media
                if (selectedMediaType == MediaType.Video) {
                    addFileDialog.DefaultExt = ".avi";
                    addFileDialog.Filter = "Video Files (.avi)|*.avi";
                } else {
                    addFileDialog.DefaultExt = ".mp3";
                    addFileDialog.Filter = "Audio Files (.mp3)|*.mp3";
                }

                // Show open file dialog box
                Nullable<bool> result = addFileDialog.ShowDialog();

                // If the user selected a file
                if (result == true) {

                    // Store the new files full path
                    String inFilePath = addFileDialog.FileName;

                    // Set the output directory where the file will be copied
                    String outFilePath = currentDirectory;

                    // This extracts the file name including the file extension from the full directory path
                    int stringSplitter1 = inFilePath.LastIndexOf('\\');
                    String inFileFullName = inFilePath.Substring(stringSplitter1 + 1);

                    // This adds the in file name to the output directory path
                    String outFileFullPath = outFilePath + inFileFullName;

                    // Copies the selected file to the new directory
                    File.Copy(inFilePath, outFileFullPath);

                    // This removes the file extension so we are left with just the file name
                    int stringSplitter2 = inFileFullName.LastIndexOf('.');
                    String fileName = inFileFullName.Substring(0, stringSplitter2);

                    // This is used to copy the template jpg file from the "Images" folder to the users selected directory with the name of the seleceted file
                    String outImageFullPath = outFilePath + fileName + ".jpg";
                    String templatePath = imageDirectory + "Button_MediaTemplate.jpg";

                    if (File.Exists(templatePath)) File.Copy(templatePath, outImageFullPath, true);
                    
                    // Update the browser wrap panel
                    updateBrowserPanel(currentDirectory);
                }
            }
        }




        //***************************************************************************************************************************************************
        // Gesure command checking functions

        // This function is used to check if the user is currently performing a volume command
        // A volume command is performed when the user's left arm is half bent and left hand is above left shoulder
        private Boolean performingVolumeCommand() {

            Boolean commandPerformed = false;

            if (volumeCommandGesture && LeftLowerArm == 1) {
                
                // Sets the text content and text colour in the GUI if the gesture is performed
                commandPerformed = true;
                lblCommandDisplay.Content = "Performing a volume command!";
                lblCommandDisplay.Foreground = System.Windows.Media.Brushes.Green;
            } else {

                // Sets the text content and text colour in the GUI if the gesture is not performed
                commandPerformed = false;
                lblCommandDisplay.Content = "Not performing a command!";
                lblCommandDisplay.Foreground = System.Windows.Media.Brushes.Red;
            }

            return commandPerformed;
        }


        // This function is used to check if the user is currently performing a skip command
        // A skip command is performed when the user's left arm is straight and left hand is roughly level with left shoulder
        private Boolean performingSkipCommand() {

            Boolean commandPerformed = false;

            // This checks the position of the user's arms
            if (skipCommandGesture && LeftLowerArm == 0) {

                // Sets the text content and text colour in the GUI if the gesture is performed
                commandPerformed = true;
                lblCommandDisplay.Content = "Performing a skip command!";
                lblCommandDisplay.Foreground = System.Windows.Media.Brushes.Green;
            } else {

                // Sets the text content and text colour in the GUI if the gesture is not performed
                commandPerformed = false;
                lblCommandDisplay.Content = "Not performing a command!";
                lblCommandDisplay.Foreground = System.Windows.Media.Brushes.Red;
            }

            // Returns true or false depening on wheather the gesture has been succesfully perfromed
            return commandPerformed;
        }
        //***************************************************************************************************************************************************






        //***************************************************************************************************************************************************
        // Media window scroll functions

        // This function is called when the media window skip region is scrolled
        private void MediaWindowSkipScrolled()  {

            Boolean mediaSkipScrolled = false;

            if (mediaScrollSkipStopWatch.IsRunning) {
                if (mediaScrollSkipStopWatch.ElapsedMilliseconds > 1000) {
                    mediaScrollSkipStopWatch.Stop();
                    mediaScrollSkipStopWatch.Reset();
                }
            }

            if (!mediaScrollSkipStopWatch.IsRunning) {
                if (currentMediaState == MediaState.Playing) {
                    if (scrollViewerMediaSkip.HorizontalOffset > 470) {
                        mediaSkipScrolled = true;
                        mediaPlayerRewind();
                    }
                    else if (scrollViewerMediaSkip.HorizontalOffset < 430) {
                        mediaSkipScrolled = true;
                        mediaPlayerForward();
                    }

                }
            }

            // If a gesture is performed, start the stop watch
            if (mediaSkipScrolled) mediaScrollSkipStopWatch.Start();

            // Reset the horizontal offset
            scrollViewerMediaSkip.ScrollToHorizontalOffset(450);
        }


        // This function is called when the media window volume region is scrolled
        private void MediaWindowVolumeScrolled() {

            Boolean mediaVolumeScrolled = false;

            if (mediaScrollVolumeStopWatch.IsRunning) {
                if (mediaScrollVolumeStopWatch.ElapsedMilliseconds > 200) {
                    mediaScrollVolumeStopWatch.Stop();
                    mediaScrollVolumeStopWatch.Reset();
                }
            }

            if (!mediaScrollVolumeStopWatch.IsRunning) {
                if (currentMediaState == MediaState.Playing) {
                    if (scrollViewerMediaVolume.VerticalOffset < 405) {
                        mediaVolumeScrolled = true;
                        mediaPlayerVolumeDown();
                    }
                    else if (scrollViewerMediaVolume.VerticalOffset > 445) {
                        mediaVolumeScrolled = true;
                        mediaPlayerVolumeUp();
                    }
                }
            }

            // If a gesture is performed, start the stop watch
            if (mediaVolumeScrolled) mediaScrollVolumeStopWatch.Start();

            // Reset the vertical offset
            scrollViewerMediaVolume.ScrollToVerticalOffset(425);
        }
        //***************************************************************************************************************************************************



        //***************************************************************************************************************************************************
        // Media control functions

        // This function rewinds the currently playing media by 5 minutes
        private void mediaPlayerRewind() {

            // If a video is playing, this skips it backwards by 5 minutes
            if (selectedMediaType == MediaType.Video) {

                // Get the current media position
                TimeSpan currentMediaPosition = VideoControl.Position;

                // Subtract 300 seconds (5 minutes)
                if (currentMediaPosition.TotalSeconds > 300) currentMediaPosition -= new TimeSpan(0, 0, 0, 300, 0);

                // Update the media position with the new value
                VideoControl.Position = currentMediaPosition;
            } else {
                // Not enabled skipping backwards through a playlist (At the moment items are removed from playlist when user skips forward, so cannot go backwards)
            }
        }


        // This function forwards the currently playing media by 5 minutes
        private void mediaPlayerForward() {

            // If a video is playing, this skips it forward by 5 minutes
            if (selectedMediaType == MediaType.Video) {

                // Get the current media position
                TimeSpan currentMediaPosition = VideoControl.Position;

                // Add 300 seconds (5 minutes)
                currentMediaPosition += new TimeSpan(0, 0, 0, 300, 0);

                // Update the media position with the new value
                VideoControl.Position = currentMediaPosition;
            } else {

                // If an audio file is playing and the playlist contains files, this skips to the next song in the playlist
                if (playListActive && playListArray.Count > 1) playlistNextFile();
            }
        }


        // This function pauses the currently playing media
        private void mediaPlayerPause() {

            // If the media is currently playing, then it is paused
            if (currentMediaState == MediaState.Playing)  {
                currentMediaState = MediaState.Paused;
                VideoControl.Pause();
            }
        }


        // This function plays the loaded media file
        private void mediaPlayerPlay()  {

            // If the media is currently stopped or paused, then it is played
            if (currentMediaState == MediaState.Paused || currentMediaState == MediaState.Stopped) {
                currentMediaState = MediaState.Playing;
                VideoControl.Play();
            }
        }


        // This function stops the currently playing media file
        private void mediaPlayerStop() {

            // Stop the currently playing media
            currentMediaState = MediaState.Stopped;
            VideoControl.Stop();
        }


        // This function increases the media volume
        private void mediaPlayerVolumeUp() {
            VideoControl.Volume += 0.1;
        }


        // This function decreases the media volume
        private void mediaPlayerVolumeDown() {
            VideoControl.Volume -= 0.1;
        }
        //***************************************************************************************************************************************************


        // This function is used to play the next file in the playlist
        private void playlistNextFile() {

            // Remove the first song from the playlist and update the list display in the GUI
            playListArray.RemoveAt(0);
            playListDisplayArray.RemoveAt(0);
            playListUpdate();

            // Stops the current media file and plays the next file from the playlist
            VideoControl.Stop();
            VideoControl.Source = new Uri(playListArray.ElementAt(0).ToString());
            VideoControl.Play();

            // Sets the current media state
            currentMediaState = MediaState.Playing;

            // Display the media file name in the GUI
            lblMediaFileName.Content = playListDisplayArray.ElementAt(0).ToString(); 
        }


        // This function is called when the user is moving the media slider control
        private void mediaSlider_DragStarted(object sender, DragStartedEventArgs e) {
            userIsDraggingSlider = true;
        }


        // This function is called when the user stops moving the media slider control
        private void mediaSlider_DragCompleted(object sender, DragCompletedEventArgs e) {
            userIsDraggingSlider = false;
            VideoControl.Position = TimeSpan.FromSeconds(mediaSliderProgress.Value);
        }


        // Change button state depending on scroll viewer position
        private void BrowserPanelScrolled() {
            this.PageLeftEnabled = scrollViewerBrowserPanel.HorizontalOffset > ScrollErrorMargin;
            this.PageRightEnabled = scrollViewerBrowserPanel.HorizontalOffset < scrollViewerBrowserPanel.ScrollableWidth - ScrollErrorMargin;
        }


        // Mose wheel event for controling the audio volume for the media file
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e) {

            // Media volume in relation to the mouse scroll wheel
            VideoControl.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }


        // This function is used to determine if the current folder is a category folder
        private Boolean isCategoryFolder() {

            Boolean isCategory = false;
            if (currentDirectory == videoDirectory || currentDirectory == audioDirectory || currentDirectory == videoMovieDirectory || currentDirectory == audioMusicDirectory) isCategory = true;

            return isCategory;
        }


        // Prints a string in the console
        public void print(String tmp) {
            Console.WriteLine(tmp);
        }


        // Execute shutdown tasks
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (sensorChooser != null) this.sensorChooser.Stop();
        }


        // CLR Property Wrappers for PageLeftEnabledProperty
        public bool PageLeftEnabled {
            get { return (bool)GetValue(PageLeftEnabledProperty); }
            set { this.SetValue(PageLeftEnabledProperty, value); }
        }


        // CLR Property Wrappers for PageRightEnabledProperty
        public bool PageRightEnabled {
            get { return (bool)GetValue(PageRightEnabledProperty); }
            set { this.SetValue(PageRightEnabledProperty, value); }
        }


    }
}
