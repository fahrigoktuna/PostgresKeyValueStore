using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PostgresKeyValueStore.Library
{
    [Route("[controller]")]
    [ApiController]
    public class StoreConfigurationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly StoreDbContext _dbContext;

        public StoreConfigurationController(IConfiguration configuration, StoreDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page, int pageSize, string search)
        {
            
            var query = _dbContext.Configurations.AsQueryable().AsNoTracking();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.Key.ToLower().Contains(search.ToLower()) || x.Value.ToLower().Contains(search.ToLower()));
            var totalItemCount = await query.CountAsync();
            var totalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(totalItemCount) / Convert.ToDecimal(pageSize)));
            query = query.OrderBy(x => x.Key).Skip((page - 1) * pageSize).Take(pageSize);
            var configurationList = await query.Select(x => new ConfigurationModel(x.Key, x.Value)).ToListAsync();
            return Ok(new ConfigurationPageModel(configurationList, new ConfigurationPaginationModel(totalPage, page)));
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetByKey(string key)
        {
           
            var configuration = await _dbContext.Configurations.Where(x => x.Key == key).Select(x => new ConfigurationModel(x.Key, x.Value)).FirstOrDefaultAsync();
            if (configuration == null)
                return NotFound();
            return Ok(configuration);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateUpdateConfigurationModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
               
            var configuration = Configuration.Create(request.Key, request.Value);
            await _dbContext.Configurations.AddAsync(configuration);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CreateUpdateConfigurationModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var configuration = await _dbContext.Configurations.Where(x => x.Key == request.Key).AsNoTracking().FirstOrDefaultAsync();
            if (configuration == null)
                return NotFound();

            configuration.ChangeValue(request.Value);
            _dbContext.Update(configuration);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            var configuration = await _dbContext.Configurations.Where(x => x.Key == key).AsNoTracking().FirstOrDefaultAsync();
            if (configuration == null)
                return NotFound();

            _dbContext.Remove(configuration);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
