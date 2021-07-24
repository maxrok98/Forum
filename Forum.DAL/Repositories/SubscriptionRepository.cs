﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Forum.DAL.Repositories
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ForumAppDbContext context) : base(context) { }
        public async Task<Subscription> FindInstance(string UserId, string ThreadId)
        {
            return await (from sub in _context.Subscriptions
                          where sub.ThreadId == ThreadId
                          where sub.UserId == UserId
                          select sub).FirstOrDefaultAsync();

        }
    }
}
