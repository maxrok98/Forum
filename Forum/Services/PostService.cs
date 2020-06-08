﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models;
using Forum.Services.Communication;
using Forum.Repositories;
using Forum.Contracts;

namespace Forum.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IUnitOfWork _unitOfWork;
        public PostService(IPostRepository postRepository, IVoteRepository voteRepository, IUnitOfWork unitOfWork)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _voteRepository = voteRepository;
        }

        public async Task<IEnumerable<Post>> GetAllAsync(string postName = null, PaginationFilter paginationFilter = null)
        {
            if(paginationFilter == null)
            {
                return await _postRepository.GetAllAsync(); 
            }
            if(!string.IsNullOrEmpty(postName))
            {
                return await _postRepository.GetFilteredAsync(postName);
            }
            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            return await _postRepository.GetPaged(paginationFilter, skip);
        }

        public async Task<PostResponse> GetAsync(string id)
        {
            var existingPost = await _postRepository.GetAsync(id);

            if (existingPost == null)
                return new PostResponse("Post not found.");

            return new PostResponse(existingPost);
        }

        public async Task<IEnumerable<Post>> GetOrderByDateAsync()
        {
            return await _postRepository.GetOrderByDateAsync();
        }

        public async Task<IEnumerable<Post>> GetOrderByVoteAsync()
        {
            return await _postRepository.GetOrderByVoteAsync(); 
        }

        public async Task<PostResponse> RemoveAsync(string id)
        {
            var existingPost = await _postRepository.GetAsync(id);

            if (existingPost == null)
                return new PostResponse("Post not found.");

            try
            {
                _postRepository.Remove(existingPost);
                await _unitOfWork.CompleteAsync();

                return new PostResponse(existingPost);
            }
            catch (Exception ex)
            {
                return new PostResponse($"An error occurred when deleting the post: {ex.Message}");
            }
        }

        public async Task<PostResponse> UpdateAsync(string id, Post post)
        {
            var existingPost = await _postRepository.GetAsync(id);

            if (existingPost == null)
                return new PostResponse("Post not found.");

            existingPost.Name = post.Name ?? existingPost.Name;
            existingPost.Content = post.Content ?? existingPost.Content;
            existingPost.ImageId = post.ImageId ?? existingPost.ImageId;

            try
            {
                _postRepository.Update(existingPost);
                await _unitOfWork.CompleteAsync();

                return new PostResponse(existingPost);
            }
            catch (Exception ex)
            {
                return new PostResponse($"An error occurred when updating the post: {ex.Message}");
            }
            
        }

        public async Task<PostResponse> AddAsync(Post post)
        {
            post.Id = Guid.NewGuid().ToString();
            try
            {
                await _postRepository.AddAsync(post);
                await _unitOfWork.CompleteAsync();

                return new PostResponse(post);
            }
            catch(Exception ex)
            {
                return new PostResponse($"An error occurred when saving the post: {ex.Message}");
            }
        }

        public async Task<bool> UserOwnsPostAsync(string PostId, string UserId)
        {
            var post = await _postRepository.UserOwnsPostAsync(PostId, UserId);

            if (post == null)
                return false;
            if (post.UserId != UserId)
                return false;

            return true;
        }

        public async Task<VoteResponse> Vote(string PostId, string UserId)
        {
            var post = await _postRepository.GetAsync(PostId);

            if (post == null)
                return new VoteResponse("Post not found.");

            var vote = await _voteRepository.FindInstance(PostId, UserId);
            if (vote != null)
                return new VoteResponse("You already voted.");

            var Vote = new Vote { PostId = PostId, UserId = UserId };
            try
            {
                await _voteRepository.AddAsync(Vote);
                post.Rating += 1;
                await _unitOfWork.CompleteAsync();

                return new VoteResponse(Vote);
            }
            catch(Exception ex)
            {
                return new VoteResponse($"An error occurred when saving the vote: {ex.Message}");
            }
        }

        public async Task<VoteResponse> UnVote(string PostId, string UserId)
        {
            var post = await _postRepository.GetAsync(PostId);

            if (post == null)
                return new VoteResponse("Post not found.");

            var vote = await _voteRepository.FindInstance(PostId, UserId);
            if (vote == null)
                return new VoteResponse("You did not vote.");

            try
            {
                _voteRepository.Remove(vote);
                post.Rating -= 1;
                await _unitOfWork.CompleteAsync();

                return new VoteResponse(vote);
            }
            catch(Exception ex)
            {
                return new VoteResponse($"An error occurred when deleting the vote: {ex.Message}");
            }

        }
    }
}
