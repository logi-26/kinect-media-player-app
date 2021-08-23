using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MediaApplication {

    class Person {

        public double PUSH_DIFFERANCE = 400;
        public Boolean TRACK_RIGHT_HAND = true;

        private int JOINT_HEIGHT = 10;
        private int JOINT_WIDTH = 10;

        // Struct for the joints
        public struct Joint { public int X; public int Y; public int D;}      // X position, Y position and depth

        // Creates the joints
        public Joint jAnkleRight, jAnkleLeft, jWristRight, jWristLeft, jShoulderRight, jShoulderLeft, jShoulderCenter, jKneeLeft, jKneeRight, jHipCenter, jHipLeft, jHipRight, jSpine, jElbowLeft, jElbowRight, jFootLeft, jFootRight, jHandLeft, jHandRight, jHead = new Joint();

        // These are ellipse shapes to represent each of the joints in the human skelton (21)  
        public Ellipse Head = new Ellipse();
        public Ellipse LeftPalm = new Ellipse();
        public Ellipse RightPalm = new Ellipse();
        public Ellipse LeftElbow = new Ellipse();
        public Ellipse RightElbow = new Ellipse();
        public Ellipse Body = new Ellipse();
        //public Ellipse AnkleRight = new Ellipse();
        //public Ellipse AnkleLeft = new Ellipse();
        public Ellipse WristLeft = new Ellipse();
        public Ellipse WristRight = new Ellipse();
        public Ellipse ShoulderLeft = new Ellipse();
        public Ellipse ShoulderRight = new Ellipse();
        public Ellipse ShoulderCenter = new Ellipse();
        //public Ellipse KneeRight = new Ellipse();
        //public Ellipse KneeLeft = new Ellipse();
        public Ellipse HipRight = new Ellipse();
        public Ellipse HipLeft = new Ellipse();
        public Ellipse HipCenter = new Ellipse();
        public Ellipse Face = new Ellipse();
        //public Ellipse FootLeft = new Ellipse();
        //public Ellipse FootRight = new Ellipse();

        // These are line shapes to represent each of the bones in the human skelton (19)  
        public Line Neck = new Line();
        public Line LowerBack = new Line();
        public Line Spine = new Line();
        public Line RightShoulder = new Line();
        public Line RightUpperArm = new Line();
        public Line RightForeArm = new Line();
        public Line RightHand = new Line();
        public Line RightHip = new Line();
        //public Line RightThigh = new Line();
        //public Line RightShin = new Line();
        //public Line RightFoot = new Line();
        public Line LeftShoulder = new Line();
        public Line LeftUpperArm = new Line();
        public Line LeftForeArm = new Line();
        public Line LeftHand = new Line();
        public Line LeftHip = new Line();
        //public Line LeftThigh = new Line();
        //public Line LeftShin = new Line();
        //public Line LeftFoot = new Line();

        // Additional  
        public Rectangle recHover = new Rectangle();
        public Rectangle recSelected = new Rectangle();

        // This function returns a list of joints (ellipse shapes)
        public List<Ellipse> getJoints() {

            // Creates the list and adds all of the joints
            List<Ellipse> Joints = new List<Ellipse>();
            Joints.Add(Head);
            Joints.Add(LeftPalm);
            Joints.Add(RightPalm);
            Joints.Add(LeftElbow);
            Joints.Add(RightElbow);
            Joints.Add(Body);
            //Joints.Add(AnkleRight);
            //Joints.Add(AnkleLeft);
            Joints.Add(WristLeft);
            Joints.Add(WristRight);
            Joints.Add(ShoulderLeft);
            Joints.Add(ShoulderRight);
            Joints.Add(ShoulderCenter);
            //Joints.Add(KneeRight);
            //Joints.Add(KneeLeft);
            Joints.Add(HipRight);
            Joints.Add(HipLeft);
            Joints.Add(HipCenter);
            Joints.Add(Face);
            //Joints.Add(FootLeft);
            //Joints.Add(FootRight);

            return Joints;
        }

        // This function returns a list of bones (ellipse shapes)
        public List<Line> getBones() {

            // Creates the list and adds all of the bones
            List<Line> Bones = new List<Line>();
            Bones.Add(Neck);
            Bones.Add(LowerBack);
            Bones.Add(Spine);
            Bones.Add(RightShoulder);
            Bones.Add(RightUpperArm);
            Bones.Add(RightForeArm);
            Bones.Add(RightHand);
            Bones.Add(RightHip);
            //Bones.Add(RightThigh);
            //Bones.Add(RightShin);
            //Bones.Add(RightFoot);
            Bones.Add(LeftShoulder);
            Bones.Add(LeftUpperArm);
            Bones.Add(LeftForeArm);
            Bones.Add(LeftHand);
            Bones.Add(LeftHip);
            //Bones.Add(LeftThigh);
            //Bones.Add(LeftShin);
            //Bones.Add(LeftFoot);

            return Bones;
        }

        // This function returns a list of additional boxes (rectangle shapes)
        public List<Rectangle> getAdditional() {

            List<Rectangle> Additional = new List<Rectangle>();
            Additional.Add(recSelected);
            Additional.Add(recHover);

            return Additional;
        }

        // Person constructor
        public Person(int id) {

            SolidColorBrush jointColorBrush = new SolidColorBrush();            // Sets joint colour    
            SolidColorBrush boneColorBrush = new SolidColorBrush();             // Sets bone colour     

            // This switches the joint colour for each different skeleton
            switch (id) {
                case 0:
                    jointColorBrush.Color = Colors.Blue;
                    boneColorBrush.Color = Colors.Blue;
                    break;
                case 1:
                    jointColorBrush.Color = Colors.Green;
                    boneColorBrush.Color = Colors.Green;
                    break;
                case 2:
                    jointColorBrush.Color = Colors.Yellow;
                    boneColorBrush.Color = Colors.Yellow;
                    break;
                case 3:
                    jointColorBrush.Color = Colors.Orange;
                    boneColorBrush.Color = Colors.Orange;
                    break;
                case 4:
                    jointColorBrush.Color = Colors.Pink;
                    boneColorBrush.Color = Colors.Pink;
                    break;
                case 5:
                    jointColorBrush.Color = Colors.Brown;
                    boneColorBrush.Color = Colors.Brown;
                    break;
            }

            // Loop through the list of joints
            List<Ellipse> joints = this.getJoints();
            for (int i = 0; i < joints.Count; i++) {

                joints[i].Fill = jointColorBrush;               // Colours the joint        
                joints[i].Width = JOINT_HEIGHT;                 // Sets the joint height 
                joints[i].Height = JOINT_WIDTH;                 // Sets the joint width 
            }

            // Loop through the list of bones
            List<Line> bones = this.getBones();
            for (int i = 0; i < bones.Count; i++) {

                bones[i].StrokeThickness = 5;                   // Sets the bone thickness
                bones[i].Stroke = boneColorBrush;               // Colours the bone
            }

            // Additional 
            recHover.Fill = boneColorBrush;
            recHover.Opacity = 0.4;

            recSelected.Fill = jointColorBrush;
            recSelected.Opacity = 0.4;
        }

        // This function draws all of the shapes that represents the human skeleton
        public void DrawPerson() {

            // Draws all of the ellipse shapes that represent the joints
            DrawJoint(this.Head, this.jHead.X, this.jHead.Y);
            DrawJoint(this.RightElbow, this.jElbowRight.X, this.jElbowRight.Y);
            DrawJoint(this.LeftPalm, this.jHandLeft.X, this.jHandLeft.Y);
            DrawJoint(this.LeftElbow, this.jElbowLeft.X, this.jElbowLeft.Y);
            DrawJoint(this.Body, this.jSpine.X, this.jSpine.Y);
            DrawJoint(this.RightPalm, this.jHandRight.X, this.jHandRight.Y);
            DrawJoint(this.HipLeft, this.jHipLeft.X, this.jHipLeft.Y);
            DrawJoint(this.HipRight, this.jHipRight.X, this.jHipRight.Y);
            //DrawJoint(this.AnkleLeft, this.jAnkleLeft.X, this.jAnkleLeft.Y);
            DrawJoint(this.WristRight, this.jWristRight.X, this.jWristRight.Y);
            DrawJoint(this.WristLeft, this.jWristLeft.X, this.jWristLeft.Y);
            DrawJoint(this.ShoulderRight, this.jShoulderRight.X, this.jShoulderRight.Y);
            DrawJoint(this.ShoulderLeft, this.jShoulderLeft.X, this.jShoulderLeft.Y);
            DrawJoint(this.ShoulderCenter, this.jShoulderCenter.X, this.jShoulderCenter.Y);
            //DrawJoint(this.KneeLeft, this.jKneeLeft.X, this.jKneeLeft.Y);
            //DrawJoint(this.KneeRight, this.jKneeRight.X, this.jKneeRight.Y);
            //DrawJoint(this.FootRight, this.jFootRight.X, this.jFootRight.Y);
            DrawJoint(this.HipCenter, this.jHipCenter.X, this.jHipCenter.Y);
            //DrawJoint(this.FootLeft, this.jFootLeft.X, this.jFootLeft.Y);
            //DrawJoint(this.AnkleRight, this.jAnkleRight.X, this.jAnkleRight.Y);

            // Draws all of the line shapes that represent the bones
            DrawBone(this.Neck, this.jHead.X, this.jHead.Y, this.jShoulderCenter.X, this.jShoulderCenter.Y);
            DrawBone(this.LowerBack, this.jSpine.X, this.jSpine.Y, this.jHipCenter.X, this.jHipCenter.Y);
            DrawBone(this.Spine, this.jShoulderCenter.X, this.jShoulderCenter.Y, this.jSpine.X, this.jSpine.Y);
            DrawBone(this.RightShoulder, this.jShoulderCenter.X, this.jShoulderCenter.Y, this.jShoulderRight.X, this.jShoulderRight.Y);
            DrawBone(this.RightUpperArm, this.jShoulderRight.X, this.jShoulderRight.Y, this.jElbowRight.X, this.jElbowRight.Y);
            DrawBone(this.RightForeArm, this.jElbowRight.X, this.jElbowRight.Y, this.jWristRight.X, this.jWristRight.Y);
            DrawBone(this.RightHand, this.jWristRight.X, this.jWristRight.Y, this.jHandRight.X, this.jHandRight.Y);
            DrawBone(this.RightHip, this.jHipCenter.X, this.jHipCenter.Y, this.jHipRight.X, this.jHipRight.Y);
            //DrawBone(this.RightThigh, this.jHipRight.X, this.jHipRight.Y, this.jKneeRight.X, this.jKneeRight.Y);
            //DrawBone(this.RightShin, this.jKneeRight.X, this.jKneeRight.Y, this.jAnkleRight.X, this.jAnkleRight.Y);
            //DrawBone(this.RightFoot, this.jAnkleRight.X, this.jAnkleRight.Y, this.jFootRight.X, this.jFootRight.Y);
            DrawBone(this.LeftShoulder, this.jShoulderCenter.X, this.jShoulderCenter.Y, this.jShoulderLeft.X, this.jShoulderLeft.Y);
            DrawBone(this.LeftUpperArm, this.jShoulderLeft.X, this.jShoulderLeft.Y, this.jElbowLeft.X, this.jElbowLeft.Y);
            DrawBone(this.LeftForeArm, this.jElbowLeft.X, this.jElbowLeft.Y, this.jWristLeft.X, this.jWristLeft.Y);
            DrawBone(this.LeftHand, this.jWristLeft.X, this.jWristLeft.Y, this.jHandLeft.X, this.jHandLeft.Y);
            DrawBone(this.LeftHip, this.jHipCenter.X, this.jHipCenter.Y, this.jHipLeft.X, this.jHipLeft.Y);
            //DrawBone(this.LeftThigh, this.jHipLeft.X, this.jHipLeft.Y, this.jKneeLeft.X, this.jKneeLeft.Y);
            //DrawBone(this.LeftShin, this.jKneeLeft.X, this.jKneeLeft.Y, this.jAnkleLeft.X, this.jAnkleLeft.Y);
            //DrawBone(this.LeftFoot, this.jAnkleLeft.X, this.jAnkleLeft.Y, this.jFootLeft.X, this.jFootLeft.Y);
        }

        // This functions draws a single bone
        private void DrawBone(Line bone, int X1, int Y1, int X2, int Y2) {
            bone.X1 = X1;
            bone.Y1 = Y1;
            bone.X2 = X2;
            bone.Y2 = Y2;
        }

        // This function draws a single joint
        private void DrawJoint(Ellipse joint, int X, int Y) {
            joint.Margin = new Thickness(X - joint.Width / 2, Y - joint.Height / 2, 0, 0);
        }
    }
}