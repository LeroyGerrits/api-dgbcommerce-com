﻿using DGBCommerce.Domain;
using DGBCommerce.Domain.Interfaces;
using DGBCommerce.Domain.Interfaces.Repositories;
using DGBCommerce.Domain.Models;
using DGBCommerce.Domain.Parameters;
using System.Data;

namespace DGBCommerce.Data.Repositories
{
    public class PageCategoryRepository(IDataAccessLayer dataAccessLayer) : IPageCategoryRepository
    {
        private readonly IDataAccessLayer _dataAccessLayer = dataAccessLayer;

        public async Task<IEnumerable<PageCategory>> Get(GetPageCategoriesParameters parameters)
            => await GetRaw(parameters);

        public async Task<PageCategory?> GetById(Guid id)
        {
            var pageCategories = await GetRaw(new GetPageCategoriesParameters() { Id = id });
            return pageCategories.ToList().SingleOrDefault();
        }

        private async Task<IEnumerable<PageCategory>> GetRaw(GetPageCategoriesParameters parameters)
        {
            DataTable table = await _dataAccessLayer.GetPageCategories(parameters);
            List<PageCategory> pageCategories = [];

            foreach (DataRow row in table.Rows)
            {
                pageCategories.Add(new()
                {
                    Id = new Guid(row["cat_id"].ToString()!),
                    Name = Utilities.DbNullableString(row["cat_name"])
                });
            }

            return pageCategories;
        }
    }
}