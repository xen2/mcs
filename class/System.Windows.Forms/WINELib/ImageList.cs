//
// System.Windows.Forms.ImageList.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;
using System.Collections;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>
    public sealed class ImageList : Component {

		//
		//  --- Constructor
		//

		[MonoTODO]
		public ImageList()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ImageList(IContainer cont)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public ColorDepth ColorDepth {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public IntPtr Handle {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HandleCreated {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ImageList.ImageCollection Images {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Size ImageSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ImageListStreamer ImageStream {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color TransparentColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public void Draw(Graphics g, Point pt, int n)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Draw(Graphics g, int n1, int n2, int n3)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		public event EventHandler RecreateHandle;

		//
		//  --- Protected Methods
		//
		//[MonoTODO]
		//protected virtual void Dispose(bool disposing)
		//{
		//	throw new NotImplementedException ();
		//}


		//
		// System.Windows.Forms.ImageList.ImageCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//
		//// (C) 2002 Ximian, Inc
		////
		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		//
		// </summary>

		public sealed class ImageCollection : IList, ICollection, IEnumerable {


		//
		//  --- Public Properties
		//

		[MonoTODO]
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool Empty {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Image this[int index] {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public void Add(Icon icon)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add(Image img)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add(Image img, Color col)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int AddStrip(Image value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(Image image)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(Image image)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(Image image)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
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

			//[MonoTODO]
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					throw new NotImplementedException ();
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.Remove( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			// End Of ICollection

		}// End of Subclass

	 }//End of class
}
