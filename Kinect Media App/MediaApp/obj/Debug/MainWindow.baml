   M S B A M L   `   `   `  ����K  GMediaApplication, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:MediaApplicationMediaApplication  5   	   !    MediaApplication.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   \ XMicrosoft.Kinect.Toolkit.Controls, Version=0.0.0.0, Culture=neutral, PublicKeyToken=nullS OMicrosoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null2k(http://schemas.microsoft.com/kinect/2013   5   	   <d2http://schemas.microsoft.com/expression/blend/2008   5   	   Fmc;http://schemas.openxmlformats.org/markup-compatibility/2006   5   	   ,localclr-namespace:MediaApplication   5   	   -   5        $��MediaAppWindow��5	   	     � Title$  Kinect Media Manager��5
   	   $	��500��5      $	��700��5   	   $��Segoe UI)�5   	   $
��1039��6   $
��1400��6-    � 	Resources 5      ��%    5      .��5      � -   5         � ColumnDefinitions 5   
   �� 5      $
��790* �6   �� 5      $
��388* �6    � RowDefinitions 5   
   �� 5      $	��100 �6   �� 5      $
��890* �6   .+�5#   
   � $��
10,0,10,40q�6   $��2��6#   .+�5$      8  2Microsoft.Kinect.Toolkit.Controls.KinectUserViewer  $	��100��6b   $��Center=�6o   $	��Top=�6�   4  .Microsoft.Kinect.Toolkit.Controls.KinectRegion   KinectRegion 6!   ��  �� ElementName$ kinectRegion��6�   4  .Microsoft.Kinect.Toolkit.KinectSensorChooserUI  5%      -   5        $��sensorChooserUi��5%   [   $��Center=�6&   $	��Top=�6C   $��60��6r   $��80��6~   5&        5*   
   -   5          Name$ kinectRegion��5*   &   $��1��6   $��	0,-40,0,0q�6<   $��2��6O   .��5,      � -   5         �Name$ kinectRegionGrid��5,      $��10,20,10,20q�6-   $	��730��6B    5-      �� 5.      $��* �6$   �� 5/      $	��170 �6$   .+�53      :  4Microsoft.Kinect.Toolkit.Controls.KinectScrollViewer  -   5        $��scrollViewerMainPanel��53   '   $��Disabled=�6D   $��Disabled=�6m   $��107,-100,969,370q�6�   .��55      � -   5         �Name$ wrapPanelMainPanel��55   ;   $��Center=�6    	 � Orientation$	 Vertical=�6W   $��4,0,0,0q�6�   $	��287��6�   $	��280��6�   .+�57      8  2Microsoft.Kinect.Toolkit.Controls.KinectTileButton  
   Label$
 1��6-   $	��213��67   $	��210��6C     58      $
 2��6-   $	��213��67     59      $
 3��6-     5:      $
 4��6-   5;      5<        5?      -   5        $��scrollViewerBrowserPanel��5?   '   $��Visible=�6G   $��Disabled=�6o   $��0,80,0,-102q�6�   $��1��6�   .��5A      � -	   5        $ wrapPanelBrowserPanel��5A   ;   $��Center=�6    $	 Vertical=�6Z   .+�5C        $
 1��6-     5D      $
 2��6-     5E      $
 3��6-     5F      $
 4��6-     5G      $
 5��6-     5H      $
 6��6-   5I      5J      � 5M      -
   5        $ wrapPanelControlPanel��5M   7   $��Center=�6   $	 Vertical=�6V   $��550,470,87,-50q�6m   .+�5O        $
 1��6)     5P      $
 2��6)     5Q      $
 3��6)     5R      $
 4��6)   5S      �� 5\      $��0��6   $��450,-655,90,300q�6)   $��1��6B   ._�5^      r� -   5        $��VideoControl��5_      $��0,0,0,0q�61   $��2��6B    r� LoadedBehavior$ Manual=�6V   $��2��6n   $	��800��6�   5`      ��5c      �� 5d       �� ImageSource$  Images\MediaBackground.jpg��6%    j� Stretch$
 Fill=�6N   5f        5o      -   5        $��scrollViewerMediaSkip��5o   '   $��Visible=�6D   $��Disabled=�6l   $��450,-200,85,297q�6�   $��1��6�    J� Opacity$ 0Q�6�   .��5p      � -   5        $ wrapPanelMediaSkip��5p   ;   $��Center=�6    $	 Vertical=�6W   $	��250��6n   .+�5r        $
 1��6-     5s      $
 2��6-     5t      $
 3��6-     5u      $
 4��6-     5v      $
 5��6-   5w      5x        5{      -   5        $��scrollViewerMediaVolume��5{   '   $��Disabled=�6F   $��Visible=�6o   $��450,-655,600,397q�6�   $��1��6�   $ 0Q�6�   .��5|      � -   5        $ wrapPanelMediaVolume��5|   ;   $��Center=�6    $	 Vertical=�6Y   $
��1250��6p   .+�5~        $
 1��6-     5      $
 2��6-     5�      $
 3��6-     5�      $
 4��6-     5�      $
 5��6-   5�      5�      �� 5�      -   5         ��Name$ canDraw��5�      $	��480��6*   $	��640��67   $��445,-630,69,420q�6C   $��2��6\   $��1��6p   .+�5�      �� -   5         ��Name$ imgVideo��5�      $	��480��6.   $	��640��6;   $��0,0,0,0q�6G   $	��Top=�6X   5�      �� 5�      $��Right=�6   $��Images\Logo.png��64   $
��None=�6M   $��0,-170,10,680q�6\   $��2��6s   $	��487��6�   �� 5�      -   5         ��Name$ lblCommandDisplay��5�      $��1070,-120,90,650q�63   $��14(�6M   � 5�      -   5        $��mediaVolumeBar��5�      $	��150��64   $��30��6@   $��2��6L   $��462,-190,800,-530q�6`    � Maximum$ 1Q�6{    � Value 6�   �� $ VideoControl�� �� Path$ Volume��� 5�      -   5        $ lblMediaFileName��5�      $��640,425,310,90q�62   $��Center=�6J   $��20(�6o   $
��Bold"�6}   �� 5�      -   5        $ lblMediaDuration��5�      $��1110,425,90,90q�62   $��Center=�6J   $��20(�6n   �� 5�      -   5        $��mediaSliderProgress��5�      $��450,405,90,130q�64   6y   �� 5�      -   5        $��PlayListBackgroundImage��5�      $��0��68   $��117,-355,978,300q�6H   $��1��6b   ��5�      �� 5�      $  Images\List_Background.jpg��6%   $
 Fill=�6N   5�      �� 5�      -   5        $��listBoxPlayList��5�      $��120,210,981,135q�6J   $��16(�6c   
���� ��$
��Bold"�6�   
������ 
������ ��62   �� 6�   � 5�      -   5        $ wrapPanelDebugPanel��5�   7   $��Center=�6   $	 Vertical=�6T   $��1280,-733,0,-50q�6k   .+�5�        $
 1��6)   $��75��63   $��75��6>   5�      5�      5�      5�      5�      