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
    private int emitObjectAge(object obj)
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
    private void emitAndCollectNursery()
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
    private void emitAndCollectOldGen()
    {
        Console.WriteLine("collection old gen");
        GC.Collect(1);
    }

    /*
     *
     * Check age assertion
     * 
     */
    private int checkAge(object obj,Func<int,bool> pred)
    {
        var age = emitObjectAge(obj);
        
        var str = string.Format("Garbage collection is not incrementing object {3} age: age {0} collectcount nurse {1} old gen {2}",
                age,
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                obj.GetHashCode()
                );

        Assert.IsTrue(pred(age),str);
        return age;
    }

    private void runAgeChecksNursery(object obj)
    {

        emitAndCollectNursery();

        var prevAge = checkAge(obj,i=>i > 0);

        emitAndCollectNursery();

        prevAge = checkAge(obj,i=>i > prevAge);
        
        emitAndCollectNursery();

        prevAge = checkAge(obj,i=>i > prevAge);

    }
    
    [
    private void runAgeChecksOldGen(object obj)
    {

        emitAndCollectOldGen();

        var prevAge = checkAge(obj,i=>i > 0);

        emitAndCollectOldGen();

        prevAge = checkAge(obj,i=>i > prevAge);
        
        emitAndCollectOldGen();

        prevAge = checkAge(obj,i=>i > prevAge);

    }

	[Test]
    [Ignore]
	public void TestGetObjectAgeOnSimpleObject() {
        Object obj = new Object();

        Assert.IsTrue(GC.GetObjectAge(null) == 0, "Null does not return valid age");

        runAgeChecksNursery(obj);
	}
	
    [Test]
    [Ignore]
	public void TestGetObjectAgeOnString() {
        string obj = "hello world from nebraska";

        runAgeChecksOldGen(obj);
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
        return 640L;
    }


    bool CreateObjectsOutOfScope(){

        var upperLimit = ((NURSERY_SIZE/getObjectSize(new BigObject()))*2);
        Console.WriteLine(string.Format("creating objects out of scope with upper limit of {0}",upperLimit));
        var list = new List<BigObject>();
        for(var i = 0;i< upperLimit;i++){
            var obj = new BigObject();
            list.Add(obj);
            GC.GetObjectAge(obj);
        }
        emitAndCollectNursery();
        foreach (var i in list)
        {
            GC.GetObjectAge(i);
        }
        return true;
    }

    [Test]
    public void TestGetObjectAgeMoveFromNursery()
    {
        Console.WriteLine("Running get object age move from nursery.");
        //initialy create the monster object
        var bigObject = new BigObject();
        BigObject nextBigObject;

        emitAndCollectNursery();
        Console.WriteLine("age: " + GC.GetObjectAge(bigObject));

        runAgeChecksNursery(bigObject);
        //fill up the memory to force a movement of objects
        //this should force a clean
        if(CreateObjectsOutOfScope())
        {
            nextBigObject = new BigObject();
        }
        emitAndCollectNursery();
    }
}
}
