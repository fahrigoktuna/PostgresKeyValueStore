using System.ComponentModel.DataAnnotations;

namespace PostgresKeyValueStore.Library
{
    public class StoreOptions
    {
        [Required]
        public string ConnectionString { get; set; } = default!;
    }
}
