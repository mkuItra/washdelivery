using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WashDelivery.Application.DTOs.Laundries;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using System.Threading.Tasks;
using WashDelivery.Web.ViewModels.Laundry;
using Microsoft.Extensions.Logging;

namespace WashDelivery.Web.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class LaundryController : Controller
    {
        private readonly ILaundryService _laundryService;
        private readonly ILogger<LaundryController> _logger;
        
        public LaundryController(ILaundryService laundryService, ILogger<LaundryController> logger)
        {
            _laundryService = laundryService;
            _logger = logger;
        }
        
        public async Task<IActionResult> Index()
        {
            var laundries = await _laundryService.GetAllAsync();
            return View(laundries);
        }
        
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateLaundryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _laundryService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var laundry = await _laundryService.GetByIdAsync(id);
            if (laundry == null)
                return NotFound();

            var model = new UpdateLaundryDto
            {
                Name = laundry.Name,
                ContactEmail = laundry.ContactEmail,
                ContactPhone = laundry.ContactPhone,
                Address = $"{laundry.Address.Street}, {laundry.Address.PostalCode} {laundry.Address.City}"
            };

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(string id, UpdateLaundryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _laundryService.UpdateAsync(id, dto, User.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                await _laundryService.ActivateAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                await _laundryService.DeactivateAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> Workers(string id)
        {
            var laundry = await _laundryService.GetByIdAsync(id);
            if (laundry == null)
                return NotFound();

            var workersDto = await _laundryService.GetWorkersAsync(id);
            var workers = workersDto.Select(w => new ViewModels.Laundry.LaundryWorkerDto
            {
                Id = w.Id,
                Email = w.Email,
                FirstName = w.FirstName,
                LastName = w.LastName,
                PhoneNumber = w.PhoneNumber,
                Role = w.Role
            });

            var model = new LaundryWorkersViewModel
            {
                LaundryId = id,
                LaundryName = laundry.Name,
                Workers = workers
            };

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> RemoveWorker(string laundryId, string userId)
        {
            await _laundryService.RemoveWorkerAsync(laundryId, userId);
            return RedirectToAction(nameof(Workers), new { id = laundryId });
        }
        
        [HttpPost]
        public async Task<IActionResult> AddWorkers([FromBody] AddWorkersRequest request)
        {
            try
            {
                await _laundryService.AddWorkersAsync(request.LaundryId, request.UserIds);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding workers to laundry {LaundryId}", request.LaundryId);
                return BadRequest();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> RemoveWorkers([FromBody] RemoveWorkersRequest request)
        {
            try
            {
                await _laundryService.RemoveWorkersAsync(request.LaundryId, request.UserIds);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing workers from laundry {LaundryId}", request.LaundryId);
                return BadRequest();
            }
        }
        
        public class AddWorkersRequest
        {
            public string LaundryId { get; set; } = string.Empty;
            public List<string> UserIds { get; set; } = new();
        }
        
        public class RemoveWorkersRequest
        {
            public string LaundryId { get; set; } = string.Empty;
            public List<string> UserIds { get; set; } = new();
        }
        
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var laundry = await _laundryService.GetByIdAsync(id);
            if (laundry == null)
                return NotFound();

            var viewModel = new LaundryDetailsViewModel
            {
                Id = laundry.Id,
                Name = laundry.Name,
                ContactEmail = laundry.ContactEmail,
                ContactPhone = laundry.ContactPhone,
                Address = $"{laundry.Address.Street}, {laundry.Address.PostalCode} {laundry.Address.City}",
                IsActive = laundry.IsActive,
                Rating = (double)laundry.Rating
            };

            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetLaundryDetails(string id)
        {
            var laundry = await _laundryService.GetLaundryDetailsAsync(id);
            if (laundry == null)
            {
                return NotFound();
            }
            return Json(laundry);
        }
        
        // Inne akcje...
    }
} 