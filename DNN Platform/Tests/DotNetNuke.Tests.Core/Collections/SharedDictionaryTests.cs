// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using DotNetNuke.Collections.Internal;
using NUnit.Framework;

public abstract class SharedDictionaryTests
{
    public abstract LockingStrategy LockingStrategy { get; }

    [Test]
    public void TryAdd()
    {
        const string KEY = "key";
        const string VALUE = "value";

        var sharedDictionary = new SharedDictionary<string, string>(this.LockingStrategy);

        bool doInsert = false;
        using (ISharedCollectionLock l = sharedDictionary.GetReadLock())
        {
            if (!sharedDictionary.ContainsKey(KEY))
            {
                doInsert = true;
            }
        }

        if (doInsert)
        {
            using (ISharedCollectionLock l = sharedDictionary.GetWriteLock())
            {
                if (!sharedDictionary.ContainsKey(KEY))
                {
                    sharedDictionary.Add(KEY, VALUE);
                }
            }
        }

        Assert.That(sharedDictionary.BackingDictionary, Is.EqualTo(new Dictionary<string, string> { { KEY, VALUE } }).AsCollection);
    }

    [Test]
    [TestCaseSource(nameof(GetWriteMethods))]
    public void WriteRequiresLock(Action<SharedDictionary<string, string>> writeAction)
    {
        Assert.Throws<WriteLockRequiredException>(() => writeAction.Invoke(this.InitSharedDictionary("key", "value")));
    }

    [Test]
    [TestCaseSource(nameof(GetReadMethods))]
    public void ReadRequiresLock(Action<SharedDictionary<string, string>> readAction)
    {
        Assert.Throws<ReadLockRequiredException>(() => readAction.Invoke(this.InitSharedDictionary("key", "value")));
    }

    [Test]
    public void DisposedReadLockDeniesRead()
    {
        var d = new SharedDictionary<string, string>(this.LockingStrategy);

        ISharedCollectionLock l = d.GetReadLock();
        l.Dispose();

        Assert.Throws<ReadLockRequiredException>(() => d.ContainsKey("foo"));
    }

    [Test]
    public void DisposedWriteLockDeniesRead()
    {
        var d = new SharedDictionary<string, string>(this.LockingStrategy);

        ISharedCollectionLock l = d.GetWriteLock();
        l.Dispose();

        Assert.Throws<ReadLockRequiredException>(() => d.ContainsKey("foo"));
    }

    [Test]
    public void DisposedWriteLockDeniesWrite()
    {
        var d = new SharedDictionary<string, string>(this.LockingStrategy);

        ISharedCollectionLock l = d.GetWriteLock();
        l.Dispose();

        Assert.Throws<WriteLockRequiredException>(() => d["ke"] = "foo");
    }

    [Test]
    public void WriteLockEnablesRead()
    {
        var d = this.InitSharedDictionary("key", "value");

        string actualValue = null;
        using (ISharedCollectionLock l = d.GetWriteLock())
        {
            actualValue = d["key"];
        }

        Assert.That(actualValue, Is.EqualTo("value"));
    }

    [Test]
    public void CanGetAnotherLockAfterDisposingLock()
    {
        var d = new SharedDictionary<string, string>(this.LockingStrategy);
        ISharedCollectionLock l = d.GetReadLock();
        l.Dispose();

        l = d.GetReadLock();
        l.Dispose();
    }

    [Test]
    public void DoubleDispose()
    {
        var d = new SharedDictionary<string, string>(this.LockingStrategy);

        d.Dispose();
        d.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(GetObjectDisposedExceptionMethods))]
    public void MethodsThrowAfterDisposed(Action<SharedDictionary<string, string>> methodCall)
    {
        var d = new SharedDictionary<string, string>(this.LockingStrategy);

        d.Dispose();
        Assert.Throws<ObjectDisposedException>(() => methodCall.Invoke(d));
    }

    [Test]
    public void TwoDictsShareALockWriteTest()
    {
        var lockStrategy = LockingStrategyFactory.Create(this.LockingStrategy);
        var dict1 = new SharedDictionary<string, string>(lockStrategy);
        var dict2 = new SharedDictionary<string, string>(lockStrategy);

        using (dict1.GetReadLock())
        {
            ISharedCollectionLock writeLock = null;
            try
            {
                writeLock = dict2.GetWriteLock();
            }
            catch (LockRecursionException)
            {
                Assert.Pass();
            }
            catch (Exception exception)
            {
                Assert.Fail($"Expected LockRecursionException, got {exception}");
            }
            finally
            {
                writeLock?.Dispose();
            }
        }

        Assert.Fail("Expected LockRecursionException, did not throw");
    }

    protected static IEnumerable<Action<SharedDictionary<string, string>>> GetObjectDisposedExceptionMethods()
    {
        var l = new List<Action<SharedDictionary<string, string>>> { (SharedDictionary<string, string> d) => d.GetReadLock(), (SharedDictionary<string, string> d) => d.GetWriteLock() };

        l.AddRange(GetReadMethods());
        l.AddRange(GetWriteMethods());

        return l;
    }

    protected static IEnumerable<Action<SharedDictionary<string, string>>> GetReadMethods()
    {
        var l = new List<Action<SharedDictionary<string, string>>>();

        l.Add(d => d.ContainsKey("key"));
        l.Add(d => d.Contains(new KeyValuePair<string, string>("key", "value")));
        l.Add(d => Console.WriteLine(d.Count));
        l.Add(d => d.GetEnumerator());
        l.Add(d => ((IEnumerable)d).GetEnumerator());
        l.Add(d => Console.WriteLine(d.IsReadOnly));
        l.Add(d => Console.WriteLine(d["key"]));
        l.Add(d => Console.WriteLine(d.Keys));
        l.Add(d => Console.WriteLine(d.Values));

        l.Add(d =>
        {
            var arr = new KeyValuePair<string, string>[1];
            d.CopyTo(arr, 0);
        });

        l.Add((SharedDictionary<string, string> d) =>
        {
            string value = string.Empty;
            d.TryGetValue("key", out value);
        });

        return l;
    }

    protected static IEnumerable<Action<SharedDictionary<string, string>>> GetWriteMethods()
    {
        var l = new List<Action<SharedDictionary<string, string>>>();

        l.Add(d => d.Add("more", "value"));
        l.Add(d => d.Add(new KeyValuePair<string, string>("more", "value")));
        l.Add(d => d.Clear());
        l.Add(d => d.Remove(new KeyValuePair<string, string>("more", "value")));
        l.Add(d => d.Remove("key"));
        l.Add(d => d["key"] = "different");

        return l;
    }

    private SharedDictionary<TKey, TValue> InitSharedDictionary<TKey, TValue>(TKey key, TValue value)
    {
        var sharedDict = new SharedDictionary<TKey, TValue>(this.LockingStrategy);
        sharedDict.BackingDictionary.Add(key, value);
        return sharedDict;
    }
}
