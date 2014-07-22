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

[TestFixture]
public class GCExtTest
{


    /*
     * Emits object age including the collection counts.
     *
     */
    int objectAge(object obj)
    {
        var age = GC.GetObjectAge(obj);
        Console.WriteLine("object {3} age: {0} nursery_age: {1} old_gen_age: {2}",
                    age,
                    GC.CollectionCount(0),
                    GC.CollectionCount(1),
                    obj.GetHashCode()
                    );
        return age;
    }

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
        var age = objectAge(obj);
        
        var str = string.Format("Garbage collection is not incrementing object {3} age: age {0} collectcount nurse {1} old gen {2}",
                age,
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                obj.GetHashCode()
                );

        Assert.IsTrue(pred(age),str);
        return age;
    }

    void runAgeChecksNursery(object obj)
    {

        collectNursery();

        var prevAge = checkAge(obj,i=>i > 0);

        collectNursery();

        prevAge = checkAge(obj,i=>i > prevAge);
        
        collectNursery();

        prevAge = checkAge(obj,i=>i > prevAge);

    }
    
    void runAgeChecksOldGen(object obj)
    {

        collectOldGen();

        var prevAge = checkAge(obj,i=>i > 0);

        collectOldGen();

        prevAge = checkAge(obj,i=>i > prevAge);
        
        collectOldGen();

        prevAge = checkAge(obj,i=>i > prevAge);

    }


    const int LARGE_OBJ_SIZE = 8000;
    const int NURSERY_SIZE = 4194304;

    long getObjectSize(object obj)
    {
        long size = 0;
        using(var s = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(s,obj);
            size = s.Length;
        }
        Console.WriteLine("object size " + size);
        return size;
    }


    bool CreateObjectsOutOfScope(){

        var upperLimit = ((NURSERY_SIZE/getObjectSize(new BigObject())));
        Console.WriteLine(string.Format("creating objects out of scope with upper limit of {0}",upperLimit));
        var list = new List<Tuple<BigObject,int>>();
        for(var i = 0;i< upperLimit;i++){
            var obj = new BigObject();
            var age = GC.GetObjectAge(obj);

            Console.WriteLine("obj: "+obj.GetHashCode());
            var tup = new Tuple<BigObject,int>(obj,age);
            list.Add(tup);

        }
        //the age should be greater after collecting
        collectNursery();
        Console.WriteLine("count: "+ list.Count());
        foreach (var i in list)
        {
            BigObject obj = i.Item1;
            //var age = GC.GetObjectAge(obj);
            var age = -1;
            Console.WriteLine(
                    "obj: " + obj.GetHashCode()+
                    " agep: "+i.Item2+
                    " age: "+age + 
                    " young: " + GC.CollectionCount(0) +  
                    " old: " + GC.CollectionCount(1));
        }
        return true;
    }
	
    [Test]
	public void TestGetObjectAgeOnSimpleObject() {
        Object obj = new Object();

        Assert.IsTrue(GC.GetObjectAge(null) == 0, "Null does not return valid age");

        runAgeChecksNursery(obj);
	}
	
    [Test]
	public void TestGetObjectAgeOnString() {
        string obj = "hello world from nebraska";

        runAgeChecksOldGen(obj);
	}

    [Test]
    public void TestGetObjectAgeMoveFromNursery()
    {
        Console.WriteLine("Running get object age move from nursery.");
        //initialy create the monster object
        var bigObject = new BigObject();
        BigObject nextBigObject;

        collectNursery();
        Console.WriteLine("age: " + GC.GetObjectAge(bigObject));

        runAgeChecksNursery(bigObject);
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
