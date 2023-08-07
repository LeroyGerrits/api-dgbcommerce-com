﻿using DGBCommerce.Domain;
using DGBCommerce.Domain.Enums;
using DGBCommerce.Domain.Interfaces;
using DGBCommerce.Domain.Models;
using DGBCommerce.Domain.Parameters;
using System.Data;

namespace DGBCommerce.Data.Repositories
{
    public class FaqRepository : IFaqRepository
    {
        private readonly IDataAccessLayer _dataAccessLayer;

        public FaqRepository(IDataAccessLayer dataAccessLayer)
        {
            _dataAccessLayer = dataAccessLayer;
        }

        public Task<MutationResult> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Faq>> Get()
            => await GetRaw(new GetFaqsParameters());

        public async Task<Faq?> GetById(Guid id)
        {
            var faqs = await GetRaw(new GetFaqsParameters() { Id = id });
            return faqs.ToList().SingleOrDefault();
        }

        public Task<IEnumerable<Faq>> GetByMerchant(Guid merchantId)
        {
            throw new NotImplementedException();
        }

        public Task<MutationResult> Insert(Faq item)
        {
            throw new NotImplementedException();
        }

        public Task<MutationResult> Update(Faq item)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<Faq>> GetRaw(GetFaqsParameters parameters)
        {
            DataTable table = await _dataAccessLayer.GetFaqs(parameters);
            List<Faq> faqs = new();

            foreach (DataRow row in table.Rows)
            {
                Faq faq = new()
                {
                    Id = new Guid(row["faq_id"].ToString()!),
                    Category = new FaqCategory()
                    {
                        Id = new Guid(row["faq_category"].ToString()!),
                        Name = Utilities.DbNullableString(row["faq_category_name"])
                    },
                    Title = Utilities.DbNullableString(row["faq_title"]),
                    Content = Utilities.DbNullableString(row["faq_content"]),
                    SortOrder = Utilities.DbNullableInt(row["faq_sortorder"]),
                };

                if (row["faq_keywords"] != DBNull.Value)
                {
                    string keywords = Utilities.DbNullableString(row["faq_keywords"]);
                    faq.Keywords = keywords.Split(',');
                }

                faqs.Add(faq);
            }

            return faqs;
        }
    }
}
