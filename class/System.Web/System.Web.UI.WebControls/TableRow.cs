/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableRow
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class TableRow: WebControl
	{
		private TableCellCollection cells;

		public TableRow(): base(HtmlTextWriterTag.Tr)
		{
			PreventAutoID();
		}

		public virtual TableCellCollection Cells
		{
			get
			{
				if(cells == null)
				{
					cells = new TableCellCollection(this);
				}
				return cells;
			}
		}

		public virtual HorizontalAlign HorizontalAlign
		{
			get
			{
				object o = ViewState["HorizontalAlign"];
				if(o != null)
					return (HorizontalAlign)o;
				return HorizontalAlign.NotSet;
			}
			set
			{
				ViewState["HorizontalAlign"] = value;
			}
		}

		public virtual VerticalAlign VerticalAlign
		{
			get
			{
				object o = ViewState["VerticalAlign"];
				if(o != null)
					return (VerticalAlign)o;
				return VerticalAlign.NotSet;
			}
			set
			{
				ViewState["VerticalAlign"] = value;
			}
		}

		protected override Style CreateControlStyle()
		{
			return new TableItemStyle(ViewState);
		}

		protected override ControlCollection CreateControlCollection()
		{
			return new CellControlCollection(this);
		}

		class CellControlCollection: ControlCollection
		{
			public CellControlCollection(Control owner): base(owner)
			{
			}

			public override void Add(Control child)
			{
				if(child is TableCell)
				{
					base.Add(child);
				} else
				{
					throw new ArgumentException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type", "TableRow", GetType().Name.ToString()));
				}
			}

			public override void AddAt(int index, Control child)
			{
				if(child is TableCell)
				{
					base.AddAt(index, child);
				} else
				{
					throw new ArgumentException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type", "TableRow", GetType().Name.ToString()));
				}
			}
		}
	}
}
