﻿using DGBCommerce.Domain.Models;
using DGBCommerce.Domain.Models.ViewModels;
using DGBCommerce.Domain.Parameters;

namespace DGBCommerce.Domain.Interfaces.Repositories
{
    public interface IMerchantRepository : IMutableRepository<Merchant, GetMerchantsParameters>
    {
        Task<Merchant?> GetByEmailAddressAndPassword(string emailAddress, string password, string? ipAddress);
        Task<Merchant?> GetByEmailAddress(string emailAddress);
        Task<PublicMerchant?> GetByIdPublic(Guid id);
        Task<IEnumerable<PublicMerchant>> GetPublic(GetMerchantsParameters parameters);
    }
}