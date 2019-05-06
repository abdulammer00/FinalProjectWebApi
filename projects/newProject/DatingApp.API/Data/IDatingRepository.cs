using DatingApp.API.Helpers;
using DatingApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
              void Add<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;

        Task<bool> SaveAll();

        Task<PagedList<User>> GetUsers(UserParams userParams);

        Task<User> GetUser(int id, bool isCurrentUser);

        Task<Photo> GetPhoto(int id);

        Task<Photo> GetMainPhotoForUser(int userId);

        Task<Like> GetLike(int userId, int recipientId);

        /// <summary>
        /// Get the specific message based on Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Message> GetMessage(int id);

        /// <summary>
        /// This method will support the inbox and outbox for the messages
        /// </summary>
        /// <returns></returns>
        Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);

        /// <summary>
        /// This method will be the conversation between two users.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="recipientId"></param>
        /// <returns></returns>
        Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}
