using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeapTouchPoint;
using Leap;

namespace LeapTouchPointTest
{
    [TestClass]
    public class LeapListenerTest
    {
        LeapListener TestListener = new LeapListener();

        [TestMethod]
        public void check_get_action_type_notnull()
        {
            Assert.IsNotNull(TestListener.get_action_type());
        }

        [TestMethod]
        public void check_get_action_type_isnormal()
        {
            Assert.AreEqual("normal",TestListener.get_action_type(),"get_action_type() is not normal at first run");
        }

        //Test set_action_type()
        [TestMethod]
        public void check_set_action_type_toleftclick()
        {
            TestListener.set_action_type("left_click");

            Assert.AreEqual("left_click",TestListener.get_action_type(),"set_action_type() can't return left click");
        }

        [TestMethod]
        public void check_set_action_type_torightclick()
        {
            TestListener.set_action_type("right_click");

            Assert.AreEqual("right_click", TestListener.get_action_type(), "set_action_type() can't return right click");
        }

        [TestMethod]
        public void check_set_action_type_backtonormal()
        {
            TestListener.set_action_type("normal");

            Assert.AreEqual("normal", TestListener.get_action_type(), "set_action_type() can't return back to normal");
        }
        //End Test set_action_type()

        //Test IsAMatch()
        [TestMethod]
        public void check_IsAMatch_matcheswithinparameters()
        {
            Vector newposition = new Vector(100,100,0);
            Vector originalposition = new Vector(200,200,0);

            Boolean isamatch = TestListener.IsAMatch(newposition,originalposition);

            Assert.AreEqual(true, isamatch, "IsAMatch does not match within parameters");
        }

        [TestMethod]
        public void check_IsAMatch_failsifxvalueoutsideparameters()
        {
            Vector newposition = new Vector(100, 100, 0);
            Vector originalposition = new Vector(201, 200, 0);

            Boolean isamatch = TestListener.IsAMatch(newposition, originalposition);

            Assert.AreEqual(false, isamatch, "IsAMatch does not fail when x value outside parameter");
        }

        [TestMethod]
        public void check_IsAMatch_failsifyvalueoutsideparameters()
        {
            Vector newposition = new Vector(100, 100, 0);
            Vector originalposition = new Vector(200, 201, 0);

            Boolean isamatch = TestListener.IsAMatch(newposition, originalposition);

            Assert.AreEqual(false, isamatch, "IsAMatch does not fail when y value outside parameter");
        }
        //END Test IsAMatch()

        [TestMethod]
        public void check_getStabilizedVector_returnsaveragevector()
        {
            Vector lastvector = TestListener.get_last_position();
            Vector currentvector = new Vector(200, 200, 0);
            float velocity = 7;

            Vector stabilizedvector = TestListener.getStabilizedVector(currentvector, velocity);

            Assert.AreEqual((currentvector.x+lastvector.x)/2,stabilizedvector.x);
        }

        [TestMethod]
        public void check_getFingerVector_returncorrectfingervector()
        {
            Vector finger_vector = new Vector();

            Vector lastvector = TestListener.get_last_position();
            Vector currentvector = new Vector(200, 200, 0);
            float velocity = 7;

            Vector stabilizedvector = TestListener.getStabilizedVector(currentvector, velocity);

            Assert.AreEqual((currentvector.x + lastvector.x) / 2, stabilizedvector.x);
        }


    }
}
