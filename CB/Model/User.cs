﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CB.Model
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string Role { get; set; }

        public bool IsLocked { get; set; }

        public bool IsFirstLogin { get; set; } = true;

        public bool IsWymagania { get; set; } 
    }


}
