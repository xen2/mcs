//
// System.Xml.XPath.BaseIterator
//
// Author:
//   Piers Haken (piersh@friskit.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002 Piers Haken
// (C) 2003 Atsushi Enomoto
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

namespace System.Xml.XPath
{
	internal abstract class BaseIterator : XPathNodeIterator
	{
		NSResolver _nsm;
		int position;

		internal BaseIterator (BaseIterator other)
		{
			_nsm = other._nsm;
			position = other.position;
		}
		internal BaseIterator (NSResolver nsm)
		{
			_nsm = nsm;
		}

		public NSResolver NamespaceManager
		{
			get { return _nsm; }
			set { _nsm = value; }
		}

		public virtual bool ReverseAxis {
			get { return false; }
		}

		public int ComparablePosition {
			get {
				if (ReverseAxis) {
					int diff = Count - CurrentPosition + 1;
					return diff < 1 ? 1 : diff;
				}
				else
					return CurrentPosition;
			}
		}

		public override int CurrentPosition {
			get { return position; }
		}

		public override bool MoveNext ()
		{
			if (!MoveNextCore ())
				return false;
			position++;
			if (Current != null &&
				Current.NodeType == XPathNodeType.Whitespace &&
				_nsm is XsltContext &&
				!((XsltContext) _nsm).PreserveWhitespace (Current))
				return MoveNext ();
			return true;
		}

		public abstract bool MoveNextCore ();

		public override string ToString ()
		{
			if (Current != null)
				return Current.NodeType.ToString () + "[" + CurrentPosition + "] : " + Current.Name + " = " + Current.Value;
			else
				return this.GetType().ToString () + "[" + CurrentPosition + "]";
		}
	}

	internal class WrapperIterator : BaseIterator
	{
		XPathNodeIterator iter;

		public WrapperIterator (XPathNodeIterator iter, NSResolver nsm)
			: base (nsm)
		{
			this.iter = iter;
		}

		private WrapperIterator (WrapperIterator other)
			: base (other)
		{
			iter = other.iter.Clone ();
		}

		public override XPathNodeIterator Clone ()
		{
			return new WrapperIterator (this);
		}

		public override bool MoveNextCore ()
		{
			return iter.MoveNext ();
		}

		public override XPathNavigator Current {
			get { return iter.Current; }
		}
	}

	internal abstract class SimpleIterator : BaseIterator
	{
		protected readonly XPathNavigator _nav;
		protected XPathNavigator _current;

		public SimpleIterator (BaseIterator iter) : base (iter.NamespaceManager)
		{
			_nav = iter.Current.Clone ();
		}
		protected SimpleIterator (SimpleIterator other, bool clone) : base (other)
		{
			_nav = other._nav.Clone ();
		}
		public SimpleIterator (XPathNavigator nav, NSResolver nsm) : base (nsm)
		{
			_nav = nav.Clone ();
		}

		public override XPathNavigator Current {
			get {
				if (_current == null) // position == 0
					_current = _nav.Clone ();
				return _current;
			}
		}
	}

	internal class SelfIterator : SimpleIterator
	{
		public SelfIterator (BaseIterator iter) : base (iter) {}
		public SelfIterator (XPathNavigator nav, NSResolver nsm) : base (nav, nsm) {}
		protected SelfIterator (SelfIterator other, bool clone) : base (other, true) 
		{
		}

		public override XPathNodeIterator Clone () { return new SelfIterator (this, true); }
		public override bool MoveNextCore ()
		{
			if (CurrentPosition == 0)
			{
				return true;
			}
			return false;
		}

		public override XPathNavigator Current {
			get { return _nav; }
		}
	}

	internal class NullIterator : SelfIterator
	{
		public NullIterator (BaseIterator iter) : base (iter) {}
		public NullIterator (XPathNavigator nav) : this (nav, null) {}
		public NullIterator (XPathNavigator nav, NSResolver nsm) : base (nav, nsm) {}
		private NullIterator (NullIterator other) : base (other, true) {}
		public override XPathNodeIterator Clone () { return new NullIterator (this); }
		public override bool MoveNextCore ()
		{
			return false;
		}
	}

	internal class ParensIterator : BaseIterator
	{
		BaseIterator _iter;

