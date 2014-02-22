using System;
using System.Collections.Generic;

namespace SvwiktSuite
{
    public interface IBatchEditor
    {
        void EditBatch(IList<Page> batch);
    }
}

