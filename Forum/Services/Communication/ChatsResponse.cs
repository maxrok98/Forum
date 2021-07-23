﻿using Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Services.Communication
{
    public class ChatsResponse : BaseResponse<IEnumerable<Chat>>
    {
        public ChatsResponse(IEnumerable<Chat> chats) : base(chats) { }
        public ChatsResponse(string message) : base(message) { }
    }
}
