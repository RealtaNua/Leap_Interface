using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace WpfApplication1
{
    class CustomGesture : Gesture
    {
        private static string finger_is_flicked = "no";
        private static string finger_is_dragged = "no";
        private static long flick_start_frame_id = 0;
        private static long drag_start_frame_id = 0;
        private static int finger_up_velocity = 800;
        private static int finger_down_velocity = -500;

        private static string get_finger_direction(float vertical_velocity)
        {
            string finger_direction;

            if (vertical_velocity > 0)
            {
                finger_direction = "up";
            }
            else
            {
                finger_direction = "down";
            }

            return finger_direction;
        }

        public static string IsLeftFingerFlicked(Finger finger,long frame_id)
        {
            float finger_velocity = finger.TipVelocity.Magnitude;

            string finger_direction = get_finger_direction(finger.TipVelocity.y);

            if (finger_is_flicked == "yes")
            {
                finger_is_flicked = "no";
            }

            if (finger_direction == "up" && finger_velocity > finger_up_velocity)
            {
                finger_is_flicked = "in_progress";
                flick_start_frame_id = frame_id;
            }
            else if (finger_direction == "down" && (finger.TipVelocity.y < -100) && finger_is_flicked == "in_progress")
            {
                finger_is_flicked = "yes";
            }

            //Reset if taking too long
            if (finger_is_flicked == "in_progress" && (frame_id - flick_start_frame_id > 30))
            {
                finger_is_flicked = "no";
            }

            return finger_is_flicked;
        }

        public static string IsLeftFingerDragged(Finger finger, long frame_id)
        {
            float finger_velocity = finger.TipVelocity.Magnitude;
            string finger_direction = get_finger_direction(finger.TipVelocity.y);

            long time_difference = frame_id - drag_start_frame_id;

            //Console.WriteLine("Before IsLeftFingerDragged, finger_is_dragged: " + finger_is_dragged + ", frame_id: " + frame_id + ",gesture_start_frame_id: " + drag_start_frame_id);

            if (finger_is_dragged == "no" && finger_direction == "up" && finger_velocity > 500)
            {
                finger_is_dragged = "in_progress";
                drag_start_frame_id = frame_id;
                //Console.WriteLine("Start Finger Drag Gesture...");
            }
            else if (finger_is_dragged == "in_progress" && (time_difference > 15))
            {
                finger_is_dragged = "yes";
                //Console.WriteLine("Finger is dragged is yes...");
            }
            else if (finger_is_dragged == "in_progress" && finger_direction == "down" && finger_velocity > finger_down_velocity)
            {
                //finger_is_dragged = "no";
            }

            Console.WriteLine("Inside IsLeftFingerDragged, finger_is_dragged: " + finger_is_dragged + ", frame_id: " + frame_id + ",gesture_start_frame_id: " + drag_start_frame_id + ", difference: "+ (frame_id - drag_start_frame_id));

            return finger_is_dragged;
        }

        public static string LeftFingerDragStatus()
        {
            return finger_is_dragged;
        }

        public static void StopLeftFingerDrag()
        {
            finger_is_dragged = "no";
        }
    }
}
