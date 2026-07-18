using CustomerAPI.Common;
using CustomerAPI.Model;
using CustomerAPI.DAL;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Business_Layer
{
    public class BLLCustomer
    {
        private readonly CustomerAppDbContext _context;

        public BLLCustomer(CustomerAppDbContext context)
        {
            _context = context;
        }

        // Fetch products from database
        public BllOutput fetchProducts()
        {
            try
            {
                var products = _context.TProductMsts
                    .Include(p => p.TCategoryMst)
                    .Where(p => p.is_Active)
                    .Select(p => new
                    {
                        Id = p.prod_id,
                        Name = p.prod_name,
                        Category = p.TCategoryMst.cat_name,
                        CategoryId = p.cat_id,
                        Price = p.price,
                        Quantity = p.quantity,
                        MfgDate = p.mfg_date,
                        ExpDate = p.exp_date,
                        ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=300&h=300&fit=crop"
                    })
                    .ToList();

                return new BllOutput
                {
                    Data = products,
                    IsSuccess = true,
                    ExtraData = null
                };
            }
            catch (Exception ex)
            {
                return new BllOutput
                {
                    Data = null,
                    IsSuccess = false,
                    ExtraData = ex.Message
                };
            }
        }

        public BllOutput Login(CustomerLogin login)
        {
            try
            {
                Console.WriteLine($"Login attempt for email: {login.EmailId}");

                // Check if user exists in TLoginMst
                var existingUser = _context.TLoginMsts
                    .FirstOrDefault(l => l.email_id == login.EmailId && l.is_Active);
                
                if (existingUser == null)
                {
                    Console.WriteLine($"User not found: {login.EmailId}");
                    return new BllOutput
                    {
                        Data = null,
                        IsSuccess = false,
                        ExtraData = "Email not found. Please sign up first."
                    };
                }

                Console.WriteLine($"User found: {existingUser.email_id}, UserType: {existingUser.user_type}");

                // Validate password - try decryption first, then fall back to plain text
                bool passwordMatch = false;
                
                try
                {
                    // Try to decrypt using the same fixed key as signup
                    string jwtKey = "fXIbMhIY7lrECjupMLwhVXTtCLmlUE5KWELhg/r7+Q4=";
                    byte[] keyBytes = Convert.FromBase64String(jwtKey);
                    byte[] ivBytes = new byte[16];
                    Array.Copy(keyBytes, ivBytes, 16);
                    
                    byte[] encryptedBytes = Convert.FromBase64String(existingUser.password);
                    string decryptedPassword = EncryptDecrypt.Decrypt(encryptedBytes, keyBytes, ivBytes);
                    passwordMatch = decryptedPassword == login.Password;
                }
                catch
                {
                    // If decryption fails, assume it's plain text (backward compatibility)
                    passwordMatch = existingUser.password == login.Password;
                }

                if (passwordMatch)
                {
                    Console.WriteLine("Password match - login successful");
                    return new BllOutput
                    {
                        Data = new { Email = login.EmailId, UserType = existingUser.user_type },
                        IsSuccess = true,
                        ExtraData = null
                    };
                }

                Console.WriteLine("Password mismatch");
                return new BllOutput
                {
                    Data = null,
                    IsSuccess = false,
                    ExtraData = "Invalid password"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex.Message}");
                return new BllOutput
                {
                    Data = null,
                    IsSuccess = false,
                    ExtraData = ex.Message
                };
            }
        }

        public BllOutput Signup(CustomerLogin login)
        {
            try
            {
                // Check if user already exists
                var existingUser = _context.TLoginMsts
                    .FirstOrDefault(l => l.email_id == login.EmailId);
                
                if (existingUser != null)
                {
                    return new BllOutput
                    {
                        Data = null,
                        IsSuccess = false,
                        ExtraData = "User already exists"
                    };
                }

                // Encrypt password using AES encryption with fixed key from JWT configuration
                // Use the JWT key as the encryption key for simplicity
                string jwtKey = "fXIbMhIY7lrECjupMLwhVXTtCLmlUE5KWELhg/r7+Q4="; // Same as appsettings
                byte[] keyBytes = Convert.FromBase64String(jwtKey);
                byte[] ivBytes = new byte[16]; // Fixed IV for simplicity
                Array.Copy(keyBytes, ivBytes, 16); // Use first 16 bytes of key as IV
                
                string encryptedPwd = null;
                using (System.Security.Cryptography.Aes myAes = System.Security.Cryptography.Aes.Create())
                {
                    myAes.Key = keyBytes;
                    myAes.IV = ivBytes;
                    byte[] encrypted = EncryptDecrypt.Encrypt(login.Password, myAes.Key, myAes.IV);
                    encryptedPwd = Convert.ToBase64String(encrypted);
                }

                // Create new login entry with encrypted password
                var newLogin = new TLoginMst
                {
                    email_id = login.EmailId,
                    password = encryptedPwd, // Store encrypted password in password column
                    user_type = "customer",
                    created_on = DateTime.Now,
                    is_Active = true
                };

                _context.TLoginMsts.Add(newLogin);
                _context.SaveChanges();

                // Create user details entry
                var newUserDetails = new TUserMst
                {
                    login_id = newLogin.login_id,
                    user_name = login.EmailId.Split('@')[0],
                    city = "Default",
                    pincode = "000000",
                    gender = "Other",
                    is_Active = true
                };

                _context.TUserMsts.Add(newUserDetails);
                _context.SaveChanges();

                return new BllOutput
                {
                    Data = new { Email = login.EmailId, LoginId = newLogin.login_id },
                    IsSuccess = true,
                    ExtraData = null
                };
            }
            catch (Exception ex)
            {
                return new BllOutput
                {
                    Data = null,
                    IsSuccess = false,
                    ExtraData = ex.Message
                };
            }
        }

        public BllOutput PlaceOrder(Order order)
        {
            try
            {
                // Get login_id from email
                var user = _context.TLoginMsts.FirstOrDefault(l => l.email_id == order.CustomerEmail);
                if (user == null)
                {
                    return new BllOutput
                    {
                        Data = null,
                        IsSuccess = false,
                        ExtraData = "User not found"
                    };
                }

                // Validate all items first before making any changes
                foreach (var item in order.Items)
                {
                    var product = _context.TProductMsts.FirstOrDefault(p => p.prod_id == item.StockItemId);
                    if (product == null)
                    {
                        return new BllOutput
                        {
                            Data = null,
                            IsSuccess = false,
                            ExtraData = $"Product with ID {item.StockItemId} not found"
                        };
                    }

                    if (product.quantity < item.Quantity)
                    {
                        return new BllOutput
                        {
                            Data = null,
                            IsSuccess = false,
                            ExtraData = $"Insufficient quantity for {product.prod_name}. Available: {product.quantity}, Requested: {item.Quantity}"
                        };
                    }
                }

                // Create single order entry for the entire order
                var newOrder = new TOrderMst
                {
                    ord_date = DateTime.Now,
                    ordered_by = user.login_id,
                    ord_status = order.Status,
                    prod_id = order.Items.FirstOrDefault()?.StockItemId ?? 0, // First product ID for backward compatibility
                    ord_quantity = order.Items.Sum(i => i.Quantity),
                    ord_price = order.TotalAmount
                };

                _context.TOrderMsts.Add(newOrder);
                _context.SaveChanges();

                // Create order product mappings for each item
                foreach (var item in order.Items)
                {
                    // Decrement product quantity
                    var product = _context.TProductMsts.FirstOrDefault(p => p.prod_id == item.StockItemId);
                    if (product != null)
                    {
                        product.quantity -= item.Quantity;
                    }

                    // Create order product mapping
                    var orderMapping = new TOrderProductMapping
                    {
                        order_id = newOrder.ord_id,
                        product_id = item.StockItemId,
                        quantity = item.Quantity,
                        price = item.Price,
                        isActive = true
                    };

                    _context.TOrderProductMappings.Add(orderMapping);
                }

                _context.SaveChanges();

                return new BllOutput
                {
                    Data = new { OrderId = newOrder.ord_id, Message = "Order placed successfully. Invoice sent to email." },
                    IsSuccess = true,
                    ExtraData = null
                };
            }
            catch (Exception ex)
            {
                return new BllOutput
                {
                    Data = null,
                    IsSuccess = false,
                    ExtraData = ex.Message
                };
            }
        }

        public BllOutput GetOrderHistory(string customerEmail)
        {
            try
            {
                // Get login_id from email
                var user = _context.TLoginMsts.FirstOrDefault(l => l.email_id == customerEmail);
                if (user == null)
                {
                    return new BllOutput
                    {
                        Data = null,
                        IsSuccess = false,
                        ExtraData = "User not found"
                    };
                }

                // Get orders with their product mappings - show last 5 orders grouped
                var orders = _context.TOrderMsts
                    .Include(o => o.TOrderProductMappings)
                    .ThenInclude(opm => opm.TProductMst)
                    .Where(o => o.ordered_by == user.login_id)
                    .OrderByDescending(o => o.ord_id)
                    .Take(5)
                    .Select(o => new
                    {
                        orderGroupId = o.ord_id.ToString(),
                        orderDate = o.ord_date,
                        totalAmount = o.ord_price,
                        itemCount = o.TOrderProductMappings.Count,
                        items = o.TOrderProductMappings.Select(opm => new
                        {
                            orderId = opm.opm_id,
                            itemName = opm.TProductMst.prod_name,
                            expiryDate = opm.TProductMst.exp_date ?? DateTime.Now.AddDays(30),
                            quantity = opm.quantity,
                            price = opm.price,
                            status = o.ord_status
                        }).ToList()
                    })
                    .ToList();

                return new BllOutput
                {
                    Data = orders,
                    IsSuccess = true,
                    ExtraData = null
                };
            }
            catch (Exception ex)
            {
                return new BllOutput
                {
                    Data = null,
                    IsSuccess = false,
                    ExtraData = ex.Message
                };
            }
        }
    }
}
