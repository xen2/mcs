//
// System.Globalization.CultureInfo.cs
//
// Miguel de Icaza (miguel@ximian.com)
// Dick Porter (dick@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
	[Serializable]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		static CultureInfo invariant_culture_info;
		
		bool m_isReadOnly;
		int  cultureID;
		[NonSerialized]
		int parent_lcid;
		[NonSerialized]
		int specific_lcid;
		[NonSerialized]
		int datetime_index;
		[NonSerialized]
		int number_index;
		bool m_useUserOverride;
		NumberFormatInfo numInfo;
		DateTimeFormatInfo dateTimeInfo;
		TextInfo textInfo;
		private string m_name;
		
		[NonSerialized]
		private string displayname;
		[NonSerialized]
		private string englishname;
		[NonSerialized]
		private string nativename;
		[NonSerialized]
		private string iso3lang;
		[NonSerialized]
		private string iso2lang;
		[NonSerialized]
		private string icu_name;
		[NonSerialized]
		private string win3lang;
		CompareInfo compareInfo;
		Calendar calendar;
		
		int m_dataItem;	// MS.NET serializes this.
		
		// Deserialized instances will set this to false
		[NonSerialized]
		bool constructed;
		
		private static readonly string MSG_READONLY = "This instance is read only";
		
		static public CultureInfo InvariantCulture {
			get {
				if (invariant_culture_info == null) {
					lock (typeof (CultureInfo)) {
						if (invariant_culture_info == null) {
							invariant_culture_info = new CultureInfo (0x7f, false);
							invariant_culture_info.m_isReadOnly = true;
						}
					}
				}
				
				return(invariant_culture_info);
			}
		}

		public static CultureInfo CreateSpecificCulture (string name)
		{
			if (name == null) {
				throw new ArgumentNullException ("name");
			}

			if (name == String.Empty)
				return InvariantCulture;

			CultureInfo ci = new CultureInfo ();
			if (!construct_internal_locale_from_specific_name (ci, name.ToLowerInvariant ()))
				throw new ArgumentException ("Culture name " + name +
						" is not supported.", name);

			return ci;
		}

		public static CultureInfo CurrentCulture 
		{
			get {
				return Thread.CurrentThread.CurrentCulture;
			}
		}

		public static CultureInfo CurrentUICulture 
		{
			get {
				return Thread.CurrentThread.CurrentUICulture;
			}
		}

		internal static CultureInfo ConstructCurrentCulture ()
		{
			CultureInfo ci = new CultureInfo ();
			if (!construct_internal_locale_from_current_locale (ci))
				ci = InvariantCulture;
			return ci;
		}

		internal static CultureInfo ConstructCurrentUICulture ()
		{
			return ConstructCurrentCulture ();
		}

		public virtual int LCID {
			get {
				return cultureID;
			}
		}

		public virtual string Name {
			get {
				return(m_name);
			}
		}

		public virtual string NativeName
		{
			get {
				if (!constructed) Construct ();
				return(nativename);
			}
		}
		

		[MonoTODO]
		public virtual Calendar Calendar
		{
			get { return calendar; }
		}

		[MonoTODO]
		public virtual Calendar[] OptionalCalendars
		{
			get {
				return(null);
			}
		}

		public virtual CultureInfo Parent
		{
			get {
				if (!constructed) Construct ();
				if (parent_lcid == cultureID)
					return null;
				return new CultureInfo (parent_lcid);
			}
		}

		public virtual TextInfo TextInfo
		{
			get {
				if (textInfo == null) {
					lock (this) {
						if(textInfo == null) {
							textInfo = new TextInfo (cultureID);
						}
					}
				}
				
				return(textInfo);
			}
		}

		public virtual string ThreeLetterISOLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(iso3lang);
			}
		}

		public virtual string ThreeLetterWindowsLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(win3lang);
			}
		}

		public virtual string TwoLetterISOLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(iso2lang);
			}
		}

		public bool UseUserOverride
		{
			get {
				return m_useUserOverride;
			}
		}

		internal string IcuName {
			get {
				if (!constructed) Construct ();
				return icu_name;
			}
		}

		public void ClearCachedData()
		{
			Thread.CurrentThread.CurrentCulture = null;
			Thread.CurrentThread.CurrentUICulture = null;
		}

		public virtual object Clone()
		{
			if (!constructed) Construct ();
			CultureInfo ci=(CultureInfo)MemberwiseClone ();
			ci.m_isReadOnly=false;
			return(ci);
		}

		public override bool Equals (object value)
		{
			CultureInfo b = value as CultureInfo;
			
			if (b != null)
				return b.cultureID == cultureID;
			return false;
		}

		public static CultureInfo[] GetCultures(CultureTypes types)
		{
			bool neutral=((types & CultureTypes.NeutralCultures)!=0);
			bool specific=((types & CultureTypes.SpecificCultures)!=0);
			bool installed=((types & CultureTypes.InstalledWin32Cultures)!=0);  // TODO

			return internal_get_cultures (neutral, specific, installed);
		}

		public override int GetHashCode()
		{
			return cultureID;
		}

		public static CultureInfo ReadOnly(CultureInfo ci)
		{
			if(ci==null) {
				throw new ArgumentNullException("ci");
			}

			if(ci.m_isReadOnly) {
				return(ci);
			} else {
				CultureInfo new_ci=(CultureInfo)ci.Clone ();
				new_ci.m_isReadOnly=true;
				return(new_ci);
			}
		}

		public override string ToString()
		{
			return(m_name);
		}
		
		public virtual CompareInfo CompareInfo
		{
			get {
				if (!constructed) Construct ();
				if(compareInfo==null) {
					lock (this) {
						if(compareInfo==null) {
							compareInfo=new CompareInfo (this);
						}
					}
				}
				
				return(compareInfo);
			}
		}

		internal static bool IsIDNeutralCulture (int lcid)
		{
			bool ret;
			if (!internal_is_lcid_neutral (lcid, out ret))
				throw new ArgumentException (String.Format ("Culture id 0x{:x4} is not supported.", lcid));
				
			return ret;
		}

		public virtual bool IsNeutralCulture {
			get {
				if (!constructed) Construct ();
				return ((cultureID & 0xff00) == 0 || specific_lcid == 0);
			}
		}

		public virtual NumberFormatInfo NumberFormat {
			get {
				if (!constructed) Construct ();
				if (numInfo == null){
					lock (this){
						if (numInfo == null) {
							numInfo = new NumberFormatInfo ();
							construct_number_format ();
						}
					}
				}

				return numInfo;
			}

			set {
				if (!constructed) Construct ();
				if (m_isReadOnly) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("NumberFormat");
				
				numInfo = value;
			}
		}

		public virtual DateTimeFormatInfo DateTimeFormat
		{
			get 
			{
				if (!constructed) Construct ();
				if (dateTimeInfo == null)
				{
					lock (this)
					{
						if (dateTimeInfo == null) {
							dateTimeInfo = new DateTimeFormatInfo();
							construct_datetime_format ();
						}
					}
				}

				return dateTimeInfo;
			}

			set 
			{
				if (!constructed) Construct ();
				if (m_isReadOnly) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("DateTimeFormat");
				
				dateTimeInfo = value;
			}
		}

		public virtual string DisplayName
		{
			get {
				if (!constructed) Construct ();
				return(displayname);
			}
		}

		public virtual string EnglishName
		{
			get {
				if (!constructed) Construct ();
				return(englishname);
			}
		}

		[MonoTODO]
		public static CultureInfo InstalledUICulture
		{
			get {
				return(null);
			}
		}

		public bool IsReadOnly 
		{
			get {
				return(m_isReadOnly);
			}
		}
		

		// 
		// IFormatProvider implementation
		//
		public virtual object GetFormat( Type formatType )
		{
			object format = null;

			if ( formatType == typeof(NumberFormatInfo) )
				format = NumberFormat;
			else if ( formatType == typeof(DateTimeFormatInfo) )
				format = DateTimeFormat;
			
			return format;
		}
		
		void Construct ()
		{
			construct_internal_locale_from_lcid (cultureID);
			constructed = true;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_internal_locale (string locale);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_locale_from_lcid (int lcid);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_locale_from_name (string name);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool construct_internal_locale_from_specific_name (CultureInfo ci,
				string name);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool construct_internal_locale_from_current_locale (CultureInfo ci);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static CultureInfo [] internal_get_cultures (bool neutral, bool specific, bool installed);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_datetime_format ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_number_format ();

		// Returns false if the culture can not be found, sets is_neutral if it is
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool internal_is_lcid_neutral (int lcid, out bool is_neutral);

		private void ConstructInvariant (bool use_user_override)
		{
			m_isReadOnly=false;
			cultureID=0x7f;
			this.m_useUserOverride=use_user_override;

			/* NumberFormatInfo defaults to the invariant data */
			numInfo=new NumberFormatInfo ();
			
			/* DateTimeFormatInfo defaults to the invariant data */
			dateTimeInfo=new DateTimeFormatInfo ();

			textInfo=new TextInfo ();

			m_name="";
			displayname="Invariant Language (Invariant Country)";
			englishname="Invariant Language (Invariant Country)";
			nativename="Invariant Language (Invariant Country)";
			iso3lang="IVL";
			iso2lang="iv";
			icu_name="en_US_POSIX";
			win3lang="IVL";
		}
		
		public CultureInfo (int culture, bool use_user_override)
		{
			if (culture < 0)
				throw new ArgumentOutOfRangeException ("culture");

			constructed = true;
			
			if(culture==0x007f) {
				/* Short circuit the invariant culture */
				ConstructInvariant (use_user_override);
				return;
			}

			if (!construct_internal_locale_from_lcid (culture))
				throw new ArgumentException ("Culture name " + m_name +
						" is not supported.", "name");
		}

		public CultureInfo (int culture) : this (culture, false) {}
		
		public CultureInfo (string name, bool use_user_override)
		{
			if (name == null)
				throw new ArgumentNullException ();

			constructed = true;
			
			if(name=="") {
				/* Short circuit the invariant culture */
				ConstructInvariant (use_user_override);
				return;
			}

			if (!construct_internal_locale_from_name (name.ToLowerInvariant ()))
				throw new ArgumentException ("Culture name " + name +
						" is not supported.", "name");
		}

		public CultureInfo (string name) : this (name, false) {}

		// This is used when creating by specific name and creating by
		// current locale so we can initialize the object without
		// doing any member initialization
		private CultureInfo () { constructed = true; } 
	}
}
