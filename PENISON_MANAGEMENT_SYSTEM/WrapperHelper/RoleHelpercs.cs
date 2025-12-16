using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.WrapperHelper
{
    public static class RoleHelper
    {
        public static string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case 1: return "Admin";
                case 2: return "Staff";
                case 3: return "Manager";
                default: return "Guest";
            }
        }
    }

}