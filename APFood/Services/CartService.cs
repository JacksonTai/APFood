using APFood.Areas.Identity.Data;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace APFood.Services
{
    public class CartService(
        APFoodContext context,
        IOrderService orderService,
        SessionManager sessionManager
        ) : ICartService
    {
        private readonly APFoodContext _context = context;
        private readonly IOrderService _orderService = orderService;
        private readonly SessionManager _sessionManager = sessionManager;

        public async Task CreateCustomerCart(Customer customer)
        {
            if (customer != null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var cart = new Cart
                    {
                        CustomerId = customer.Id,
                        Customer = customer,
                        Items = []
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();

                    customer.CartId = cart.Id;
                    customer.Cart = cart;
                    _context.Customers.Update(customer);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(customer));
            }
        }

        public async Task<CartViewModel> CheckoutCart(Cart cart, CartFormModel cartForm)
        {
            CartViewModel cartView = await GetCartViewAsync(cart, cartForm);
            if (cartForm.DineInOption == DineInOption.Delivery && string.IsNullOrWhiteSpace(cartForm.Location))
            {
                throw new ArgumentException("Location is required for delivery");
            }
            _sessionManager.Set(typeof(CartFormModel).Name, cartForm);
            return cartView;
        }   

        public async Task<CartViewModel> GetCartViewAsync(Cart cart, CartFormModel cartForm)
        {
            return new CartViewModel
            {
                CartItems = await GetCartItemsAsync(cart.CustomerId),
                OrderSummary = _orderService.CalculateOrderSummary(cart, cartForm),
                CartForm = cartForm
            };
        }

        public async Task<CartViewModel> GetCartViewAsync(Cart cart)
        {
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            return await GetCartViewAsync(cart, cartForm);
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

        public async Task<Cart?> GetCartAsyncWithTracking(string userId)
        {
            return await _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

        public async Task<Cart?> GetCartAsyncNoTracking(string userId)
        {
            return await _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId)
        {
            Cart? cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            return cart?.Items.ToList() ?? [];
        }

        public async Task AddItemAsync(string userId, Food food, int quantity)
        {
            Cart customerCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId) ?? throw new Exception("Cart not found");
            CartItem? existingItem = customerCart?.Items.FirstOrDefault(ci => ci.FoodId == food.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                customerCart?.Items.Add(new CartItem { FoodId = food.Id, Quantity = quantity, Cart = customerCart, Food = food });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<UpdateQuantityResponseModel> UpdateQuantityAsync(
            Cart cart, UpdateQuantityRequestModel updateQuantityRequest)
        {
            CartItem cartItem = await _context.CartItems
                .Include(ci => ci.Food)
                .FirstOrDefaultAsync(ci => ci.Id == updateQuantityRequest.ItemId) ?? throw new Exception("Cart Item not found");
            cartItem.Quantity = updateQuantityRequest.NewQuantity;
            await _context.SaveChangesAsync();
            
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            OrderSummaryModel orderSummary = _orderService.CalculateOrderSummary(cart, cartForm);
            decimal itemPrice = cartItem.Food.Price * cartItem.Quantity;

            return new UpdateQuantityResponseModel {
                ItemPrice = itemPrice,
                Subtotal = orderSummary.Subtotal,
                Total = orderSummary.Total,
                RunnerPointsRedeemed = orderSummary.RunnerPointsRedeemed
            };
        }

        public UpdateRunnerPointsResponseModel UpdateRunnerPoints(Cart cart, bool isUsingRunnerPoints)
        {
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            cartForm.IsUsingRunnerPoints = isUsingRunnerPoints;
            _sessionManager.Set(typeof(CartFormModel).Name, cartForm);

            OrderSummaryModel orderSummary = _orderService.CalculateOrderSummary(cart, cartForm);

            return new UpdateRunnerPointsResponseModel
            {
                RunnerPointsRedeemed = orderSummary.RunnerPointsRedeemed,
                Total = orderSummary.Total
            };
        }

        public UpdateDiningOptionResponseModel UpdateDiningOption(Cart cart, DineInOption dineInOption)
        {
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            cartForm.DineInOption = dineInOption;
            _sessionManager.Set(typeof(CartFormModel).Name, cartForm);

            OrderSummaryModel orderSummary = _orderService.CalculateOrderSummary(cart, cartForm);

            return new UpdateDiningOptionResponseModel
            {
                DeliveryFee = orderSummary.DeliveryFee,
                Total = orderSummary.Total,
                RunnerPointsRedeemed = orderSummary.RunnerPointsRedeemed
            };
        }

        public async Task<RemoveItemResponseModel> RemoveItemAsync(Cart cart, int itemId)
        {
            CartItem cartItem = await _context.CartItems.FindAsync(itemId) ?? throw new Exception("Cart Item not found");
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            OrderSummaryModel orderSummary = _orderService.CalculateOrderSummary(cart, cartForm);

            return new RemoveItemResponseModel { 
                Subtotal = orderSummary.Subtotal, 
                Total = orderSummary.Total,
                RunnerPointsRedeemed = orderSummary.RunnerPointsRedeemed
            };
        }

        public async Task ClearCartAsync(string userId)
        {
            Cart cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId) ?? throw new Exception("Cart ot found");
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }
 
    }
}
