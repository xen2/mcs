//
// System.Windows.Forms.GridColumnStylesCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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
using System.Collections;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class GridColumnStylesCollection : BaseCollection, IList {

		private GridColumnStylesCollection(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public virtual int Add(DataGridColumnStyle column) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange(DataGridColumnStyle [] columns)
		{
			//FIXME:
		}

		[MonoTODO]
		public void Clear()
		{
			//FIXME:
		}

		[MonoTODO]
		public bool Contains(PropertyDescriptor propDesc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(string name) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(DataGridColumnStyle column) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(DataGridColumnStyle element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(DataGridColumnStyle column)
		{
			//FIXME:
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			//FIXME:
		}

		[MonoTODO]
		public void ResetPropertyDescriptors()
		{
			//FIXME:
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event CollectionChangeEventHandler CollectionChanged;

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override ArrayList List {
			get {
				return base.List;
				//FIXME:
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected void OnCollectionChanged(CollectionChangeEventArgs ccevent)
		{
			//FIXME:
		}
		/// <summary>
		/// IList Interface implmentation.
		/// </summary>
		bool IList.IsReadOnly{
			get{
				// We allow addition, removeal, and editing of items after creation of the list.
				return false;
			}
		}
		bool IList.IsFixedSize{
			get{
				// We allow addition and removeal of items after creation of the list.
				return false;
			}
		}

//		[MonoTODO]
//		DataGridColumnStyle IList.this[int index]{
//			get{
//				throw new NotImplementedException ();
//			}
//		}
//		
		[MonoTODO]
		public DataGridColumnStyle this[PropertyDescriptor propDesc]{
			get{
				throw new NotImplementedException ();
			}
		}

//		[MonoTODO]
//		public DataGridColumnStyle this[string columnName]{
//			get{
//				throw new NotImplementedException ();
//			}
//		}

		[MonoTODO]
		void IList.Clear(){
			//FIXME:
		}

//		[MonoTODO]
//		DataGridColumnStyle IList.[int index]{
//			get{
//				throw new NotImplementedException ();
//			}
//		}
		[MonoTODO]
		public object this[int index]{
			get{
				throw new NotImplementedException ();
			}
			set{
				//FIXME:
			}
		}
		
		[MonoTODO]
		int IList.Add( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.Contains( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.IndexOf( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Insert(int index, object value){
			//FIXME:
		}

		[MonoTODO]
		void IList.Remove( object value){
			//FIXME:
		}

		[MonoTODO]
		void IList.RemoveAt( int index){
			//FIXME:
		}
		// End of IList interface

		/// <summary>
		/// ICollection Interface implmentation.
		/// </summary>
		int ICollection.Count{
			get{
				throw new NotImplementedException ();
			}
		}
		bool ICollection.IsSynchronized{
			get{
				throw new NotImplementedException ();
			}
		}
		object ICollection.SyncRoot{
			get{
				throw new NotImplementedException ();
			}
		}
		void ICollection.CopyTo(Array array, int index){
			//FIXME:
		}
		// End Of ICollection
	}
}
