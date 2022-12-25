using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExtendLib.TypeBuilderHelp;

namespace ExtendLib.Tests
{
    [TestClass()]
    public class ClassExtendTests
    {
        [TestMethod()]
        public void ToDynamicTest()
        {
            {
                var dynamicTyper = new DynaTyper("tmpAsm",Guid.NewGuid().ToString());


                dynamicTyper.InsertField("UserData", typeof(object), new MyTestClass());
                dynamicTyper.InsertInterface(typeof(IHaveUserData));

                var dynamicObj = dynamicTyper.CreateObject();
                Assert.IsTrue(dynamicObj is IHaveUserData);

                var userDataObject = (IHaveUserData)dynamicObj;
                Assert.IsTrue(userDataObject.UserData != null);
                Assert.IsTrue(userDataObject.UserData is MyTestClass);
            }


        }

        public class MyTestClass
        {
            public int a = 1;
            public int b { get; set; } = 1;
        }


        public interface IHaveUserData
        {
            object UserData { get; set; }
        }
    }
}