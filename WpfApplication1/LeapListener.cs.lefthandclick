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
using Leap;



namespace WpfApplication1
{

    class LeapListener : Listener
    {
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
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
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
            Frame frame = controller.Frame();

//            SafeWriteLine("Frame id: " + frame.Id
//                        + ", timestamp: " + frame.Timestamp
//                        + ", hands: " + frame.Hands.Count
//                        + ", fingers: " + frame.Fingers.Count);
//                        + ", tools: " + frame.Tools.Count
//                       + ", gestures: " + frame.Gestures().Count);

            if (!frame.Fingers.Empty)
            {

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
                SafeWriteLine("");
            }
        }

        /********************************* Custom Methods ***********************************/
        public static Vector last_position = Vector.Zero;
        public static long last_frame_Id = 0;
        public static string action_type = "normal";

        public void set_action_type(string action)
        {
            action_type = action;
        }

        public string get_action_type()
        {
            return action_type;
        }

        public Boolean IsAMatch(Vector new_position,Vector last_known_position)
        {
            Boolean is_a_match = false;
            //SafeWriteLine("Inside IsAMatch, new_x - old_x = " + Math.Abs(new_position.x - last_known_position.x) + " & new_y - old_y = " + Math.Abs(new_position.y - last_known_position.y));
            //SafeWriteLine("new_x / old_x = " + new_position.x + "/" + last_known_position.x + " & new_y - old_y = " + new_position.y + "/" + last_known_position.y);

            if ((Math.Abs(new_position.x - last_known_position.x) <= 100) && (Math.Abs(new_position.y - last_known_position.y) <= 100))
            {
                is_a_match = true;
            }

            return is_a_match;
        }

        public Vector getStabilizedVector(Vector cursor_position)
        {

            if (last_position.x != 0 || last_position.y != 0)
            {
                if (IsAMatch(cursor_position, last_position))
                {
                    cursor_position.x = (cursor_position.x + last_position.x) / 2;
                    cursor_position.y = (cursor_position.y + last_position.y) / 2;
                }
            }
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
                    finger_vector.y = (int)(screen.HeightPixels - (yScreenIntersect * 2 * screen.HeightPixels));
                }
                //SafeWriteLine("yScreenIntersect / HeightPixels" + "/" + yScreenIntersect + "/" + screen.HeightPixels);
            }

            return finger_vector;
        }

        public int leftHandFingers(FingerList fingers, Finger cursor_finger)
        {
            int fingers_on_left_hand = 0;

            foreach(Finger finger in fingers)
            {
                if (!finger.Equals(cursor_finger))
                {
                    float x_diff = cursor_finger.TipPosition.x - finger.TipPosition.x;
                    if (x_diff > 5)
                    {
                        fingers_on_left_hand += 1;
                    }
                }
            }

            return fingers_on_left_hand;
        }

        public Vector trackLeapCursor(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();
            Vector cursor_position = Vector.Zero;
            Boolean cursor_is_tapped = false;
            //int framecounter = 0;

            //SafeWriteLine("last_frame_id_before: " + last_frame_Id);

            controller.EnableGesture(Leap.Gesture.GestureType.TYPEKEYTAP);
            


            if (!frame.Fingers.Empty)
            {

                // Check if the hand has any fingers
                FingerList fingers = frame.Fingers;
                if (!fingers.Empty)
                {
                    Vector right_finger_position = getFingerVector(controller, fingers.Rightmost);
                    Boolean is_matched = IsAMatch(right_finger_position, last_position);

                    //SafeWriteLine("is_Matched? = " + is_matched);

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
                     else
                    {
                        cursor_position = last_position;
                    }
                     */

                    cursor_position = getStabilizedVector(cursor_position);

                    int LeapX = 0;
                    int LeapY = 0;

                    //                            LeapX = (int)cursor_position.x + 30;
                    //                            LeapY = (int)cursor_position.y + 20;

                    LeapX = (int)cursor_position.x + 20;
                    LeapY = (int)cursor_position.y + 20;

                    if (fingers.Count > 1 && is_matched)
                    {

            // Get gestures
    		GestureList gestures = frame.Gestures ();

            //Set gesture config settings
            if (
                controller.Config.SetFloat("Gesture.KeyTap.MinDownVelocity", 30.0f)
                && controller.Config.SetFloat("Gesture.KeyTap.HistorySeconds", 0.2f)
                && controller.Config.SetFloat("Gesture.KeyTap.MinDistance", 0.2f)
            )
            {
                controller.Config.Save();
            }

            if (gestures.Count > 0)
            {
                for (int i = 0; i < gestures.Count; i++)
                {
                    Gesture gesture = gestures[i];

                    if (gesture.Pointables.Leftmost.TipPosition.Equals(fingers.Leftmost.TipPosition))
                    {
                        switch (gesture.Type)
                        {
                            case Gesture.GestureType.TYPEKEYTAP:
                                cursor_is_tapped = true;

                                KeyTapGesture keytap = new KeyTapGesture(gesture);
                                SafeWriteLine("Tap id: " + keytap.Id
                                           + ", " + keytap.State
                                           + ", position: " + keytap.Position
                                           + ", direction: " + keytap.Direction);
                                break;
                        }
                    }
                }
            }

                        int triggerFingers = leftHandFingers(frame.Fingers,fingers.Rightmost);
                        //cursor_position = getFingerVector(controller,fingers.Rightmost);

                        //cursor_position = fingers.Rightmost.TipPosition;
                        SafeWriteLine("Frame id: " + frame.Id + ", last_frame_id:" + last_frame_Id);

                        //if (frame.Id - last_frame_Id > 10)
                        if(frame.Id - last_frame_Id >= 1)
                        {


                            switch (triggerFingers)
                            {

                                case 1:
                                    //MouseInput.LeftClickDown(LeapX, LeapY);
                                    if (cursor_is_tapped)
                                    {
                                        MouseInput.LeftClick(LeapX,LeapY);
                                        cursor_is_tapped = false;

                                        if (get_action_type() == "normal")
                                        {
                                            set_action_type("left_click");
                                        }
                                    }

                                    last_frame_Id = frame.Id;
                                    break;

                                case 2:
                                    MouseInput.RightClick(LeapX, LeapY);
                                    if (get_action_type() == "normal" || get_action_type() == "left_click")
                                    {
                                        set_action_type("right_click");
                                    }
                                    last_frame_Id = frame.Id;
                                    break;

                            } //End Switch
                        }//End If Frame >10
                    } else if (MouseInput.MouseLeftClickStatus() == "Down")
                    {
                         MouseInput.LeftClickUp(LeapX,LeapY);

                    }else if(frame.Id - last_frame_Id > 10 && get_action_type() != "normal")
                    {
                        set_action_type("normal");
                    }// End fingers.count > 1

                    last_position = cursor_position;
                }
                else
                {
                    last_position = Vector.Zero;
                } // End if Fingers empty
            }

           // SafeWriteLine("last_frame_id_after: " + last_frame_Id);

            if (cursor_position.y > 735)
                cursor_position.y = 735;

            return cursor_position;
        }

    }

}
/*
class Sample
{
	public static void Main ()
	{
		// Create a sample listener and controller
		LeapListener listener = new LeapListener ();
		Controller controller = new Controller ();

		// Have the sample listener receive events from the controller
		controller.AddListener (listener);

		// Keep this process running until Enter is pressed
		Console.WriteLine ("Press Enter to quit...");
		Console.ReadLine ();

		// Remove the sample listener when done
		controller.RemoveListener (listener);
		controller.Dispose ();
	}
}
*/