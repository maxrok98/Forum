﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Forum.DAL.Repositories
{
    public class VoteRepository : Repository<Vote>, IVoteRepository
    {
        public VoteRepository(ForumAppDbContext context) : base(context) { }

        public async Task<Vote> FindInstance(string PostId, string UserId)
        {
            return await (from vote in _context.Votes
                          where vote.PostId == PostId
                          where vote.UserId == UserId
                          select vote).FirstOrDefaultAsync();
        }
    }
}
