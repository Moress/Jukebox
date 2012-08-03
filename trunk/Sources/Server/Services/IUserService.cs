using System.ServiceModel;
using Jukebox.Server.Models;

namespace Jukebox.Server.Services
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    interface IUserService
    {
        [OperationContract]
        User GetUser();
    }
}
