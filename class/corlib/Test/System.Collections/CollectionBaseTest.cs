//
// System.Collections.CollectionBase
// Test suite for System.Collections.CollectionBase
//
// Authors:
//    Nick D. Drochak II
//    Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001 Nick D. Drochak II
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//


using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections
{

[TestFixture]
public class CollectionBaseTest : Assertion
{
	// We need a concrete class to test the abstract base class
	public class ConcreteCollection : CollectionBase 
	{
		// These fields are used as markers to test the On* hooks.
		public bool onClearFired;
		public bool onClearCompleteFired;

		public bool onInsertFired;
		public int onInsertIndex;
		public bool onInsertCompleteFired;
		public int onInsertCompleteIndex;

		public bool onRemoveFired;
		public int onRemoveIndex;
		public bool onRemoveCompleteFired;
		public int onRemoveCompleteIndex;

		public bool onSetFired;
		public int onSetOldValue;
		public int onSetNewValue;
		public bool onSetCompleteFired;
		public int onSetCompleteOldValue;
		public int onSetCompleteNewValue;
		public int mustThrowException;
		public bool onValidateFired;

		// This constructor is used to test OnValid()
		public ConcreteCollection()	
		{
			IList listObj;
			listObj = this;
			listObj.Add(null);
		}

		// This constructor puts consecutive integers into the list
		public ConcreteCollection(int i) {
			IList listObj;
			listObj = this;

			int j;
			for (j = 0; j< i; j++) {
				listObj.Add(j);
			}
		}

		void CheckIfThrow ()
		{
			if (mustThrowException > 0) {
				mustThrowException--;
				if (mustThrowException == 0)
					throw new Exception ();
			}
		}
		
		// A helper method to look at a value in the list at a specific index
		public int PeekAt(int index)
		{
			IList listObj;
			listObj = this;
			return (int) listObj[index];
		}

		protected override void OnValidate (object value) {
			this.onValidateFired = true;
			CheckIfThrow ();
			base.OnValidate (value);
		}

		// Mark the flag if this hook is fired
		protected override void OnClear() {
			this.onClearFired = true;
			CheckIfThrow ();
		}

		// Mark the flag if this hook is fired
		protected override void OnClearComplete() 
		{
			this.onClearCompleteFired = true;
			CheckIfThrow ();
		}

		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnInsert(int index, object value) 
		{
			this.onInsertFired = true;
			this.onInsertIndex = index;
			CheckIfThrow ();
		}

		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnInsertComplete(int index, object value) 
		{
			this.onInsertCompleteFired = true;
			this.onInsertCompleteIndex = index;
			CheckIfThrow ();
		}
		
		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnRemove(int index, object value) 
		{
			this.onRemoveFired = true;
			this.onRemoveIndex = index;
			CheckIfThrow ();
		}
		
		// Mark the flag, and save the paramter if this hook is fired
		protected override void OnRemoveComplete(int index, object value) 
		{
			this.onRemoveCompleteFired = true;
			this.onRemoveCompleteIndex = index;
			CheckIfThrow ();
		}
		
		// Mark the flag, and save the paramters if this hook is fired
		protected override void OnSet(int index, object oldValue, object newValue) 
		{
			this.onSetFired = true;
			this.onSetOldValue = (int) oldValue;
			this.onSetNewValue = (int) newValue;
			CheckIfThrow ();
		}
		
		// Mark the flag, and save the paramters if this hook is fired
		protected override void OnSetComplete(int index, object oldValue, object newValue) 
		{
			this.onSetCompleteFired = true;
			this.onSetCompleteOldValue = (int) oldValue;
			this.onSetCompleteNewValue = (int) newValue;
			CheckIfThrow ();
		}

		public IList BaseList {
			get { return base.List; }
		}
	}  // public class ConcreteCollection

	// Check the count property
	[Test]
	public void Count() {
		ConcreteCollection myCollection;
		myCollection = new ConcreteCollection(4);
		Assert(4 == myCollection.Count);
	}

	// Make sure GetEnumerator returns an object
	[Test]
	public void GetEnumerator() {
		ConcreteCollection myCollection;
		myCollection = new ConcreteCollection(4);
		Assert(null != myCollection.GetEnumerator());
	}

	// OnValid disallows nulls
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void OnValid() {
		ConcreteCollection myCollection;
		myCollection = new ConcreteCollection();
	}

	// Test various Insert paths
	[Test]
	public void Insert() {
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 3;
		// The constructor inserts
		myCollection = new ConcreteCollection(numberOfItems);
		Assert(myCollection.onInsertFired);
		Assert(myCollection.onInsertCompleteFired);

		// Using the IList interface, check inserts in the middle
		IList listObj = myCollection;
		listObj.Insert(1, 9);
		Assert(myCollection.onInsertIndex == 1);
		Assert(myCollection.onInsertCompleteIndex == 1);
		Assert(myCollection.PeekAt(1) == 9);
	}

	// Test Clear and it's hooks
	[Test]
	public void Clear() 
	{
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 1;
		myCollection = new ConcreteCollection(numberOfItems);
		myCollection.Clear();
		Assert(myCollection.Count == 0);
		Assert(myCollection.onClearFired);
		Assert(myCollection.onClearCompleteFired);
	}

	// Test RemoveAt, other removes and the hooks
	[Test]
	public void Remove() 
	{
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 3;
		// Set up a test collection
		myCollection = new ConcreteCollection(numberOfItems);

		// The list is 0-based.  So if we remove the second one
		myCollection.RemoveAt(1);

		// We should see the original third one in it's place
		Assert(myCollection.PeekAt(1) == 2);
		Assert(myCollection.onRemoveFired);
		Assert(myCollection.onRemoveIndex == 1);
		Assert(myCollection.onRemoveCompleteFired);
		Assert(myCollection.onRemoveCompleteIndex == 1);
		IList listObj = myCollection;
		listObj.Remove(0);
		// Confirm parameters are being passed to the hooks
		Assert(myCollection.onRemoveIndex == 0);
		Assert(myCollection.onRemoveCompleteIndex == 0);
	}

	// Test the random access feature
	[Test]
	public void Set() 
	{
		ConcreteCollection myCollection;
		int numberOfItems;
		numberOfItems = 3;
		myCollection = new ConcreteCollection(numberOfItems);
		IList listObj = myCollection;
		listObj[0] = 99;
		Assert((int) listObj[0] == 99);
		Assert(myCollection.onSetFired);
		Assert(myCollection.onSetCompleteFired);
		Assert(myCollection.onSetOldValue == 0);
		Assert(myCollection.onSetCompleteOldValue == 0);
		Assert(myCollection.onSetNewValue == 99);
		Assert(myCollection.onSetCompleteNewValue == 99);
	}

	[Test]
	public void InsertComplete_Add ()
	{
		ConcreteCollection coll = new ConcreteCollection (0);
		coll.mustThrowException = 1;

		try {
			coll.BaseList.Add (0);
		} catch {
		}
		AssertEquals (0, coll.Count);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void ValidateCalled ()
	{
		ConcreteCollection coll = new ConcreteCollection (0);
		coll.mustThrowException = 1;

		try {
			coll.BaseList [5] = 8888;
		} catch (ArgumentOutOfRangeException) {
			throw;
		} finally {
			AssertEquals (false, coll.onValidateFired);
		}
	}

	[Test]
	public void SetCompleteCalled ()
	{
		ConcreteCollection coll = new ConcreteCollection (0);

		coll.BaseList.Add (88);
		coll.mustThrowException = 1;
		try {
			coll.BaseList [0] = 11;
		} catch {
		} finally {
			AssertEquals (false, coll.onSetCompleteFired);
		}
	}
}

}
