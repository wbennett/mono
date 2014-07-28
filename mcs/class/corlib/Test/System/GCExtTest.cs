// GCExtTest.cs - NUnit Test Cases for the System.Object struct
//
// Paul Bennett (wm.paul.bennett@gmail.com)
// 

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MonoTests.System
{
[Serializable]
public class BigObject
{
    decimal @a1;
    decimal @a11;
    decimal @a111;
    decimal @a1111;
    decimal @a11111;
    decimal @a111111;
    decimal @a1111111;
    decimal @a11111111;
    decimal @a111111111;
    decimal @a1111111111;
    decimal @a11111111111;
    decimal @a111111111111;
    decimal @a1111111111111;
    decimal @a11111111111111;
    decimal @a111111111111111;
    decimal @a1111111111111111;
    decimal @a11111111111111111;
    decimal @a111111111111111111;
    decimal @a1111111111111111111;
    decimal @a11111111111111111111;
    decimal @a111111111111111111111;
    decimal @a1111111111111111111111;
    decimal @a11111111111111111111111;
    decimal @a111111111111111111111111;
    decimal @a1111111111111111111111111;
    decimal @a11111111111111111111111111;
    decimal @a111111111111111111111111111;
    decimal @a11111111111111111111111111111;
    decimal @a111111111111111111111111111111;
    decimal @a1111111111111111111111111111111;
    decimal @a11111111111111111111111111111111;
    decimal @a2;
    decimal @a22;
    decimal @a222;
    decimal @a2222;
    decimal @a22222;
    decimal @a222222;
    decimal @a2222222;
    decimal @a22222222;
    decimal @a222222222;
    decimal @a2222222222;
    decimal @a22222222222;
    decimal @a222222222222;
    decimal @a2222222222222;
    decimal @a22222222222222;
    decimal @a222222222222222;
    decimal @a2222222222222222;
    decimal @a22222222222222222;
    decimal @a222222222222222222;
    decimal @a2222222222222222222;
    decimal @a22222222222222222222;
    decimal @a222222222222222222222;
    decimal @a2222222222222222222222;
    decimal @a22222222222222222222222;
    decimal @a222222222222222222222222;
    decimal @a2222222222222222222222222;
    decimal @a22222222222222222222222222;
    decimal @a222222222222222222222222222;
    decimal @a22222222222222222222222222222;
    decimal @a222222222222222222222222222222;
    decimal @a2222222222222222222222222222222;
    decimal @a22222222222222222222222222222222;
    decimal @a3;
    decimal @a33;
    decimal @a333;
    decimal @a3333;
    decimal @a33333;
    decimal @a333333;
    decimal @a3333333;
    decimal @a33333333;
    decimal @a333333333;
    decimal @a3333333333;
    decimal @a33333333333;
    decimal @a333333333333;
    decimal @a3333333333333;
    decimal @a33333333333333;
    decimal @a333333333333333;
    decimal @a3333333333333333;
    decimal @a33333333333333333;
    decimal @a333333333333333333;
    decimal @a3333333333333333333;
    decimal @a33333333333333333333;
    decimal @a333333333333333333333;
    decimal @a3333333333333333333333;
    decimal @a33333333333333333333333;
    decimal @a333333333333333333333333;
    decimal @a3333333333333333333333333;
    decimal @a33333333333333333333333333;
    decimal @a333333333333333333333333333;
    decimal @a33333333333333333333333333333;
    decimal @a333333333333333333333333333333;
    decimal @a3333333333333333333333333333333;
    decimal @a33333333333333333333333333333333;
}

public class TestObj
{
    public TestObj()
    {
    }
}

public class ObjectTestState
{
    public int PrevObjectAge {get;set;}
    public object Object {get;set;}
}

[TestFixture]
public class GCExtTest
{


    /*
     *
     * Collects nursery
     */
    void collectNursery()
    {
        Console.WriteLine("collecting nursery");
        //collect on the nursery
        GC.Collect(0);
    }

    /*
     *
     * Collects the older store
     *
     */
    void collectOldGen()
    {
        Console.WriteLine("collection old gen");
        GC.Collect(1);
    }

    /*
     *
     * Check age assertion
     * 
     */
    int checkAge(object obj,Func<int,bool> pred)
    {
        var age = GC.GetObjectAge(obj);
        
        var str = string.Format("Garbage collection is not incrementing object {3} age: age {0} collectcount nurse {1} old gen {2}",
                age,
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                obj.GetHashCode()
                );

        Assert.IsTrue(pred(age),str);
        return age;
    }


    const int LARGE_OBJ_SIZE = 8000;
    const int NURSERY_SIZE = 4194304;


    bool CreateObjectsOutOfScope(){

        var runAssertions = true;
        var g0Start = GC.CollectionCount(0);
        var g1Start = GC.CollectionCount(1);
        var upperLimit = 7;
        Console.WriteLine(string.Format("creating objects out of scope with upper limit of {0}",upperLimit));
        var list = new List<ObjectTestState>();
        for(var i = 0;i< upperLimit;i++){
            var obj = new BigObject();
            var age = GC.GetObjectAge(obj);
            if(runAssertions)
                Assert.IsTrue(age == 0,"Age should be zero at this point");

            list.Add(new ObjectTestState(){
                    PrevObjectAge = age,
                    Object = obj
                    });

        }


        for(var i = 0;i<4;i++)
        {
            //the age should be greater after collecting
            collectNursery();
            collectOldGen();
            collectNursery();
            collectOldGen();
            foreach (var el in list)
            {
                var obj = el.Object;
                var age = GC.GetObjectAge(obj);

                Console.WriteLine(
                        "obj: " + obj.GetHashCode()+
                        " agep: "+el.PrevObjectAge+
                        " age: "+age + 
                        " young: " + GC.CollectionCount(0) +  
                        " old: " + GC.CollectionCount(1));
                if(!runAssertions)
                    continue;

                Assert.IsTrue(age > el.PrevObjectAge,"age should greater than previous age");

                el.PrevObjectAge = age;
            }
        }

        return true;
    }
	
    [Test]
	public void TestGetObjectAgeOnSimpleObject() {
        Object obj = new TestObj();
        var prev = GC.GetObjectAge(obj);
        Assert.IsTrue(GC.GetObjectAge(null) == -1, "Null does not return valid age");
        Assert.IsTrue(GC.GetObjectAge(obj) == prev,"Expecting age to not have changed but it did",GC.GetObjectAge(obj));
        GC.Collect(1);
        Assert.IsTrue(GC.GetObjectAge(obj) == prev,"Expecting age to not have changed but it did",GC.GetObjectAge(obj));
        GC.Collect(0);
        Console.WriteLine("simple object collection count {0}",GC.CollectionCount(0));
        if(GC.CollectionCount(0) > 0)
            Assert.IsTrue(GC.GetObjectAge(obj) > prev,"Expecting age to be one but got {0}",GC.GetObjectAge(obj));
	}

    [Test]
	public void TestGetObjectAgeOnString() {
        string obj = "hello world from nebraska";
        var prev = GC.GetObjectAge(obj);
        GC.Collect(0);
        Assert.IsTrue(GC.GetObjectAge(obj) == prev,"string should not be in nursery");
        GC.Collect(1);
        Assert.IsTrue(GC.GetObjectAge(obj) != prev,"string should be checked during major collection");
	}

    [Test]
    public void TestGetObjectAgeMoveFromNursery()
    {
        Console.WriteLine("Running get object age move from nursery.");
        //initialy create the monster object
        var bigObject = new BigObject();
        BigObject nextBigObject;

        collectNursery();
        collectNursery();
        Console.WriteLine("age: " + GC.GetObjectAge(bigObject));
        //fill up the memory to force a movement of objects
        //this should force a clean
        if(CreateObjectsOutOfScope())
        {
            nextBigObject = new BigObject();
        }
        collectNursery();
    }
}
}
