﻿using DGBCommerce.Domain;
using DGBCommerce.Domain.Enums;
using DGBCommerce.Domain.Interfaces;
using DGBCommerce.Domain.Models;
using DGBCommerce.Domain.Parameters;
using System.Data;
using System.Linq;

namespace DGBCommerce.Data.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly IDataAccessLayer _dataAccessLayer;

        public ShopRepository(IDataAccessLayer dataAccessLayer)
        {
            _dataAccessLayer = dataAccessLayer;
        }

        public Task<MutationResult> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Shop>> Get()
            => await GetRaw(new GetShopsParameters());

        public async Task<Shop> GetById(Guid id)
        { 
            var shops = await GetRaw(new GetShopsParameters() { Id = id });
            return shops.ToList().Single();
        }

        public Task<IEnumerable<Shop>> GetByMerchant(Guid merchantId)
        {
            throw new NotImplementedException();
        }

        public Task<MutationResult> Insert(Shop item)
        {
            throw new NotImplementedException();
        }

        public Task<MutationResult> Update(Shop item)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<Shop>> GetRaw(GetShopsParameters parameters)
        {
            DataTable table = await _dataAccessLayer.GetShops(parameters);
            List<Shop> shops = new();

            foreach (DataRow row in table.Rows)
            {
                shops.Add(new Shop()
                {
                    Id = new Guid(row["shp_id"].ToString()!),
                    Name = Utilities.DbNullableString(row["shp_name"]),
                    Merchant = new Merchant()
                    {
                        Id = new Guid(row["shp_merchant"].ToString()!),
                        EmailAddress = Utilities.DbNullableString(row["shp_merchant_email_address"]),
                        Gender = (Gender)Convert.ToInt32(row["shp_merchant_gender"]),
                        LastName = Utilities.DbNullableString(row["shp_merchant_lastname"]),
                    }
                });
            }

            return shops;
        }
    }
}
