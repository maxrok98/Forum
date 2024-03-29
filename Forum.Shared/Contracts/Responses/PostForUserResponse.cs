﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Shared.Contracts.Responses
{
    public class PostForUserResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public DateTime DateOfEvent { get; set; }
        public int VotesAmount { get; set; }
        public int ComentsAmount { get; set; }
    }
}
