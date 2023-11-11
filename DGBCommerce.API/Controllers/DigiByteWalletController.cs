using DGBCommerce.API.Controllers.Attributes;
using DGBCommerce.Domain.Exceptions;
using DGBCommerce.Domain.Interfaces.Repositories;
using DGBCommerce.Domain.Interfaces.Services;
using DGBCommerce.Domain.Models;
using DGBCommerce.Domain.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace DGBCommerce.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DigiByteWalletController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtUtils _jwtUtils;
        private readonly IDigiByteWalletRepository _digiByteWalletRepository;
        private readonly IRpcService _rpcService;

        public DigiByteWalletController(
            IHttpContextAccessor httpContextAccessor,
            IJwtUtils jwtUtils,
            IDigiByteWalletRepository digiByteWalletRepository,
            IRpcService rpcService)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtUtils = jwtUtils;
            _digiByteWalletRepository = digiByteWalletRepository;
            _rpcService = rpcService;
        }

        [AuthenticationRequired]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DigiByteWallet>>> Get(string? name, string? address)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            var digiByteWallets = await _digiByteWalletRepository.Get(new GetDigiByteWalletsParameters()
            {
                MerchantId = authenticatedMerchantId.Value,
                Name = name,
                Address = address
            });
            return Ok(digiByteWallets.ToList());
        }

        [AuthenticationRequired]
        [HttpGet("{id}")]
        public async Task<ActionResult<DigiByteWallet>> Get(Guid id)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            var digiByteWallet = await _digiByteWalletRepository.GetById(authenticatedMerchantId.Value, id);
            if (digiByteWallet == null)
                return NotFound();

            return Ok(digiByteWallet);
        }

        [AuthenticationRequired]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] DigiByteWallet value)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            try
            {
                var validateAddressResponse = await _rpcService.ValidateAddress(value.Address);
                if (!validateAddressResponse.IsValid)
                    return BadRequest(new { message = "The DigiByte address you supplied is not valid." });
            }
            catch (RpcException)
            {
                return BadRequest(new { message = "DigiByte node could not be contacted." });
            }

            var result = await _digiByteWalletRepository.Create(value, authenticatedMerchantId.Value);
            return Ok(result);
        }

        [AuthenticationRequired]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] DigiByteWallet value)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            try
            {
                var validateAddressResponse = await _rpcService.ValidateAddress(value.Address);
                if (!validateAddressResponse.IsValid)
                    return BadRequest(new { message = "The DigiByte address you supplied is not valid." });
            }
            catch (RpcException)
            {
                return BadRequest(new { message = "DigiByte node could not be contacted." });
            }
            
            var digiByteWallet = await _digiByteWalletRepository.GetById(authenticatedMerchantId.Value, id);
            if (digiByteWallet == null)
                return NotFound();

            var result = await _digiByteWalletRepository.Update(value, authenticatedMerchantId.Value);
            return Ok(result);
        }

        [AuthenticationRequired]
        [HttpDelete("{id}")]
        public async Task<ActionResult<DigiByteWallet>> Delete(Guid id)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            var digiByteWallet = await _digiByteWalletRepository.GetById(authenticatedMerchantId.Value, id);
            if (digiByteWallet == null)
                return NotFound();

            var result = await _digiByteWalletRepository.Delete(id, authenticatedMerchantId.Value);
            return Ok(result);
        }
    }
}