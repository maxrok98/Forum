﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.DAL.Models;
using Forum.Services.Communication;

namespace Forum.Services
{
    public interface IThreadService
    {
        Task<IEnumerable<Thread>> GetAllAsync();
        Task<Thread> GetAsync(string id);
        Task<ThreadResponse> AddAsync(Thread thread);
        Task<ThreadResponse> UpdateAsync(string id, Thread thread);
        Task<ThreadResponse> RemoveAsync(string id);
        Task<SubscriptionResponse> Subscribe(string userId, string threadId);
        Task<SubscriptionResponse> UnSubscribe(string userId, string threadId);
    }
}
