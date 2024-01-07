﻿using DGBCommerce.Domain.Models;
using DGBCommerce.Domain.Parameters;

namespace DGBCommerce.Domain.Interfaces.Repositories
{
    public interface IPageCategoryRepository : IPublicRepository<PageCategory, GetPageCategoriesParameters> { }
}