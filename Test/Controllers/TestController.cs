using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Test.Entity;
using Test.Repository;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly INguyenEntityRepository _entityRepository;
        private readonly IFullAuditedEntityRepository _fullAuditedEntityRepository;

        public TestController(INguyenEntityRepository entityRepository, IFullAuditedEntityRepository fullAuditedEntityRepository)
        {
            _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
            _fullAuditedEntityRepository = fullAuditedEntityRepository ?? throw new ArgumentNullException(nameof(fullAuditedEntityRepository));
        }

        [HttpGet]
        public IActionResult GetAsync()
        {
            var a = _entityRepository.AsQueryable().ToList();
            return Ok(a);
        }

        [HttpPost]
        public async  Task<IActionResult> PostAsync(NguyenEntity tesst)
        {
             await _entityRepository.AddAsync(tesst);
            await _entityRepository.SaveChangesAsync();
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
            _fullAuditedEntityRepository.Add(tesst);
            _fullAuditedEntityRepository.SaveChanges();
            return Ok(" Thêm thành công");
        }
    }
}
