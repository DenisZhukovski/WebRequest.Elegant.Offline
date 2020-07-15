using System.Threading.Tasks;

namespace WebRequest.Elegant.Offline.DataSync
{
    public interface IDataUploader
    {
        Task SendLocalChangesAsync();
    }
}
