using System.ComponentModel.DataAnnotations;

namespace PostgresKeyValueStore.Library
{
    public record ConfigurationModel(string Key, string Value);
    public record ConfigurationPaginationModel(int TotalPages, int CurrentPages);
    public record ConfigurationPageModel(List<ConfigurationModel> Data, ConfigurationPaginationModel Pagination);
    public record CreateUpdateConfigurationModel([Required] string Key, [Required] string Value);
}
