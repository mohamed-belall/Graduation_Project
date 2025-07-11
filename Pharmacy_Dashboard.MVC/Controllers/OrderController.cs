using AutoMapper;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.Enums;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Pharmacies;
using Graduation_Project.Core.Specifications.MedicineSpecifications;
using Graduation_Project.Core.Specifications.PharmacySpecifications;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_Dashboard.MVC.ViewModel.OrderViewModels;
using System.Security.Claims;

namespace Pharmacy_Dashboard.MVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPatientService _patientService;
        //private readonly INotificationService _notificationService;

        public OrderController(IUnitOfWork unitOfWork, IMapper mapper, IPatientService patientService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _patientService = patientService;
            //_notificationService = notificationService;
        }

        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        [HttpGet]
        public async Task<IActionResult> Index(OrderPageParams orderPageParams)/*(int pageNumber = 1, int pharmacyId = 1, OrderStatus? orderStatusFilter = null, DateTime? dateFilter = null)*/
        {
            // TODO : get registerd pharmacy Id
            var pharmacyId = int.Parse(User.FindFirstValue(Identifiers.PharmacyId));

            if (!ModelState.IsValid)
            {
                return View(new OrdersListViewModel
                {
                    Orders = new List<OrderContent>(),
                    CurrentPage = 1,
                    TotalPages = 1,
                    CurrentFilter = new OrderFilter()
                });
            }

            int pageSize = 20;
            var ordersSpecs = new OrdersSpecifications(pharmacyId, orderPageParams.pageNumber, pageSize, orderPageParams.orderStatusFilter, orderPageParams.dateFilter);
            var orders = await _unitOfWork.Repository<PharmacyOrder>().GetAllWithSpecAsync(ordersSpecs);
            var count = await _unitOfWork.Repository<PharmacyOrder>().GetCountAsync(new OrdersSpecifications(1, orderPageParams.orderStatusFilter, orderPageParams.dateFilter));

            if (orders is null)
                ModelState.AddModelError("", "Orders Is Empty");

            var viewModel = new OrdersListViewModel
            {
                Orders = _mapper.Map<List<OrderContent>>(orders),
                CurrentPage = orderPageParams.pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                CurrentFilter = new OrderFilter
                {
                    OrderStatus = orderPageParams.orderStatusFilter,
                    DateFilter = orderPageParams.dateFilter
                }
            };

            // return it to view
            return View(viewModel);

        }

        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        [HttpGet]
        public async Task<IActionResult> GetOrder(int id)
        {
            // get the order from db
            var orderSpecs = new OrderWithMedicinesSpecs(id);
            var order = await _unitOfWork.Repository<PharmacyOrder>().GetWithSpecsAsync(orderSpecs);

            // map the order
            var viewModel = _mapper.Map<OrderCardViewModel>(order);

            // return
            return View("Card", viewModel);
        }

        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(UpdatedOrderParams updatedOrder)
        {
            PharmacyOrder? order = new PharmacyOrder();

            if (updatedOrder.OrderStatus == OrderStatus.Confirmed)
            {
                // load order from db 
                order = await _unitOfWork.Repository<PharmacyOrder>().GetAsync(updatedOrder.Id);

                // update its status and delivery date
                order.Status = updatedOrder.OrderStatus;

                order.DeliverDate = updatedOrder.DeliveryDate;
            }

            // if status will be delivered ==> Decrease the medicines from stock table 
            if (updatedOrder.OrderStatus == OrderStatus.Completed)
            {
                // get the order from db
                var orderSpecs = new OrderWithMedicinesQuantitySpecs(updatedOrder.Id);
                order = await _unitOfWork.Repository<PharmacyOrder>().GetWithSpecsAsync(orderSpecs);

                var medicineStockDictionary = order.Pharmacy.pharmacyMedicineStocks.ToDictionary(p => p.MedicineId);

                // get medicines,quantity from the order (MedicinePharmacyOrder Table)
                foreach (var medicinePharmacyOrder in order.MedicinePharmacyOrders)
                {
                    if (medicineStockDictionary.TryGetValue(medicinePharmacyOrder.MedicineId, out var pharmacyMedicineStock))
                        pharmacyMedicineStock.Quantity -= medicinePharmacyOrder.Quantity;
                }

                // Update Order Status
                order.Status = OrderStatus.Completed;

                // Increase Patient Points
                await _patientService.UpdatePoints(order.PatientId, Points.CompletedOrder);
                // push notifications
                //await _notificationService.SendNotificationAsync(order.Patient.ApplicationUserId, $"New {Points.CompletedAppointment} Points", "New Points");

            }

            // save it 
            _unitOfWork.Repository<PharmacyOrder>().Update(order);
            await _unitOfWork.Repository<PharmacyOrder>().SaveAsync();

            TempData["Message"] = $"Order #{order.Id} has been {order.Status.ToString().ToLower()}.";

            // return to Orders Table
            return RedirectToAction("GetOrder", new { id = order.Id });
        }
    }
}
