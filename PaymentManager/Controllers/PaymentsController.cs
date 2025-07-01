using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaymentManager.Models;

namespace PaymentManager.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PaymentDbContext _context;


        public PaymentsController(PaymentDbContext context)
        {
            _context = context;
        }

        // GET: Obtener la lista de todos los pagos ordenados del mas reciente al menos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync());
        }


        // GET: Mostrar formulario para crear un nuevo pago 
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guardar un nuevo pago en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,PaymentDate,Category,Reference,CreatedAt")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }


        // GET: Mostrar formulario para editar un pago exitente (se ubica el pago con su id)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Actualizar un pago existente en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,PaymentDate,Category,Reference,CreatedAt")] Payment payment)
        {
            if (id != payment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }


        // POST: Elimina un pago especifico por id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST:Elimina varios pagos seleccionados 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                var payments = _context.Payments.Where(p => ids.Contains(p.Id));
                _context.Payments.RemoveRange(payments);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
