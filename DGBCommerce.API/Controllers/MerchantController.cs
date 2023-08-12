using DGBCommerce.API.Controllers.Attributes;
using DGBCommerce.API.Controllers.Requests;
using DGBCommerce.API.Services;
using DGBCommerce.Domain.Interfaces;
using DGBCommerce.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DGBCommerce.Domain;

namespace DGBCommerce.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MerchantController : ControllerBase
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtUtils _jwtUtils;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMerchantRepository _merchantRepository;

        public MerchantController(
            ILogger<CategoryController> logger,
            IHttpContextAccessor httpContextAccessor,
            IJwtUtils jwtUtils,
            IAuthenticationService authenticationService,
            IMerchantRepository merchantRepository
            )
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtUtils = jwtUtils;
            _authenticationService = authenticationService;
            _merchantRepository = merchantRepository;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticationRequest model)
        {
            var response = _authenticationService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [AuthenticationRequired]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Merchant>>> Get()
        {
            IEnumerable<Merchant> merchants = await _merchantRepository.Get();
            return Ok(merchants.ToList());
        }

        [AuthenticationRequired]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Merchant value)
        {
            var merchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (merchantId == null)
                return BadRequest("Merchant not authorized.");

            var result = await _merchantRepository.Create(value, merchantId.Value);

            if(result.ErrorCode == 0)
                return CreatedAtAction(nameof(Get), new { id = result.Identifier });
            else
                return BadRequest(result.Message);
            
        }

        [AuthenticationRequired]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] Merchant value)
        {
            Merchant? merchant = await _merchantRepository.GetById(id);
            if (merchant == null) return NotFound();

            var result = await _merchantRepository.Update(value);
            if (result.ErrorCode == 0)
                return Ok(value);
            else
                return BadRequest(result.Message);
        }

        [AuthenticationRequired]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Merchant>> Delete(Guid id)
        {
            Merchant? merchant = await _merchantRepository.GetById(id);
            if (merchant == null) return NotFound();

            var result = await _merchantRepository.Delete(id);
            if (result.ErrorCode == 0)
                return Ok();
            else
                return BadRequest(result.Message);
        }
    }
}