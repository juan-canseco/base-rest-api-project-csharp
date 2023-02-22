using System.Data;

namespace Products.Application.Interfaces.Persistence
{
    public class DapperConfig
    {
        public string ConnectionString { get; set; } = default!;
    }
    public interface IDapperContext
    {
        IDbConnection CreateConnection();
    }
}
