﻿using Forum.Shared.Contracts;
using Forum.Shared.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Forum.Components.Services
{
    public class PostService : IPostService
    {
        private readonly HttpClient _httpClient;

        public PostService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PageResponse<PostResponse>> loadPosts(string Page, string PageSize, string ThreadId,  string SearchTerm, string OrderBy, string Type, string DaysAtTown)
        {
            return await _httpClient.GetFromJsonAsync<PageResponse<PostResponse>>(ApiRoutes.Posts.GetAll + "/?PageNumber=" + Page + "&PageSize=" + PageSize + "&threadId=" + ThreadId + "&orderBy=" + OrderBy + "&postName=" + SearchTerm + "&type=" + Type + "&daysAtTown=" + DaysAtTown);
        }
    }
}
