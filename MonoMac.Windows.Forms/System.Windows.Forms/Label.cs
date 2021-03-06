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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Peter Bartok, pbartok@novell.com
//
//

// COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
//TODO: Comented out this line
//using System.Windows.Forms.Theming;

namespace System.Windows.Forms
{
	[DefaultProperty ("Text")]
	[Designer ("System.Windows.Forms.Design.LabelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[ToolboxItem ("System.Windows.Forms.Design.AutoSizeToolboxItem," + Consts.AssemblySystem_Design)]
	[DefaultBindingProperty ("Text")]
#endif
	public partial class Label : Control
	{
		private bool autosize;
		private bool auto_ellipsis;
		private Image image;
		private bool render_transparent;
		private FlatStyle flat_style;
		private bool use_mnemonic;
		private int image_index = -1;
#if NET_2_0
		private string image_key = string.Empty;
#endif
		private ImageList image_list;
		internal ContentAlignment image_align;
		internal StringFormat string_format;
		internal ContentAlignment text_align;
		static SizeF req_witdthsize = new SizeF (0,0);

		#region Events
		static object AutoSizeChangedEvent = new object ();
		static object TextAlignChangedEvent = new object ();

#if NET_2_0

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
#endif
		public
#if NET_2_0
		new
#endif
		event EventHandler AutoSizeChanged {
			add { Events.AddHandler (AutoSizeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
#endif

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		public event EventHandler TextAlignChanged {
			add { Events.AddHandler (TextAlignChangedEvent, value); }
			remove { Events.RemoveHandler (TextAlignChangedEvent, value); }
		}
		#endregion


		#region Public Properties

		[DefaultValue (false)]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
#if NET_2_0
		public
#else
		internal
#endif
		bool AutoEllipsis {
			get { return this.auto_ellipsis; }
			set
			{
				if (this.auto_ellipsis != value) {
					this.auto_ellipsis = value;

					if (this.auto_ellipsis)
						string_format.Trimming = StringTrimming.EllipsisCharacter;
					else
						string_format.Trimming = StringTrimming.Character;

					if (Parent != null)
						Parent.PerformLayout (this, "AutoEllipsis");
					this.Invalidate ();
				}
			}
		}

#if NET_2_0
		[Browsable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[EditorBrowsable (EditorBrowsableState.Always)]
#endif
		[DefaultValue(false)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public
#if NET_2_0
		override
#else
		virtual
#endif
		bool AutoSize {
			get { return autosize; }
			set {
				if (autosize == value)
					return;

#if NET_2_0
				base.SetAutoSizeMode (AutoSizeMode.GrowAndShrink);
				base.AutoSize = value;
#endif
				autosize = value;
				CalcAutoSize ();
				Invalidate ();

				OnAutoSizeChanged (EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				base.BackgroundImage = value;
				Invalidate ();
			}
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}
#endif
	
		[DefaultValue(BorderStyle.None)]
		[DispId(-504)]
		public virtual BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}


		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable;}
		}

#if NET_2_0
		protected override Padding DefaultMargin {
			get { return new Padding (3, 0, 3, 0); }
		}
#endif

		[DefaultValue(FlatStyle.Standard)]
		public FlatStyle FlatStyle {
			get { return flat_style; }
			set {
				if (!Enum.IsDefined (typeof (FlatStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for FlatStyle", value));

				if (flat_style == value)
					return;

				flat_style = value;
				if (Parent != null)
					Parent.PerformLayout (this, "FlatStyle");
				Invalidate ();
			}
		}

		[Localizable(true)]
		public Image Image {
			get {
				if (this.image != null)
					return this.image;

				if (this.image_index >= 0)
					if (this.image_list != null)
						return this.image_list.Images[this.image_index];

#if NET_2_0
				if (!string.IsNullOrEmpty (this.image_key))
					if (this.image_list != null)
						return this.image_list.Images[this.image_key];
#endif
				return null;
			}
			set {
				if (this.image != value) {
					this.image = value;
					this.image_index = -1;
#if NET_2_0
					this.image_key = string.Empty;
#endif
					this.image_list = null;

#if NET_2_0
					if (this.AutoSize && this.Parent != null)
						this.Parent.PerformLayout (this, "Image");
#endif

					Invalidate ();
				}
			}
		}

		[DefaultValue(ContentAlignment.MiddleCenter)]
		[Localizable(true)]
		public ContentAlignment ImageAlign {
			get { return image_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ContentAlignment", value));

				if (image_align == value)
					return;

				image_align = value;
				Invalidate ();
			}
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		[TypeConverter (typeof (ImageIndexConverter))]
#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#endif
		public int ImageIndex {
			get { 
				if (ImageList == null) {
					return -1;
				}
					
				if (image_index >= image_list.Images.Count) {
					return image_list.Images.Count - 1;
				}
					
				return image_index;
			}
			set {

				if (value < -1)
					throw new ArgumentException ();

				if (this.image_index != value) {
					this.image_index = value;
					this.image = null;
#if NET_2_0
					this.image_key = string.Empty;
#endif
					Invalidate ();
				}
			}
		}

#if NET_2_0
		[Localizable (true)]
		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (ImageKeyConverter))]
		public string ImageKey {
			get { return this.image_key; }
			set {
				if (this.image_key != value) {
					this.image = null;
					this.image_index = -1;
					this.image_key = value;
					this.Invalidate ();
				}
			}
		}
#endif

		[DefaultValue(null)]
#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#endif
		public ImageList ImageList {
			get { return image_list;}
			set {
				if (image_list == value)
					return;
					
				image_list = value;

				if (image_list != null && image_index !=-1)
					Image = null;

				Invalidate ();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}


#if NET_2_0
		public override	Size GetPreferredSize (Size proposedSize)
		{
			return InternalGetPreferredSize (proposedSize);
		}
#endif

		[Browsable(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual int PreferredHeight {
			get { return InternalGetPreferredSize (Size.Empty).Height; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual int PreferredWidth {
			get { return InternalGetPreferredSize (Size.Empty).Width; }
		}

#if NET_2_0
		[Obsolete ("This property has been deprecated.  Use BackColor instead.")]
#endif
		protected virtual bool RenderTransparent {
			get { return render_transparent; }
			set { render_transparent = value;}
		}
			
		[Browsable(false)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop  {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[DefaultValue(true)]
		public bool UseMnemonic {
			get { return use_mnemonic; }
			set {
				if (use_mnemonic != value) {
					use_mnemonic = value;
					SetUseMnemonic (use_mnemonic);
					Invalidate ();
				}
			}
		}

		#endregion

		#region Public Methods

		protected Rectangle CalcImageRenderBounds (Image image, Rectangle r, ContentAlignment align)
		{
			Rectangle rcImageClip = r;
			rcImageClip.Inflate (-2,-2);

			int X = r.X;
			int Y = r.Y;

			if (align == ContentAlignment.TopCenter ||
				align == ContentAlignment.MiddleCenter ||
				align == ContentAlignment.BottomCenter) {
				X += (r.Width - image.Width) / 2;
			} else if (align == ContentAlignment.TopRight ||
				align == ContentAlignment.MiddleRight||
				align == ContentAlignment.BottomRight) {
				X += (r.Width - image.Width);
			}

			if( align == ContentAlignment.BottomCenter ||
				align == ContentAlignment.BottomLeft ||
				align == ContentAlignment.BottomRight) {
				Y += r.Height - image.Height;
			} else if(align == ContentAlignment.MiddleCenter ||
					align == ContentAlignment.MiddleLeft ||
					align == ContentAlignment.MiddleRight) {
				Y += (r.Height - image.Height) / 2;
			}

			rcImageClip.X = X;
			rcImageClip.Y = Y;
			rcImageClip.Width = image.Width;
			rcImageClip.Height = image.Height;

			return rcImageClip;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose (disposing);

			if (disposing)
				string_format.Dispose ();
		}


#if !NET_2_0
		protected virtual void OnAutoSizeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AutoSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			if (autosize)
				CalcAutoSize();
			Invalidate ();
		}

#if NET_2_0
		protected override void OnPaddingChanged (EventArgs e)
		{
			base.OnPaddingChanged (e);
		}
#endif

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

#if NET_2_0
		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}
#endif

		protected virtual void OnTextAlignChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [TextAlignChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
			if (autosize)
				CalcAutoSize ();
			Invalidate ();
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			if (IsMnemonic (charCode, Text)) {
				// Select item next in line in tab order
				if (this.Parent != null)
					Parent.SelectNextControl(this, true, false, false, false);
				return true;
			}
			
			return base.ProcessMnemonic (charCode);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		public override string ToString()
		{
			return base.ToString () + ", Text: " + Text;
		}


		#endregion Public Methods

		#region Private Methods

		private void OnHandleCreatedLB (Object o, EventArgs e)
		{
			if (autosize)
				CalcAutoSize ();
		}

		private void SetUseMnemonic (bool use)
		{
			if (use)
				string_format.HotkeyPrefix = HotkeyPrefix.Show;
			else
				string_format.HotkeyPrefix = HotkeyPrefix.None;
		}

		#endregion Private Methods
#if NET_2_0
		[DefaultValue (false)]
		public bool UseCompatibleTextRendering {
			get { return use_compatible_text_rendering; }
			set { use_compatible_text_rendering = value; }
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}
#endif
	}
}
