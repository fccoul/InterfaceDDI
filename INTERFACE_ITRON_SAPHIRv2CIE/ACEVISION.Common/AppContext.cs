using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
   public class AppContext
    {
        private static object _token = new object();

        private static User _currentUser;
        public static User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                    _currentUser = new User();
                return _currentUser;
            }
            set
            {
                if (_currentUser == null)
                    _currentUser = new User();

                _currentUser = value;
            }
        }
    }
}
