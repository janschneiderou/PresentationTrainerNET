   M S B A M L   `   `   `  ����N  JPresentationTrainer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:PresentationTrainerPresentationTrainer  5      +    %PresentationTrainer.InterruptFeedback   9PresentationTrainer!clr-namespace:PresentationTrainer  W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      d `Microsoft.Kinect.Wpf.Controls, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e350k(http://schemas.microsoft.com/kinect/2014  5      -   5        
�������.��5	      0  *Microsoft.Kinect.Wpf.Controls.KinectRegion  -   5           Name$  kinectRegion��5	      
�������.��5
   
   � .+�5      r� -   5        $��
pauseSound��5       r� Source$ Sounds\smw_pause.wav?�6-    r� LoadedBehavior$ Manual=�6K   $y�Visible=�6d   � 5      -   5         �Name$ 	OutroGrid��5      $
��Left=�6&   $	��700��6A   $	��Top=�6N   $
��1000��6f   ���#1B9ED2.+�5      �� -   5        $��
outroLabel��5      $	��AAA��6*   $��35(�68   $
��Left=�6G   $��
10,156,0,0q�6c   $	��Top=�6w   
�������$	��299��6�   $	��980��6�   �� 5      -   5        $��GoBackButton��5      
������ $
��Left=�6G   $��502,540,0,0q�6b   $	��Top=�6w   $	��128��6�   $	��102��6�   .��5      �� $$��Images/presentation-yellow.png��6   5      �� 5      -   5        $��GoToExercises��5      
������ $
��Left=�6H   $��166,540,0,0q�6c   $	��Top=�6x   $	��127��6�   $	��101��6�   .��5      �� $!��Images/exercises-yellow.png��6   5      �� 5      $��	Freestyle��6   $
��Left=�6,   $��43��6G   $��520,647,0,0q�6S   $	��Top=�6h   $	��140��6�   
�������$��20(�6�    J� RenderTransformOrigin$ 	0.607,0.5&�6�   �� 5      $!��Specific Skill (recomended)��6   $
��Left=�6>   $��43��6Y   $��100,647,0,0q�6e   $	��Top=�6z   $	��254��6�   
�������$��20(�6�   �� 5      $O��ISelect and option and continue your presentation starting with your slide��6   $
��Left=�6l   
�������$��25(�6�   $��
44,474,0,0q�6�   $	��Top=�6�   $	��912��6�   5      � 5      -   5        $ InstructGrid��5      $
��Left=�6)   $	��700��6D   $	��Top=�6Q   $
��1000��6i   ���#1B9ED2$y�	Collapsed=�6�   .+�5      �� -	   5        $��descriptionLabel��5      $	��AAA��60   $��35(�6>   $
��Left=�6M   $��
10,156,0,0q�6i   $	��Top=�6}   
�������$	��270��6�   $	��980��6�   �� 5      -
   5        $��	iconImage��5      $��Center=�6)   $	��236��6F   $��272,441,490,0q�6S   $	��Top=�6j   $	��238��6�   �� 5       -   5        $��InstructButtonNext��5       ���#FFB400
�������$��30(�6\   $
��Next��6j   $
��Left=�6y   $��906,270,-3,0q�6�   $	��Top=�6�   $��97��6�   $��94��6�   6�   5!      � 5#      -   5        $ 	IntroGrid��5#      $
��Left=�6(   $	��700��6C   $	��Top=�6P   $
��1000��6h   ���#1B9ED2$y�	Collapsed=�6�   .+�5$      �� -   5        $��
introLabel��5$      $��Label��6*   $
��Left=�6:   $��35(�6U   $��
10,160,0,0q�6c   
�������$	��Top=�6�   $	��918��6�   $	��150��6�   &    PresentationTrainer.EvidenceItem  5%      -   5          Name$ evidence��5%   4   $
��Left=�6F   $��	0,327,0,0q�6a   $	��Top=�6t   $
��1000��6�   $	��373��6�   �� 5&      -   5        $��IntroButtonNext��5&      ���#FFB400
�������$��30(�6Y   $
��Next��6g   $
��Left=�6v   $��906,270,-3,0q�6�   $	��Top=�6�   $��97��6�   $��94��6�   6�   5'      �� 5)      $��Center=�6   $	��100��61   $��0,51,0,0q�6>   $��Images/hand-yellow.png��6P   $	��Top=�6p   $	��100��6�   5,      5-      5.      