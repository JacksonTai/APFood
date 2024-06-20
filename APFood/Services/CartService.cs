using APFood.Areas.Identity.Data;
using APFood.Constants.Order;
using APFood.Constants;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace APFood.Services
{
    public class CartService(
        APFoodContext context,
        SessionManager sessionManager
        ) : ICartService
    {
        private readonly APFoodContext _context = context;
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

        public async Task<CartViewModel> CheckoutCart(string userId, CartFormModel cartForm)
        {
            CartViewModel cartView = await GetCartViewAsync(userId, cartForm);
            if (cartForm.DineInOption == DineInOption.Delivery && string.IsNullOrWhiteSpace(cartForm.Location))
            {
                throw new ArgumentException("Location is required for delivery");
            }
            _sessionManager.Set(typeof(CartFormModel).Name, cartForm);
            return cartView;
        }   

        public async Task<CartViewModel> GetCartViewAsync(string userId, CartFormModel cartForm)
        {
            Customer customer = await _context.Customers.FindAsync(userId) ?? throw new Exception("User not found");
            decimal subtotal = await GetCartTotalAsync(userId);
            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal runnerPointsRedeemed = cartForm.IsUsingRunnerPoints ? customer.Points : 0;
            decimal total = subtotal + deliveryFee - runnerPointsRedeemed;

            cartForm.IsUsingRunnerPoints = cartForm.IsUsingRunnerPoints;

            return new CartViewModel
            {
                CartItems = await GetCartItemsAsync(userId),
                OrderSummary = new OrderSummaryModel
                {
                    Subtotal = subtotal,
                    DeliveryFee = deliveryFee,
                    RunnerPointsRedeemed = runnerPointsRedeemed,
                    Total = total
                },
                RunnerPoints = customer.Points,
                CartForm = cartForm
            };
        }

        public async Task<CartViewModel> GetCartViewAsync(string userId)
        {
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            return await GetCartViewAsync(userId, cartForm);
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
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

            decimal subtotal = await GetCartTotalAsync(cart.CustomerId);
            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal runnerPointsRedeemed = cartForm.IsUsingRunnerPoints ? cart.Customer.Points : 0;
            decimal total = subtotal + deliveryFee - runnerPointsRedeemed;
            decimal itemPrice = cartItem.Food.Price * cartItem.Quantity;

            return new UpdateQuantityResponseModel { ItemPrice = itemPrice, Subtotal = subtotal, Total = total };
        }

        public async Task<UpdateRunnerPointsResponseModel> UpdateRunnerPointsAsync(Cart cart, bool isUsingRunnerPoints)
        {
            string userId = cart.CustomerId;
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();

            cartForm.IsUsingRunnerPoints = isUsingRunnerPoints;
            _sessionManager.Set(typeof(CartFormModel).Name, cartForm);

            decimal subtotal = await GetCartTotalAsync(userId);
            decimal runnerPointsRedeemed = cartForm.IsUsingRunnerPoints ? cart.Customer.Points : 0;
            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal total = subtotal + deliveryFee - runnerPointsRedeemed;

            return new UpdateRunnerPointsResponseModel { RunnerPointsRedeemed = runnerPointsRedeemed, Total = total };
        }   

        public async Task<UpdateDiningOptionResponseModel> UpdateDiningOption(Cart cart, DineInOption dineInOption)
        {
            string userId = cart.CustomerId;
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            cartForm.DineInOption = dineInOption;
            _sessionManager.Set(typeof(CartFormModel).Name, cartForm);

            decimal subtotal = await GetCartTotalAsync(userId);
            decimal runnerPointsRedeemed = cartForm.IsUsingRunnerPoints ? cart.Customer.Points : 0;
            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal total = subtotal + deliveryFee - runnerPointsRedeemed;

            return new UpdateDiningOptionResponseModel { DeliveryFee = deliveryFee, Total = total };
        }

        public async Task<RemoveItemResponseModel> RemoveItemAsync(Cart cart, int itemId)
        {
            CartItem cartItem = await _context.CartItems.FindAsync(itemId) ?? throw new Exception("Cart Item not found");
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            string userId = cart.CustomerId;
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) ?? new CartFormModel();
            Customer customer = await _context.Customers.FindAsync(userId) ?? throw new Exception("User not found");

            decimal subtotal = await GetCartTotalAsync(userId);
            decimal runnerPointsRedeemed = cartForm.IsUsingRunnerPoints ? customer.Points : 0;
            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal total = subtotal + deliveryFee - runnerPointsRedeemed;

            return new RemoveItemResponseModel { Subtotal = subtotal, Total = total };
        }

        public async Task ClearCartAsync(string userId)
        {
            Cart cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId) ?? throw new Exception("Cart ot found");
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            Cart? cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            return cart?.Items.Sum(ci => ci.Food.Price * ci.Quantity) ?? 0;
        }

    }
}
