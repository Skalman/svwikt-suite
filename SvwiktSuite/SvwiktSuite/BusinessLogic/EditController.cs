using System;
using System.Threading;
using System.Collections.Generic;

namespace SvwiktSuite
{
    public class EditController
    {
        private MediaWikiApi mwApi;
        private Thread thread;

        public OptionsStruct Options { get; private set; }

        public EditController(MediaWikiApi mediaWikiApi)
        {
            mwApi = mediaWikiApi;
            thread = null;
            Options = new OptionsStruct();
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
            public TranslationLinkUpdater.SaveCallback OnSave = null;
            public TranslationLinkUpdater.PageDoneCallback OnPageDone = null;
            public TranslationLinkUpdater.LogCallback OnLog = null;

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

            thread = new Thread(new ThreadStart(RealEdit));
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

        protected void RealEdit()
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
                var tl = new TranslationLinkUpdater(
                    api: mwApi,
                    saveCallback: Options.OnSave,
                    pageDoneCallback: Options.OnPageDone,
                    logCallback: Options.OnLog);
                tl.Update(pages: pages);
            } catch (ThreadInterruptedException)
            {
                Console.WriteLine("Thread interrupted [no worries]");
            } catch (Language.LanguageException ex)
            {
                LogAll("Language exception: {0}", ex.Message);
            } catch (TranslationLinkUpdater.SortException ex)
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

