using System;
using Gtk;
using Mono.Data.Sqlite;
using System.Threading;
using System.Collections.Generic;

namespace SvwiktSuite
{
    public partial class MainWindow: Gtk.Window
    {
        private EditController editCtrl;
        private EditController.OptionsStruct editOpts;

        public MainWindow(
            EditController editController)
            : base (Gtk.WindowType.Toplevel)
        {
            Build();
            editCtrl = editController;
            editOpts = editCtrl.Options;
            editOpts.OnLog = Log;
            editOpts.OnPageDone = PageDoneCallback;
            editOpts.OnSave = SaveCallback;
            editOpts.OnEditDone = EditDoneCallback;

            OnEntryStartChanged(null, null);
            OnEntryMaxChanged(null, null);
        }

        protected void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
            a.RetVal = true;
            editCtrl.Quit();
        }

        protected void OnBtnUpdateClicked(object sender, EventArgs e)
        {
            SelectSource(true);
            try
            {
                editCtrl.Edit();

                btnUpdate.Sensitive = false;
                buttonCancel.Sensitive = true;
                buttonCancel.GrabFocus();
                btnUpdate.HasDefault = false;
                buttonApproveChange.HasDefault = true;

            } catch (MediaWikiApi.NotLoggedInException)
            {
                var signInWindow = new SignInWindow(editCtrl);
                signInWindow.Show();
                signInWindow.Destroyed += delegate(object sender2, EventArgs e2)
                {
                    if (editCtrl.IsLoggedIn)
                        OnBtnUpdateClicked(sender2, e2);
                };
            }
        }

        protected void Log(string message)
        {
            Gtk.Application.Invoke(delegate
            {
                if (textviewLog.Buffer.CharCount > 15000)
                {
                    textviewLog.Buffer.Text = message + "\n" + textviewLog.Buffer.Text.Substring(0, 10000) + "...";
                } else
                {
                    textviewLog.Buffer.Text = message + "\n" + textviewLog.Buffer.Text;
                }
            }
            );
            Console.WriteLine(message);
        }

        protected void PageDoneCallback(string title, bool addExclamation=true)
        {
            Gtk.Application.Invoke(delegate
            {
                if (addExclamation)
                    entryStart.Text = title + "!";
                else
                    entryStart.Text = title;
            }
            );
        }

        protected void EditDoneCallback()
        {
            Gtk.Application.Invoke(delegate
            {
                entrySummary.Text = "";
                textviewBefore.Buffer.Text = "";
                textviewAfter.Buffer.Text = "";
                vboxConfirmEdit.Sensitive = false;

                buttonCancel.Sensitive = false;
                btnUpdate.Sensitive = true;
                btnUpdate.GrabFocus();
                btnUpdate.HasDefault = true;
                buttonApproveChange.HasDefault = false;
            }
            );
        }

        protected volatile string saveCallbackAnswer = null;

        protected bool SaveCallback(string title,
                                 string summary, out string changedSummary,
                                 string before, string after, out string changedWikitext)
        {
            PageDoneCallback(title, addExclamation: false);
            if (!checkbuttonConfirm.Active)
            {
                changedSummary = summary;
                changedWikitext = after;
                return true;
            }

            Gtk.Application.Invoke(delegate
            {
                entrySummary.Text = summary;
                textviewBefore.Buffer.Text = before;
                textviewAfter.Buffer.Text = after;
                vboxConfirmEdit.Sensitive = true;
                entrySummary.GrabFocus();
            }
            );
            saveCallbackAnswer = null;

            /*
             * Trying to use thread interruption caused exceptions
             * in Api.Save().
             */

            while (saveCallbackAnswer == null)
            {
                Thread.Sleep(300);
            }
            changedSummary = entrySummary.Text;
            changedWikitext = textviewAfter.Buffer.Text;
            Gtk.Application.Invoke(delegate
            {
                vboxConfirmEdit.Sensitive = false;
                entrySummary.Text = "";
                textviewBefore.Buffer.Text = "";
                textviewAfter.Buffer.Text = "";
            }
            );
            return saveCallbackAnswer == "save";
        }

        protected void OnButtonCancelClicked(object sender, EventArgs e)
        {
            editCtrl.InterruptEdit();
        }

        protected void OnButtonSkipClicked(object sender, EventArgs e)
        {
            saveCallbackAnswer = "skip";
        }

        protected void OnButtonApproveChangeClicked(object sender, EventArgs e)
        {
            saveCallbackAnswer = "save";
        }

        protected void OnButtonSpecifySourceClicked(object sender, EventArgs e)
        {
            SelectSource(false);
        }

        protected void OnComboboxSourceChanged(object sender, EventArgs e)
        {
            SelectSource(true);
        }

        protected void SelectSource(bool useExistingSelection)
        {
            if (comboboxSource.Active == 0)
            {
                editOpts.Source = EditController.SourceType.Dump;
                if (!useExistingSelection || editOpts.DumpFilename == null)
                {
                    var fc = new FileChooserDialog(
                    "Välj dump-fil",
                    this,
                    FileChooserAction.Open,
                    "Avbryt", ResponseType.Cancel,
                    "Öppna", ResponseType.Accept);

                    if (fc.Run() == (int)ResponseType.Accept)
                        editOpts.DumpFilename = fc.Filename;

                    fc.Destroy();
                }

                if (editOpts.DumpFilename == null)
                {
                    buttonSpecifySource.Label = "...";
                } else
                {
                    if (editOpts.DumpFilename.Length < 18)
                        buttonSpecifySource.Label = editOpts.DumpFilename;
                    else
                        buttonSpecifySource.Label = "..." +
                            editOpts.DumpFilename.Substring(editOpts.DumpFilename.Length - 18);
                }
                buttonSpecifySource.Sensitive = false;
            } else if (comboboxSource.Active == 1)
            {
                // editController.Options.Source = EditController.SourceType.Titles;
                throw new NotImplementedException();
            } else if (comboboxSource.Active == 2)
            {
                editOpts.Source = EditController.SourceType.Live;
                buttonSpecifySource.Label = "Från kategori";
                buttonSpecifySource.Sensitive = false;
            }
        }

        protected void OnEntryMaxChanged(object sender, EventArgs e)
        {
            int val;

            if (entryMax.Text == "" || (int.TryParse(entryMax.Text, out val) && val >= -1))
            {
                editOpts.MaxPages = entryMax.Text == "" ? -1 : val;
                entryMax.ModifyBase(StateType.Normal);
            } else
            {
                editOpts.MaxPages = 0;
                entryMax.ModifyBase(StateType.Normal, new Gdk.Color(255, 204, 204));
            }
        }

        protected void OnEntryStartChanged(object sender, EventArgs e)
        {
            editOpts.StartAt = entryStart.Text;
        }
    }
}
