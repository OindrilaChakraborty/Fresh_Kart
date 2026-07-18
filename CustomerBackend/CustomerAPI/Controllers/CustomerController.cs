using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using CustomerAPI.Business_Layer;
using CustomerAPI.Common;
using CustomerAPI.DAL;
using CustomerAPI.Model;

namespace CustomerAPI.Controllers
{
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        public BLLCustomer Customer;

        #region caching
        private readonly MemoryCaching _cache;
        private readonly IConfiguration _configuration;
        private readonly int _CacheAbsoluteDurationMinutes;
        private readonly int _CacheSlidingDurationMinutes;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;
        #endregion

        public CustomerController(MemoryCaching cache, IConfiguration configuration, CustomerAppDbContext context)
        {
            Customer = new BLLCustomer(context);
            _cache = cache;
            _configuration = configuration;
            _CacheAbsoluteDurationMinutes = _configuration.GetValue<int?>("CacheSettings:CacheAbsoluteDurationMinutes") ?? 30;
            _CacheSlidingDurationMinutes = _configuration.GetValue<int?>("CacheSettings:CacheSlidingDurationMinutes") ?? 30;
            _cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High);
        }

        [HttpPost("login")]
        public CommonEntity login([FromBody] CustomerLogin Input)
        {
            try
            {
                var BLLCall = Customer.Login(Input);
                if (BLLCall.IsSuccess)
                {
                    var JWTOKEN = Auth.GenerateToken(Input, _configuration);

                    var cookieoptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.UtcNow.AddMinutes(30)
                    };
                    Response.Cookies.Append("X-Access-Token", JWTOKEN, cookieoptions);
                    return new CommonEntity { Data = BLLCall.Data, IsSuccess = true, Message = "Login Successful", StatusCode = 200, Token = JWTOKEN };
                }
                else
                {
                    return new CommonEntity { Data = null, IsSuccess = false, Message = BLLCall.ExtraData?.ToString() ?? "Login Failed", StatusCode = 401 };
                }
            }
            catch (Exception ex)
            {
                return new CommonEntity { Data = null, IsSuccess = false, Message = ex.Message, StatusCode = 500 };
            }
        }

        [HttpPost("signup")]
        public CommonEntity signup([FromBody] CustomerLogin Input)
        {
            try
            {
                var BLLCall = Customer.Signup(Input);
                if (BLLCall.IsSuccess)
                {
                    var JWTOKEN = Auth.GenerateToken(Input, _configuration);

                    var cookieoptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.UtcNow.AddMinutes(30)
                    };
                    Response.Cookies.Append("X-Access-Token", JWTOKEN, cookieoptions);
                    return new CommonEntity { Data = BLLCall.Data, IsSuccess = true, Message = "Signup Successful", StatusCode = 200, Token = JWTOKEN };
                }
                else
                {
                    return new CommonEntity { Data = null, IsSuccess = false, Message = BLLCall.ExtraData?.ToString() ?? "Signup Failed", StatusCode = 401 };
                }
            }
            catch (Exception ex)
            {
                return new CommonEntity { Data = null, IsSuccess = false, Message = ex.Message, StatusCode = 500 };
            }
        }

        [HttpGet("fetchProducts")]
        public CommonEntity fetchProducts()
        {
            try
            {
                var CACHEKEY = "StockItems_API";

                if (_cache.IsExist<object>(CACHEKEY, out var cache))
                {
                    var data = _cache.Get<object>(CACHEKEY);
                    return new CommonEntity
                    {
                        Data = data,
                        IsSuccess = true,
                        Message = "Stock items fetched from Cache",
                        StatusCode = 200
                    };
                }

                var stockData = Customer.fetchProducts();
                if (stockData.IsSuccess)
                {
                    _cache.Set<object>(CACHEKEY, stockData.Data, _cacheEntryOptions);
                    return new CommonEntity
                    {
                        Data = stockData.Data,
                        IsSuccess = true,
                        Message = "Stock items fetched successfully",
                        StatusCode = 200
                    };
                }
                else
                {
                    return new CommonEntity
                    {
                        Data = null,
                        IsSuccess = false,
                        Message = "Internal Error!!",
                        StatusCode = 500
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommonEntity
                {
                    Data = null,
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        [Authorize]
        [HttpPost("PlaceOrder")]
        public CommonEntity PlaceOrder([FromBody] Order Input)
        {
            try
            {
                var BLLCall = Customer.PlaceOrder(Input);
                if (BLLCall.IsSuccess)
                {
                    // Clear cache to reflect updated stock
                    _cache.Remove("StockItems_API");
                    return new CommonEntity { Data = BLLCall.Data, IsSuccess = true, Message = "Order placed successfully. Invoice sent to email.", StatusCode = 200 };
                }
                else
                {
                    return new CommonEntity { Data = null, IsSuccess = false, Message = BLLCall.ExtraData?.ToString() ?? "Internal Error!!!", StatusCode = 500 };
                }
            }
            catch (Exception ex)
            {
                return new CommonEntity { Data = null, IsSuccess = false, Message = ex.Message, StatusCode = 500 };
            }
        }

        [HttpGet("GetOrderHistory")]
        public CommonEntity GetOrderHistory()
        {
            try
            {
                // Get email from JWT token - check multiple possible claim types
                var customerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                    ?? User.FindFirst("sub")?.Value 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(customerEmail))
                {
                    return new CommonEntity
                    {
                        Data = null,
                        IsSuccess = false,
                        Message = "User email not found in token",
                        StatusCode = 401
                    };
                }

                var orderData = Customer.GetOrderHistory(customerEmail);
                if (orderData.IsSuccess)
                {
                    return new CommonEntity
                    {
                        Data = orderData.Data,
                        IsSuccess = true,
                        Message = "Order history fetched successfully",
                        StatusCode = 200
                    };
                }
                else
                {
                    return new CommonEntity
                    {
                        Data = null,
                        IsSuccess = false,
                        Message = "Internal Error!!",
                        StatusCode = 500
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommonEntity
                {
                    Data = null,
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }
}
