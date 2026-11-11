
using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ImagesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Upload an event banner.
        // Only logged-in organisers can upload event banners.
        [HttpPost("event-banner")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadEventBanner(
            IFormFile image)
        {
            if (!HttpContext.Session.IsOrganiser())
            {
                return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
            }

            return await Upload(
                image,
                "raceday/event-banners");
        }

        // Upload a profile picture.
        // The user must be logged in.
        [HttpPost("profile-picture")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadProfilePicture(
            IFormFile image)
        {
            if (HttpContext.Session.GetUser() is null)
            {
                return Unauthorized();
            }

            return await Upload(
                image,
                "raceday/profile-pictures");
        }

        // Shared method for uploading images to Cloudinary.
        private async Task<IActionResult> Upload(
            IFormFile image,
            string folder)
        {
            // Check whether an image was selected.
            if (image == null || image.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please choose an image file."
                });
            }

            // Check whether the uploaded file is an image.
            if (string.IsNullOrWhiteSpace(image.ContentType) ||
                !image.ContentType.StartsWith(
                    "image/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only image files are accepted."
                });
            }

            // Read credentials from the API configuration.
            // These match the names saved using dotnet user-secrets.
            var cloudName =
                _configuration["CLOUDINARY_CLOUD_NAME"];

            var apiKey =
                _configuration["CLOUDINARY_API_KEY"];

            var apiSecret =
                _configuration["CLOUDINARY_API_SECRET"];

            // Check that all three credentials are available.
            if (string.IsNullOrWhiteSpace(cloudName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(apiSecret))
            {
                return StatusCode(503, new
                {
                    message =
                        "Cloudinary is not configured. " +
                        "Check the RaceDay.API user-secrets configuration."
                });
            }

            try
            {
                // Connect to Cloudinary.
                var account = new Account(
                    cloudName,
                    apiKey,
                    apiSecret);

                var cloudinary = new Cloudinary(account);

                // Read the selected image.
                await using var stream =
                    image.OpenReadStream();

                // Prepare the image for uploading.
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(
                        Path.GetFileName(image.FileName),
                        stream),

                    Folder = folder,
                    UseFilename = false,
                    UniqueFilename = true
                };

                // Upload the image.
                var result =
                    await cloudinary.UploadAsync(uploadParams);

                // Handle errors returned by Cloudinary.
                if (result.Error != null)
                {
                    return StatusCode(502, new
                    {
                        message =
                            "Cloudinary upload failed: " +
                            result.Error.Message
                    });
                }

                // Check whether Cloudinary returned an image URL.
                if (result.SecureUrl == null)
                {
                    return StatusCode(502, new
                    {
                        message =
                            "Image upload completed, but no image URL was returned."
                    });
                }

                // Return the uploaded image URL.
                return Ok(new
                {
                    imageUrl = result.SecureUrl.ToString()
                });
            }
            catch (Exception)
            {
                // Do not expose Cloudinary credentials or
                // internal exception details to website users.
                return StatusCode(500, new
                {
                    message =
                        "An error occurred while uploading the image. " +
                        "Check the API logs for more information."
                });
            }
        }
    }
}
