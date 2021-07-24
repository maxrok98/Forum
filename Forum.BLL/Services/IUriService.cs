﻿using Forum.Shared.Contracts.Requests.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.BLL.Services
{
    public interface IUriService
    {
        Uri GetPostUri(string postId);
        Uri GetAllPostUri(string url, PaginationQuery paginationQuerry = null);
    }
}
