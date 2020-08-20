﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models;
using Forum.Services.Communication;
using Forum.Repositories;

namespace Forum.Services
{
    public class ThreadService : IThreadService
    {
        private readonly IThreadRepository _threadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISubscriptionRepository _subscriptionRepository;
        public ThreadService(IThreadRepository threadRepository, ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork)
        {
            _threadRepository = threadRepository;
            _unitOfWork = unitOfWork;
            _subscriptionRepository = subscriptionRepository;
        }
        public async Task<ThreadResponse> RemoveAsync(string id)
        {
            var existingThread = await _threadRepository.GetAsync(id);

            if (existingThread == null)
                return new ThreadResponse("Thread not found");

            try
            {
                _threadRepository.Remove(existingThread);
                await _unitOfWork.CompleteAsync();

                return new ThreadResponse(existingThread);
            }
            catch(Exception ex)
            {
                return new ThreadResponse($"An error occurred when deleting the thread: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Thread>> GetAllAsync()
        {
            return await _threadRepository.GetAllAsync();
        }

        public async Task<Thread> GetAsync(string id)
        {
            return await _threadRepository.GetAsync(id);
        }

        public async Task<ThreadResponse> AddAsync(Thread thread)
        {
            thread.Id = Guid.NewGuid().ToString();
            try
            {
                await _threadRepository.AddAsync(thread);
                await _unitOfWork.CompleteAsync();

                return new ThreadResponse(thread);
            }
            catch (Exception ex)
            {
                return new ThreadResponse($"An error occurred when saving the thread: {ex.Message}");
            }
        }

        public async Task<ThreadResponse> UpdateAsync(string id, Thread thread)
        {
            var existingThread = await _threadRepository.GetAsync(id);

            if (existingThread == null)
                return new ThreadResponse("Thread not found.");

            existingThread.Name = thread.Name ?? existingThread.Name;
            existingThread.Description = thread.Description ?? existingThread.Description;
            existingThread.ImageId = thread.ImageId ?? existingThread.ImageId;

            try
            {
                _threadRepository.Update(existingThread);
                await _unitOfWork.CompleteAsync();

                return new ThreadResponse(existingThread);
            }
            catch (Exception ex)
            {
                return new ThreadResponse($"An error occurred when updating the thread: {ex.Message}");
            }

        }

        public async Task<SubscriptionResponse> Subscribe(string userId, string threadId)
        {
            var thread = await _threadRepository.GetAsync(threadId);

            if (thread == null)
                return new SubscriptionResponse("Thread not found.");

            var sub = await _subscriptionRepository.FindInstance(userId, threadId);
            if (sub != null)
                return new SubscriptionResponse("You already subscribed.");

            var Sub = new Subscription { ThreadId = threadId, UserId = userId };
            try
            {
                await _subscriptionRepository.AddAsync(Sub);
                await _unitOfWork.CompleteAsync();

                return new SubscriptionResponse(Sub);
            }
            catch(Exception ex)
            {
                return new SubscriptionResponse($"An error occurred when saving the subscription: {ex.Message}");
            }

        }

        public async Task<SubscriptionResponse> UnSubscribe(string userId, string threadId)
        {
            var thread = await _threadRepository.GetAsync(threadId);

            if (thread == null)
                return new SubscriptionResponse("Thread not found.");

            var sub = await _subscriptionRepository.FindInstance(userId, threadId);
            if (sub == null)
                return new SubscriptionResponse("You are not subscribed.");

            var Sub = new Subscription { ThreadId = threadId, UserId = userId };
            try
            {
                _subscriptionRepository.Remove(Sub);
                await _unitOfWork.CompleteAsync();

                return new SubscriptionResponse(Sub);
            }
            catch(Exception ex)
            {
                return new SubscriptionResponse($"An error occurred when deleting the subscription: {ex.Message}");
            }
        }
    }
}
