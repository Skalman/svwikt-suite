using System;
using System.Net;

namespace SvwiktSuite
{
    public partial class SignInWindow : Gtk.Dialog
    {
        protected EditController editCtrl;

        public SignInWindow(EditController editController)
        {
            this.Build();
            entryPassword.GrabFocus();
            editCtrl = editController;
        }

        protected void OnButtonOkClicked(object sender, EventArgs e)
        {
            try
            {
                labelStatus.Text = "Loggar in...";
                if (editCtrl.SignIn(entryUsername.Text, entryPassword.Text))
                {
                    Console.WriteLine("Successfully signed in {0}", editCtrl.SignedInUser);
                    Destroy();
                } else
                {
                    Console.WriteLine("Wrong username/password");
                    labelStatus.Text = "Fel användarnamn/lösenord";
                    entryPassword.Text = "";
                    entryPassword.GrabFocus();
                }
            } catch (WebException ex)
            {
                labelStatus.Text = "Nätverksfel";
            }
        }

        protected void OnButtonCancelClicked(object sender, EventArgs e)
        {
            Destroy();
        }

    }
}

