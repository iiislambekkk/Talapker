// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.S3;

namespace Talapker.Web.Areas.Identity.Pages.Account.Manage
{
    [RequestSizeLimit(100_000_000)]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IS3StorageService _s3Service;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<SharedResource>  localizer,
            IS3StorageService s3Service,
            IConfiguration configuration,
            ILogger<IndexModel> logger
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
            _s3Service = s3Service;
            _configuration = configuration;
            _logger =  logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        ///
        /// 
        [BindProperty]
        public IFormFile? AvatarFile { get; set; } // <-- отдельно, не внутри Input
        
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "FirstName")]
            public string FirstName { get; set; }
            
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "LastName")]
            public string LastName { get; set; }
            
            public string InitialAvatarPath { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName  = user.FirstName,
                LastName = user.LastName,
                InitialAvatarPath = $"{_configuration["CloudflareCdnUrl"]}/{user.AvatarKey}"
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }
            

            Console.WriteLine(AvatarFile == null);
            
            Console.WriteLine(AvatarFile?.Length);
            
            if (AvatarFile is { Length: > 0 })
            {
                var oldAvatarKey = user.AvatarKey; // capture old key
                var uniqueKey = $"identity/{Guid.NewGuid()}-{AvatarFile.FileName}";
                _logger.LogInformation("Uploading new avatar for user {UserId}. Key: {Key}", user.Id, uniqueKey);

                try
                {
                    var presignedUrl = await _s3Service.GetPresignedUrl(uniqueKey, AvatarFile.ContentType);
                    _logger.LogInformation("Got presigned URL for key {Key}", uniqueKey);

                    await using var stream = AvatarFile.OpenReadStream();
                    var content = new StreamContent(stream);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(AvatarFile.ContentType);

                    using var http = new HttpClient();
                    var response = await http.PutAsync(presignedUrl, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorText = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Failed to upload avatar {Key}. Status: {Status}, Error: {Error}",
                            uniqueKey, response.StatusCode, errorText);
                        throw new Exception($"Upload failed: {(int)response.StatusCode} {errorText}");
                    }

                    _logger.LogInformation("Successfully uploaded avatar for user {UserId}. Key: {Key}", user.Id, uniqueKey);

                    user.AvatarKey = uniqueKey;

                    if (!string.IsNullOrEmpty(oldAvatarKey))
                    {
                        _logger.LogInformation("Deleting old avatar for user {UserId}. Key: {Key}", user.Id, oldAvatarKey);
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _s3Service.DeleteAsync(oldAvatarKey);
                                _logger.LogInformation("Successfully deleted old avatar {Key}", oldAvatarKey);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete old avatar {Key}", oldAvatarKey);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during avatar upload for user {UserId}", user.Id);
                    throw;
                }
            }
            
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["ProfileUpdated"];
            return RedirectToPage();
        }
    }
}
