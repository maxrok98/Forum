﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Shared.Contracts.Requests;
using Forum.DAL.Models;
using Forum.BLL.Services.Communication;
using Forum.Shared.Contracts;

namespace Forum.BLL.Services
{
    public interface IPostService
    {
        Task<PostsResponse> GetAllAsync(string postName = null, string threadId = null, PaginationFilter paginationFilter = null, string orderByQueryString = null, string type = null, string daysAtTown = null);
        Task<PostResponse> GetAsync(string id);
        Task<int> GetCountOfAllPostsAsync();
        Task<PostResponse> AddAsync(Post post, PostType postType);
        Task<PostResponse> UpdateAsync(string id, Post post);
        Task<PostResponse> RemoveAsync(string id);
        Task<VoteResponse> Vote(string PostId, string UserId);
        Task<VoteResponse> UnVote(string PostId, string UserId);
        Task<CalendarResponse> AddToCalendar(string PostId, string UserId);
        Task<CalendarResponse> RemoveFromCalendar(string PostId, string UserId);
        Task<bool> UserOwnsPostAsync(string PostId, string UserId);
    }
}
