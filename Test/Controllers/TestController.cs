using EF.Support.RepositoryAsync;

using Microsoft.AspNetCore.Mvc;

using Test.Entity;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IRepositoryAsync<NguyenEntity> _nguyenRepository;
        private readonly IRepositoryAsync<Testtiep> _fullAuditedEntityRepository;

        public TestController(IRepositoryAsync<NguyenEntity> nguyenRepository, IRepositoryAsync<Testtiep> fullAuditedEntityRepository)
        {
            _nguyenRepository = nguyenRepository ?? throw new ArgumentNullException(nameof(nguyenRepository));
            _fullAuditedEntityRepository = fullAuditedEntityRepository ?? throw new ArgumentNullException(nameof(fullAuditedEntityRepository));
        }

        [HttpGet]
        public IActionResult GetAsync()
        {
            try
            {
                var a = _nguyenRepository.AsNoTrackingQueryable().ToList();
                return Ok(a);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(NguyenEntity tesst)
        {
            await _nguyenRepository.AddAsync(tesst);
            await _nguyenRepository.SaveChangesAsync();
            return Ok(" Thêm thành công");
        }

        [HttpGet("Full")]
        public IActionResult GetFullAsync()
        {
            var a = _fullAuditedEntityRepository.AsQueryable().ToList();
            return Ok(a);
        }

        [HttpPost("Full")]
        public IActionResult PostFullAsync(Testtiep tesst)
        {
            tesst.Id = Guid.NewGuid();
            _fullAuditedEntityRepository.AddAsync(tesst);
            _fullAuditedEntityRepository.SaveChangesAsync();
            return Ok(" Thêm thành công");
        }
    }
}
