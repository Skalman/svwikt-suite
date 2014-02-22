using System;
using System.Threading;
using System.Collections.Generic;

namespace SvwiktSuite
{
    public class EditController
    {
        private MediaWikiApi mwApi;
        private Thread thread;

        public Language Language { get; private set; }

        public OptionsStruct Options { get; private set; }

        protected TranslationEditor translationEditor;

        public EditController(MediaWikiApi mediaWikiApi)
        {
            mwApi = mediaWikiApi;
            thread = null;
            Language = new Language(mwApi);
            Options = new OptionsStruct();
            translationEditor = new TranslationEditor(this, mwApi);
        }

        public enum SourceType
        {
            Dump,
            Live,
            Titles }
        ;

        public class OptionsStruct
        {
            public int MaxPages = 1;
            public string StartAt = "";
            public SourceType Source = SourceType.Dump;
            public string DumpFilename = null;

            public delegate bool SaveCallback(Page page);

            public SaveCallback OnSave = null;
            public TranslationEditor.PageDoneCallback OnPageDone = null;
            public TranslationEditor.LogCallback OnLog = null;

            public delegate void EditDoneCallback();

            public EditDoneCallback OnEditDone = null;
        }

        public bool IsLoggedIn
        {
            get
            {
                return mwApi.SignedInUser != null;
            }
        }

        public string SignedInUser
        {
            get
            {
                return mwApi.SignedInUser;
            }
        }

        public void Quit()
        {
            InterruptEdit();
        }

        public void Edit()
        {
            if (!IsLoggedIn)
                throw new MediaWikiApi.NotLoggedInException();
            if (thread != null)
                throw new Exception("Already running thread.");

            Console.WriteLine("signed in as {0}", mwApi.SignedInUser);

            thread = new Thread(new ThreadStart(EditStart));
            thread.Start();
        }

        public void InterruptEdit()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Interrupt();
                if (Options.OnEditDone != null)
                    Options.OnEditDone();
            }
            thread = null;
        }

        protected void EditStart()
        {
            try
            {
                IEnumerable<Page> pages;
                var maxPages = Options.MaxPages;
                var startAt = Options.StartAt;
                if (Options.Source == SourceType.Dump)
                {
                    var dr = new DumpReader(Options.DumpFilename);
                    pages = dr.Pages(
                        namespaces: new SortedSet<int>() {0},
                        startAt: startAt,
                        maxPages: maxPages
                    );
                } else if (Options.Source == SourceType.Live)
                {
                    pages = mwApi.PagesInCategory(
                    "Svenska/Alla uppslag",
                    ns: 0,
                    step: 250,
                    maxPages: maxPages,
                    startAt: startAt
                    );
                } else if (Options.Source == SourceType.Titles)
                {
                    // TODO
                    // pages = Api.GetPages();
                    throw new NotImplementedException();
                } else
                {
                    throw new Exception("Internal error: Invalid value of Options.Source");
                }
                Edit(pages);
            } catch (ThreadInterruptedException)
            {
                Console.WriteLine("Thread interrupted [no worries]");
            } catch (Language.LanguageException ex)
            {
                LogAll("Language exception: {0}", ex.Message);
            } catch (TranslationEditor.SortException ex)
            {
                LogAll("Sort exception: {0}", ex.Message);
            } catch (MediaWikiApi.NotLoggedInException)
            {
                Log("Abort: Not logged in!");
            }
            Console.WriteLine("Thread finished");
            if (Options.OnEditDone != null)
                Options.OnEditDone();
        }

        protected void Edit(IEnumerable<Page> pages)
        {
            Log("EDIT PAGES");

            translationEditor.OnLog = Log;

            var step = 2000;

            var pagesInBatch = new List<Page>();
            foreach (var page in pages)
            {
                pagesInBatch.Add(page);
                if (pagesInBatch.Count == step)
                {
                    EditBatch(pagesInBatch);
                    pagesInBatch.Clear();
                }
            }
            if (pagesInBatch.Count != 0)
                EditBatch(pagesInBatch);
        }

        protected void EditBatch(IList<Page> batch)
        {
            Console.WriteLine("EDIT BATCH");

            translationEditor.EditBatch(batch);
            SaveBatch(batch);
        }

        protected void SaveBatch(IList<Page> batch)
        {
            foreach (var page in batch)
            {
                if (page.Changed)
                {
                    var proposedText = page.Text;
                    if (Options.OnSave == null || Options.OnSave(page))
                    {
                        try
                        {
                            mwApi.SavePage(
                                page,
                                nocreate: true,
                                bot: proposedText == page.Text);
                            LogAll("{0}: Saved ({1})",
                                     page.Title,
                                     page.Summary.Text);
                        } catch (MediaWikiApi.EditConflictException)
                        {
                            LogAll("{0}: Edit conflict, skipped ({1})",
                                      page.Title,
                                      page.Summary.Text);
                        }

                    } else
                    {
                        LogAll("{0}: Chose not to save ({1})",
                                 page.Title,
                                 page.Summary.Text);
                    }
                } else
                {
                    Console.WriteLine("{0}: No update",
                                      page.Title);
                }
            }

        }

        public bool SignIn(string username, string password)
        {
            return mwApi.SignIn(username, password);
        }

        protected void LogAll(string message, params object[] args)
        {
            Log(string.Format(message, args));
        }

        protected void Log(string message)
        {
            if (Options.OnLog != null)
                Options.OnLog(message);
        }
    }
}

