using DGBCommerce.API.Controllers.Attributes;
using DGBCommerce.API.Controllers.Requests;
using DGBCommerce.API.Services;
using DGBCommerce.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DGBCommerce.Domain;
using System.Text;
using Microsoft.Extensions.Options;
using DGBCommerce.Domain.Parameters;
using DGBCommerce.Domain.Models.ViewModels;
using DGBCommerce.Domain.Interfaces.Repositories;

namespace DGBCommerce.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MerchantController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtUtils _jwtUtils;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMailService _mailService;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IMerchantPasswordResetLinkRepository _merchantPasswordResetLinkRepository;

        public MerchantController(
            IOptions<AppSettings> appSettings,
            IHttpContextAccessor httpContextAccessor,
            IJwtUtils jwtUtils,
            IAuthenticationService authenticationService,
            IMailService mailService,
            IMerchantRepository merchantRepository,
            IMerchantPasswordResetLinkRepository merchantPasswordResetLinkRepository
            )
        {
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _jwtUtils = jwtUtils;
            _authenticationService = authenticationService;
            _mailService = mailService;
            _merchantRepository = merchantRepository;
            _merchantPasswordResetLinkRepository = merchantPasswordResetLinkRepository;
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<PublicMerchant>>> GetPublic(string? firstName, string? lastName)
        {
            var merchants = await _merchantRepository.GetPublic(new GetMerchantsParameters()
            {
                FirstName = firstName,
                LastName = lastName
            });
            return Ok(merchants.ToList());
        }

        [AllowAnonymous]
        [HttpGet("public/{id}")]
        public async Task<ActionResult<PublicMerchant>> GetPublic(Guid id)
        {
            var merchant = await _merchantRepository.GetByIdPublic(id);
            if (merchant == null)
                return NotFound();

            return Ok(merchant);
        }

        [AuthenticationRequired]
        [HttpGet("{id}")]
        public async Task<ActionResult<Merchant>> Get(Guid id)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            var merchant = await _merchantRepository.GetById(authenticatedMerchantId.Value, id);
            if (merchant == null)
                return NotFound();

            return Ok(merchant);
        }

        [AllowAnonymous]
        [HttpGet("{id}/{password}")]
        public async Task<ActionResult<Merchant>> Get(Guid id, string password)
        {
            var merchant = await _merchantRepository.GetByIdAndPassword(id, password);
            if (merchant == null)
                return NotFound();

            return Ok(merchant);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Merchant value)
        {
            value.PasswordSalt = Utilities.GenerateSalt();
            value.Password = Utilities.GenerateRandomString(50);
            var result = await _merchantRepository.Create(value, Guid.Empty);

            if (result.Success)
            {
                string accountActivationUrl = $"{_appSettings.UrlDgbCommerceWebsite}/account-activate/{result.Identifier}/{value.Password}";

                StringBuilder sbMail = new();
                sbMail.Append($"<p>Hi {value.Username},</p>");
                sbMail.Append($"<p>Your account was registered. Before you can use your account, you will need to activate it. Click on the following link to activate:</p>");
                sbMail.Append($"<p><a href=\"{accountActivationUrl}\">{accountActivationUrl}</a></p>");
                sbMail.Append($"<p>DGB Commerce</p>");
                _mailService.SendMail(value.EmailAddress, "Activate your DGB Commerce account", sbMail.ToString());
            }

            return Ok(result);
        }

        [AuthenticationRequired]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] Merchant value)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            var merchant = await _merchantRepository.GetById(authenticatedMerchantId.Value, id);
            if (merchant == null)
                return NotFound();

            var result = await _merchantRepository.Update(value, authenticatedMerchantId.Value);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPut("activate-account")]
        public async Task<ActionResult> ActivateAccount([FromBody] ActivateAccountRequest model)
        {
            var merchant = await _merchantRepository.GetByIdAndPassword(model.Id, model.CurrentPassword);
            if (merchant == null)
                return NotFound();

            if (merchant.Activated.HasValue)
                return BadRequest("Merchant is already activated.");

            var hashedNewPassword = Utilities.HashStringSha256(merchant.PasswordSalt + model.NewPassword);
            var result = await _merchantRepository.UpdatePasswordAndActivate(merchant, hashedNewPassword, merchant.Id!.Value);
            return Ok(result);
        }

        [AuthenticationRequired]
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            // First retrieve the merchant by ID so we can get the salt
            var merchantById = await _merchantRepository.GetById(authenticatedMerchantId.Value, authenticatedMerchantId.Value);
            if (merchantById == null)
                return NotFound();

            // Hash the current password using the salt
            var hashedCurrentPassword = Utilities.HashStringSha256(merchantById.PasswordSalt + model.CurrentPassword);

            // Retrieve merchant by ID and hashed password
            var merchant = await _merchantRepository.GetByIdAndPassword(authenticatedMerchantId.Value, hashedCurrentPassword);
            if (merchant == null)
                return BadRequest(new { message = "Your current password is incorrect." });

            // Hash the new password using the salt
            var hashedNewPassword = Utilities.HashStringSha256(merchant.PasswordSalt + model.NewPassword);

            var result = await _merchantRepository.UpdatePassword(merchant, hashedNewPassword, merchant.Id!.Value);
            return Ok(result);
        }

        [AuthenticationRequired]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Merchant>> Delete(Guid id)
        {
            var authenticatedMerchantId = _jwtUtils.GetMerchantId(_httpContextAccessor);
            if (authenticatedMerchantId == null)
                return BadRequest("Merchant not authorized.");

            var merchant = await _merchantRepository.GetById(authenticatedMerchantId.Value, id);
            if (merchant == null)
                return NotFound();

            var result = await _merchantRepository.Delete(id, authenticatedMerchantId.Value);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<ActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            // First retrieve the account by e-mail address so we can get the salt
            var merchantByEmailAddress = await _merchantRepository.GetByEmailAddress(model.EmailAddress);
            if (merchantByEmailAddress == null)
                return BadRequest(new { message = "E-mail address or password is incorrect" });

            // Hash the password using the salt
            model.Password = Utilities.HashStringSha256(merchantByEmailAddress.PasswordSalt + model.Password);

            var response = _authenticationService.Authenticate(model);
            if (response == null)
                return BadRequest(new { message = "E-mail address or password is incorrect" });

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPassword([FromBody] string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(_appSettings.UrlDgbCommerceWebsite))
                throw new Exception("DGB Commerce Website URL not configured.");

            var merchant = await _merchantRepository.GetByEmailAddress(emailAddress);
            if (merchant == null)
                return Ok(); // Return OK even when using a wrong e-mail address because we want to display a generic message rather than hinting the e-mail address exists

            MerchantPasswordResetLink passwordResetLink = new()
            {
                Merchant = merchant,
                Date = DateTime.UtcNow,
                IpAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                Key = Utilities.GenerateRandomString(50)
            };

            var result = await _merchantPasswordResetLinkRepository.Create(passwordResetLink, Guid.Empty);
            if (result.Success)
            {
                string passwordResetUrl = $"{_appSettings.UrlDgbCommerceWebsite}/reset-password/{result.Identifier}/{passwordResetLink.Key}";

                StringBuilder sbMail = new();
                sbMail.Append($"<p>Hi {merchant.Username},</p>");
                sbMail.Append($"<p>A new password for your account was requested. If this was you, click on the following link to proceed setting a new password:</p>");
                sbMail.Append($"<p><a href=\"{passwordResetUrl}\">{passwordResetUrl}</a></p>");
                sbMail.Append($"<p>If this wasn't you, ignore this link.</p>");
                sbMail.Append($"<p>DGB Commerce</p>");
                _mailService.SendMail(merchant.EmailAddress, "DGB Commerce password reset", sbMail.ToString());
            }

            return Ok();
        }
    }
}