
// This file has been generated by the GUI designer. Do not modify.
namespace SvwiktSuite
{
	public partial class SignInWindow
	{
		private global::Gtk.VBox dialog1_VBox1;
		private global::Gtk.Table table2;
		private global::Gtk.Entry entryPassword;
		private global::Gtk.Entry entryUsername;
		private global::Gtk.Label label1;
		private global::Gtk.Label label2;
		private global::Gtk.Label labelStatus;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget SvwiktSuite.SignInWindow
			this.Name = "SvwiktSuite.SignInWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("Logga in");
			this.Icon = global::Gdk.Pixbuf.LoadFromResource ("SvwiktSuite.icon.png");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child SvwiktSuite.SignInWindow.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.dialog1_VBox1 = new global::Gtk.VBox ();
			this.dialog1_VBox1.Name = "dialog1_VBox1";
			this.dialog1_VBox1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox1.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table (((uint)(3)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.entryPassword = new global::Gtk.Entry ();
			this.entryPassword.CanFocus = true;
			this.entryPassword.Name = "entryPassword";
			this.entryPassword.IsEditable = true;
			this.entryPassword.ActivatesDefault = true;
			this.entryPassword.Visibility = false;
			this.entryPassword.InvisibleChar = '•';
			this.table2.Add (this.entryPassword);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table2 [this.entryPassword]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.entryUsername = new global::Gtk.Entry ();
			this.entryUsername.CanDefault = true;
			this.entryUsername.CanFocus = true;
			this.entryUsername.Name = "entryUsername";
			this.entryUsername.Text = global::Mono.Unix.Catalog.GetString ("Skalbot");
			this.entryUsername.IsEditable = true;
			this.entryUsername.ActivatesDefault = true;
			this.entryUsername.InvisibleChar = '•';
			this.table2.Add (this.entryUsername);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table2 [this.entryUsername]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Användarnamn");
			this.table2.Add (this.label1);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table2 [this.label1]));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Lösenord");
			this.table2.Add (this.label2);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table2 [this.label2]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.labelStatus = new global::Gtk.Label ();
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Xalign = 0F;
			this.labelStatus.UseMarkup = true;
			this.table2.Add (this.labelStatus);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2 [this.labelStatus]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox1.Add (this.table2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox1 [this.table2]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			w1.Add (this.dialog1_VBox1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(w1 [this.dialog1_VBox1]));
			w8.Position = 0;
			w8.Expand = false;
			w8.Fill = false;
			// Internal child SvwiktSuite.SignInWindow.ActionArea
			global::Gtk.HButtonBox w9 = this.ActionArea;
			w9.Name = "dialog1_ActionArea";
			w9.Spacing = 10;
			w9.BorderWidth = ((uint)(5));
			w9.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w10 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w9 [this.buttonCancel]));
			w10.Expand = false;
			w10.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w11 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w9 [this.buttonOk]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 307;
			this.DefaultHeight = 218;
			this.buttonOk.HasDefault = true;
			this.Show ();
			this.buttonCancel.Clicked += new global::System.EventHandler (this.OnButtonCancelClicked);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}