		public ParensIterator (BaseIterator iter) : base (iter.NamespaceManager) 
		{
			_iter = iter;
		}
		private ParensIterator (ParensIterator other) : base (other) 
		{
			_iter = (BaseIterator) other._iter.Clone ();
		}
		public override XPathNodeIterator Clone () { return new ParensIterator (this); }
		public override bool MoveNextCore ()
		{
			return _iter.MoveNext ();
		}

		public override XPathNavigator Current { get { return _iter.Current; }}

		public override int Count { get { return _iter.Count; } }
	}

	internal class ParentIterator : SimpleIterator
	{
		bool canMove;
		public ParentIterator (BaseIterator iter) : base (iter)
		{
			canMove = _nav.MoveToParent ();
			_current = _nav;
		}
		private ParentIterator (ParentIterator other, bool dummy) : base (other, true)
		{
			_current = _nav;
			canMove = other.canMove;
		}
		public ParentIterator (XPathNavigator nav, NSResolver nsm) : base (nav, nsm) {}
		public override XPathNodeIterator Clone () { return new ParentIterator (this, true); }
		public override bool MoveNextCore ()
		{
			if (!canMove)
				return false;
			canMove = false;
			return true;
		}
	}

	internal class ChildIterator : SimpleIterator
	{
		public ChildIterator (BaseIterator iter) : base (iter) {}
		private ChildIterator (ChildIterator other) : base (other, true) {}
		public override XPathNodeIterator Clone () { return new ChildIterator (this); }
		public override bool MoveNextCore ()
		{
			bool fSuccess = (CurrentPosition == 0) ? _nav.MoveToFirstChild () : _nav.MoveToNext ();
			if (fSuccess) {
				Current.MoveTo (_nav);
			}
			return fSuccess;
		}
	}

	internal class FollowingSiblingIterator : SimpleIterator
	{
		public FollowingSiblingIterator (BaseIterator iter) : base (iter) {}
		private FollowingSiblingIterator (FollowingSiblingIterator other) : base (other, true) {}
		public override XPathNodeIterator Clone () { return new FollowingSiblingIterator (this); }
		public override bool MoveNextCore ()
		{
			switch (_nav.NodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				// They have no siblings.
				return false;
			}
			if (_nav.MoveToNext ())
			{
				Current.MoveTo (_nav);
				return true;
			}
			return false;
		}
	}

	internal class PrecedingSiblingIterator : SimpleIterator
	{
		bool finished;
		bool started;
		XPathNavigator startPosition;

		public PrecedingSiblingIterator (BaseIterator iter) : base (iter)
		{
			startPosition = iter.Current.Clone ();
		}
		private PrecedingSiblingIterator (PrecedingSiblingIterator other) : base (other, true) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
		}

		public override XPathNodeIterator Clone () { return new PrecedingSiblingIterator (this); }
		public override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				switch (_nav.NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Namespace:
					// They have no siblings.
					finished = true;
					return false;
				}

