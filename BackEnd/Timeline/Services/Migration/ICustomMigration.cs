using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.Migration
{
    public interface ICustomMigration
    {
        string GetName();
        Task Execute(DatabaseContext database);
    }
}
