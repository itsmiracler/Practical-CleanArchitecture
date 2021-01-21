﻿using ClassifiedAds.Blazor.Modules.Core.Components;
using ClassifiedAds.Blazor.Modules.Products.Components;
using ClassifiedAds.Blazor.Modules.Products.Models;
using ClassifiedAds.Blazor.Modules.Products.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassifiedAds.Blazor.Modules.Products.Pages
{
    public partial class List
    {
        [CascadingParameter]
        Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [Inject]
        public ProductService ProductService { get; set; }

        [Inject]
        public NavigationManager NavManager { get; set; }

        [Inject]
        public ILogger<List> Logger { get; set; }

        protected AuditLogsDialog AuditLogsDialog { get; set; }

        protected ConfirmDialog DeleteDialog { get; set; }

        public List<ProductModel> Products { get; set; }

        public ProductModel DeletingProduct { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            Products = await ProductService.GetProductsAsync();
        }

        protected void QuickAddProduct()
        {
            NavManager.NavigateTo("/products/add");
        }

        protected async Task EditProduct(ProductModel product)
        {
            var authenticationState = await AuthenticationStateTask;
            NavManager.NavigateTo($"/products/edit/{product.Id}");
        }

        protected async Task ViewAuditLogs(ProductModel product)
        {
            var logs = await ProductService.GetAuditLogsAsync(product.Id);
            AuditLogsDialog.Show(logs);
        }

        protected void DeleteProduct(ProductModel product)
        {
            DeletingProduct = product;
            DeleteDialog.Show();
        }

        public async void ConfirmedDeleteProduct()
        {
            await ProductService.DeleteProductAsync(DeletingProduct.Id);
            DeleteDialog.Close();
            Products = await ProductService.GetProductsAsync();
            StateHasChanged();
        }
    }
}