				_nav.MoveToFirst ();
				if (!_nav.IsSamePosition (startPosition)) {
					Current.MoveTo (_nav);
					return true;
				}
			} else {
				if (!_nav.MoveToNext ()) {
					finished = true;
					return false;
				}
			}
			if (_nav.ComparePosition (startPosition) != XmlNodeOrder.Before) {
				// Note that if _nav contains only 1 node, it won't be Same.
				finished = true;
				return false;
			} else {
				Current.MoveTo (_nav);
				return true;
			}
		}
		public override bool ReverseAxis {
			get { return true; }
		}
	}

	internal class AncestorIterator : SimpleIterator
	{
		int currentPosition;
		ArrayList navigators;
		XPathNavigator startPosition;

		public AncestorIterator (BaseIterator iter) : base (iter)
		{
			startPosition = iter.Current.Clone ();
		}

		private AncestorIterator (AncestorIterator other)
			: base (other, true)
		{
			startPosition = other.startPosition;
			if (other.navigators != null)
				navigators = (ArrayList) other.navigators;
			currentPosition = other.currentPosition;
		}

		public override XPathNodeIterator Clone ()
		{
			return new AncestorIterator (this);
		}

		private void CollectResults ()
		{
			navigators = new ArrayList ();

			XPathNavigator ancestors = startPosition.Clone ();
			if (!ancestors.MoveToParent ())
				return;
			while (ancestors.NodeType != XPathNodeType.Root) {
				navigators.Add (ancestors.Clone ());
				ancestors.MoveToParent ();
			}
			currentPosition = navigators.Count;
		}

		public override bool MoveNextCore ()
		{
			if (navigators == null) {
				CollectResults ();
				if (startPosition.NodeType != XPathNodeType.Root) {
					// First time it returns Root
					_nav.MoveToRoot ();
					Current.MoveTo (_nav);
					return true;
				}
			}
			if (currentPosition == 0)
				return false;
			_nav.MoveTo ((XPathNavigator) navigators [--currentPosition]);
			Current.MoveTo (_nav);
			return true;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override int Count {
			get {
				if (navigators == null)
					CollectResults ();
				return navigators.Count;
			}
		}
	}

	internal class AncestorOrSelfIterator : SimpleIterator
	{
		int currentPosition;
		ArrayList navigators;
		XPathNavigator startPosition;

		public AncestorOrSelfIterator (BaseIterator iter) : base (iter)
		{
			startPosition = iter.Current.Clone ();
		}

		private AncestorOrSelfIterator (AncestorOrSelfIterator other)
			: base (other, true)
		{
			startPosition = other.startPosition;
			if (other.navigators != null)
				navigators = (ArrayList) other.navigators.Clone ();
			currentPosition = other.currentPosition;
		}

		public override XPathNodeIterator Clone ()
		{
			return new AncestorOrSelfIterator (this);
		}

		private void CollectResults ()
		{
			navigators = new ArrayList ();

			XPathNavigator ancestors = startPosition.Clone ();
			if (!ancestors.MoveToParent ())
				return;
			while (ancestors.NodeType != XPathNodeType.Root) {
				navigators.Add (ancestors.Clone ());
				ancestors.MoveToParent ();
			}
			currentPosition = navigators.Count;
		}

		public override bool MoveNextCore ()
		{
			if (navigators == null) {
				CollectResults ();
				if (startPosition.NodeType != XPathNodeType.Root) {
					// First time it returns Root
					_nav.MoveToRoot ();
					Current.MoveTo (_nav);
					return true;
				}
			}
			if (currentPosition == -1)
				return false;
			if (currentPosition-- == 0) {
				_nav.MoveTo (startPosition);
				Current.MoveTo (_nav);
				return true; // returns self.
			}
			_nav.MoveTo ((XPathNavigator) navigators [currentPosition]);
			Current.MoveTo (_nav);
			return true;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override int Count {
			get {
				if (navigators == null)
					CollectResults ();
				return navigators.Count + 1;
			}
		}
	}

	internal class DescendantIterator : SimpleIterator
	{
		private int _depth;
		private bool _finished;

		public DescendantIterator (BaseIterator iter) : base (iter) {}

		private DescendantIterator (DescendantIterator other) : base (other)
		{
			_depth = other._depth;
			_finished = other._finished;
		}

		public override XPathNodeIterator Clone () { return new DescendantIterator (this); }

		public override bool MoveNextCore ()
		{
			if (_finished)
				return false;

			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				Current.MoveTo (_nav);
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					Current.MoveTo (_nav);
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new XPathException ("There seems some bugs on the XPathNavigator implementation class: " + _nav.GetType ());
				_depth --;
			}
			_finished = true;
			return false;
		}
	}

	internal class DescendantOrSelfIterator : SimpleIterator
	{
		private int _depth;
		private bool _finished;

		public DescendantOrSelfIterator (BaseIterator iter) : base (iter) {}

		private DescendantOrSelfIterator (DescendantOrSelfIterator other) : base (other, true)
		{
			_depth = other._depth;
		}

		public override XPathNodeIterator Clone () { return new DescendantOrSelfIterator (this); }

		public override bool MoveNextCore ()
		{
			if (_finished)
				return false;

			if (CurrentPosition == 0)
			{
				// self
				Current.MoveTo (_nav);
				return true;
			}
			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				Current.MoveTo (_nav);
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					Current.MoveTo (_nav);
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new XPathException ("There seems some bugs on the XPathNavigator implementation class: " + _nav.GetType ());
				_depth --;
			}
			_finished = true;
			return false;
		}
	}

	internal class FollowingIterator : SimpleIterator
	{
		private bool _finished = false;
		public FollowingIterator (BaseIterator iter) : base (iter) {}
		private FollowingIterator (FollowingIterator other) : base (other, true) {}
		public override XPathNodeIterator Clone () { return new FollowingIterator (this); }
		public override bool MoveNextCore ()
		{
			if (_finished)
				return false;
			bool checkChildren = true;
			if (CurrentPosition == 0)
			{
				checkChildren = false;
				switch (_nav.NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Namespace:
					_nav.MoveToParent ();
					checkChildren = true;
					break;
				default:
					if (_nav.MoveToNext ())
					{
						Current.MoveTo (_nav);
						return true;
					} else {
						while (_nav.MoveToParent ()) {
							if (_nav.MoveToNext ()) {
								Current.MoveTo (_nav);
								return true;
							}
						}
					}
					break;
				}
			}
			if (checkChildren)
			{
				if (_nav.MoveToFirstChild ())
				{
					Current.MoveTo (_nav);
					return true;
				}
				do
				{
					if (_nav.MoveToNext ())
					{
						Current.MoveTo (_nav);
						return true;
					}
				}
				while (_nav.MoveToParent ());
			}
			_finished = true;
			return false;
		}
	}

	internal class PrecedingIterator : SimpleIterator
	{
		bool finished;
		bool started;
		XPathNavigator startPosition;

		public PrecedingIterator (BaseIterator iter) : base (iter) 
		{
			startPosition = iter.Current.Clone ();
		}
		private PrecedingIterator (PrecedingIterator other) : base (other, true) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
		}
		public override XPathNodeIterator Clone () { return new PrecedingIterator (this); }
		public override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				_nav.MoveToRoot ();
			}
			bool loop = true;
			while (loop) {
				while (!_nav.MoveToFirstChild ()) {
					while (!_nav.MoveToNext ()) {
						if (!_nav.MoveToParent ()) { // Should not finish, at least before startPosition.
							finished = true;
							return false;
						}
					}
					break;
				}
				if (_nav.IsDescendant (startPosition))
					continue;
				loop = false;
				break;
			}
			if (_nav.ComparePosition (startPosition) != XmlNodeOrder.Before) {
				// Note that if _nav contains only 1 node, it won't be Same.
				finished = true;
				return false;
			} else {
				Current.MoveTo (_nav);
				return true;
			}
		}
		public override bool ReverseAxis {
			get { return true; }
		}
	}

	internal class NamespaceIterator : SimpleIterator
	{
		public NamespaceIterator (BaseIterator iter) : base (iter) {}
		private NamespaceIterator (NamespaceIterator other) : base (other, true) {}
		public override XPathNodeIterator Clone () { return new NamespaceIterator (this); }
		public override bool MoveNextCore ()
		{
			if (CurrentPosition == 0)
			{
				if (_nav.MoveToFirstNamespace ())
				{
					Current.MoveTo (_nav);
					return true;
				}
			}
			else if (_nav.MoveToNextNamespace ())
			{
				Current.MoveTo (_nav);
				return true;
			}
			return false;
		}
	}

	internal class AttributeIterator : SimpleIterator
	{
		public AttributeIterator (BaseIterator iter) : base (iter) {}
		private AttributeIterator (AttributeIterator other) : base (other, true) {}
		public override XPathNodeIterator Clone () { return new AttributeIterator (this); }
		public override bool MoveNextCore ()
		{
			if (CurrentPosition == 0)
			{
				if (_nav.MoveToFirstAttribute ())
				{
					Current.MoveTo (_nav);
					return true;
				}
			}
			else if (_nav.MoveToNextAttribute ())
			{
				Current.MoveTo (_nav);
				return true;
			}
			return false;			
		}
	}

	internal class AxisIterator : BaseIterator
	{
		private SimpleIterator _iter;
		private NodeTest _test;
			
		string name, ns;
		XPathNodeType matchType;

		public AxisIterator (SimpleIterator iter, NodeTest test) : base (iter.NamespaceManager)
		{
			_iter = iter;
			_test = test;
			test.GetInfo (out name, out ns, out matchType, NamespaceManager);
//			if (name != null)
//				name = Current.NameTable.Add (name);

//			if (ns != null)
//				ns = Current.NameTable.Add (ns);
		}

		private AxisIterator (AxisIterator other) : base (other)
		{
			_iter = (SimpleIterator) other._iter.Clone ();
			_test = other._test;
			name = other.name;
			ns = other.ns;
			matchType = other.matchType;
		}
		public override XPathNodeIterator Clone () { return new AxisIterator (this); }

		public override bool MoveNextCore ()
		{
			while (_iter.MoveNext ())
			{
				if (_test.Match (NamespaceManager, Current))
				{
					return true;
				}
			}
			return false;
		}
		public override XPathNavigator Current { get { return _iter.Current; }}

		public override bool ReverseAxis {
			get { return _iter.ReverseAxis; }
		}
	}

	internal class SimpleSlashIterator : BaseIterator
	{
		private NodeSet _expr;
		private BaseIterator _left, _right;
		private XPathNavigator _current;

		public SimpleSlashIterator (BaseIterator left, NodeSet expr)
			: base (left.NamespaceManager)
		{
			this._left = left;
			this._expr = expr;
		}

		private SimpleSlashIterator (SimpleSlashIterator other)
			: base (other)
		{
			_expr = other._expr;
			_left = (BaseIterator) other._left.Clone ();
			if (other._right != null)
				_right = (BaseIterator) other._right.Clone ();
		}

		public override XPathNodeIterator Clone () { return new SimpleSlashIterator (this); }

		public override bool MoveNextCore ()
		{
			while (_right == null || !_right.MoveNext ()) {
				if (!_left.MoveNext ())
					return false;
				_right = _expr.EvaluateNodeSet (_left);
			}
			if (_current == null)
				_current = _right.Current.Clone ();
			else
				if (! _current.MoveTo (_right.Current) )
					_current = _right.Current.Clone ();
			return true;
		}

		public override XPathNavigator Current {
			get { return _current; }
		}
	}

	internal class SlashIterator : BaseIterator
	{
		private BaseIterator _iterLeft;
		private BaseIterator _iterRight;
		private NodeSet _expr;
		ArrayList _navStore;
		SortedList _iterList;
		bool _finished;
		BaseIterator _nextIterRight;

		public SlashIterator (BaseIterator iter, NodeSet expr, bool requireSorting) : base (iter.NamespaceManager)
		{
			_iterLeft = iter;
			_expr = expr;

			if (requireSorting)
				CollectResults ();
		}

		private SlashIterator (SlashIterator other) : base (other)
		{
			_iterLeft = (BaseIterator) other._iterLeft.Clone ();
			if (other._iterRight != null)
				_iterRight = (BaseIterator) other._iterRight.Clone ();
			_expr = other._expr;
			if (other._iterList != null)
				_iterList = (SortedList) other._iterList.Clone ();
			if (other._navStore != null)
				_navStore = (ArrayList) other._navStore.Clone ();
			_finished = other._finished;
			if (other._nextIterRight != null)
				_nextIterRight = (BaseIterator) other._nextIterRight.Clone ();
		}
		public override XPathNodeIterator Clone () { return new SlashIterator (this); }

		public override bool MoveNextCore ()
		{
			if (_finished)
				return false;
			if (_navStore != null) {
				// Which requires sorting::
				if (_navStore.Count < CurrentPosition + 1) {
					_finished = true;
					return false;
				}
				while (_navStore.Count > CurrentPosition + 1) {
					if (((XPathNavigator) _navStore [CurrentPosition + 1]).IsSamePosition (
						(XPathNavigator) _navStore [CurrentPosition]))
						_navStore.RemoveAt (CurrentPosition + 1);
					else
						break;
				}

				return true;
			}
			// Which does not require sorting::
			
			if (_iterRight == null) { // First time
				if (!_iterLeft.MoveNext ())
					return false;
				_iterRight = _expr.EvaluateNodeSet (_iterLeft);
				_iterList = new SortedList (XPathIteratorComparer.Instance);
			}

			while (true) {
				while (!_iterRight.MoveNext ()) {
					if (_iterList.Count > 0) {
						int last = _iterList.Count - 1;
						_iterRight = (BaseIterator) _iterList.GetByIndex (last);
						_iterList.RemoveAt (last);
						break;
					} else if (_nextIterRight != null) {
						_iterRight = _nextIterRight;
						_nextIterRight = null;
						break;
					} else if (!_iterLeft.MoveNext ()) {
						_finished = true;
						return false;
					}
					else
						_iterRight = _expr.EvaluateNodeSet (_iterLeft);
				}
				bool loop = true;
				while (loop) {
					loop = false;
					if (_nextIterRight == null) {
						bool noMoreNext = false;
						while (_nextIterRight == null || !_nextIterRight.MoveNext ()) {
							if(_iterLeft.MoveNext ())
								_nextIterRight = _expr.EvaluateNodeSet (_iterLeft);
							else {
								noMoreNext = true;
								break;
							}
						}
						if (noMoreNext)
							_nextIterRight = null; // FIXME: More efficient code. Maybe making noMoreNext class scope would be better.
					}
					if (_nextIterRight != null) {
						switch (_iterRight.Current.ComparePosition (_nextIterRight.Current)) {
						case XmlNodeOrder.After:
							_iterList [_iterRight] = _iterRight;
							_iterRight = _nextIterRight;
							_nextIterRight = null;
							loop = true;
							break;
						case XmlNodeOrder.Same:
							if (!_nextIterRight.MoveNext ())
								_nextIterRight = null;

							else {
								int last = _iterList.Count;
								_iterList [_nextIterRight] = _nextIterRight;
								if (last != _iterList.Count) {
									_nextIterRight = (BaseIterator) _iterList.GetByIndex (last);
									_iterList.RemoveAt (last);
								}
							}

							loop = true;
							break;
						}
					}
				}
				return true;
			}
		}

		private void CollectResults ()
		{
			_navStore = new ArrayList ();
			while (true) {
				while (_iterRight == null || !_iterRight.MoveNext ()) {
					if (!_iterLeft.MoveNext ()) {
						_navStore.Sort (XPathNavigatorComparer.Instance);
						return;
					}
					_iterRight = _expr.EvaluateNodeSet (_iterLeft);
				}
				XPathNavigator nav = _iterRight.Current;
				_navStore.Add (nav.Clone ());
			}
		}

		public override XPathNavigator Current { 
			get {
				if (CurrentPosition <= 0) return null;
				if (_navStore != null) {
					return (XPathNavigator) _navStore [CurrentPosition - 1];
				} else {
					return _iterRight.Current;
				}
			}
		}

		public override int Count { get { return _navStore == null ? base.Count : _navStore.Count; } }
	}

	internal class PredicateIterator : BaseIterator
	{
		private BaseIterator _iter;
		private Expression _pred;
		private XPathResultType resType;
		private bool finished;

		public PredicateIterator (BaseIterator iter, Expression pred) : base (iter.NamespaceManager)
		{
			_iter = iter;
			_pred = pred;
			resType = pred.GetReturnType (iter);
		}

		private PredicateIterator (PredicateIterator other) : base (other)
		{
			_iter = (BaseIterator) other._iter.Clone ();
			_pred = other._pred;
			resType = other.resType;
			finished = other.finished;
		}
		public override XPathNodeIterator Clone () { return new PredicateIterator (this); }

		public override bool MoveNextCore ()
		{
			if (finished)
				return false;
			while (_iter.MoveNext ())
			{
				switch (resType) {
					case XPathResultType.Number:
						if (_pred.EvaluateNumber (_iter) != _iter.ComparablePosition)
							continue;
						finished = true;
						break;
					case XPathResultType.Any: {
						object result = _pred.Evaluate (_iter);
						if (result is double)
						{
							if ((double) result != _iter.ComparablePosition)
								continue;
							finished = true;
						}
						else if (!XPathFunctions.ToBoolean (result))
							continue;
					}
						break;
					default:
						if (!_pred.EvaluateBoolean (_iter))
							continue;
						break;
				}

				return true;
			}
			return false;
		}
		public override XPathNavigator Current { get { return _iter.Current; }}
		public override bool ReverseAxis {
			get { return _iter.ReverseAxis; }
		}
	}

	internal class ListIterator : BaseIterator
	{
		private IList _list;

		public ListIterator (BaseIterator iter, IList list) : base (iter.NamespaceManager)
		{
			_list = list;
		}
		
		public ListIterator (IList list, NSResolver nsm) : base (nsm)
		{
			_list = list;
		}

		private ListIterator (ListIterator other) : base (other)
		{
			_list = other._list;
		}
		public override XPathNodeIterator Clone () { return new ListIterator (this); }

		public override bool MoveNextCore ()
		{
			if (CurrentPosition >= _list.Count)
				return false;
			return true;
		}
		public override XPathNavigator Current {
			get {
				if (_list.Count == 0)
					return null;
				return (XPathNavigator) _list [CurrentPosition - 1]; 
			}
		}

		public override int Count { get { return _list.Count; } }
	}

	
	internal class UnionIterator : BaseIterator
	{
		private BaseIterator _left, _right;
		private bool keepLeft;
		private bool keepRight;
		XPathNavigator _current;

		public UnionIterator (BaseIterator iter, BaseIterator left, BaseIterator right) : base (iter.NamespaceManager)
		{
			_left = left;
			_right = right;
		}

		private UnionIterator (UnionIterator other) : base (other)
		{
			_left = (BaseIterator) other._left.Clone ();
			_right = (BaseIterator) other._right.Clone ();
			keepLeft = other.keepLeft;
			keepRight = other.keepRight;
			if (other._current != null)
				_current = other._current.Clone ();
		}
		public override XPathNodeIterator Clone () { return new UnionIterator (this); }

		public override bool MoveNextCore ()
		{
			if (!keepLeft)
				keepLeft = _left.MoveNext ();
			if (!keepRight)
				keepRight = _right.MoveNext ();

			if (!keepLeft && !keepRight)
				return false;

			if (!keepRight) {
				keepLeft = false;
				SetCurrent (_left);
				return true;
			} else if (!keepLeft) {
				keepRight = false;
				SetCurrent (_right);
				return true;
			}

			switch (_left.Current.ComparePosition (_right.Current)) {
			case XmlNodeOrder.Same:
				// consume both. i.e. don't output duplicate result.
				keepLeft = keepRight = false;
				SetCurrent (_right);
				return true;
			case XmlNodeOrder.Before:
			case XmlNodeOrder.Unknown: // Maybe happen because of "document(a) | document(b)"
				keepLeft = false;
				SetCurrent (_left);
				return true;
			case XmlNodeOrder.After:
				keepRight = false;
				SetCurrent (_right);
				return true;
			default:
				throw new InvalidOperationException ("Should not happen.");
			}
		}

		private void SetCurrent (XPathNodeIterator iter)
		{
			if (_current == null)
				_current = iter.Current.Clone ();
			else
				if (! _current.MoveTo (iter.Current) )
					_current = iter.Current.Clone ();
		}

		public override XPathNavigator Current
		{
			get { return _current; }
		}
	}

	internal class OrderedIterator : BaseIterator
	{
		BaseIterator iter;
		ArrayList list;
		int index = -1;

		public OrderedIterator (BaseIterator iter)
			: base (iter.NamespaceManager)
		{
//			if (iter.Ordered)
			if (false)
				this.iter = iter;
			else {
				list = new ArrayList ();
				while (iter.MoveNext ())
					list.Add (iter.Current);
				list.Sort (XPathNavigatorComparer.Instance);
			}
		}

		private OrderedIterator (OrderedIterator other, bool dummy)
			: base (other)
		{
			if (other.iter != null)
				iter = (BaseIterator) other.iter.Clone ();
			list = other.list;
			index = other.index;
		}

		public override XPathNodeIterator Clone ()
		{
			return new OrderedIterator (this);
		}

		public override bool MoveNextCore ()
		{
			if (iter != null)
				return iter.MoveNext ();
			else if (index++ < list.Count)
				return true;
			index--;
			return false;
		}

		public override XPathNavigator Current {
			get { return iter != null ? iter.Current : index < 0 ? null : (XPathNavigator) list [index]; }
		}
	}
}
