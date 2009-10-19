using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.DAO;

namespace Parser.Interfaces
{

    public interface IParser
    {
        string Website { get;  }

        void Parse();        

        event EventHandler DataAvailable;

        List<Results> RetrieveData();
    }
}
