﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Forum.DAL.Models;
using Forum.DAL.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Forum.DAL.Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostSorting sorting;
        public PostRepository(ForumAppDbContext context) : base(context)
        {
            sorting = new PostSorting();
        }

        public async Task<int> GetCountOfAllPostsAsync()
        {
            return await _context.Posts.CountAsync();
        }

        public async Task<int> GetCountOfFilteredPostsInThreadAsync(string postName, string threadId, string type, string daysAtTown)
        {
            IQueryable<Post> query = _context.Posts;
            if (type == "event")
                query = _context.Posts.OfType<Event>().Where(p => p.DateOfEvent >= DateTime.Now && p.DateOfEvent < DateTime.Now.AddDays(Convert.ToInt32(daysAtTown)));
            if (type == "place") query = _context.Posts.OfType<Place>();
            if (!string.IsNullOrEmpty(postName))
                query = query.Where(p => p.Name.Contains(postName));
            if (!string.IsNullOrEmpty(threadId))
            {
                StringBuilder whereQuery = new StringBuilder();
                foreach (var thread in threadId.Trim().Split(" "))
                    whereQuery.Append("ThreadId==\"" + thread + "\" || ");

                var whereQ = whereQuery.ToString().Remove(whereQuery.Length - 3);
                query = query.Where(whereQ);
            }
            return await query.CountAsync();
        }

        public override async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts.Include(p => p.Coments).ThenInclude(p => p.SubComents).Include(p => p.Thread).Include(p => p.User).ToListAsync();
        }
        public async Task<IEnumerable<Post>> GetFilteredAndPagedFromThreadAsync(string postName, string threadId, PaginationFilter paginationFilter, string orderByQueryString, string type, string daysAtTown)
        {
            IQueryable<Post> query = _context.Posts;
            if (type == "event")
                query = _context.Posts.OfType<Event>().Where(p => p.DateOfEvent >= DateTime.Now && p.DateOfEvent < DateTime.Now.AddDays(Convert.ToInt32(daysAtTown)));
            if (type == "place") query = _context.Posts.OfType<Place>();
            query = query.Include(p => p.Coments).ThenInclude(p => p.SubComents).Include(p => p.Thread).Include(p => p.User);
            if (!string.IsNullOrEmpty(postName))
                query = query.Where(p => p.Name.Contains(postName));
            if (!string.IsNullOrEmpty(threadId))
            {
                StringBuilder whereQuery = new StringBuilder();
                foreach (var thread in threadId.Trim().Split(" "))
                    whereQuery.Append("ThreadId==\"" + thread + "\" || ");

                var whereQ = whereQuery.ToString().Remove(whereQuery.Length - 3);
                query = query.Where(whereQ);
            }
            query = query.OrderBy(sorting.ApplySort(orderByQueryString));
            if (paginationFilter != null)
            {
                var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
                query = query.Skip(skip).Take(paginationFilter.PageSize);
            }
            return await query.AsNoTracking().ToListAsync();

        }

        public override async Task<Post> GetAsync(string id)
        {
            return await (from p in _context.Posts
                          where p.Id == id
                          select p).Include(p => p.Thread).Include(p => p.User).Include(p => p.Coments).ThenInclude(x => x.User).FirstOrDefaultAsync();
        }
        public async Task<Post> UserOwnsPostAsync(string PostId, string UserId)
        {
            return await (from p in _context.Posts
                          where p.Id == PostId
                          where p.UserId == UserId
                          select p).FirstAsync();
        }
    }
}
