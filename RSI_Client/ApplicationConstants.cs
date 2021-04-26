using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSI_Client
{
    public static class ApplicationConstants
    {
        public static string[] userSearches = new string[] { "(Filter) None", "Name", "Book title", "Admin" };
        public static string[] bookSearches = new string[] { "(Filter) None", "Title", "Older than (dd/MM/yyyy)", "Newer than (dd/MM/yyyy)", "Author name" };
        public static string[] authorSearches = new string[] { "(Filter) None", "Name", "Book title" };
    }
}
