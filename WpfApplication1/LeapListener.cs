/******************************************************************************\
* Copyright (C) 2012-2013 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/
using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Leap;



namespace LeapTouchPoint
{

    public class LeapListener : Listener
    {
        //Constant config variables, these are updated at startup using the method setConfigFileParameters()
        
        private ConfigFileSettings configFile = new ConfigFileSettings();
        static Dictionary<string, string> configDic = new Dictionary<string,string>();
        static double yAxisMultiplier = 1;
        static double fineSensitivityMultiplier = 1;
        static int secondsBeforeLocking = 5;
        static int number_of_frames_before_locking = 100;

        //Static Variables
        public static Vector last_position = Vector.Zero;
        public static Vector last_position_2 = Vector.Zero;
        public static Vector last_position_3 = Vector.Zero;
        private static Frame frame = new Frame();
        public static long last_frame_Id = 0;
        private static Gesture gesture_detected = null;
        private static long unlock_frame_id = 0;
        public static Boolean lock_status = false;
        private static string action_type = "normal";
        


        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");

            // enable gesture recognition for screen locking
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            //controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            //controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            //controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);
        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            frame = controller.Frame();

            SafeWriteLine("Frame id: " + frame.Id
                        + ", timestamp: " + frame.Timestamp
                        + ", hands: " + frame.Hands.Count
                        + ", fingers: " + frame.Fingers.Count);
//                        + ", tools: " + frame.Tools.Count
//                       + ", gestures: " + frame.Gestures().Count);

            if (!frame.Fingers.Empty)
            {
            //detect any gestures
            GestureList gestures = frame.Gestures ();

            for (int i = 0; i < gestures.Count; i++)
            {
                Gesture gesture = gestures[i];

                switch (gesture.Type)
                {
                    case Gesture.GestureType.TYPECIRCLE:
                        gesture_detected = gesture;
                        break;
                }
            }
            //END detect Gestures

                // Check if the hand has any fingers
                FingerList fingers = frame.Fingers;
                if (!fingers.Empty)
                {
                    // Calculate the average finger tip position
                    Vector finger_Pos = Vector.Zero;
                    
                    finger_Pos = getFingerVector(controller,fingers.Rightmost);
                    
//                    SafeWriteLine("Device sees " + fingers.Count
//                                + " finger(s), cursor finger position: " + finger_Pos);
                }
            }
            if (!frame.Hands.Empty || !frame.Gestures().Empty)
            {
                //SafeWriteLine("");
            }

        }

        /********************************* Custom Methods ***********************************/

        public void setConfigFileParameters()
        {
            configDic = configFile.getDefaultConfigParameters();

            yAxisMultiplier = double.Parse(configDic["yAxisSlider"]);
            fineSensitivityMultiplier = double.Parse(configDic["fineSensitivitySlider"]);
            secondsBeforeLocking = int.Parse(configDic["secondsBeforeLocking"]);
            number_of_frames_before_locking = (secondsBeforeLocking * 20);
        }

        public void set_action_type(string action)
        {
            action_type = action;
        }

        public string get_action_type()
        {
            return action_type;
        }

        public void set_last_position(Vector position)
        {
            last_position = position;
        }

        public Vector get_last_position()
        {
            return last_position;
        }

        public void LeapMoveMouse(int x, int y)
        {
            MouseInput.MoveMouse(new System.Drawing.Point(x, y));
        }

        public Boolean IsAMatch(Vector new_position,Vector last_known_position)
        {
            Boolean is_a_match = false;

            if ((Math.Abs(new_position.x - last_known_position.x) <= 100) && (Math.Abs(new_position.y - last_known_position.y) <= 100))
            {
                is_a_match = true;
            }

            return is_a_match;
        }

        public Vector getStabilizedVector(Vector cursor_position, float cursor_velocity)
        {
            if (cursor_position.y < 0)
            {
                cursor_position.y = 0;
            } 
            
            if (last_position.x != 0 || last_position.y != 0)
            {
                if (IsAMatch(cursor_position, last_position))
                {


                    if (cursor_velocity <= fineSensitivityMultiplier)
                    {
                        //cursor_position.x = (cursor_position.x + last_position.x + last_position_2.x + last_position_3.x) / 4;
                        //cursor_position.y = (cursor_position.y + last_position.y + last_position_2.y + last_position_3.y) / 4;
                        cursor_position.x = last_position.x;
                        cursor_position.y = last_position.y;
                    }
                    else
                    {
                        //cursor_position.x = (cursor_position.x + last_position.x) / 2;
                        //cursor_position.y = (cursor_position.y + last_position.y) / 2;
                    }
                }

            }

            //SafeWriteLine("Cursor finger velocity: " + cursor_velocity + ", x: " + cursor_position.x + ", y: " + cursor_position.y);

                return cursor_position;
        }

        public Vector getFingerVector(Controller controller, Finger finger)
        {
            Vector finger_vector = new Vector();
            
            // Get the closest screen intercepting a ray projecting from the finger
            Leap.Screen screen = controller.CalibratedScreens.ClosestScreenHit(finger);


            if (screen != null && screen.IsValid)
            {
                Vector screenStabilized = screen.Intersect(finger, true);
                var xScreenIntersect = screenStabilized.x;
                var yScreenIntersect = screenStabilized.y;
                
                if (xScreenIntersect.ToString() != "NaN")
                {
                    finger_vector.x = (int)(xScreenIntersect * screen.WidthPixels);
                    finger_vector.y = (int)(screen.HeightPixels - (yScreenIntersect * screen.HeightPixels * yAxisMultiplier));
                }
                //SafeWriteLine("yScreenIntersect / HeightPixels" + "/" + yScreenIntersect + "/" + screen.HeightPixels);
            }

            return finger_vector;
        }

        public ArrayList leftHandFingers(FingerList fingers, Finger cursor_finger)
        {
            ArrayList fingers_on_left_hand = new ArrayList();

            foreach(Finger finger in fingers)
            {
                if (!finger.Equals(cursor_finger))
                {
                    float x_diff = cursor_finger.TipPosition.x - finger.TipPosition.x;
                    if (x_diff > 5)
                    {
                        fingers_on_left_hand.Add(finger);
                    }
                }
            }

            return fingers_on_left_hand;
        }

        public void gestureConfigCircle(Controller controller)
        {
            //controller.Config.SetFloat("Gesture.Circle.")
        }
        
        // Function that detects the gesture to lock or unlock screen cursor using Leap
        public Boolean lockOrUnlock()
        {

            if(gesture_detected != null)
            {
                CircleGesture circle = new CircleGesture(gesture_detected);

                //circle.Progress indicates number of times circle is detected in a row
                if (circle.Progress >= 1 && circle.DurationSeconds <= 0.5)
                {
                    // Calculate clock direction using the angle between circle normal and pointable
                    if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4)
                    {
                        //Clockwise to unlock
                            lock_status = false;
                    }
                    else
                    {
                        //anti-clockwise to lock
                        lock_status = true;
                    }
                }

                gesture_detected = null;
            }

            //Lock the screen if it exceeds configured number of frames
            lockScreenAfterTimeout();

            return lock_status;
        }

        //Lock the screen if duration exceeds configured number of frames (check number_of_frames_before_locking)
        private void lockScreenAfterTimeout()
        {
            if (unlock_frame_id == 0 || frame.Fingers.Count > 0)
            {
                // Update unlock_frame_id during first unlock or whenever fingers are detected
                unlock_frame_id = frame.Id;
            }
            //Lock screen if duration exceeded
            else if ((frame.Id - unlock_frame_id) > number_of_frames_before_locking)
            {
                lock_status = true;
                unlock_frame_id = 0;
            }
        }

        //WORK IN PROGRESS//

        public Boolean scrollUpDown(Controller controller)
        {
            Boolean return_status = false;
            Frame frame = controller.Frame();

            HandList hands = frame.Hands;
            if (hands.Count == 1)
            {
                foreach(Hand hand in hands)
                {
                    if (hand.Fingers.Count > 3)
                    {
                        if (hand.PalmVelocity.y != 0)
                        {
                            MouseInput.ScrollUpDown(hand.PalmVelocity.y);
                            return_status = true;
                        }
                    }
                }
            }

            return return_status;

        }
        //WORK IN PROGRESS//

        public Vector trackLeapCursor(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();
            Vector cursor_position = Vector.Zero;

            //SafeWriteLine("last_frame_id_before: " + last_frame_Id);
            if (frame.Hands.Count > 0)
            {
                scrollUpDown(controller);
            } else
            {
                        

                // Check if the hand has any fingers
                FingerList fingers = frame.Fingers;
                if (!fingers.Empty)
                {
                    Finger right_finger = fingers.Rightmost;
                    Vector right_finger_position = getFingerVector(controller, right_finger);
                    float right_finger_velocity = right_finger.TipVelocity.Magnitude;

                    Boolean is_matched = IsAMatch(right_finger_position, last_position);

                    //SafeWriteLine("is_Matched? = " + is_matched);

                    //SafeWriteLine("Frame id: " + frame.Id + ", is_matched: "+ is_matched +", timestamp: " + frame.Timestamp + ", hands: " + frame.Hands.Count + ", fingers: " + frame.Fingers.Count);

                    //Set cursor_position
                    if (last_position.x == 0 && last_position.y == 0)
                    {
                        cursor_position = right_finger_position;
                    }
                    else if (is_matched)
                    {
                        cursor_position = right_finger_position;
                    }
                        /*
                     else if(get_action_type() == "left_click" || get_action_type() == "right_click")
                    {
                        cursor_position = last_position;
                    }
                         */


                    cursor_position = getStabilizedVector(cursor_position, right_finger_velocity);

                    int LeapX = 0;
                    int LeapY = 0;

                    //                            LeapX = (int)cursor_position.x + 30;
                    //                            LeapY = (int)cursor_position.y + 20;

                    LeapX = (int)cursor_position.x;
                    LeapY = (int)cursor_position.y;

                        if (fingers.Count > 1 && is_matched)
                        {

                            ArrayList triggerFingers = leftHandFingers(frame.Fingers, right_finger);
                            //cursor_position = getFingerVector(controller,fingers.Rightmost);

                            //cursor_position = fingers.Rightmost.TipPosition;
                            //SafeWriteLine("Frame id: " + frame.Id + ", last_frame_id:" + last_frame_Id);

                            //if (frame.Id - last_frame_Id > 10)
                            if (frame.Id - last_frame_Id >= 10)
                            {


                                switch (triggerFingers.Count)
                                {
                                    case 1:

                                        if (get_action_type() == "normal")
                                        {
                                            foreach (Finger finger in triggerFingers)
                                            {
                                                //Check Gesture for Left Click
                                                if (CustomGesture.IsLeftFingerFlicked(finger, frame.Id) == "yes")
                                                {
                                                    MouseInput.LeftClick();
                                                    set_action_type("left_click");
                                                    SafeWriteLine("left_click detected. Fingers: " + frame.Fingers.Count);
                                                }
                                                //Check Gesture for Left Drag
                                                else if (CustomGesture.IsLeftFingerDragged(finger, frame.Id) == "yes" && MouseInput.MouseLeftClickStatus() != "Down")
                                                {
                                                    //MouseInput.LeftClickDown(LeapX, LeapY);
                                                    MouseInput.LeftClickDown();
                                                    set_action_type("left_click");
                                                    SafeWriteLine("left_drag detected. Fingers: " + frame.Fingers.Count);
                                                }

                                                //SafeWriteLine("Tip_mag: " + finger.TipVelocity.Magnitude + ", Tip_y: " + finger.TipVelocity.y + ", Tip_z: " + finger.TipVelocity.z + ", Fingers: " + frame.Fingers.Count);



                                            }


                                            //MouseInput.LeftClickDown(LeapX, LeapY);

                                            //set_action_type("left_click");
                                        }

                                        last_frame_Id = frame.Id;
                                        break;

                                    case 2:
                                        //MouseInput.RightClick(LeapX, LeapY);

                                        MouseInput.RightClick();

                                        if (get_action_type() == "normal" || get_action_type() == "left_click")
                                        {
                                            set_action_type("right_click");
                                        }
                                        last_frame_Id = frame.Id;
                                        break;

                                } //End Switch
                            }
                            else if ((frame.Id - last_frame_Id >= 5) && (get_action_type() == "left_click"))
                            {
                                //Hold Mouse left button for Drag & Drop
                                //MouseInput.LeftClickDown(LeapX, LeapY);
                                //set_action_type("left_drag");
                            }//End If Frame >10

                        } // End If finger_count >= 2
                        else if (MouseInput.MouseLeftClickStatus() == "Down")
                        {
                            //Release Mouse left button for Drag & Drop once left hand finger no longer detected
                            MouseInput.LeftClickUp();
                            CustomGesture.StopLeftFingerDrag();

                            SafeWriteLine("left_drag stopped. Fingers: " + frame.Fingers.Count);

                            set_action_type("normal");

                        }
                        else if (frame.Id - last_frame_Id > 10 && get_action_type() != "normal")
                        {
                            set_action_type("normal");

                            if (CustomGesture.LeftFingerDragStatus() == "yes")
                            {
                                CustomGesture.StopLeftFingerDrag();
                                SafeWriteLine("left_drag stopped. Fingers: " + frame.Fingers.Count);
                            }
                        }// End fingers.count > 1



                    last_position = cursor_position;
                    last_position_2 = last_position;
                    last_position_3 = last_position_2;
                }
                else
                {
                    last_position = Vector.Zero;
                    last_position_2 = Vector.Zero;
                    last_position_3 = Vector.Zero; 
                    SafeWriteLine("No Fingers Detected!");

                } // End if Fingers empty

                // SafeWriteLine("last_frame_id_after: " + last_frame_Id);

            } //End ScrollUpDown

            return cursor_position;
        } // End TrackLeapCursor() function

    } // End LeapListener Class

} // End NameSpace LeapTouchPoint
