﻿using Forum.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.BLL.Services.Communication
{
    public class UsersResponse : BaseResponse<IEnumerable<User>>
    {
        public int amountOfUsers { get; set; }
        public UsersResponse(IEnumerable<User> users, int amountOfUsers) : base(users)
        {
            this.amountOfUsers = amountOfUsers;
        }
        public UsersResponse(string message) : base(message) { }
    }
}
